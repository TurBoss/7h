using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<string> Arguments { get; private set; } = new List<string>();
        public static bool RepairMode = false;

        public static void RelaunchIfNotAdmin()
        {
            if (!RunningAsAdmin())
            {
                Console.WriteLine("Running as admin required!");
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.Arguments = String.Join(" ", Arguments.ToArray());
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Assembly.GetEntryAssembly().CodeBase;
                proc.Verb = "runas";
                try
                {
                    Process.Start(proc);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("This program must be run as an administrator! \n\n" + ex.ToString());
                    Environment.Exit(0);
                }
            }
        }

        private static bool RunningAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            List<string> arguments = new List<string>();
            RelaunchIfNotAdmin();

            for (int index = 0; index < e.Args.Length; index += 2)
            {
                arguments.Add(e.Args[index]);
            }
            Arguments = arguments;

            if (Arguments.Contains("uninstall"))
            {
                MainWindow = new Uninstall();
                MainWindow.Show();
            }
            else if (Arguments.Contains("modify"))
            {
                RepairMode = true;
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
            else
            {
                RepairMode = false;
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
        }
    }
}
