using _7thHeaven.Code;
using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeaven.Classes.WCF;
using SeventhHeaven.ViewModels;
using SeventhHeaven.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SeventhHeavenUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public const string uniqueAppGuid = "F73958FA-160F-4185-AE8F-CF5B7EA89494";

        public static Mutex uniqueMutex;

        private bool hasShownErrorWindow = false;

        public App()
        {
            uniqueMutex = new Mutex(true, uniqueAppGuid, out bool gotMutex);
            GC.KeepAlive(App.uniqueMutex);

            if (SingleInstance.IsFirstInstance(uniqueAppGuid, true))
            {
                SingleInstance.OnSecondInstanceStarted += SingleInstance_OnSecondInstanceStarted;
            }
            else
            {
                // second instance so notify first instance
                SingleInstance.NotifyFirstInstance(uniqueAppGuid);
            }

            if (!gotMutex)
            {
                App.Current.Shutdown(); // only one instance of the app opened at a time so shutdown
                return;
            }
        }

        private void SingleInstance_OnSecondInstanceStarted(object sender, SecondInstanceStartedEventArgs e)
        {
            // e.Args[0] = path to 7th Heaven .exe
            ProcessCommandLineArgs(e.Args);
        }

        internal static void ProcessCommandLineArgs(string[] args, bool closeAfterProcessing = false)
        {
            bool hasLaunchCommand = args.Any(s => s.Equals("/LAUNCH", StringComparison.InvariantCultureIgnoreCase));

            foreach (string parm in args)
            {
                if (parm.StartsWith("iros://", StringComparison.InvariantCultureIgnoreCase))
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow window = App.Current.MainWindow as MainWindow;
                        window.ViewModel.AddIrosUrlToSubscriptions(parm);
                    });
                }
                else if (parm.StartsWith("/OPENIRO:", StringComparison.InvariantCultureIgnoreCase))
                {
                    string irofile = null;
                    string irofilenoext = null;

                    try
                    {
                        irofile = parm.Substring(9);
                        irofilenoext = Path.GetFileNameWithoutExtension(irofile);
                        Logger.Info("Importing IRO from Windows " + irofile);

                        ModImporter.ImportMod(irofile, ModImporter.ParseNameFromFileOrFolder(irofilenoext), true, false);
                    }
                    catch (Exception ex)
                    {
                        Sys.Message(new WMessage("Mod " + irofilenoext + " failed to import: " + ex.ToString(), true));
                        continue;
                    }

                    Sys.Message(new WMessage($"Auto imported mod {irofilenoext}", true));
                }
                else if (parm.StartsWith("/PROFILE:", StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        string profileToLoad = Path.Combine(Sys.PathToProfiles, $"{parm.Substring(9)}.xml");
                        Profile profile = Util.Deserialize<Profile>(profileToLoad);

                        Sys.Settings.CurrentProfile = parm.Substring(9);
                        Sys.ActiveProfile = profile;

                        Sys.Message(new WMessage($"Loaded profile {Sys.Settings.CurrentProfile}"));
                        Sys.ActiveProfile.RemoveDeletedItems(doWarn: true);

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow window = App.Current.MainWindow as MainWindow;
                            window.ViewModel.RefreshProfile();
                        });
                    }
                    catch (Exception e)
                    {
                        Sys.Message(new WMessage($"Could not load profile {parm.Substring(9)}", WMessageLogLevel.Error, e));
                    }
                }
                else if (parm.Equals("/LAUNCH", StringComparison.InvariantCultureIgnoreCase))
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow window = App.Current.MainWindow as MainWindow;
                        window.ViewModel.LaunchGame();
                    });
                }
                else if (parm.Equals("/QUIT", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (hasLaunchCommand)
                    {
                        bool isGameLaunching = false;

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MainWindow window = App.Current.MainWindow as MainWindow;
                            isGameLaunching = window.ViewModel.IsGameLaunching;
                        });

                        while (isGameLaunching || GameLauncher.IsFF7Running())
                        {
                            Thread.Sleep(10000); // sleep 10 seconds and check again until ff7 not running

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                MainWindow window = App.Current.MainWindow as MainWindow;
                                isGameLaunching = window.ViewModel.IsGameLaunching;
                            });
                        }

                        // wait for ff7 to close before closing down 7th Heaven
                    }

                    ShutdownApp();
                }
                else if (parm.Equals("/MINI", StringComparison.InvariantCultureIgnoreCase))
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        App.Current.MainWindow.WindowState = WindowState.Minimized;
                    });
                }
                else if (parm.StartsWith("/PACKIRO:", StringComparison.InvariantCultureIgnoreCase))
                {
                    string pathToFolder;

                    try
                    {
                        pathToFolder = parm.Substring(9);
                        string folderName = new DirectoryInfo(pathToFolder).Name;
                        string parentDir = new DirectoryInfo(pathToFolder).Parent.FullName;

                        PackIroViewModel packViewModel = new PackIroViewModel()
                        {
                            PathToSourceFolder = pathToFolder,
                            PathToOutputFile = Path.Combine(parentDir, $"{folderName}.iro"),
                        };

                        Sys.Message(new WMessage($"Packing {folderName} into .iro ...", true));

                        Task packTask = packViewModel.PackIro();
                        packTask.ContinueWith((result) =>
                        {
                            Sys.Message(new WMessage(packViewModel.StatusText, true));

                            if (closeAfterProcessing)
                            {
                                ShutdownApp();
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Sys.Message(new WMessage($"Failed to pack into .iro", WMessageLogLevel.Error, e) { IsImportant = true });
                    }
                }
                else if (parm.StartsWith("/UNPACKIRO:", StringComparison.InvariantCultureIgnoreCase))
                {
                    string pathToIro;

                    try
                    {
                        pathToIro = parm.Substring(11);
                        string fileName = Path.GetFileNameWithoutExtension(pathToIro);
                        string pathToDir = Path.GetDirectoryName(pathToIro);

                        UnpackIroViewModel unpackViewModel = new UnpackIroViewModel()
                        {
                            PathToIroFile = pathToIro,
                            PathToOutputFolder = Path.Combine(pathToDir, fileName),
                        };

                        Sys.Message(new WMessage($"Unpacking {fileName}.iro into subfolder '{fileName}' ...", true));

                        Task unpackTask = unpackViewModel.UnpackIro();
                        unpackTask.ContinueWith((result) =>
                        {
                            Sys.Message(new WMessage(unpackViewModel.StatusText, true));

                            if (closeAfterProcessing)
                            {
                                ShutdownApp();
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Sys.Message(new WMessage($"Failed to unpack .iro", WMessageLogLevel.Error, e) { IsImportant = true });
                    }
                }
            }
        }

        private static void ShutdownApp()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                App.Current.Shutdown();
            });
        }

        public static Version GetAppVersion()
        {
            return typeof(SeventhHeavenUI.App).Assembly.GetName().Version;
        }

        public static bool IsRunningAppAsAdministrator()
        {
            // reference: https://stackoverflow.com/questions/11660184/c-sharp-check-if-run-as-administrator
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }

        internal static string GetAppName()
        {
            foreach (object item in System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(false))
            {
                if (item is System.Reflection.AssemblyTitleAttribute)
                {
                    return (item as System.Reflection.AssemblyTitleAttribute).Title;
                }
            }

            return "7th Heaven"; // default if can't find for some reason
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupExceptionHandling();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            Dispatcher.UnhandledException += (s, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"! Unhandled exception ({source})";
            Logger.Error(message);
            Logger.Error(exception);

            if (!hasShownErrorWindow)
            {
                hasShownErrorWindow = true;
                UnhandledErrorWindow.Show(exception);
            }
        }

        public static void ForceUpdateUI()
        {
            DispatcherFrame frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
        }

    }
}
