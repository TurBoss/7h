using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using Microsoft.Win32;
using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using SeventhHeaven.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SeventhHeavenUI.ViewModels
{
    enum TabIndex
    {
        MyMods,
        BrowseCatalog
    }

    public class MainWindowViewModel : ViewModelBase
    {
        #region Data Members And Properties

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal const string _msgReqMissing =
    @"This mod requires the following mods to also be active, but I could not find them:
{0}
It may not work correctly unless you install them.";

        internal const string _msgBadVer =
            @"This mod requires the following mods, but you do not have a supported version:
{0}
You may need to update these mods.";

        internal const string _msgRequired =
            @"This mod requires that you activate the following mods:
{0}
They will be automatically turned on.";

        internal const string _msgRemove =
            @"This mod requires that you deactivate the following mods:
{0}
They will be automatically turned off.";

        internal const string _forbidMain =
            @"You cannot activate this mod, because it is incompatible with {0}. You will have to deactivate {0} before you can enable this mod.";

        internal const string _forbidDependent =
            @"You cannot activate this mod, because it requires {0} to be active, but {0} is incompatible with {1}. You will have to deactivate {1} before you can enable this mod.";
        internal const string _showAllText = "Select All";
        internal const string _unknownText = "Unknown";

        public List<string> AppHints = new List<string>()
        {
            "Did you know? You can drag and drop a mod to reorder your list.",
            "You can middle-click a mod to activate or deactivate it.",
            "You can double-click a mod to open the mod configuration window.",
            "You can double-click a mod in Browse Catalog to start downloading it.",
            "You can click the Auto Sort button to quickly put your mod list in the recommended order.",
            "Mod Authors: Did you know you can base64 encode images into your Readme.html?",
            "You can double-click a profile in the manage profiles window to load the selected profile.",
            "Right-click My Mods tab to open your Mod Library Folder",
            "Right-click Browse Catalog tab to open your Catalog settings",
            "You can find answers to many questions or problems on the forum at https://forums.qhimm.com.",
            "You can find answers to many questions or problems from the Help menu."
        };

        private string _catFile;
        private string _statusMessage;
        private string _currentProfile;
        private string _searchText;
        private int _selectedTabIndex;
        private List<FilterItemViewModel> _availableFilters;

        private Mod _previewMod;
        private string _previewModAuthor;
        private string _previewModName;
        private string _previewModVersion;
        private string _previewModReleaseDate;
        private string _previewModReleaseNotes;
        private string _previewModCategory;
        private string _previewModDescription;
        private string _previewModLink;
        private Uri _previewModImageSource;
        private bool _previewModHasReadMe;

        public MyModsViewModel MyMods { get; set; }

        public CatalogViewModel CatalogMods { get; set; }

        private Visibility _loadingGifVisibility;
        private bool _isFlashingStatus;
        private Visibility _noImageTextVisibility;

        public string WindowTitle
        {
            get
            {
                return $"{App.GetAppName()} v{App.GetAppVersion().ToString()} - Mod Manager for Final Fantasy 7 [{CurrentProfile}]";
            }
        }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(ReadmeVisibility));

                    ReloadAvailableFilters();

                    if ((TabIndex)_selectedTabIndex == TabIndex.MyMods)
                    {
                        UpdateModPreviewInfo(MyMods.GetSelectedMod());
                    }
                    else
                    {
                        UpdateModPreviewInfo(CatalogMods.GetSelectedMod());
                    }
                }
            }
        }

        internal void LaunchGame(bool variableDump = false, bool debugLogging = false)
        {
            GameLaunchWindow.Show(variableDump, debugLogging);
        }

        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                Logger.Info(_statusMessage);
                NotifyPropertyChanged();
            }
        }

        public string CurrentProfile
        {
            get
            {
                return _currentProfile;
            }
            set
            {
                _currentProfile = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(WindowTitle));
            }
        }

        public string PreviewModAuthor
        {
            get
            {
                return _previewModAuthor;
            }
            set
            {
                _previewModAuthor = value;
                NotifyPropertyChanged();
            }
        }

        public string PreviewModName
        {
            get
            {
                return _previewModName;
            }
            set
            {
                _previewModName = value;
                NotifyPropertyChanged();
            }
        }

        public string PreviewModVersion
        {
            get
            {
                return _previewModVersion;
            }
            set
            {
                _previewModVersion = value;
                NotifyPropertyChanged();
            }
        }

        public string PreviewModReleaseDate
        {
            get
            {
                return _previewModReleaseDate;
            }
            set
            {
                _previewModReleaseDate = value;
                NotifyPropertyChanged();
            }
        }

        public string PreviewModReleaseNotes
        {
            get
            {
                return _previewModReleaseNotes;
            }
            set
            {
                _previewModReleaseNotes = value;
                NotifyPropertyChanged();
            }
        }

        public string PreviewModCategory
        {
            get
            {
                return _previewModCategory;
            }
            set
            {
                _previewModCategory = value;
                NotifyPropertyChanged();
            }
        }

        public string PreviewModDescription
        {
            get
            {
                return _previewModDescription;
            }
            set
            {
                _previewModDescription = value;
                NotifyPropertyChanged();
            }
        }

        public string PreviewModLink
        {
            get
            {
                return _previewModLink;
            }
            set
            {
                _previewModLink = value;
                NotifyPropertyChanged();
            }
        }

        public bool PreviewModHasReadMe
        {
            get
            {
                return _previewModHasReadMe;
            }
            set
            {
                _previewModHasReadMe = value;
                NotifyPropertyChanged();
            }
        }

        public Uri PreviewModImageSource
        {
            get
            {
                return _previewModImageSource;
            }
            set
            {
                if (_previewModImageSource == null || !_previewModImageSource.Equals(value))
                {
                    _previewModImageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<FilterItemViewModel> AvailableFilters
        {
            get
            {
                if (_availableFilters == null)
                    _availableFilters = new List<FilterItemViewModel>();

                return _availableFilters;
            }
            set
            {
                _availableFilters = value;
                NotifyPropertyChanged();
            }
        }

        public List<FilterItemViewModel> CheckedCategories
        {
            get
            {
                return AvailableFilters.Where(c => c.FilterType == FilterItemType.Category && c.IsChecked && c.Name != _showAllText).ToList();
            }
        }

        public List<FilterItemViewModel> CheckedTags
        {
            get
            {
                return AvailableFilters.Where(c => c.FilterType == FilterItemType.Tag && c.IsChecked && c.Name != _showAllText).ToList();
            }
        }

        public Visibility LoadingGifVisibility
        {
            get
            {
                return _loadingGifVisibility;
            }
            set
            {
                if (_loadingGifVisibility != value)
                {
                    _loadingGifVisibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility NoImageTextVisibility
        {
            get
            {
                return _noImageTextVisibility;
            }
            set
            {
                if (_noImageTextVisibility != value)
                {
                    _noImageTextVisibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsFlashingStatus
        {
            get
            {
                return _isFlashingStatus;
            }
            set
            {
                _isFlashingStatus = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility ReadmeVisibility
        {
            get
            {
                if ((TabIndex)SelectedTabIndex == TabIndex.BrowseCatalog)
                    return Visibility.Hidden;

                return Visibility.Visible;
            }
        }


        #endregion

        public MainWindowViewModel()
        {
            SearchText = "";

            MyMods = new MyModsViewModel();
            MyMods.SelectedModChanged += ModsViewModel_SelectedModChanged;

            CatalogMods = new CatalogViewModel();
            CatalogMods.SelectedModChanged += CatalogViewModel_SelectedModChanged;

            LoadingGifVisibility = Visibility.Hidden;
        }


        /// <summary>
        /// Initializes the view model and initializes other files/profiles for use with 7th Heaven.
        /// This should be called first to ensure the app is fully initialized
        /// </summary>
        public void InitViewModel()
        {
            Sys.MessageReceived += Sys_MessageReceived;
            Sys.StatusChanged += new EventHandler<ModStatusEventArgs>(Sys_StatusChanged);

            // Set the Downloads Interface so Sys can use Download methods defined in the CatalogViewModel
            Sys.Downloads = CatalogMods;

            MegaIros.Logger = Logger.Info;

            GeneralSettingsViewModel.AutoDetectSystemPaths();

            CopyDllAndUpdaterExe(); // TODO: change or fix app updater process

            LoadCatalogXmlFile();

            Sys.InitLoaderContext();

            ThemeSettingsViewModel.LoadThemeFromFile();

            InitActiveProfile();


            // check if the settings window should be showed (on first start or if errors in settings)
            bool showSettings = false;
            if (Sys.Settings.IsFirstStart)
            {
                Sys.Settings.IsFirstStart = false;
                showSettings = true;
            }
            else
            {
                IEnumerable<string> errors = Sys.Settings.VerifySettings();
                if (errors.Any())
                {
                    string msg = "The following errors were found in your configuration:\n" +
                                 string.Join("\n", errors) + "\n" +
                                 "The settings window will now be displayed so you can fix them.";
                    MessageDialogWindow.Show(msg, "Config Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    showSettings = true;
                }
            }

            if (showSettings)
            {
                ShowGeneralSettingsWindow();
            }

            Sys.TryAutoImportMods();


            CatalogMods.RefreshListRequested += ModList_RefreshRequested;
            MyMods.RefreshListRequested += ModList_RefreshRequested;

            CatalogMods.CheckForCatalogUpdatesAsync(new CatCheckOptions());

            CatalogMods.ReloadModList();

            ReloadAvailableFilters();

            // this deletes any temp images that were extracted from IRO archives
            // ... the temp images are used for the configure mod window
            // ... the app does not release the file lock on the images at runtime of the app so 
            // ... this will ensure the images from last app session are deleted
            ConfigureModViewModel.DeleteTempFolder();

            // TODO: check for app updates
            StatusMessage = $"{App.GetAppName()} v{App.GetAppVersion().ToString()} started - Click here to view the app log.  |  Hint: {GetRandomHint()}";

        }

        /// <summary>
        /// Clears the search text and applied filters.
        /// Triggered when my mods or catalog mods list is refreshed from button click
        /// </summary>
        private void ModList_RefreshRequested()
        {
            SearchText = "";
            ReloadAvailableFilters(recheckFilters: false);
        }

        private void CatalogViewModel_SelectedModChanged(object sender, CatalogModItemViewModel selected)
        {
            if ((TabIndex)SelectedTabIndex == TabIndex.BrowseCatalog)
            {
                UpdateModPreviewInfo(selected);
            }
        }

        private void ModsViewModel_SelectedModChanged(object sender, InstalledModViewModel selected)
        {
            if ((TabIndex)SelectedTabIndex == TabIndex.MyMods)
            {
                UpdateModPreviewInfo(selected);
            }
        }

        private void Sys_StatusChanged(object sender, ModStatusEventArgs e)
        {
            CatalogMods.UpdateModDetails(e.ModID);

            if (e.Status == ModStatus.Installed)
            {
                // remove newly installed mod from info cache incase it is stale or the install location changed
                InstalledItem mod = Sys.Library.GetItem(e.ModID);
                string mfile = mod.LatestInstalled.InstalledLocation;
                InstalledItem.RemoveFromInfoCache(mfile);
                MyMods.ReloadModListFromUIThread(MyMods.GetSelectedMod()?.InstallInfo.ModID, SearchText, CheckedCategories, CheckedTags);
            }

            if (e.Status == ModStatus.Installed && e.OldStatus != ModStatus.Installed && Sys.Settings.HasOption(GeneralOptions.AutoActiveNewMods))
                MyMods.ToggleActivateMod(e.ModID);
            if (e.OldStatus == ModStatus.Installed && e.Status == ModStatus.NotInstalled && Sys.ActiveProfile.Items.Any(i => i.ModID.Equals(e.ModID)))
                MyMods.ToggleActivateMod(e.ModID);


            if (e.Status == ModStatus.InfoChanged)
            {
                // update mod preview info page when a change (e.g. image downloaded) has happeend for selected mod
                if ((TabIndex)SelectedTabIndex == TabIndex.MyMods)
                {
                    InstalledModViewModel currentlySelected = MyMods.GetSelectedMod();

                    if (currentlySelected?.InstallInfo.ModID == e.ModID)
                    {
                        UpdateModPreviewInfo(currentlySelected, forceUpdate: true);
                    }
                }
                else
                {
                    CatalogModItemViewModel currentlySelected = CatalogMods.GetSelectedMod();

                    if (currentlySelected?.Mod.ID == e.ModID)
                    {
                        UpdateModPreviewInfo(currentlySelected, forceUpdate: true);
                    }
                }
            }

        }

        /// <summary>
        /// Used to cleanup any resources (e.g. when shutting down app).
        /// Saves profile, Unsubscribes from Events, and nullifys certain objects.
        /// </summary>
        internal void CleanUp()
        {
            SaveActiveProfile();
            Sys.Save();

            MyMods.SelectedModChanged -= ModsViewModel_SelectedModChanged;
            MyMods.RefreshListRequested -= ModList_RefreshRequested;
            MyMods.ClearModList();
            MyMods.ModList = null;
            MyMods = null;

            CatalogMods.SelectedModChanged -= CatalogViewModel_SelectedModChanged;
            CatalogMods.RefreshListRequested -= ModList_RefreshRequested;
            CatalogMods.CatalogModList.Clear();
            CatalogMods = null;
        }

        private void UpdateModPreviewInfo(InstalledModViewModel selected, bool forceUpdate = false)
        {
            if (selected == null)
            {
                _previewMod = null;
                PreviewModAuthor = "";
                PreviewModName = "";
                PreviewModVersion = "";
                PreviewModReleaseDate = "";
                PreviewModReleaseNotes = "";
                PreviewModCategory = "";
                PreviewModDescription = "";
                PreviewModLink = "";
                PreviewModImageSource = null;
                return;
            }

            if (_previewMod?.ID == selected.InstallInfo?.ModID && !forceUpdate)
            {
                // no change in selected
                return;
            }

            _previewMod = selected.InstallInfo.CachedDetails;

            PreviewModAuthor = selected.Author;
            PreviewModVersion = selected.InstallInfo.CachedDetails.LatestVersion.Version.ToString();
            PreviewModName = $"{selected.Name} v{PreviewModVersion}";
            PreviewModReleaseDate = selected.ReleaseDate;
            PreviewModReleaseNotes = selected.InstallInfo.CachedDetails.LatestVersion.ReleaseNotes;
            PreviewModCategory = selected.Category;
            PreviewModDescription = selected.InstallInfo.CachedDetails.Description;
            PreviewModLink = selected.InstallInfo.CachedDetails.Link;

            InstalledVersion latestVersion = selected.InstallInfo.LatestInstalled;
            if (latestVersion != null)
            {
                PreviewModHasReadMe = latestVersion.HasData("readme.md") || latestVersion.HasData("readme.html") || latestVersion.HasData("readme.txt");
            }
            else
            {
                PreviewModHasReadMe = false;
            }

            if (!string.IsNullOrWhiteSpace(selected.InstallInfo.CachedDetails.LatestVersion.PreviewImage))
            {
                string pathToImage = Sys.ImageCache.GetImagePath(selected.InstallInfo.CachedDetails.LatestVersion.PreviewImage, selected.InstallInfo.CachedDetails.ID);
                SetPreviewImage(pathToImage, forceUpdate);
            }
            else
            {
                // no preview image found cached for mod
                NoImageTextVisibility = Visibility.Visible;
                LoadingGifVisibility = Visibility.Hidden;
                PreviewModImageSource = null;
            }
        }

        private void UpdateModPreviewInfo(CatalogModItemViewModel selected, bool forceUpdate = false)
        {
            if (selected == null)
            {
                _previewMod = null;
                PreviewModAuthor = "";
                PreviewModName = "";
                PreviewModVersion = "";
                PreviewModReleaseDate = "";
                PreviewModReleaseNotes = "";
                PreviewModCategory = "";
                PreviewModDescription = "";
                PreviewModLink = "";
                PreviewModImageSource = null;
                return;
            }

            if (_previewMod?.ID == selected.Mod?.ID && !forceUpdate)
            {
                // no change in selected
                return;
            }

            _previewMod = selected.Mod;

            PreviewModAuthor = selected.Author;
            PreviewModVersion = selected.Mod.LatestVersion.Version.ToString();
            PreviewModName = $"{selected.Name} v{PreviewModVersion}";
            PreviewModReleaseDate = selected.ReleaseDate;
            PreviewModReleaseNotes = selected.Mod.LatestVersion.ReleaseNotes;
            PreviewModCategory = selected.Category;
            PreviewModDescription = selected.Mod.Description;
            PreviewModLink = selected.Mod.Link;
            PreviewModHasReadMe = false; // no READMEs for catalog (only installed mods)

            string pathToImage = Sys.ImageCache.GetImagePath(selected.Mod.LatestVersion.PreviewImage, selected.Mod.ID);

            SetPreviewImage(pathToImage, forceUpdate);
        }

        private void SetPreviewImage(string pathToImage, bool forceUpdate = false)
        {
            NoImageTextVisibility = Visibility.Hidden;
            LoadingGifVisibility = Visibility.Visible;
            Uri newImageUri = pathToImage == null ? null : new Uri(pathToImage);

            if (!forceUpdate && newImageUri?.AbsolutePath == PreviewModImageSource?.AbsolutePath)
            {
                LoadingGifVisibility = Visibility.Hidden; // image does not have to be changed so hide the loading gif
            }
            else
            {
                PreviewModImageSource = newImageUri;
            }
        }

        private void InitActiveProfile()
        {
            Sys.ActiveProfile = null;

            Directory.CreateDirectory(Path.Combine(Sys.SysFolder, "profiles"));

            if (!String.IsNullOrWhiteSpace(Sys.Settings.CurrentProfile) && File.Exists(Sys.PathToCurrentProfileFile))
            {
                try
                {
                    Sys.ActiveProfile = Util.Deserialize<Profile>(Sys.PathToCurrentProfileFile);
                    RefreshProfile();
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Failed to load current profile xml ... Setting current profile to null");
                    Sys.Settings.CurrentProfile = null;
                }
            }

            if (Sys.ActiveProfile == null)
            {
                Sys.ActiveProfile = new Profile();
                Sys.Settings.CurrentProfile = "Default";
                Sys.Save();

                CurrentProfile = Sys.Settings.CurrentProfile;
            }
        }


        /// <summary>
        /// Sets <see cref="CurrentProfile"/> to match what is in <see cref="Sys.Settings"/> and Reloads 'My Mods' list for the profile.
        /// </summary>
        public void RefreshProfile()
        {
            if (Sys.ActiveProfile == null)
            {
                return;
            }

            CurrentProfile = Sys.Settings.CurrentProfile;


            // reload list of active mods for the profile
            MyMods.ClearModList();
            MyMods.ReloadModList(null, SearchText, CheckedCategories, CheckedTags);
        }

        /// <summary>
        /// Saves <see cref="Sys.ActiveProfile"/> to disk.
        /// </summary>
        internal static bool SaveActiveProfile()
        {
            if (Sys.ActiveProfile == null)
            {
                return false;
            }

            try
            {
                using (FileStream fs = new FileStream(Sys.PathToCurrentProfileFile, FileMode.Create))
                {
                    Util.Serialize(Sys.ActiveProfile, fs);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        private void Sys_MessageReceived(object sender, MessageEventArgs e)
        {
            string receivedMessage = e.Message.Text;
            if (!String.IsNullOrEmpty(e.Message.Link))
            {
                receivedMessage += $" - {e.Message.Link}";
            }

            // log message to app log or status bar
            if (e.Message.LogLevel != WMessageLogLevel.LogOnly)
            {
                StatusMessage = receivedMessage;
            }
            else
            {
                Logger.Info(receivedMessage);
            }

            // include exception in logs if it exists in message received
            if (e.Message.LoggedException != null)
            {
                Logger.Error(e.Message.LoggedException);
            }

            // flash status bar if important
            if (e.Message.IsImportant)
            {
                FlashStatusBar();
            }
        }

        private void FlashStatusBar(int timeToFlashInMilliseconds = 500)
        {
            IsFlashingStatus = true;

            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(timeToFlashInMilliseconds);
                IsFlashingStatus = false;
            });
        }

        private void LoadCatalogXmlFile()
        {
            try
            {
                _catFile = Path.Combine(Sys.SysFolder, "catalog.xml");
                Sys.SetNewCatalog(Util.Deserialize<Catalog>(_catFile));
            }
            catch (Exception e)
            {
                Logger.Warn(e, "failed to load catalog.xml - initializing empty catalog ...");
                Sys.SetNewCatalog(new Catalog());
            }
        }

        private static void CopyDllAndUpdaterExe()
        {
            try
            {
                string src = Path.Combine(Sys._7HFolder, "SharpCompressU.cpy");
                string dst = Path.Combine(Sys._7HFolder, "SharpCompressU.dll");

                if (File.Exists(dst))
                {
                    File.Delete(dst);
                    File.Copy(src, dst);
                }

                src = Path.Combine(Sys._7HFolder, "Updater.cpy");
                dst = Path.Combine(Sys._7HFolder, "Updater.exe");
                if (File.Exists(dst))
                {
                    File.Delete(dst);
                    File.Copy(src, dst);
                }
            }
            catch (IOException iex)
            {
                Logger.Warn(iex);
            }
            catch (UnauthorizedAccessException uae)
            {
                Logger.Warn(uae);
            }
        }

        internal static bool CheckAllowedActivate(Guid modID)
        {
            InstalledItem mod = Sys.Library.GetItem(modID);
            InstalledVersion inst = mod.LatestInstalled;
            string mfile = Path.Combine(Sys.Settings.LibraryLocation, inst.InstalledLocation);
            bool hasCode;
            var modInfo = mod.GetModInfo();

            if (mfile.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var arc = new _7thWrapperLib.IrosArc(mfile))
                {
                    hasCode = arc.AllFolderNames().Any(s => s.EndsWith("hext", StringComparison.InvariantCultureIgnoreCase));
                }
            }
            else if (Directory.Exists(mfile))
            {
                hasCode = Directory.GetDirectories(mfile, "*", System.IO.SearchOption.AllDirectories)
                                   .Any(s => s.EndsWith("hext", StringComparison.InvariantCultureIgnoreCase));
            }
            else
                hasCode = false;

            if (modInfo != null)
            {
                hasCode |= modInfo.LoadPlugins.Any();
                hasCode |= modInfo.LoadLibraries.Any();
                hasCode |= modInfo.LoadAssemblies.Any();
                hasCode |= modInfo.LoadPrograms.Any();
            }

            if (!hasCode) return true;


            if (Sys.Settings.HasOption(GeneralOptions.WarnAboutModCode))
            {
                // invoke the message on the Dispatcher UI Thread since this could be called from background threads
                return App.Current.Dispatcher.Invoke(() =>
                {
                    string msg = "This mod '{0}' contains data that could potentially harm your computer. You should only activate mods you trust.\n\nDo you still want to activate this mod?";
                    msg = String.Format(msg, mod.CachedDetails.Name);

                    AllowModToRunWindow warningWindow = new AllowModToRunWindow(msg);
                    warningWindow.ShowDialog();

                    if (warningWindow.ViewModel.IsChecked)
                    {
                        // set settings to turn off warning
                        Sys.Settings.RemoveOption(GeneralOptions.WarnAboutModCode);
                        Sys.Save();
                    }

                    if (warningWindow.ViewModel.YesRadioButtonIsChecked)
                    {
                        return true;
                    }

                    return false;
                });
            }

            return true;
        }

        internal void DoSearch()
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                // user is now searching by text so clear checked filters/tags
                AvailableFilters.ForEach(a => a.IsChecked = false);
            }

            if ((TabIndex)SelectedTabIndex == TabIndex.BrowseCatalog)
            {
                CatalogMods.ClearRememberedSearchTextAndCategories();
                CatalogMods.ReloadModList(CatalogMods.GetSelectedMod()?.Mod?.ID, SearchText, CheckedCategories, CheckedTags);
            }
            else
            {
                MyMods.ClearRememberedSearchTextAndCategories();
                MyMods.ReloadModList(MyMods.GetSelectedMod()?.InstallInfo?.ModID, SearchText, CheckedCategories, CheckedTags);
            }

            ReloadAvailableFilters();
        }

        internal void OpenPreviewModLink()
        {
            if (string.IsNullOrEmpty(PreviewModLink))
            {
                Logger.Warn("link is null/empty");
                return;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = PreviewModLink
                };

                Process.Start(startInfo);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

        }

        /// <summary>
        /// Opens the read-me of the selected mod being previewed.
        /// First looks for .html -> .md -> .txt in that order.
        /// </summary>
        internal void OpenPreviewModReadMe()
        {
            InstalledVersion inst = Sys.Library.GetItem(_previewMod.ID)?.LatestInstalled;

            if (inst == null)
            {
                Logger.Warn("cant find installed version");
                return;
            }

            bool hasReadmeFile = false;
            string tempDirPath = Path.Combine(Sys.SysFolder, "temp");
            string tempFileName = null;

            string pathToTempFile;

            Directory.CreateDirectory(tempDirPath);

            if (inst.HasData("readme.html"))
            {
                tempFileName = "readme.html";
                hasReadmeFile = true;
            }
            else if (inst.HasData("readme.md"))
            {
                tempFileName = "readme.md";
                hasReadmeFile = true;
            }
            else if (inst.HasData("readme.txt"))
            {
                tempFileName = "readme.txt";
                hasReadmeFile = true;
            }

            if (!hasReadmeFile)
            {
                Logger.Warn($"no readme file found for {_previewMod.Name} - {_previewMod.ID}");
                return;
            }

            pathToTempFile = Path.Combine(tempDirPath, tempFileName);

            try
            {
                using (Stream s = inst.GetData(tempFileName))
                {
                    using (FileStream tempFile = new FileStream(pathToTempFile, FileMode.Create, FileAccess.Write))
                    {
                        s.CopyTo(tempFile);
                    }
                }

                if (pathToTempFile.EndsWith(".txt"))
                {
                    Process.Start("notepad.exe", pathToTempFile);
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = pathToTempFile
                    };
                    Process.Start(startInfo);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "failed to open readme");
                return;
            }

        }

        /// <summary>
        /// Opens the 'Profiles' window to manage and switch profiles
        /// </summary>
        internal void ShowProfilesWindow()
        {
            SaveActiveProfile(); // ensure current profile is saved to disk so it shows in list of profiles

            OpenProfileWindow profileWindow = new OpenProfileWindow();
            bool? dialogResult = profileWindow.ShowDialog();

            if (dialogResult.GetValueOrDefault(false))
            {
                RefreshProfile();
            }
        }

        internal void ShowChunkToolWindow()
        {
            ChunkToolWindow toolWindow = new ChunkToolWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            toolWindow.ShowDialog();
        }

        internal void ShowGeneralSettingsWindow()
        {
            GeneralSettingsWindow settingsWindow = new GeneralSettingsWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            bool? didSave = settingsWindow.ShowDialog();

            if (didSave.GetValueOrDefault(false) && settingsWindow.ViewModel.SubscriptionsChanged)
            {
                CatalogMods.ForceCheckCatalogUpdateAsync();
            }
        }

        internal void ShowIroToolsWindow()
        {
            IroCreateWindow createWindow = new IroCreateWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            createWindow.ShowDialog();
        }

        internal void ShowGLConfigWindow()
        {
            string spec = Path.Combine(Sys._7HFolder, "ConfigSpec-FF7OpenGL.xml");
            string cfg = Path.Combine(Path.GetDirectoryName(Sys.Settings.FF7Exe), "ff7_opengl.cfg");

            ConfigureGLWindow gLWindow = new ConfigureGLWindow() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            gLWindow.Init(spec, cfg);
            gLWindow.ShowDialog();
        }


        /// <summary>
        /// sets IsChecked to value of <paramref name="isChecked"/> for all items in <paramref name="filterItems"/> except for the 'Show All' category 
        /// </summary>
        private void ToggleIsCheckedForAll(bool isChecked, List<FilterItemViewModel> filterItems)
        {
            foreach (var item in filterItems.Where(c => c.Name != _showAllText && c.FilterType != FilterItemType.Separator).ToList())
            {
                item.IsChecked = isChecked;
            }
        }

        /// <summary>
        /// Updates the IsChecked property of the 'Show All' item based on all other items in <paramref name="filterItems"/> being checked or not.
        /// </summary>
        private void CheckOrUncheckShowAllFilter(List<FilterItemViewModel> filterItems)
        {
            FilterItemViewModel item = filterItems.FirstOrDefault(cat => cat.Name == _showAllText);
            item.SetIsChecked(filterItems.Where(cat => cat.Name != _showAllText && cat.FilterType != FilterItemType.Separator).All(cat => cat.IsChecked));
        }

        internal List<string> GetTagsForSelectedTab()
        {
            List<string> tags = new List<string>();

            if ((TabIndex)SelectedTabIndex == TabIndex.BrowseCatalog)
            {
                foreach (List<string> modTags in Sys.Catalog.Mods.Where(c => c.Tags.Count > 0).Select(c => c.Tags))
                {
                    tags.AddRange(modTags);
                }

                // include the name of the catalogs as a tag
                List<string> catalogNames = Sys.Catalog.Mods.Where(m => !string.IsNullOrWhiteSpace(m.SourceCatalogName))
                                                            .Select(m => m.SourceCatalogName)
                                                            .Distinct().ToList();

                tags.AddRange(catalogNames);
            }
            else
            {
                foreach (List<string> modTags in Sys.Library.Items.Where(m => m.CachedDetails.Tags.Count > 0).Select(m => m.CachedDetails.Tags))
                {
                    tags.AddRange(modTags);
                }
            }

            tags = tags.Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1)}") // make Tag display as proper case e.g. 'My tag' intead of 'my tag'
                       .Distinct(StringComparer.CurrentCultureIgnoreCase)
                       .OrderBy(s => s)
                       .ToList();

            return tags;
        }

        internal List<string> GetCategoriesForSelectedTab()
        {
            List<string> categories = null;

            if ((TabIndex)SelectedTabIndex == TabIndex.BrowseCatalog)
            {
                categories = Sys.Catalog.Mods.Where(c => !string.IsNullOrEmpty(c.Category))
                                             .Select(c => c.Category)
                                             .Distinct()
                                             .ToList();
            }
            else
            {
                categories = Sys.Library.Items.Where(c => !string.IsNullOrEmpty(c.CachedDetails.Category))
                                              .Select(c => c.CachedDetails.Category)
                                              .Distinct()
                                              .ToList();
            }

            categories = categories.OrderBy(s => s).ToList();

            // mods with no category are filtered with the 'Unknown' category
            if (!categories.Contains(_unknownText))
            {
                categories.Add(_unknownText);
            }

            return categories;
        }

        internal void ApplyFiltersAndReloadList()
        {
            if (CheckedCategories.Count > 0 || CheckedTags.Count > 0)
            {
                //user is now filtering by tags/category so clear search text
                SearchText = "";
            }

            if ((TabIndex)SelectedTabIndex == TabIndex.BrowseCatalog)
            {
                MyMods.ClearRememberedSearchTextAndCategories();
                CatalogMods.ReloadModList(CatalogMods.GetSelectedMod()?.Mod?.ID, SearchText, CheckedCategories, CheckedTags);
            }
            else
            {
                MyMods.ClearRememberedSearchTextAndCategories();
                MyMods.ReloadModList(MyMods.GetSelectedMod()?.InstallInfo?.ModID, SearchText, CheckedCategories, CheckedTags);
            }

            ReloadAvailableFilters();
        }

        internal void ReloadAvailableFilters(bool recheckFilters = true)
        {
            List<string> tags = GetTagsForSelectedTab();
            List<string> categories = GetCategoriesForSelectedTab();
            List<FilterItemViewModel> newList = new List<FilterItemViewModel>();

            newList.AddRange(categories.Select(c => new FilterItemViewModel(c, FilterItemType.Category)
            {
                OnChecked = new Action<bool>(isChecked => CheckOrUncheckShowAllFilter(AvailableFilters))
            }).ToList());

            if (tags.Count > 0)
            {
                newList.Add(new FilterItemViewModel("", FilterItemType.Separator));

                newList.AddRange(tags.Select(t => new FilterItemViewModel(t, FilterItemType.Tag)
                {
                    OnChecked = new Action<bool>(isChecked => CheckOrUncheckShowAllFilter(AvailableFilters))
                }).ToList());
            }


            // re-check items
            if (recheckFilters)
            {
                List<FilterItemViewModel> oldItems = AvailableFilters.ToList();

                foreach (FilterItemViewModel item in newList)
                {
                    if (oldItems.Any(c => c.Name == item.Name && c.IsChecked && c.FilterType == item.FilterType))
                    {
                        item.IsChecked = true;
                    }
                }
            }

            // setup 'Show All' filter item as first in the list
            bool allChecked = newList.Where(c => c.FilterType != FilterItemType.Separator).All(c => c.IsChecked);
            FilterItemViewModel showAllItem = new FilterItemViewModel(_showAllText, FilterItemType.ShowAll)
            {
                IsChecked = allChecked,
                OnChecked = new Action<bool>(isChecked => ToggleIsCheckedForAll(isChecked, AvailableFilters))
            };
            newList.Insert(0, showAllItem);

            AvailableFilters = newList;
        }

        /// <summary>
        /// Opens /Resources/Help/index.html if it exists as a new process
        /// </summary>
        internal void LaunchHelpPage()
        {
            string helpFile = Path.Combine(new string[] { Sys._7HFolder, "Resources", "Help", "index.html" });

            if (!File.Exists(helpFile))
            {
                Sys.Message(new WMessage("Cannot open help - Resources/Help/index.html file not found"));
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = helpFile
            };

            Process.Start(startInfo);
        }

        internal string GetRandomHint()
        {
            if (AppHints == null)
            {
                return "";
            }

            Random r = new Random();
            return AppHints[r.Next(0, AppHints.Count)];
        }

        internal void ShowGameLaunchSettingsWindow()
        {
            GameLaunchSettingsWindow launchSettingsWindow = new GameLaunchSettingsWindow();
            launchSettingsWindow.ShowDialog();
        }
    }

    internal class CatCheckOptions
    {
        public bool ForceCheck { get; set; }
    }

}
