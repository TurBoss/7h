using _7thHeaven.Code;
using _7thWrapperLib;
using Iros._7th;
using Iros._7th.Workshop;
using Microsoft.Win32;
using SeventhHeaven.ViewModels;
using SeventhHeaven.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.Classes
{
    public enum FF7Version
    {
        Unknown = -1,
        Steam,
        ReRelease,
        Original98
    }

    internal enum GraphicsRenderer
    {
        SoftwareRenderer = 0,
        D3DHardwareAccelerated = 1,
        CustomDriver = 3
    }

    /// <summary>
    /// Responsibile for the entire process that happens for launching the game
    /// </summary>
    public class GameLauncher
    {
        #region Data Members and Properties

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static GameLauncher _instance;

        public static GameLauncher Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameLauncher();

                return _instance;
            }
        }

        private Dictionary<string, Process> _alsoLaunchProcesses = new Dictionary<string, Process>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, _7HPlugin> _plugins = new Dictionary<string, _7HPlugin>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<_7thWrapperLib.ProgramInfo, Process> _sideLoadProcesses = new Dictionary<_7thWrapperLib.ProgramInfo, Process>();

        public delegate void OnProgressChanged(string message);
        public event OnProgressChanged ProgressChanged;

        public FF7Version InstallVersion { get; set; }
        public string DriveLetter { get; set; }

        #endregion

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_FORCEMINIMIZE = 11;
        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        public static bool LaunchGame(bool varDump, bool debug, bool launchWithNoMods = false)
        {
            Instance.RaiseProgressChanged($"Checking FF7 is not running ...");
            if (IsFF7Running())
            {
                Instance.RaiseProgressChanged("\tFF7 is already running. Only 1 instance is allowed. Aborting ...");
                return false;
            }

            Instance.RaiseProgressChanged($"Checking FF7 .exe exists at {Sys.Settings.FF7Exe} ...");
            if (!File.Exists(Sys.Settings.FF7Exe))
            {
                Instance.RaiseProgressChanged("\tfile not found. Aborting ...");
                Instance.RaiseProgressChanged("FF7.exe not found. You may need to configure 7H using the Settings>General Settings menu.");
                return false;
            }


            //
            // GAME CONVERTER - Make sure game is ready for mods
            // 
            GameConverter converter = new GameConverter(new ConversionSettings()
            {
                InstallPath = Path.GetDirectoryName(Sys.Settings.FF7Exe)
            });
            converter.MessageSent += GameConverter_MessageSent;


            Instance.RaiseProgressChanged("Verifying installed game is compatible ...");
            if (converter.IsGamePirated())
            {
                Instance.RaiseProgressChanged("Incompatible game files found. Unable to continue.");
                Logger.Info(FileUtils.ListAllFiles(converter.InstallPath));
                return false;
            }

            Instance.RaiseProgressChanged("Verifying game is full/max install ...");
            if (!converter.VerifyFullInstallation())
            {
                string messageToUser = "Your FF7 installation folder is missing critical file(s). When you installed FF7, you may have accidentally chosen to install the 'Standard Install', but the 'Maximum Install' is required.\n\n7th Heaven can repair this for you automatically. Simply insert one of your game discs and try again.";
                Instance.RaiseProgressChanged(messageToUser);
                return false;
            }

            Instance.RaiseProgressChanged("Verifying addtional files for battle,kernel,movies exist ...");
            if (!converter.VerifyAdditionalFilesExist())
            {
                Instance.RaiseProgressChanged("Failed to verify/copy missing additional files. Aborting...");
                return false;
            }

            Instance.RaiseProgressChanged("Verifying movie files exist ...");
            if (!converter.AllMovieFilesExist(Sys.Settings.MovieFolder))
            {
                Instance.RaiseProgressChanged($"\tcould not find all movie files at {Sys.Settings.MovieFolder}");

                string otherMovieFolder = Path.Combine(converter.InstallPath, "data", "movies");
                if (converter.AllMovieFilesExist(otherMovieFolder))
                {
                    Instance.RaiseProgressChanged($"\tall files found at {otherMovieFolder}. Updating movie path setting.");
                    Sys.Settings.MovieFolder = otherMovieFolder;
                }
                else
                {
                    Instance.RaiseProgressChanged($"\tmovie files also not found at {otherMovieFolder}");
                    Instance.RaiseProgressChanged($"\tattempting to copy movie files from disc(s) ...");

                    if (!converter.CopyMovieFilesToFolder(Sys.Settings.MovieFolder))
                    {
                        Instance.RaiseProgressChanged("7th Heaven was unable to locate one or more movie files (view above details for missing files). 7th Heaven can repair this for you automatically. Simply insert the corresponding disc # and try again.\n\nIf you do not have your game disc(s) or you have an alternate location where your movie files are stored, then please go to Settings>General Settings and correct your Movies Path.");
                        return false;
                    }
                }
            }

            Instance.RaiseProgressChanged("Verifying music files exist ...");
            if (!converter.AllMusicFilesExist())
            {
                converter.CopyMusicFiles();

                if (!converter.AllMusicFilesExist())
                {
                    Instance.RaiseProgressChanged($"WARNING: .ogg music files are missing under '{converter.InstallPath}\\music\\vgmstream\\'. You will need to use a music mod in order to hear game music, or select the MIDI option in the Game Driver settings.");
                }
            }

            Instance.RaiseProgressChanged("Verifying additional folders exist ...");
            converter.CreateMissingFolders();


            string backupFolderPath = Path.Combine(converter.InstallPath, "7H2.0-BACKUP", DateTime.Now.ToString("yyyyMMddHHmmss"));


            converter.CheckAndCopyOldGameConverterFiles(backupFolderPath);

            Instance.RaiseProgressChanged("Verifying latest game driver is installed ...");
            if (!converter.InstallLatestGameDriver(backupFolderPath))
            {
                Instance.RaiseProgressChanged("Something went wrong trying to detect/install game driver. Aborting ...");
                return false;
            }


            Instance.RaiseProgressChanged("Verifying game driver plugins/shaders folders exist ...");
            converter.CopyMissingPluginsAndShaders();


            Instance.RaiseProgressChanged("Verifying ff7 exe ...");
            if (new FileInfo(Sys.Settings.FF7Exe).Name.Equals("ff7.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                // only compare exes are different if ff7.exe set in Settings (and not something like ff7_bc.exe)
                if (converter.IsExeDifferent())
                {
                    Instance.RaiseProgressChanged("\tff7.exe detected to be different. creating backup and copying correct .exe...");
                    if (converter.BackupExe(backupFolderPath))
                    {
                        bool didCopy = converter.CopyFF7ExeToGame();
                        if (!didCopy)
                        {
                            Instance.RaiseProgressChanged("\tfailed to copy ff7.exe. Aborting ...");
                            return false;
                        }
                    }
                    else
                    {
                        Instance.RaiseProgressChanged("\tfailed to create backup of ff7.exe. Aborting ...");
                        return false;
                    }
                }
            }

            if (converter.IsConfigExeDifferent())
            {
                Instance.RaiseProgressChanged("\tFF7Config.exe detected to be different. creating backup and copying correct .exe...");
                if (converter.BackupFF7ConfigExe(backupFolderPath))
                {
                    bool didCopy = converter.CopyFF7ConfigExeToGame();
                    if (!didCopy)
                    {
                        Instance.RaiseProgressChanged("\tfailed to copy FF7Config.exe. Aborting ...");
                        return false;
                    }
                }
                else
                {
                    Instance.RaiseProgressChanged("\tfailed to create backup of FF7Config.exe. Aborting ...");
                    return false;
                }
            }




            //
            //
            //
            Instance.RaiseProgressChanged("Checking a profile is active ...");
            if (Sys.ActiveProfile == null)
            {
                Instance.RaiseProgressChanged("\tactive profile not found. Aborting ...");
                Instance.RaiseProgressChanged("Create a profile first in Settings>Profiles and try again.");
                return false;
            }

            Instance.RaiseProgressChanged("Checking mod compatibility requirements ...");
            if (!SanityCheckCompatibility())
            {
                Instance.RaiseProgressChanged("\tfailed mod compatibility check. Aborting ...");
                return false;
            }

            Instance.RaiseProgressChanged("Checking mod constraints for compatibility ...");
            if (!SanityCheckSettings())
            {
                Instance.RaiseProgressChanged("\tfailed mod constraint check. Aborting ...");
                return false;
            }

            Instance.RaiseProgressChanged("Checking mod load order requirements ...");
            if (!VerifyOrdering())
            {
                Instance.RaiseProgressChanged("\tfailed mod load order check. Aborting ...");
                return false;
            }


            //
            // Get Drive Letter and auto mount if needed
            //
            Instance.RaiseProgressChanged("Looking for game disc ...");
            Instance.DriveLetter = GetDriveLetter();

            if (!string.IsNullOrEmpty(Instance.DriveLetter))
            {
                Instance.RaiseProgressChanged($"Found game disc at {Instance.DriveLetter} ...");
            }
            else
            {
                Instance.RaiseProgressChanged($"Failed to find game disc ...");

                if (!OSHasAutoMountSupport())
                {
                    Instance.RaiseProgressChanged($"OS does not support auto mounting virtual disc. You must mount a virtual disk named FF7DISC1.ISO, insert a USB flash drive named FF7DISC1, or rename a Hard Drive to FF7DISC1 ...");
                    return false;
                }
                else
                {
                    if (Sys.Settings.GameLaunchSettings.AutoMountGameDisc)
                    {
                        Instance.RaiseProgressChanged($"Auto mounting virtual game disc ...");
                        bool didMount = MountIso();

                        if (!didMount)
                        {
                            Instance.RaiseProgressChanged($"Failed to auto mount virtual disc at {Path.Combine(Sys._7HFolder, "Resources", "FF7DISC1.ISO")} ...");
                            return false;
                        }

                        Instance.RaiseProgressChanged("Looking for game disc after mounting ...");
                        Instance.DriveLetter = GetDriveLetter();

                        if (string.IsNullOrEmpty(Instance.DriveLetter))
                        {
                            Instance.RaiseProgressChanged($"Failed to find game disc after auto mounting ...");
                            return false;
                        }

                        Instance.RaiseProgressChanged($"Found game disc at {Instance.DriveLetter} ...");
                    }
                }
            }

            //
            // Update Registry with new launch settings
            // 
            Instance.SetRegistryValues();
            Instance.SetCompatibilityFlagsInRegistry();


            //
            // Determine if game will be ran as 'vanilla' with mods so don't have to inject with EasyHook
            //
            bool runAsVanilla = false;
            string vanillaMsg = "";

            if (launchWithNoMods)
            {
                vanillaMsg = "User requested to play with no mods. Launching game as 'vanilla' ...";
                runAsVanilla = true;
            }
            else if (Sys.ActiveProfile.ActiveItems.Count == 0)
            {
                vanillaMsg = "No mods have been activated. Launching game as 'vanilla' ...";
                runAsVanilla = true;
            }
            else if (Sys.Settings.GameLaunchSettings.SelectedRenderer != (int)GraphicsRenderer.CustomDriver)
            {
                vanillaMsg = "Selected Renderer is not set to 'Custom Driver'. Launching game as 'vanilla' ...";
                runAsVanilla = true;
            }



            RuntimeProfile runtimeProfile = null;

            if (!runAsVanilla)
            {
                //
                // Create Runtime Profile for Active Mods
                //
                Instance.RaiseProgressChanged("Creating Runtime Profile ...");
                runtimeProfile = CreateRuntimeProfile();

                if (runtimeProfile == null)
                {
                    Instance.RaiseProgressChanged("\tfailed to create Runtime Profile for active mods ...");
                    return false;
                }

                //
                // Start Turbolog for Variable Dump
                //
                if (varDump)
                {
                    Instance.RaiseProgressChanged("Variable Dump set to true. Starting TurBoLog ...");
                    StartTurboLogForVariableDump(runtimeProfile);
                }

                //
                // Copy EasyHook.dll to FF7
                //
                Instance.RaiseProgressChanged("Copying EasyHook.dll to FF7 path (if not found or older version detected) ...");
                CopyEasyHookDlls();
            }


            //
            // Copy input.cfg to FF7
            //
            Instance.RaiseProgressChanged("Copying ff7input.cfg to FF7 path ...");
            bool didCopyCfg = CopyKeyboardInputCfg();

            if (!didCopyCfg)
            {
                return false;
            }


            //
            // Setup log file if debugging
            //
            if (debug)
            {
                runtimeProfile.Options |= RuntimeOptions.DetailedLog;
                runtimeProfile.LogFile = Path.Combine(Path.GetDirectoryName(Sys.Settings.FF7Exe), "log.txt");

                Instance.RaiseProgressChanged($"Debug Logging set to true. Detailed logging will be written to {runtimeProfile.LogFile} ...");
            }

            //
            // Check/Disable Reunion Mod
            //
            Instance.RaiseProgressChanged("Checking if Reunion mod is installed ...");
            Instance.RaiseProgressChanged($"\tfound: {IsReunionModInstalled()}");

            bool didDisableReunion = false;
            if (IsReunionModInstalled() && Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch)
            {
                Instance.RaiseProgressChanged("Disabling Reunion mod (rename ddraw.dll -> Reunion.dll.bak) ...");
                EnableOrDisableReunionMod(doEnable: false);
                didDisableReunion = true;
            }

            // start FF7 proc as normal and return true when running the game as vanilla
            if (runAsVanilla)
            {
                Instance.RaiseProgressChanged(vanillaMsg);
                LaunchFF7Exe();

                if (didDisableReunion)
                {
                    Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(5000); // wait 5 seconds before renaming the dll so the game and gl driver can fully initialize
                        Instance.RaiseProgressChanged("Re-enabling Reunion mod (rename Reunion.dll.bak -> ddraw.dll) ...");
                        EnableOrDisableReunionMod(doEnable: true);
                    });
                }

                return true;
            }

            //
            // Attempt to Create FF7 Proc and Inject with EasyHook
            //
            int pid;
            try
            {
                Instance.RaiseProgressChanged($"Launching additional programs to run (if any) ...");
                Instance.LaunchAdditionalProgramsToRunPrior();

                RuntimeParams parms = new RuntimeParams
                {
                    ProfileFile = Path.GetTempFileName()
                };

                Instance.RaiseProgressChanged($"Writing temporary runtime profile file to {parms.ProfileFile} ...");

                using (FileStream fs = new FileStream(parms.ProfileFile, FileMode.Create))
                    Util.SerializeBinary(runtimeProfile, fs);

                // attempt to launch the game a few times in the case of an ApplicationException that can be thrown by EasyHook it seems randomly at times
                // ... The error tends to go away the second time trying but we will try multiple times before failing
                // ... if we fail to inject with EasyHook then we will give the user a chance to set the compatibility flag to fix the issue
                string lib = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "7thWrapperLib.dll");

                bool didInject = false;
                int attemptCount = 0;
                int totalAttempts = 7;
                pid = -1;

                while (!didInject && attemptCount < totalAttempts)
                {
                    didInject = false;

                    try
                    {
                        Instance.RaiseProgressChanged($"Attempting to inject with EasyHook: try # {attemptCount + 1} ...");

                        // attempt to inject on background thread so we can have a timeout if the process does not return in 10 seconds
                        // a successful injection should only take ~3 seconds
                        var waitTask = Task.Factory.StartNew(() =>
                        {
                            EasyHook.RemoteHooking.CreateAndInject(Sys.Settings.FF7Exe, String.Empty, 0, lib, null, out pid, parms);
                        }).ContinueWith((taskResult) =>
                        {
                            if (taskResult.IsFaulted)
                            {
                                // an error occurred when injecting so concatenate all errors to display in output
                                didInject = false;
                                string errors = "";
                                foreach (Exception ex in taskResult.Exception.InnerExceptions)
                                {
                                    if (ex is AggregateException)
                                    {
                                        errors += string.Join("; ", (ex as AggregateException).InnerExceptions.Select(s => s.Message));
                                    }
                                    else
                                    {
                                        errors += $"{ex.Message}; ";
                                    }
                                }

                                Instance.RaiseProgressChanged($"\treceived errors: {errors} ...");
                            }
                            else
                            {
                                didInject = true;
                            }
                        }).ConfigureAwait(false);

                        // Wait 10 seconds for the injection to complete
                        DateTime startTime = DateTime.Now;
                        while (!waitTask.GetAwaiter().IsCompleted)
                        {
                            TimeSpan elapsed = DateTime.Now.Subtract(startTime);
                            if (elapsed.Seconds > 10)
                            {
                                Instance.RaiseProgressChanged($"\treached timeout waiting for injection ...");
                                didInject = false;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Instance.RaiseProgressChanged($"\treceived unknown error: {e.Message} ...");
                    }
                    finally
                    {
                        attemptCount++;
                    }

                    // If the CreateAndInject() process fails to inject it will attempt to kill the process it started...
                    // ... this could fail and leave the process open which causes problems for the next attempt so make sure the process is killed on failure
                    if (!didInject)
                    {
                        string procName = Path.GetFileNameWithoutExtension(Sys.Settings.FF7Exe);
                        var openProcs = Process.GetProcessesByName(procName);
                        foreach (Process proc in openProcs)
                        {
                            if (!proc.HasExited)
                                proc.Kill();
                        }
                    }
                }

                if (!didInject)
                {
                    Instance.RaiseProgressChanged($"Failed to inject after max amount of tries ({totalAttempts}) ...");
                    Sys.Settings.GameLaunchSettings.HasReceivedCode5Error = true; // update launch settings to notify that user has received a code 5 error in the code

                    // give user option to set compat flag and try again
                    var viewModel = MessageDialogWindow.Show("Failed to inject with EasyHook after trying multiple times. This is usually fixed by setting the 'Code 5 Fix' to 'On' in the Game Launcher Settings.\n\nDo you want to apply the setting and try again?",
                                                             "Error - Failed To Start Game",
                                                             MessageBoxButton.YesNo,
                                                             MessageBoxImage.Warning);

                    if (viewModel.Result == MessageBoxResult.Yes)
                    {
                        Instance.RaiseProgressChanged($"Setting compatibility fix and trying again ...");
                        Sys.Settings.GameLaunchSettings.Code5Fix = true;

                        Instance.SetCompatibilityFlagsInRegistry();

                        try
                        {
                            EasyHook.RemoteHooking.CreateAndInject(Sys.Settings.FF7Exe, String.Empty, 0, lib, null, out pid, parms);
                        }
                        catch (ApplicationException aex)
                        {
                            if (aex.Message.IndexOf("Unknown error in injected assembler code", StringComparison.InvariantCultureIgnoreCase) >= 0)
                            {
                                Instance.RaiseProgressChanged($"Still failed to inject with EasyHook after setting compatibility fix. Aborting ...");
                                MessageDialogWindow.Show("Failed inject with EasyHook even after setting the compatibility flags", "Failed To Start Game", MessageBoxButton.OK, MessageBoxImage.Error);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Instance.RaiseProgressChanged($"\tuser chose not to set compatibility fix. Aborting ...");
                        return false;
                    }
                }


                Instance.RaiseProgressChanged("Getting FF7 proc ...");
                var ff7Proc = Process.GetProcessById(pid);
                if (ff7Proc != null)
                {
                    ff7Proc.EnableRaisingEvents = true;
                    if (debug)
                    {
                        Instance.RaiseProgressChanged("debug logging set to true. wiring up turbolog file to open after game exit ...");
                        ff7Proc.Exited += (o, e) =>
                        {
                            Process.Start(runtimeProfile.LogFile);
                        };
                    }
                }

                /// load plugins and sideload other programs for mods
                Instance.RaiseProgressChanged("Starting plugins and programs for mods ...");
                foreach (RuntimeMod mod in runtimeProfile.Mods)
                {
                    if (mod.LoadPlugins.Any())
                    {
                        mod.Startup();
                        foreach (string dll in mod.GetLoadPlugins())
                        {
                            _7HPlugin plugin;
                            if (!Instance._plugins.TryGetValue(dll, out plugin))
                            {
                                System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom(dll);

                                plugin = asm.GetType("_7thHeaven.Plugin")
                                            .GetConstructor(Type.EmptyTypes)
                                            .Invoke(null) as _7HPlugin;

                                Instance._plugins.Add(dll, plugin);
                                Instance.RaiseProgressChanged($"\tplugin added: {dll}");
                            }

                            Instance.RaiseProgressChanged($"\tstarting plugin: {dll}");
                            plugin.Start(mod);
                        }
                    }

                    Instance.LaunchProgramsForMod(mod);
                }

                // wire up process to stop plugins and side processes when proc has exited
                Instance.RaiseProgressChanged("Setting up FF7 .exe to stop plugins and mod programs after exiting ...");
                ff7Proc.Exited += (o, e) =>
                {
                    foreach (var plugin in Instance._plugins.Values)
                        plugin.Stop();

                    Instance.RaiseProgressChanged("Stopping other programs for mods started by 7H ...");
                    Instance.StopAllSideProcessesForMods();

                    if (Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc)
                    {
                        Instance.RaiseProgressChanged("Auto unmounting game disc ...");
                        UnmountIso();
                    }

                    // ensure Reunion is re-enabled when ff7 process exists in case it failed above for any reason
                    if (didDisableReunion && File.Exists(Path.Combine(Path.GetDirectoryName(Sys.Settings.FF7Exe), "Reunion.dll.bak")))
                    {
                        EnableOrDisableReunionMod(doEnable: true); 
                    }
                };


                Instance.RaiseProgressChanged("Waiting for FF7 .exe to respond ...");
                DateTime start = DateTime.Now;
                int secondsToWait = 30;
                while (ff7Proc.Responding == false)
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(start);
                    if (elapsed.Seconds > secondsToWait)
                        break;
                }

                if (didDisableReunion)
                {
                    Instance.RaiseProgressChanged("Re-enabling Reunion mod (rename Reunion.dll.bak -> ddraw.dll) ...");
                    EnableOrDisableReunionMod(doEnable: true);
                }

                // ensure ff7 window is active at end of launching
                if (ff7Proc.MainWindowHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(ff7Proc.MainWindowHandle);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);


                Instance.RaiseProgressChanged("Exception occurred while trying to start FF7 ...");
                MessageDialogWindow.Show(e.ToString(), "Error starting FF7", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
            finally
            {
                converter.MessageSent -= GameConverter_MessageSent;
            }
        }

        private static void GameConverter_MessageSent(string message)
        {
            Instance.RaiseProgressChanged(message);
        }

        internal static RuntimeProfile CreateRuntimeProfile()
        {
            string ff7Folder = Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string pathToDataFolder = Path.Combine(ff7Folder, "data");
            List<RuntimeMod> runtimeMods = null;

            try
            {
                runtimeMods = Sys.ActiveProfile.ActiveItems.Select(i => i.GetRuntime(Sys._context))
                                                           .Where(i => i != null)
                                                           .ToList();
            }
            catch (VariableAliasNotFoundException aex)
            {
                Instance.RaiseProgressChanged($"\tfailed to get list of Runtime Mods due to a missing variable for a mod: {aex.Message}");
                return null;
            }
            catch (Exception e)
            {
                throw;
            }


            RuntimeProfile runtimeProfiles = new RuntimeProfile()
            {
                MonitorPaths = new List<string>() {
                    pathToDataFolder,
                    Sys.Settings.AaliFolder,
                    Sys.Settings.MovieFolder,
                },
                ModPath = Sys.Settings.LibraryLocation,
                OpenGLConfig = Sys.ActiveProfile.OpenGLConfig,
                FF7Path = ff7Folder,
                gameFiles = Directory.GetFiles(ff7Folder, "*.*", SearchOption.AllDirectories),
                Mods = runtimeMods
            };

            Instance.RaiseProgressChanged("\tadding paths to monitor ...");
            runtimeProfiles.MonitorPaths.AddRange(Sys.Settings.ExtraFolders.Where(s => s.Length > 0).Select(s => Path.Combine(ff7Folder, s)));
            return runtimeProfiles;
        }

        private static void CopyEasyHookDlls()
        {
            string source = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string dir = Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string pathToTargetEasyHook = Path.Combine(dir, "EasyHook.dll");
            string pathToSourceEasyHook = Path.Combine(source, "EasyHook.dll");

            // compare file versions to determine if file can be skipped
            bool hasOldVersion = false;

            if (File.Exists(pathToTargetEasyHook))
            {
                FileVersionInfo sourceVersion = FileVersionInfo.GetVersionInfo(pathToSourceEasyHook);
                FileVersionInfo targetVersion = FileVersionInfo.GetVersionInfo(pathToTargetEasyHook);

                if (new Version(targetVersion.FileVersion).CompareTo(new Version(sourceVersion.FileVersion)) < 0)
                {
                    Instance.RaiseProgressChanged("\tEasyHook.dll detected to be older version ...");
                    hasOldVersion = true;
                }
            }



            if (!File.Exists(pathToTargetEasyHook) || hasOldVersion)
            {
                File.Copy(pathToSourceEasyHook, pathToTargetEasyHook, true);
                Instance.RaiseProgressChanged("\tEasyHook.dll copied ...");
            }
            else
            {
                Instance.RaiseProgressChanged("\tskipped copying EasyHook.dll ...");

            }



            string pathToTargetEasyHook32 = Path.Combine(dir, "EasyHook32.dll");
            string pathToSourceEasyHook32 = Path.Combine(source, "EasyHook32.dll");
            hasOldVersion = false;

            if (File.Exists(pathToTargetEasyHook32))
            {
                FileVersionInfo sourceVersion = FileVersionInfo.GetVersionInfo(pathToSourceEasyHook32);
                FileVersionInfo targetVersion = FileVersionInfo.GetVersionInfo(pathToTargetEasyHook32);

                if (new Version(targetVersion.FileVersion).CompareTo(new Version(sourceVersion.FileVersion)) < 0)
                {
                    Instance.RaiseProgressChanged("\tEasyHook32.dll detected to be older version ...");
                    hasOldVersion = true;
                }
            }


            if (!File.Exists(pathToTargetEasyHook32) || hasOldVersion)
            {
                File.Copy(pathToSourceEasyHook32, pathToTargetEasyHook32, true);
                Instance.RaiseProgressChanged("\tEasyHook32.dll copied ...");
            }
            else
            {
                Instance.RaiseProgressChanged("\tskipped copying EasyHook32.dll ...");
            }
        }

        private static void StartTurboLogForVariableDump(RuntimeProfile runtimeProfiles)
        {
            string turboLogProcName = Path.Combine(Sys._7HFolder, "TurBoLog.exe");

            // remove from dictionary (and stop other turbolog exe) if exists
            if (Instance._alsoLaunchProcesses.ContainsKey(turboLogProcName))
            {
                if (!Instance._alsoLaunchProcesses[turboLogProcName].HasExited)
                {
                    Instance._alsoLaunchProcesses[turboLogProcName].Kill();
                }
                Instance._alsoLaunchProcesses.Remove(turboLogProcName);
            }

            runtimeProfiles.MonitorVars = Sys._context.VarAliases.Select(kv => new Tuple<string, string>(kv.Key, kv.Value)).ToList();

            ProcessStartInfo psi = new ProcessStartInfo(turboLogProcName)
            {
                WorkingDirectory = Path.GetDirectoryName(Sys.Settings.FF7Exe)
            };
            Process aproc = Process.Start(psi);

            Instance._alsoLaunchProcesses.Add(turboLogProcName, aproc);
            aproc.EnableRaisingEvents = true;
            aproc.Exited += (o, e) => Instance._alsoLaunchProcesses.Remove(turboLogProcName);
        }


        /// <summary>
        /// Launches FF7.exe without loading any mods.
        /// </summary>
        internal static bool LaunchFF7Exe()
        {
            try
            {
                Process ff7Proc = Process.Start(Sys.Settings.FF7Exe);

                ff7Proc.EnableRaisingEvents = true;
                ff7Proc.Exited += (o, e) =>
                {
                    if (Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc)
                    {
                        Instance.RaiseProgressChanged("Auto unmounting game disc ...");
                        UnmountIso();
                    }
                };

                return true;
            }
            catch (Exception ex)
            {
                Instance.RaiseProgressChanged($"An exception occurred while trying to start FF7 at {Sys.Settings.FF7Exe} ...");
                Logger.Error(ex);
                return false;
            }
        }

        internal static bool SanityCheckSettings()
        {
            List<string> changes = new List<string>();
            foreach (var constraint in GetConstraints())
            {
                if (!constraint.Verify(out string msg))
                {
                    MessageDialogWindow.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (msg != null)
                {
                    changes.Add(msg);
                }
            }

            if (changes.Any())
            {
                MessageDialogWindow.Show($"The following settings have been changed to make these mods compatible:\n{String.Join("\n", changes)}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return true;
        }

        internal static bool SanityCheckCompatibility()
        {
            List<InstalledItem> profInst = Sys.ActiveProfile.ActiveItems.Select(pi => Sys.Library.GetItem(pi.ModID)).ToList();

            foreach (InstalledItem item in profInst)
            {
                ModInfo info = item.GetModInfo();

                if (info == null)
                {
                    continue;
                }

                foreach (var req in info.Compatibility.Requires)
                {
                    var rInst = profInst.Find(i => i.ModID.Equals(req.ModID));
                    if (rInst == null)
                    {
                        MessageDialogWindow.Show(String.Format("Mod {0} requires you to activate {1} as well.", item.CachedDetails.Name, req.Description), "Missing Required Activation");
                        return false;
                    }
                    else if (req.Versions.Any() && !req.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version))
                    {
                        MessageDialogWindow.Show(String.Format("Mod {0} requires you to activate {1}, but you do not have a compatible version installed. Try updating it?", item.CachedDetails.Name, rInst.CachedDetails.Name), "Unsupported Mod Version");
                        return false;
                    }
                }

                foreach (var forbid in info.Compatibility.Forbids)
                {
                    var rInst = profInst.Find(i => i.ModID.Equals(forbid.ModID));
                    if (rInst == null)
                    {
                        continue; //good!
                    }

                    if (forbid.Versions.Any() && forbid.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version))
                    {
                        MessageDialogWindow.Show($"Mod {item.CachedDetails.Name} is not compatible with the version of {rInst.CachedDetails.Name} you have installed. Try updating it?", "Incompatible Mod");
                        return false;
                    }
                    else
                    {
                        MessageDialogWindow.Show($"Mod {item.CachedDetails.Name} is not compatible with {rInst.CachedDetails.Name}. You will need to disable it.", "Incompatible Mod");
                        return false;
                    }
                }
            }

            return true;
        }

        internal static bool VerifyOrdering()
        {
            var details = Sys.ActiveProfile
                             .ActiveItems
                             .Select(i => Sys.Library.GetItem(i.ModID))
                             .Select(ii => new { Mod = ii, Info = ii.GetModInfo() })
                             .ToDictionary(a => a.Mod.ModID, a => a);

            List<string> problems = new List<string>();

            foreach (int i in Enumerable.Range(0, Sys.ActiveProfile.ActiveItems.Count))
            {
                Iros._7th.Workshop.ProfileItem mod = Sys.ActiveProfile.ActiveItems[i];
                var info = details[mod.ModID].Info;

                if (info == null)
                {
                    continue;
                }

                foreach (Guid after in info.OrderAfter)
                {
                    if (Sys.ActiveProfile.ActiveItems.Skip(i).Any(pi => pi.ModID.Equals(after)))
                    {
                        problems.Add($"Mod {details[mod.ModID].Mod.CachedDetails.Name} is meant to come BELOW mod {details[after].Mod.CachedDetails.Name} in the load order");
                    }
                }

                foreach (Guid before in info.OrderBefore)
                {
                    if (Sys.ActiveProfile.ActiveItems.Take(i).Any(pi => pi.ModID.Equals(before)))
                    {
                        problems.Add($"Mod {details[mod.ModID].Mod.CachedDetails.Name} is meant to come ABOVE mod {details[before].Mod.CachedDetails.Name} in the load order");
                    }
                }
            }

            if (problems.Any())
            {
                if (MessageDialogWindow.Show($"The following mods will not work properly in the current order:\n{String.Join("\n", problems)}\nDo you want to continue anyway?", "Load Order Incompatible", MessageBoxButton.YesNo).Result != MessageBoxResult.Yes)
                    return false;
            }

            return true;
        }

        internal static List<Constraint> GetConstraints()
        {
            List<Constraint> constraints = new List<Constraint>();
            foreach (Iros._7th.Workshop.ProfileItem pItem in Sys.ActiveProfile.ActiveItems)
            {
                InstalledItem inst = Sys.Library.GetItem(pItem.ModID);
                ModInfo info = inst.GetModInfo();

                if (info == null)
                {
                    continue;
                }

                foreach (var cSetting in info.Compatibility.Settings)
                {
                    if (!String.IsNullOrWhiteSpace(cSetting.MyID))
                    {
                        var setting = pItem.Settings.Find(s => s.ID.Equals(cSetting.MyID, StringComparison.InvariantCultureIgnoreCase));
                        if ((setting == null) || (setting.Value != cSetting.MyValue)) continue;
                    }

                    Iros._7th.Workshop.ProfileItem oItem = Sys.ActiveProfile.ActiveItems.Find(i => i.ModID.Equals(cSetting.ModID));
                    if (oItem == null) continue;

                    InstalledItem oInst = Sys.Library.GetItem(cSetting.ModID);
                    Constraint ct = constraints.Find(c => c.ModID.Equals(cSetting.ModID) && c.Setting.Equals(cSetting.TheirID, StringComparison.InvariantCultureIgnoreCase));
                    if (ct == null)
                    {
                        ct = new Constraint() { ModID = cSetting.ModID, Setting = cSetting.TheirID };
                        constraints.Add(ct);
                    }

                    ct.ParticipatingMods.Add(inst.CachedDetails.Name);
                    if (cSetting.Require.HasValue)
                    {
                        ct.Require.Add(cSetting.Require.Value);
                    }

                    foreach (var f in cSetting.Forbid)
                    {
                        ct.Forbid.Add(f);
                    }
                }

                foreach (var setting in info.Options)
                {
                    Constraint ct = constraints.Find(c => c.ModID.Equals(pItem.ModID) && c.Setting.Equals(setting.ID, StringComparison.InvariantCultureIgnoreCase));
                    if (ct == null)
                    {
                        ct = new Constraint() { ModID = pItem.ModID, Setting = setting.ID };
                        constraints.Add(ct);
                    }
                    ct.Option = setting;
                }

            }

            return constraints;
        }

        internal static bool OSHasAutoMountSupport()
        {
            Version osVersion = Environment.OSVersion.Version;
            if (osVersion.Major < 6)
            {
                return false;
            }
            else if (osVersion.Major == 6)
            {
                if (osVersion.Minor < 2)
                {
                    return false; // on an OS below Win 8
                }
            }

            return true;
        }

        public static bool IsReunionModInstalled()
        {
            string installPath = Path.GetDirectoryName(Sys.Settings.FF7Exe);


            if (!Directory.Exists(installPath))
            {
                return false;
            }

            return Directory.GetFiles(installPath).Any(s => new FileInfo(s).Name.Equals("ddraw.dll", StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool EnableOrDisableReunionMod(bool doEnable)
        {
            string installPath = Path.GetDirectoryName(Sys.Settings.FF7Exe);

            if (!Directory.Exists(installPath))
            {
                return true;
            }

            try
            {
                string pathToDll = Path.Combine(installPath, "ddraw.dll");
                string backupName = Path.Combine(installPath, "Reunion.dll.bak");

                // disable Reunion by renaming ddraw.dll to Reunion.dll.bak
                if (!doEnable)
                {
                    if (File.Exists(pathToDll))
                    {
                        File.Move(pathToDll, backupName);
                        return true;
                    }
                    else
                    {
                        Instance.RaiseProgressChanged($"\tcould not find ddraw.dll at {pathToDll}");
                        return false;
                    }
                }
                else
                {
                    if (File.Exists(backupName))
                    {
                        File.Move(backupName, pathToDll);
                        Instance.RaiseProgressChanged($"\trenamed {backupName} to {pathToDll}");
                        return true;
                    }
                    else
                    {
                        Instance.RaiseProgressChanged($"\tcould not find Reunion.dll.bak at {backupName}");
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Scans all drives looking for the drive labeled "FF7DISC1", "FF7DISC2", or "FF7DISC3" and returns the corresponding drive letter.
        /// If not found returns empty string. Returns the drive letter for <paramref name="labelToFind"/> if not null
        /// </summary>
        public static string GetDriveLetter(string labelToFind = null)
        {
            List<string> labels = null;
            if (string.IsNullOrWhiteSpace(labelToFind))
            {
                labels = new List<string>() { "FF7DISC1", "FF7DISC2", "FF7DISC3" };
            }
            else
            {
                labels = new List<string>() { labelToFind };
            }

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && labels.Any(s => s.Equals(drive.VolumeLabel, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return drive.Name;
                }
            }

            return "";
        }

        /// <summary>
        /// Updates Registry with new values from <see cref="Sys.Settings.GameLaunchSettings"/>
        /// </summary>
        public void SetRegistryValues()
        {
            Instance.RaiseProgressChanged("Applying values to registry ...");

            string ff7KeyPath = $"{RegistryHelper.GetKeyPath(FF7RegKey.SquareSoftKeyPath)}\\Final Fantasy VII";
            string virtualStorePath = $"{RegistryHelper.GetKeyPath(FF7RegKey.VirtualStoreKeyPath)}\\Final Fantasy VII";

            string installPath = Path.GetDirectoryName(Sys.Settings.FF7Exe) + @"\";
            string pathToData = Path.Combine(installPath, @"data\");
            string pathToMovies = Path.Combine(installPath, "data", @"movies\");

            // Add registry key values for paths and drive letter
            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::AppPath = {installPath}");
            RegistryHelper.SetValue(ff7KeyPath, "AppPath", installPath);

            Instance.RaiseProgressChanged($"\t {virtualStorePath}::AppPath = {installPath}");
            RegistryHelper.SetValue(virtualStorePath, "AppPath", installPath);


            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::DataPath = {pathToData}");
            RegistryHelper.SetValue(ff7KeyPath, "DataPath", pathToData);

            Instance.RaiseProgressChanged($"\t {virtualStorePath}::DataPath = {pathToData}");
            RegistryHelper.SetValue(virtualStorePath, "DataPath", pathToData);

            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::MoviePath = {pathToMovies}");
            RegistryHelper.SetValue(ff7KeyPath, "MoviePath", pathToMovies);

            Instance.RaiseProgressChanged($"\t {virtualStorePath}::MoviePath = {pathToMovies}");
            RegistryHelper.SetValue(virtualStorePath, "MoviePath", pathToMovies);

            // setting the drive letter may not happen if auto update disc path is not set
            if (Sys.Settings.GameLaunchSettings.AutoUpdateDiscPath && !string.IsNullOrWhiteSpace(DriveLetter))
            {
                Instance.RaiseProgressChanged($"\t {ff7KeyPath}::DataDrive = {DriveLetter}");
                RegistryHelper.SetValue(ff7KeyPath, "DataDrive", DriveLetter);

                Instance.RaiseProgressChanged($"\t {virtualStorePath}::DataDrive = {DriveLetter}");
                RegistryHelper.SetValue(virtualStorePath, "DataDrive", DriveLetter);
            }

            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::DiskNo = 0");
            RegistryHelper.SetValue(ff7KeyPath, "DiskNo", 0, RegistryValueKind.DWord);

            Instance.RaiseProgressChanged($"\t {virtualStorePath}::DiskNo = 0");
            RegistryHelper.SetValue(virtualStorePath, "DiskNo", 0, RegistryValueKind.DWord);

            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::FullInstall = 1");
            RegistryHelper.SetValue(ff7KeyPath, "FullInstall", 1, RegistryValueKind.DWord);

            Instance.RaiseProgressChanged($"\t {virtualStorePath}::FullInstall = 1");
            RegistryHelper.SetValue(virtualStorePath, "FullInstall", 1, RegistryValueKind.DWord);


            Instance.RaiseProgressChanged($"\t {RegistryHelper.GetKeyPath(FF7RegKey.FF7AppKeyPath)}::Path = {installPath.TrimEnd('\\')}");
            RegistryHelper.SetValue(RegistryHelper.GetKeyPath(FF7RegKey.FF7AppKeyPath), "Path", installPath.TrimEnd('\\'));


            // Add registry key values for Graphics
            string graphicsKeyPath = $"{ff7KeyPath}\\1.00\\Graphics";
            string graphicsVirtualKeyPath = $"{virtualStorePath}\\1.00\\Graphics";

            Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Driver = {Sys.Settings.GameLaunchSettings.SelectedRenderer}");
            RegistryHelper.SetValue(graphicsKeyPath, "Driver", Sys.Settings.GameLaunchSettings.SelectedRenderer, RegistryValueKind.DWord);

            Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::Driver = {Sys.Settings.GameLaunchSettings.SelectedRenderer}");
            RegistryHelper.SetValue(graphicsVirtualKeyPath, "Driver", Sys.Settings.GameLaunchSettings.SelectedRenderer, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.SelectedRenderer == 3)
            {
                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::DriverPath = ff7_opengl.fgd");
                RegistryHelper.SetValue(graphicsKeyPath, "DriverPath", "ff7_opengl.fgd");

                Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::DriverPath = ff7_opengl.fgd");
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "DriverPath", "ff7_opengl.fgd");

                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Mode = 2");
                RegistryHelper.SetValue(graphicsKeyPath, "Mode", 2, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::Mode = 2");
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "Mode", 2, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Options = 0x12");
                RegistryHelper.SetValue(graphicsKeyPath, "Options", 0x12, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::Options = 0x12");
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "Options", 0x12, RegistryValueKind.DWord);
            }
            else
            {
                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::DriverPath = ");
                RegistryHelper.SetValue(graphicsKeyPath, "DriverPath", "");

                Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::DriverPath = ");
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "DriverPath", "");

                int mode = Sys.Settings.GameLaunchSettings.FullScreenMode ? 2 : 1;

                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Mode = {mode}");
                RegistryHelper.SetValue(graphicsKeyPath, "Mode", mode, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::Mode = {mode}");
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "Mode", mode, RegistryValueKind.DWord);

                if (Sys.Settings.GameLaunchSettings.UseRiva128GraphicsOption)
                {
                    Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Options = 0x0000000a");
                    RegistryHelper.SetValue(graphicsKeyPath, "Options", 0x0000000a, RegistryValueKind.DWord);

                    Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::Options = 0x0000000a");
                    RegistryHelper.SetValue(graphicsVirtualKeyPath, "Options", 0x0000000a, RegistryValueKind.DWord);
                }
                else if (Sys.Settings.GameLaunchSettings.UseTntGraphicsOption)
                {
                    Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Options = 0x12");
                    RegistryHelper.SetValue(graphicsKeyPath, "Options", 0x12, RegistryValueKind.DWord);

                    Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::Options = 0x12");
                    RegistryHelper.SetValue(graphicsVirtualKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                }
                else
                {
                    Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Options = 0");
                    RegistryHelper.SetValue(graphicsKeyPath, "Options", 0, RegistryValueKind.DWord);

                    Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::Options = 0");
                    RegistryHelper.SetValue(graphicsVirtualKeyPath, "Options", 0, RegistryValueKind.DWord);
                }
            }

            byte[] emptyGuidBytes = Guid.Empty.ToByteArray();

            Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::DD_GUID = {BitConverter.ToString(emptyGuidBytes).Replace("-", ",")}");
            RegistryHelper.SetValue(graphicsKeyPath, "DD_GUID", emptyGuidBytes, RegistryValueKind.Binary);

            Instance.RaiseProgressChanged($"\t {graphicsVirtualKeyPath}::DD_GUID = {BitConverter.ToString(emptyGuidBytes).Replace("-", ",")}");
            RegistryHelper.SetValue(graphicsVirtualKeyPath, "DD_GUID", emptyGuidBytes, RegistryValueKind.Binary);


            // Add registry key values for MIDI
            string midiKeyPath = $"{ff7KeyPath}\\1.00\\MIDI";
            string midiVirtualKeyPath = $"{virtualStorePath}\\1.00\\MIDI";

            Instance.RaiseProgressChanged($"\t {midiKeyPath}::MIDI_DeviceID = 0");
            RegistryHelper.SetValue(midiKeyPath, "MIDI_DeviceID", 0x00000000, RegistryValueKind.DWord);

            Instance.RaiseProgressChanged($"\t {midiVirtualKeyPath}::MIDI_DeviceID = 0");
            RegistryHelper.SetValue(midiVirtualKeyPath, "MIDI_DeviceID", 0x00000000, RegistryValueKind.DWord);


            Instance.RaiseProgressChanged($"\t {midiKeyPath}::MIDI_data = {Sys.Settings.GameLaunchSettings.SelectedMidiDevice}");
            RegistryHelper.SetValue(midiKeyPath, "MIDI_data", Sys.Settings.GameLaunchSettings.SelectedMidiDevice);

            Instance.RaiseProgressChanged($"\t {midiVirtualKeyPath}::MIDI_data = {Sys.Settings.GameLaunchSettings.SelectedMidiDevice}");
            RegistryHelper.SetValue(midiVirtualKeyPath, "MIDI_data", Sys.Settings.GameLaunchSettings.SelectedMidiDevice);

            Instance.RaiseProgressChanged($"\t {midiKeyPath}::MusicVolume = {Sys.Settings.GameLaunchSettings.MusicVolume}");
            RegistryHelper.SetValue(midiKeyPath, "MusicVolume", Sys.Settings.GameLaunchSettings.MusicVolume, RegistryValueKind.DWord);

            Instance.RaiseProgressChanged($"\t {midiVirtualKeyPath}::MusicVolume = {Sys.Settings.GameLaunchSettings.MusicVolume}");
            RegistryHelper.SetValue(midiVirtualKeyPath, "MusicVolume", Sys.Settings.GameLaunchSettings.MusicVolume, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.LogarithmicVolumeControl)
            {
                Instance.RaiseProgressChanged($"\t {midiKeyPath}::Options = 1");
                RegistryHelper.SetValue(midiKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {midiVirtualKeyPath}::Options = 1");
                RegistryHelper.SetValue(midiVirtualKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
            }
            else
            {
                Instance.RaiseProgressChanged($"\t {midiKeyPath}::Options = 0");
                RegistryHelper.SetValue(midiKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {midiVirtualKeyPath}::Options = 0");
                RegistryHelper.SetValue(midiVirtualKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
            }

            // Add registry key values for Sound
            string soundKeyPath = $"{ff7KeyPath}\\1.00\\Sound";
            string soundVirtualKeyPath = $"{virtualStorePath}\\1.00\\Sound";

            byte[] soundGuidBytes = Sys.Settings.GameLaunchSettings.SelectedSoundDevice.ToByteArray();

            Instance.RaiseProgressChanged($"\t {soundKeyPath}::Sound_GUID = {BitConverter.ToString(soundGuidBytes).Replace("-", ",")}");
            RegistryHelper.SetValue(soundKeyPath, "Sound_GUID", soundGuidBytes, RegistryValueKind.Binary);

            Instance.RaiseProgressChanged($"\t {soundVirtualKeyPath}::Sound_GUID = {BitConverter.ToString(soundGuidBytes).Replace("-", ",")}");
            RegistryHelper.SetValue(soundVirtualKeyPath, "Sound_GUID", soundGuidBytes, RegistryValueKind.Binary);

            Instance.RaiseProgressChanged($"\t {soundKeyPath}::SFXVolume = {Sys.Settings.GameLaunchSettings.SfxVolume}");
            RegistryHelper.SetValue(soundKeyPath, "SFXVolume", Sys.Settings.GameLaunchSettings.SfxVolume, RegistryValueKind.DWord);

            Instance.RaiseProgressChanged($"\t {soundVirtualKeyPath}::SFXVolume = {Sys.Settings.GameLaunchSettings.SfxVolume}");
            RegistryHelper.SetValue(soundVirtualKeyPath, "SFXVolume", Sys.Settings.GameLaunchSettings.SfxVolume, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.ReverseSpeakers)
            {
                Instance.RaiseProgressChanged($"\t {soundKeyPath}::Options = 1");
                RegistryHelper.SetValue(soundKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {soundVirtualKeyPath}::Options = 1");
                RegistryHelper.SetValue(soundVirtualKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
            }
            else
            {
                Instance.RaiseProgressChanged($"\t {soundKeyPath}::Options = 0");
                RegistryHelper.SetValue(soundKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {soundVirtualKeyPath}::Options = 0");
                RegistryHelper.SetValue(soundVirtualKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Set/Delete compatibility flags in registry for ~ 640x480 HIGHDPIAWARE
        /// </summary>
        public void SetCompatibilityFlagsInRegistry()
        {
            Instance.RaiseProgressChanged($"Applying compatibility flags in registry (if any) ...");

            RegistryKey ff7CompatKey;
            string keyPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

            // delete compatibility flags if set in launch settings
            if (!Sys.Settings.GameLaunchSettings.Code5Fix && !Sys.Settings.GameLaunchSettings.HighDpiFix)
            {
                Instance.RaiseProgressChanged("\t Code 5 fix / High DPI fix set to false - deleting flags if exist ...");
                ff7CompatKey = Registry.CurrentUser.OpenSubKey(keyPath, true);
                if (ff7CompatKey.GetValue(Sys.Settings.FF7Exe) != null)
                {
                    Instance.RaiseProgressChanged("\t\t compatibility flags found - deleting");
                    ff7CompatKey.DeleteValue(Sys.Settings.FF7Exe);
                }

                return;
            }


            string compatString = "~ ";
            // Add 640x480 compatibility flag if set in settings
            if (Sys.Settings.GameLaunchSettings.Code5Fix)
            {
                Instance.RaiseProgressChanged("\t Code 5 fix set to true - applying 640x480 compatibility flag in registry");
                compatString += "640x480 ";
            }

            if (Sys.Settings.GameLaunchSettings.HighDpiFix)
            {
                Instance.RaiseProgressChanged("\t High DPI Fix set to true - applying HIGHDPIAWARE compatibility flag in registry");
                compatString += "HIGHDPIAWARE";
            }

            Instance.RaiseProgressChanged($"\t HKEY_CURRENT_USER\\{keyPath}::{Sys.Settings.FF7Exe} = {compatString}");
            ff7CompatKey = Registry.CurrentUser.OpenSubKey(keyPath, true);
            ff7CompatKey?.SetValue(Sys.Settings.FF7Exe, compatString);
        }

        /// <summary>
        /// Mounts FF7DISC1.ISO in 'Resources' folder to virtual drive (if Win 8+)
        /// </summary>
        /// <returns></returns>
        public static bool MountIso()
        {
            Version osVersion = Environment.OSVersion.Version;
            if (osVersion.Major < 6)
            {
                return false;
            }
            else if (osVersion.Major == 6)
            {
                if (osVersion.Minor < 2)
                {
                    return false; // on an OS below Win 8
                }
            }

            try
            {
                string isoPath = Path.Combine(Sys._7HFolder, "Resources", "FF7DISC1.ISO");

                if (!File.Exists(isoPath))
                {
                    return false;
                }

                using (PowerShell ps = PowerShell.Create())
                {
                    var result = ps.AddCommand("Mount-DiskImage").AddParameter("ImagePath", isoPath).Invoke();
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Unmounts FF7DISC1
        /// </summary>
        /// <returns></returns>
        public static bool UnmountIso()
        {
            if (!OSHasAutoMountSupport())
            {
                return false;
            }

            try
            {
                string isoPath = Path.Combine(Sys._7HFolder, "Resources", "FF7DISC1.ISO");

                if (!File.Exists(isoPath))
                {
                    return false;
                }

                using (PowerShell ps = PowerShell.Create())
                {
                    var result = ps.AddCommand("Dismount-DiskImage").AddParameter("ImagePath", isoPath).Invoke();
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        public static bool CopyKeyboardInputCfg()
        {
            // ensure a custom.cfg file exists in Controls folder
            string pathToCustomCfg = Path.Combine(Sys.PathToControlsFolder, "custom.cfg");
            Directory.CreateDirectory(Sys.PathToControlsFolder);

            if (!File.Exists(pathToCustomCfg))
            {
                Instance.RaiseProgressChanged("\tno custom.cfg file found in /Resources/Controls folder. Creating copy of ff7input.cfg");
                GameLaunchSettingsViewModel.CopyInputCfgToCustomCfg(forceCopy: false);
            }

            string pathToCfg = Path.Combine(Sys.PathToControlsFolder, Sys.Settings.GameLaunchSettings.InGameConfigOption);

            Instance.RaiseProgressChanged($"\tusing control configuration file {Sys.Settings.GameLaunchSettings.InGameConfigOption} ...");
            if (!File.Exists(pathToCfg))
            {
                Instance.RaiseProgressChanged($"\tWARNING: input cfg file not found at {pathToCfg}");
                return true;
            }

            try
            {
                string targetPath = Path.Combine(Path.GetDirectoryName(Sys.Settings.FF7Exe), "ff7input.cfg");

                Instance.RaiseProgressChanged($"\tcopying {pathToCfg} to {targetPath} ...");
                File.Copy(pathToCfg, targetPath, true);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Instance.RaiseProgressChanged($"\tfailed to copy .cfg file: {e.Message}");
                return false;
            }

            return true;
        }

        public static bool IsFF7Running()
        {
            string fileName = Path.GetFileNameWithoutExtension(Sys.Settings.FF7Exe);
            return Process.GetProcessesByName(fileName).Length > 0;
        }

        /// <summary>
        /// Kills any currently running process found in <see cref="_sideLoadProcesses"/>
        /// </summary>
        private void StopAllSideProcessesForMods()
        {
            foreach (var valuePair in _sideLoadProcesses.ToList())
            {
                _7thWrapperLib.ProgramInfo info = valuePair.Key;
                Process sideProc = valuePair.Value;
                string procName = sideProc.ProcessName;

                if (!sideProc.HasExited)
                {
                    sideProc.Kill();
                }

                // Kill all instances with same process name if necessary
                if (info.CloseAllInstances)
                {
                    foreach (Process otherProc in Process.GetProcessesByName(procName))
                    {
                        if (!otherProc.HasExited)
                            otherProc.Kill();
                    }
                }
            }
        }

        internal void LaunchProgramsForMod(RuntimeMod mod)
        {
            if (!mod.LoadPrograms.Any())
            {
                return;
            }

            mod.Startup();

            foreach (var program in mod.GetLoadPrograms())
            {
                if (!_sideLoadProcesses.ContainsKey(program))
                {
                    Instance.RaiseProgressChanged($"\tstarting program: {program.PathToProgram}");
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetDirectoryName(program.PathToProgram),
                        FileName = program.PathToProgram,
                        Arguments = program.ProgramArgs,
                        UseShellExecute = false,
                    };
                    Process aproc = Process.Start(psi);

                    aproc.EnableRaisingEvents = true;
                    aproc.Exited += (_o, _e) => _sideLoadProcesses.Remove(program);

                    _sideLoadProcesses.Add(program, aproc);
                    Instance.RaiseProgressChanged($"\t\tstarted ...");
                    System.Threading.Thread.Sleep(1500); // add a small delay to after a process is started so it has time to initialize/load (hopefully before ff7 is started)

                    // force the process to become minimized
                    if (aproc.MainWindowHandle != IntPtr.Zero)
                    {
                        ShowWindow(aproc.MainWindowHandle.ToInt32(), SW_FORCEMINIMIZE);
                    }
                }
                else
                {
                    if (!_sideLoadProcesses[program].HasExited)
                    {
                        Instance.RaiseProgressChanged($"\tprogram already running: {program.PathToProgram}");
                    }
                }
            }
        }


        /// <summary>
        /// Starts the processes with the specified arguments that are set in <see cref="Sys.Settings.ProgramsToLaunchPrior"/>.
        /// </summary>
        internal void LaunchAdditionalProgramsToRunPrior()
        {
            // launch other processes set in settings
            foreach (ProgramLaunchInfo al in Sys.Settings.ProgramsToLaunchPrior.Where(s => !String.IsNullOrWhiteSpace(s.PathToProgram)))
            {
                if (!_alsoLaunchProcesses.ContainsKey(al.PathToProgram))
                {
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetDirectoryName(al.PathToProgram),
                        FileName = al.PathToProgram,
                        Arguments = al.ProgramArgs
                    };
                    Process aproc = Process.Start(psi);

                    _alsoLaunchProcesses.Add(al.PathToProgram, aproc);
                    aproc.EnableRaisingEvents = true;
                    aproc.Exited += (_o, _e) => _alsoLaunchProcesses.Remove(al.PathToProgram);
                }
            }
        }

        internal void RaiseProgressChanged(string messageToLog)
        {
            Logger.Info(messageToLog);
            ProgressChanged?.Invoke(messageToLog);
        }
    }
}
