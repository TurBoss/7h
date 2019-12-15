using Iros._7th;
using Iros._7th.Workshop;
using Microsoft.Win32;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool _showContextMenuInExplorer;

        private ObservableCollection<SubscriptionSettingViewModel> _subscriptionList;
        private string _newUrlText;
        private string _newNameText;
        private bool _isSubscriptionPopupOpen;

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

        public bool ShowContextMenuInExplorer
        {
            get
            {
                return _showContextMenuInExplorer;
            }
            set
            {
                _showContextMenuInExplorer = value;
                NotifyPropertyChanged();
            }
        }


        public bool IsSubscriptionPopupOpen
        {
            get
            {
                return _isSubscriptionPopupOpen;
            }
            set
            {
                _isSubscriptionPopupOpen = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<SubscriptionSettingViewModel> SubscriptionList
        {
            get
            {
                if (_subscriptionList == null)
                    _subscriptionList = new ObservableCollection<SubscriptionSettingViewModel>();

                return _subscriptionList;
            }
            set
            {
                _subscriptionList = value;
                NotifyPropertyChanged();
            }
        }

        public string NewUrlText
        {
            get
            {
                return _newUrlText;
            }
            set
            {
                _newUrlText = value;
                NotifyPropertyChanged();
            }
        }

        public string NewNameText
        {
            get
            {
                return _newNameText;
            }
            set
            {
                _newNameText = value;
                NotifyPropertyChanged();
            }
        }

        public GeneralSettingsViewModel()
        {
            NewUrlText = "";
            NewNameText = "";
        }

        internal void LoadSettings()
        {
            AutoDetectSystemPaths();

            foreach (Subscription item in Sys.Settings.Subscriptions)
            {
                SubscriptionList.Add(new SubscriptionSettingViewModel(item.Url, item.Name));
            }

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
            ShowContextMenuInExplorer = Sys.Settings.HasOption(GeneralOptions.Show7HInFileExplorerContextMenu);
        }

        public static void AutoDetectSystemPaths()
        {
            if (String.IsNullOrEmpty(Sys.Settings.FF7Exe))
            {
                Logger.Info("FF7 Exe path is empty. Auto detecting paths ...");

                string registry_path = null;
                string ff7 = null;

                try
                {
                    registry_path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Square Soft, Inc.\Final Fantasy VII";
                    ff7 = (string)Registry.GetValue(registry_path, "AppPath", null);
                }
                catch
                {
                    // could fail if game not installed
                }

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


            List<Subscription> updatedSubscriptions = new List<Subscription>();

            foreach (SubscriptionSettingViewModel item in SubscriptionList.ToList())
            {
                var existingSub = Sys.Settings.Subscriptions.FirstOrDefault(s => s.Url == item.Url);

                if (existingSub == null)
                {
                    existingSub = new Subscription() { Name = item.Name, Url = item.Url };
                }
                else
                {
                    existingSub.Name = item.Name;
                }

                updatedSubscriptions.Add(existingSub);
            }

            Sys.Settings.Subscriptions = updatedSubscriptions;
            Sys.Settings.AlsoLaunch = AlsoLaunchInput.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
            Sys.Settings.ExtraFolders = ExtraFoldersInput.ToLower()
                                                         .Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                                         .Distinct()
                                                         .ToList();

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
            else
            {
                RemoveIrosUrlAssociationFromRegistry();
            }

            if (OpenModFilesWith7H)
            {
                AssociateIroFilesWith7H();
                newOptions.Add(GeneralOptions.OpenModFilesWith7H);
            }
            else
            {
                RemoveIroFileAssociationFromRegistry();
            }

            if (WarnAboutModCode)
                newOptions.Add(GeneralOptions.WarnAboutModCode);

            if (ShowContextMenuInExplorer)
            {
                // TODO: add context menu option to Explorer
                newOptions.Add(GeneralOptions.Show7HInFileExplorerContextMenu);
            }


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
        /// Deletes Registry keys/values (if they exist) to unassociate .iro mod files with 7H
        /// </summary>
        /// <param name="key"> could be HKEY_CLASSES_ROOT or HKEY_CURRENT_USER/Software/Classes </param>
        private static bool RemoveIroFileAssociationFromRegistry(RegistryKey key)
        {
            try
            {
                List<string> subkeys = key.GetSubKeyNames().Where(k => k == "7thHeaven" || k == ".iro").ToList();
                bool deletedKeys = false;

                if (subkeys.Contains("7thHeaven"))
                {
                    key.DeleteSubKeyTree("7thHeaven");
                    deletedKeys = true;
                }

                if (subkeys.Contains(".iro"))
                {
                    key.DeleteSubKeyTree(".iro");
                    deletedKeys = true;
                }

                if (deletedKeys)
                {
                    //Refresh Shell/Explorer so icon cache updates
                    //do this now because we don't care so much about assoc. URL if it fails
                    SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Warn(e); // could be error thrown if already deleted
                return false;
            }
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

        /// <summary>
        /// Deletes Registry key/values (if they exist) to unasssociate iros:// URL with 7H
        /// </summary>
        /// <param name="key"> could be HKEY_CLASSES_ROOT or HKEY_CURRENT_USER/Software/Classes </param>
        /// <returns></returns>
        private static bool RemoveIrosUrlAssociationFromRegistry(RegistryKey key)
        {
            try
            {
                List<string> subkeys = key.GetSubKeyNames().Where(k => k == "iros").ToList();

                if (subkeys.Contains("iros"))
                {
                    key.DeleteSubKeyTree("iros");

                    //Refresh Shell/Explorer so icon cache updates
                    //do this now because we don't care so much about assoc. URL if it fails
                    SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Warn(e); // could be error thrown if already deleted
                return false;
            }
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
                Sys.Message(new WMessage("Failed to register .iro mod files with 7th Heaven"));
                return false;
            }


        }

        internal static bool RemoveIroFileAssociationFromRegistry()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot;
                bool global = RemoveIroFileAssociationFromRegistry(key);
                if (!global)
                {
                    key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                    global = RemoveIroFileAssociationFromRegistry(key);
                }

                return global;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Sys.Message(new WMessage("Failed to un-register .iro mod files with 7th Heaven"));
                return false;
            }
        }

        internal static bool RemoveIrosUrlAssociationFromRegistry()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot;
                bool global = RemoveIrosUrlAssociationFromRegistry(key);
                if (!global)
                {
                    key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                    global = RemoveIrosUrlAssociationFromRegistry(key);
                }

                return global;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Sys.Message(new WMessage("Failed to un-register iros:// links with 7th Heaven"));
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
                Sys.Message(new WMessage("Failed to register iros:// links with 7th Heaven"));
                return false;
            }
        }

        internal void EditSelectedSubscription(SubscriptionSettingViewModel selected)
        {
            IsSubscriptionPopupOpen = true;
            NewUrlText = selected.Url;
            NewNameText = selected.Name ?? "";
        }

        internal void AddNewSubscription()
        {
            IsSubscriptionPopupOpen = true;
            string clipboardContent = "";

            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                clipboardContent = Clipboard.GetText(TextDataFormat.Text);
            }

            if (!string.IsNullOrWhiteSpace(clipboardContent) && clipboardContent.StartsWith("iros://"))
            {
                NewUrlText = clipboardContent;
            }
        }

        /// <summary>
        /// Adds or Edits subscription and closes subscription popup
        /// </summary>
        internal bool SaveSubscription()
        {
            if (!NewUrlText.StartsWith("iros://"))
            {
                Sys.Message(new WMessage("URL must be in iros:// format"));
                return false;
            }

            if (!SubscriptionList.Any(s => s.Url == NewUrlText))
            {
                ResolveCatalogNameFromUrl(NewUrlText, resolvedName => 
                {
                    NewNameText = resolvedName;
                    SubscriptionList.Add(new SubscriptionSettingViewModel(NewUrlText, NewNameText));
                    CloseSubscriptionPopup();
                });
            }
            else
            {
                SubscriptionSettingViewModel toEdit = SubscriptionList.FirstOrDefault(s => s.Url == NewUrlText);
                toEdit.Name = NewNameText;
                CloseSubscriptionPopup();
            }

            return true;
        }

        internal void CloseSubscriptionPopup()
        {
            IsSubscriptionPopupOpen = false;
            NewUrlText = "";
            NewNameText = "";
        }

        internal void RemoveSelectedSubscription(SubscriptionSettingViewModel selected)
        {
            SubscriptionList.Remove(selected);
        }

        /// <summary>
        /// Downloads catalog.xml to temp file and gets Name of the catalog. 
        /// resolved name gets passed to delegate method that is called after download
        /// </summary>
        /// <param name="catalogUrl"></param>
        /// <param name="callback"></param>
        internal void ResolveCatalogNameFromUrl(string catalogUrl, Action<string> callback)
        {
            string name = "";

            string uniqueFileName = $"cattemp{Path.GetRandomFileName()}.xml"; // save temp catalog update to unique filename so multiple catalog updates can download async
            string path = Path.Combine(Sys.SysFolder, "temp", uniqueFileName);

            Sys.Downloads.Download(catalogUrl, path, $"Resolving catalog name for {catalogUrl}", new Install.InstallProcedureCallback(e =>
            {
                bool success = (e.Error == null && e.Cancelled == false);

                if (success)
                {
                    try
                    {
                        Catalog c = Util.Deserialize<Catalog>(path);
                        name = c.Name ?? "";

                        // delete temp file if it exists
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Failed to deserialize catalog - {ex.Message}");
                    }
                }

                callback(name);

            }), new Action(() =>
            {
                // delete temp file on cancel if it exists
                if (File.Exists(path))
                {
                    File.Delete(path); 
                }

                callback(name);

            }));
        }

    }
}
