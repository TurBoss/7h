using Iros._7th.Workshop;
using Microsoft.Win32;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.ViewModels
{
    public class GeneralSettingsViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _fF7ExePathInput;
        private string _moviesPathInput;
        private string _texturesPathInput;
        private string _libraryPathInput;
        private bool _keepOldModsAfterUpdating;
        private bool _activateInstalledModsAuto;
        private bool _importLibraryFolderAuto;
        private bool _checkForUpdatesAuto;
        private bool _bypassCompatibilityLocks;
        private string _subscriptionsInput;
        private string _extraFoldersInput;
        private string _alsoLaunchInput;
        private bool _launchWithCompatFlags;

        public string FF7ExePathInput
        {
            get
            {
                return _fF7ExePathInput;
            }
            set
            {
                _fF7ExePathInput = value;
                NotifyPropertyChanged();
            }
        }

        public string MoviesPathInput
        {
            get
            {
                return _moviesPathInput;
            }
            set
            {
                _moviesPathInput = value;
                NotifyPropertyChanged();
            }
        }

        public string TexturesPathInput
        {
            get
            {
                return _texturesPathInput;
            }
            set
            {
                _texturesPathInput = value;
                NotifyPropertyChanged();
            }
        }

        public string LibraryPathInput
        {
            get
            {
                return _libraryPathInput;
            }
            set
            {
                _libraryPathInput = value;
                NotifyPropertyChanged();
            }
        }

        public string SubscriptionsInput
        {
            get
            {
                return _subscriptionsInput;
            }
            set
            {
                _subscriptionsInput = value;
                NotifyPropertyChanged();
            }
        }

        public string ExtraFoldersInput
        {
            get
            {
                return _extraFoldersInput;
            }
            set
            {
                _extraFoldersInput = value;
                NotifyPropertyChanged();
            }
        }

        public string AlsoLaunchInput
        {
            get
            {
                return _alsoLaunchInput;
            }
            set
            {
                _alsoLaunchInput = value;
                NotifyPropertyChanged();
            }
        }

        public bool KeepOldModsAfterUpdating
        {
            get
            {
                return _keepOldModsAfterUpdating;
            }
            set
            {
                _keepOldModsAfterUpdating = value;
                NotifyPropertyChanged();
            }
        }

        public bool ActivateInstalledModsAuto
        {
            get
            {
                return _activateInstalledModsAuto;
            }
            set
            {
                _activateInstalledModsAuto = value;
                NotifyPropertyChanged();
            }
        }

        public bool ImportLibraryFolderAuto
        {
            get
            {
                return _importLibraryFolderAuto;
            }
            set
            {
                _importLibraryFolderAuto = value;
                NotifyPropertyChanged();
            }
        }

        public bool CheckForUpdatesAuto
        {
            get
            {
                return _checkForUpdatesAuto;
            }
            set
            {
                _checkForUpdatesAuto = value;
                NotifyPropertyChanged();
            }
        }

        public bool BypassCompatibilityLocks
        {
            get
            {
                return _bypassCompatibilityLocks;
            }
            set
            {
                _bypassCompatibilityLocks = value;
                NotifyPropertyChanged();
            }
        }

        public bool LaunchWithCompatFlags
        {
            get
            {
                return _launchWithCompatFlags;
            }
            set
            {
                _launchWithCompatFlags = value;
                NotifyPropertyChanged();
            }
        }

        public GeneralSettingsViewModel()
        {
        }

        internal void LoadSettings()
        {
            AutoDetectSystemPaths();

            SubscriptionsInput = string.Join("\n", Sys.Settings.SubscribedUrls.Distinct().ToArray());
            ExtraFoldersInput = string.Join("\n", Sys.Settings.ExtraFolders.Distinct().ToArray());
            AlsoLaunchInput = string.Join("\n", Sys.Settings.AlsoLaunch.Distinct().ToArray());

            FF7ExePathInput = Sys.Settings.FF7Exe;
            LibraryPathInput = Sys.Settings.LibraryLocation;
            MoviesPathInput = Sys.Settings.MovieFolder;
            TexturesPathInput = Sys.Settings.AaliFolder;

            KeepOldModsAfterUpdating = Sys.Settings.Options.HasFlag(GeneralOptions.KeepOldVersions);
            ActivateInstalledModsAuto = Sys.Settings.Options.HasFlag(GeneralOptions.AutoActiveNewMods);
            ImportLibraryFolderAuto = Sys.Settings.Options.HasFlag(GeneralOptions.AutoImportMods);
            CheckForUpdatesAuto = Sys.Settings.Options.HasFlag(GeneralOptions.CheckForUpdates);
            BypassCompatibilityLocks = Sys.Settings.Options.HasFlag(GeneralOptions.BypassCompatibility);
            LaunchWithCompatFlags = Sys.Settings.Options.HasFlag(GeneralOptions.SetEXECompatFlags);

            if (Sys.Settings.VersionUpgradeCompleted < Sys.Version)
            {
                if (String.IsNullOrWhiteSpace(Sys.Settings.MovieFolder))
                {
                    Sys.Settings.MovieFolder = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Square Soft, Inc.\Final Fantasy VII", "MoviePath", null);
                }

                if (MessageBox.Show("Would you like 7th Heaven to open catalog subscription links that begin with iros://?", "iros:// Link Setup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (!WriteLinkReg()) throw new Exception("Could not create keys");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        MessageBox.Show("Unable to register links: " + ex.ToString(), "Failed To Register Links", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            Sys.Settings.VersionUpgradeCompleted = Sys.Version;
        }

        public static void AutoDetectSystemPaths()
        {
            if (String.IsNullOrEmpty(Sys.Settings.FF7Exe))
            {
                Logger.Info("FF7 Exe path is empty. Auto detecting paths ...");

                string registry_path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Square Soft, Inc.\Final Fantasy VII";
                string ff7 = (string)Registry.GetValue(registry_path, "AppPath", null);

                if (!string.IsNullOrEmpty(ff7))
                {
                    Sys.Settings.AaliFolder = ff7 + @"mods\Textures\";
                    Sys.Settings.FF7Exe = ff7 + @"FF7.exe";

                    Sys.Settings.MovieFolder = (string)Registry.GetValue(registry_path, "MoviePath", null);

                    Sys.Settings.LibraryLocation = ff7 + @"mods\7th Heaven\";

                    Sys.Settings.ExtraFolders.Add("direct");
                    Sys.Settings.ExtraFolders.Add("music");
                }
                else
                {
                    Logger.Warn("Auto detect paths failed - could not determine ff7.exe path from Windows Registry.");
                }
            }
        }

        internal bool SaveSettings()
        {
            if (!ValidateSettings())
            {
                return false;
            }

            Sys.Settings.SubscribedUrls = SubscriptionsInput.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
            Sys.Settings.ExtraFolders = ExtraFoldersInput.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
            Sys.Settings.AlsoLaunch = AlsoLaunchInput.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();


            // ensure 'direct' and 'music' folders are always in ExtraFolders list
            if (!Sys.Settings.ExtraFolders.Contains("direct", StringComparer.InvariantCultureIgnoreCase))
            {
                Sys.Settings.ExtraFolders.Add("direct");
            }

            if (!Sys.Settings.ExtraFolders.Contains("music", StringComparer.InvariantCultureIgnoreCase))
            {
                Sys.Settings.ExtraFolders.Add("music");
            }

            Sys.Settings.FF7Exe = FF7ExePathInput;
            Sys.Settings.LibraryLocation = LibraryPathInput;
            Sys.Settings.MovieFolder = MoviesPathInput;
            Sys.Settings.AaliFolder = TexturesPathInput;

            GeneralOptions newOptions = 0;

            if (KeepOldModsAfterUpdating)
                newOptions |= GeneralOptions.KeepOldVersions;

            if (ActivateInstalledModsAuto)
                newOptions |= GeneralOptions.AutoActiveNewMods;

            if (ImportLibraryFolderAuto)
                newOptions |= GeneralOptions.AutoImportMods;

            if (CheckForUpdatesAuto)
                newOptions |= GeneralOptions.CheckForUpdates;

            if (BypassCompatibilityLocks)
                newOptions |= GeneralOptions.BypassCompatibility;

            if (LaunchWithCompatFlags)
                newOptions |= GeneralOptions.SetEXECompatFlags;

            Sys.Settings.Options = newOptions;

            // Clear EXE compatibility flags if user opts out
            if (!Sys.Settings.Options.HasFlag(GeneralOptions.SetEXECompatFlags))
            {
                RegistryKey ff7CompatKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                if (ff7CompatKey.GetValue(Sys.Settings.FF7Exe) != null) ff7CompatKey.DeleteValue(Sys.Settings.FF7Exe);
            }

            Directory.CreateDirectory(Sys.Settings.LibraryLocation);

            Sys.Message(new WMessage() { Text = "General settings have been updated!" });

            return true;
        }

        private bool ValidateSettings(bool showMessage = true)
        {
            string validationMessage = "";
            bool isValid = true;

            if (string.IsNullOrEmpty(FF7ExePathInput))
            {
                validationMessage = "Missing FF7 Exe path.";
                isValid = false;
            }

            if (string.IsNullOrEmpty(LibraryPathInput))
            {
                validationMessage = "Missing Library path.";
                isValid = false;
            }

            if (string.IsNullOrEmpty(TexturesPathInput))
            {
                validationMessage = "Missing Textures (Aali OpenGL) path.";
                isValid = false;
            }

            if (string.IsNullOrEmpty(MoviesPathInput))
            {
                validationMessage = "Missing Movie path.";
                isValid = false;
            }

            if (showMessage && !isValid)
            {
                MessageBox.Show(validationMessage, "Settings Not Valid", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return isValid;
        }

        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private bool WriteLinkReg(RegistryKey key)
        {
            string app = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //Create Prog_ID in Registry so we can associate file types
            //TODO: Add additional subkeys to define an "Unpack" option for IROs
            var progid = key.CreateSubKey("7thHeaven");
            if (progid == null) return false;
            var icon = progid.CreateSubKey("DefaultIcon");
            var shell = progid.CreateSubKey("shell");
            var open = shell.CreateSubKey("open");
            var command = open.CreateSubKey("command");
            progid.SetValue(String.Empty, "7thHeaven Mod File");
            icon.SetValue(String.Empty, "\"" + app + "\"");
            command.SetValue(String.Empty, "\"" + app + "\" /OPENIRO:\"%1\"");

            //Associate .iro mod files with 7H's Prog_ID- .IRO extension
            var iroext = key.CreateSubKey(".iro");
            if (iroext == null) return false;
            iroext.SetValue(String.Empty, "7thHeaven");

            //Refresh Shell/Explorer so icon cache updates
            //do this now because we don't care so much about assoc. URL if it fails
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

            //Associate iros:// URL with 7H
            var iros = key.CreateSubKey("iros");
            if (iros == null) return false;
            icon = iros.CreateSubKey("DefaultIcon");
            shell = iros.CreateSubKey("shell");
            open = shell.CreateSubKey("open");
            command = open.CreateSubKey("command");
            iros.SetValue(String.Empty, "7H Catalog Subscription");
            icon.SetValue(String.Empty, "\"" + app + "\"");
            command.SetValue(String.Empty, "\"" + app + "\" \"%1\"");
            return true;
        }

        private bool WriteLinkReg()
        {
            var key = Microsoft.Win32.Registry.ClassesRoot;
            bool global = WriteLinkReg(key);
            if (!global)
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                global = WriteLinkReg(key);
            }
            return global;
        }
    }
}
