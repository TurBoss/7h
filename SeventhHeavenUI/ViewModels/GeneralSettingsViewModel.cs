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
        private bool _openIrosLinks;
        private bool _openModFilesWith7H;
        private bool _warnAboutModCode;

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

        public bool OpenIrosLinks
        {
            get
            {
                return _openIrosLinks;
            }
            set
            {
                _openIrosLinks = value;
                NotifyPropertyChanged();
            }
        }

        public bool OpenModFilesWith7H
        {
            get
            {
                return _openModFilesWith7H;
            }
            set
            {
                _openModFilesWith7H = value;
                NotifyPropertyChanged();
            }
        }

        public bool WarnAboutModCode
        {
            get
            {
                return _warnAboutModCode;
            }
            set
            {
                _warnAboutModCode = value;
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

            KeepOldModsAfterUpdating = Sys.Settings.HasOption(GeneralOptions.KeepOldVersions);
            ActivateInstalledModsAuto = Sys.Settings.HasOption(GeneralOptions.AutoActiveNewMods);
            ImportLibraryFolderAuto = Sys.Settings.HasOption(GeneralOptions.AutoImportMods);
            CheckForUpdatesAuto = Sys.Settings.HasOption(GeneralOptions.CheckForUpdates);
            BypassCompatibilityLocks = Sys.Settings.HasOption(GeneralOptions.BypassCompatibility);
            OpenIrosLinks = Sys.Settings.HasOption(GeneralOptions.OpenIrosLinksWith7H);
            OpenModFilesWith7H = Sys.Settings.HasOption(GeneralOptions.OpenModFilesWith7H);
            WarnAboutModCode = Sys.Settings.HasOption(GeneralOptions.WarnAboutModCode);

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

            Sys.Settings.SubscribedUrls = SubscriptionsInput.Split(new string[] { "\n", "\r\n", " " }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
            Sys.Settings.ExtraFolders = ExtraFoldersInput.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
            Sys.Settings.AlsoLaunch = AlsoLaunchInput.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

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

            List<GeneralOptions> newOptions = new List<GeneralOptions>();

            if (KeepOldModsAfterUpdating)
                newOptions.Add(GeneralOptions.KeepOldVersions);

            if (ActivateInstalledModsAuto)
                newOptions.Add(GeneralOptions.AutoActiveNewMods);

            if (ImportLibraryFolderAuto)
                newOptions.Add(GeneralOptions.AutoImportMods);

            if (CheckForUpdatesAuto)
                newOptions.Add(GeneralOptions.CheckForUpdates);

            if (BypassCompatibilityLocks)
                newOptions.Add(GeneralOptions.BypassCompatibility);

            if (OpenIrosLinks)
            {
                AssociateIrosUrlWith7H();
                newOptions.Add(GeneralOptions.OpenIrosLinksWith7H);
            }

            if (OpenModFilesWith7H)
            {
                AssociateIroFilesWith7H();
                newOptions.Add(GeneralOptions.OpenModFilesWith7H);
            }

            if (WarnAboutModCode)
                newOptions.Add(GeneralOptions.WarnAboutModCode);

            Sys.Settings.Options = newOptions;

            // Clear EXE compatibility flags if user opts out
            if (!Sys.Settings.HasOption(GeneralOptions.SetEXECompatFlags))
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

            if (string.IsNullOrWhiteSpace(FF7ExePathInput))
            {
                validationMessage = "Missing FF7 Exe path.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(LibraryPathInput))
            {
                validationMessage = "Missing Library path.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(TexturesPathInput))
            {
                validationMessage = "Missing Textures (Aali OpenGL) path.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(MoviesPathInput))
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

        /// <summary>
        /// Update Registry to associate .iro mod files with 7H
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool AssociateIroFilesWith7H(RegistryKey key)
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

            return true;
        }

        /// <summary>
        /// Update Registry to asssociate iros:// URL with 7H
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool AssociateIrosUrlWith7H(RegistryKey key)
        {
            string app = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var iros = key.CreateSubKey("iros");
            if (iros == null) return false;

            var icon = iros.CreateSubKey("DefaultIcon");
            var shell = iros.CreateSubKey("shell");
            var open = shell.CreateSubKey("open");
            var command = open.CreateSubKey("command");

            iros.SetValue(String.Empty, "7H Catalog Subscription");
            icon.SetValue(String.Empty, "\"" + app + "\"");

            command.SetValue(String.Empty, "\"" + app + "\" \"%1\"");

            //Refresh Shell/Explorer so icon cache updates
            //do this now because we don't care so much about assoc. URL if it fails
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

            return true;
        }

        internal static bool AssociateIroFilesWith7H()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot;
                bool global = AssociateIroFilesWith7H(key);
                if (!global)
                {
                    key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                    global = AssociateIroFilesWith7H(key);
                }

                return global;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Sys.Message(new WMessage("Failed to associate iros:// links with 7th Heaven"));
                return false;
            }


        }

        internal static bool AssociateIrosUrlWith7H()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot;
                bool global = AssociateIrosUrlWith7H(key);
                if (!global)
                {
                    key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                    global = AssociateIrosUrlWith7H(key);
                }
                return global;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Sys.Message(new WMessage("Failed to associate .iro mod files with 7th Heaven"));
                return false;
            }
        }
    }
}
