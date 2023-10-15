using _7thHeaven.Code;
using Iros._7th.Workshop;
using Iros._7th;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SeventhHeaven.Windows;

namespace SeventhLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public const string uniqueAppGuid = "CAFECAFE-CAFE-CAFE-CAFE-CAFECAFECAFE";

        public static Mutex uniqueMutex;

        private bool hasShownErrorWindow = false;

        public App()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            uniqueMutex = new Mutex(true, uniqueAppGuid, out bool gotMutex);
            GC.KeepAlive(App.uniqueMutex);

            //TODO: Add support for .NET 6.0
            //if (SingleInstance.IsFirstInstance(uniqueAppGuid, true))
            //{
            //    SingleInstance.OnSecondInstanceStarted += SingleInstance_OnSecondInstanceStarted;
            //}
            //else
            //{
            //    // second instance so notify first instance
            //    SingleInstance.NotifyFirstInstance(uniqueAppGuid);
            //}

            if (!gotMutex)
            {
                App.Current.Shutdown(); // only one instance of the app opened at a time so shutdown
                return;
            }
        }

        private static void ModPatchImportProgressChanged(string message, double percentComplete)
        {
            Logger.Info(message);
        }

        internal static void ShutdownApp()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                App.Current.Shutdown();
            });
        }

        public static Version GetAppVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return new Version(fileVersionInfo.ProductVersion);
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
            // Enable Visual styles for Winform applications to support plugins that uses Winforms as a UI
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // check if default language saved in app settings; otherwise detect language from thread
            // string defaultLang = Sys.Settings.AppLanguage;

            // if (string.IsNullOrWhiteSpace(defaultLang))
            // {
            //    defaultLang = GetCultureFromCurrentThread();
            // }

            // SetLanguageDictionary(defaultLang);
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

        public static string GetCultureFromCurrentThread()
        {
            return Thread.CurrentThread.CurrentCulture.ToString();
        }

        internal void SetLanguageDictionary(string cultureCode)
        {
            ResourceDictionary dict = new ResourceDictionary();

            if (string.IsNullOrWhiteSpace(cultureCode) || cultureCode.Length < 2) cultureCode = "en";

            if (cultureCode == "pt-BR")
            {
                dict.Source = new Uri("Resources\\Languages\\StringResources.br.xaml", UriKind.Relative);
            }
            else
            {
                cultureCode = cultureCode.Substring(0, 2);

                switch (cultureCode)
                {
                    case "en":
                        dict.Source = new Uri("Resources\\StringResources.xaml", UriKind.Relative);
                        break;
                    case "fr":
                        dict.Source = new Uri("Resources\\Languages\\StringResources.fr.xaml", UriKind.Relative);
                        break;
                    case "es":
                        dict.Source = new Uri("Resources\\Languages\\StringResources.es.xaml", UriKind.Relative);
                        break;
                    case "de":
                        dict.Source = new Uri("Resources\\Languages\\StringResources.de.xaml", UriKind.Relative);
                        break;
                    case "gr":
                        dict.Source = new Uri("Resources\\Languages\\StringResources.gr.xaml", UriKind.Relative);
                        break;
                    case "it":
                        dict.Source = new Uri("Resources\\Languages\\StringResources.it.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources\\StringResources.xaml", UriKind.Relative);
                        cultureCode = "en";
                        break;
                }
            }

            Sys.Settings.AppLanguage = cultureCode;

            this.Resources.MergedDictionaries.RemoveAt(1); // remove the default string resources dictionary (second in merged dictionary in App.xaml)
            this.Resources.MergedDictionaries.Add(dict);
        }

        public static string GetAppLanguage()
        {
            try
            {
                string defaultLang = Sys.Settings.AppLanguage;

                if (string.IsNullOrWhiteSpace(defaultLang))
                {
                    defaultLang = "en";
                }

                return defaultLang;
            }
            catch (Exception)
            {
                return "en";
            }
        }

    }
}
