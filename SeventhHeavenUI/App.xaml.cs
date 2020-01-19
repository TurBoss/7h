using SeventhHeaven.Windows;
using System;
using System.Diagnostics;
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
