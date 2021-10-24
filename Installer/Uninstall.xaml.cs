using Installer.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Installer
{
    /// <summary>
    /// Interaction logic for Uninstall.xaml
    /// </summary>
    public partial class Uninstall : Window
    {
        public Uninstall()
        {
            InitializeComponent();
            Loaded += UninstallWindow_Loaded;
        }

        private void UninstallWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeMode = ResizeMode.NoResize;
        }

        private void ClearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                try
                {
                    fi.Delete();
                }
                catch (Exception) { } // Ignore all exceptions
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                try
                {
                    di.Delete();
                }
                catch (Exception) { } // Ignore all exceptions
            }
        }

        private void UninstallBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = Registry.Uninstall.GetInstallLocation();
            ClearFolder(path);
            Registry.Uninstall.removeShellIntegration();
            Registry.Uninstall.RemoveUninstallerKeys();
            string pathShortcut = Environment.GetEnvironmentVariable("ProgramData") + "\\Microsoft\\Windows\\Start Menu\\Programs\\7th Heaven";
            if (Directory.Exists(pathShortcut))
            {
                Directory.Delete(pathShortcut);
            }
            pathShortcut = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\7thHeaven.lnk";
            if (File.Exists(pathShortcut))
            {
                File.Delete(pathShortcut);
            }
            MessageBox.Show("Uninstalled Successfully", "7th Haven uninstaller");
            MessageBox.Show("7th heaven has been removed, Please Uninstall and Reinstall your Copy of FFVII", "Uninstall Notice");
            Environment.Exit(0);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
