using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace SeventhHeavenUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception: {e.Exception.Message}");
        }
    }
}
