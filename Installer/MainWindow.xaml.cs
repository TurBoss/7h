using Installer.Classes;
using Installer.Registry;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;  // for WPF support
using System.Windows.Media;
using System.Windows.Navigation;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static String installer7thHeavenVersion = "2.3.4.0";

        public static bool versionCompaire(string vs)
        {
            List<ushort> i1 = new List<ushort>();
            List<ushort> i2 = new List<ushort>();
            string[] s1 = installer7thHeavenVersion.Split('.');
            string[] s2 = vs.Split('.');
            foreach (string s in s1) i1.Add(ushort.Parse(s));
            foreach (string s in s2) i2.Add(ushort.Parse(s));
            for(ushort i = 0; i < i1.Count; i++) if (i1[i] > i2[i]) return true;
            return false;
        }
        public MainWindow()
        {
            InitializeComponent();
            if (App.RepairMode)
            {
                BrowserFFVIIBtn.Foreground = new SolidColorBrush(Color.FromRgb(0,0,0));
                browserInstallLocation.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                InstallBtn.Content = "Repair";

                ff7InstallDirectory.Text = Registry.Uninstall.GetGameLocation();
                sevenHInstalPathTxt.Text = Registry.Uninstall.GetInstallLocation();
            }
            else
            {
                string ff7Dir = Registry.Uninstall.GetGameLocation();
                string thdir = Registry.Uninstall.GetInstallLocation();
                if (thdir != "")
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Path.Combine(thdir, "7th Heaven.exe"));
                    if (versionCompaire(fvi.FileVersion))
                    {
                        if (MessageBox.Show("Detected Old Install Would you like to update it?", "7th heaven install detected.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            InstallBtn.Content = "Upgrade";
                            ff7InstallDirectory.Text = Registry.Uninstall.GetGameLocation();
                            sevenHInstalPathTxt.Text = Registry.Uninstall.GetInstallLocation();
                        }
                        else
                        {
                            InstallBtn.Content = "Reinstall";
                            ff7InstallDirectory.Text = Registry.Uninstall.GetGameLocation();
                            sevenHInstalPathTxt.Text = Registry.Uninstall.GetInstallLocation();
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("Detected Old Install Would you like to repair it?", "7th heaven install detected.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            BrowserFFVIIBtn.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                            browserInstallLocation.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                            InstallBtn.Content = "Repair";

                            ff7InstallDirectory.Text = Registry.Uninstall.GetGameLocation();
                            sevenHInstalPathTxt.Text = Registry.Uninstall.GetInstallLocation();
                        }
                        else
                        {
                            InstallBtn.Content = "Reinstall";
                            ff7InstallDirectory.Text = Registry.Uninstall.GetGameLocation();
                            sevenHInstalPathTxt.Text = Registry.Uninstall.GetInstallLocation();
                        }
                    }
                }
            }
            Status_Text.Content = "Waiting for user input";

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeMode = ResizeMode.NoResize;
        }

        public void CreateShortcutForAllUsers()
        {
            string path;
            IWshShortcut shortcut;
            WshShell shell = new WshShell();
            try
            {
                path = Environment.GetEnvironmentVariable("ProgramData") + "\\Microsoft\\Windows\\Start Menu\\Programs\\7th Heaven";
                Directory.CreateDirectory(path);
                path += "\\7thHeaven.lnk";
                shortcut = shell.CreateShortcut(path);
                shortcut.TargetPath = sevenHInstalPathTxt.Text + "\\7th Heaven.exe";
                shortcut.Save();
            }
            catch (Exception) { }

            if ((bool)enableDesktopShortcutTickBox.IsChecked)
            {
                try
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\7thHeaven.lnk";
                    shortcut = shell.CreateShortcut(path);
                    shortcut.TargetPath = sevenHInstalPathTxt.Text + "\\7th Heaven.exe";
                    shortcut.Save();
                }catch (Exception) {  }
            }
        }


        private void ExtractFiles(string zipFile, string destDirectory)
        {
            prgBar.Value = 0;
            Status_Text.Content = "Extracting "+zipFile;
            using (ZipArchive source = ZipFile.Open(zipFile, ZipArchiveMode.Read, null))
            {
                prgBar.Maximum = source.Entries.Count-1;
                var count = 0;
                foreach (ZipArchiveEntry entry in source.Entries)
                {
                    count++;
                    string fullPath = Path.GetFullPath(Path.Combine(destDirectory, entry.FullName));

                    if (Path.GetFileName(fullPath).Length != 0)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        // The boolean parameter determines whether an existing file that has the same name as the destination file should be overwritten
                        entry.ExtractToFile(fullPath, true);
                    }
                    prgBar.Value = count;
                }
            }
        }

        private void Install()
        {
            if (!Directory.Exists(ff7InstallDirectory.Text))
            {
                MessageBox.Show("Unable to access FFVII Install Directory have you installed it?");
                return;
            }

            if (
                !System.IO.File.Exists(ff7InstallDirectory.Text + "\\ff7.exe") &&
                !System.IO.File.Exists(ff7InstallDirectory.Text + "\\ff7_en.exe") &&
                !System.IO.File.Exists(ff7InstallDirectory.Text + "\\ff7_es.exe") &&
                !System.IO.File.Exists(ff7InstallDirectory.Text + "\\ff7_de.exe") &&
                !System.IO.File.Exists(ff7InstallDirectory.Text + "\\ff7_fr.exe")
            )
            {
                MessageBox.Show("Unable to find FFVII Game did you select the correct directory for it?");
                return;
            }

            string tmpZip = EmbeddedZip.tempPath + "{" + Guid.NewGuid().ToString() + "}.zip";
            EmbeddedZip.ExtractZip("7thHeaven", tmpZip);
            if (!Directory.Exists(sevenHInstalPathTxt.Text))
            {
                Directory.CreateDirectory(sevenHInstalPathTxt.Text);
            }
            // copy 7th heaven to install directory
            ExtractFiles(tmpZip, sevenHInstalPathTxt.Text);
            System.IO.File.Delete(tmpZip);

            // install FFNx
            tmpZip = EmbeddedZip.tempPath + "{" + Guid.NewGuid().ToString() + "}.zip";
            EmbeddedZip.ExtractZip("FFNx", tmpZip);
            ExtractFiles(tmpZip, ff7InstallDirectory.Text);
            System.IO.File.Delete(tmpZip);

            Status_Text.Content = "Creating setup utillity.";
            Registry.Uninstall.CreateUninstallerKeys(sevenHInstalPathTxt.Text+"\\", ff7InstallDirectory.Text+"\\");
            
            /*Process proc = new Process();
            proc.StartInfo.FileName = "regedit.exe";
            proc.StartInfo.Arguments = "\"" + ff7InstallDirectory.Text + "\\FFNx.reg\"";
            Console.WriteLine(proc.StartInfo.FileName+" "+proc.StartInfo.Arguments);
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
            proc.WaitForExit();*/

            string writeTo = sevenHInstalPathTxt.Text + "\\setup.exe";
            string readFrom = Assembly.GetEntryAssembly().CodeBase;
            System.IO.File.Copy(new Uri(readFrom).LocalPath, writeTo, true);

            Status_Text.Content = "Installer Finished.";
            CreateShortcutForAllUsers();
            InstallBtn.Content = "Close";
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            ff7InstallDirectory.Text = RegistryHelper.GetInstallLocation();
            ff7InstallDirectory.Select(ff7InstallDirectory.Text.Length, 0);
        }

        private void BrowserFFVIIBtn_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).IsEnabled)
            {
                var dlg = new FolderPicker();
                dlg.InputPath = ff7InstallDirectory.Text;
                if (dlg.ShowDialog() == true)
                {
                    ff7InstallDirectory.Text = dlg.ResultPath;
                }
            }
        }

        private void browserInstallLocation_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).IsEnabled)
            {
                var path = sevenHInstalPathTxt.Text;
                var pathParts = path.Split(new[] { '\\' });
                string[] newPath = new string[pathParts.Length - 1];
                for (ushort i = 0; i < pathParts.Length - 1; i++)
                {
                    newPath[i] = pathParts[i];
                }
                var dlg = new FolderPicker();
                dlg.InputPath = String.Join("\\", newPath);
                if (dlg.ShowDialog() == true)
                {
                    sevenHInstalPathTxt.Text = dlg.ResultPath + "\\7th Heaven";
                }
            }
        }

        private void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)InstallBtn.Content == "Close")
            {
                Environment.Exit(0);
            }
            else
            {
                Install();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}