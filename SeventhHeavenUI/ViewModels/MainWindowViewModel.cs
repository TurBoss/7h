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
        private string _catFile;
        private string _statusMessage;
        private string _currentProfile;
        private string _searchText;
        private int _selectedTabIndex;
        private List<string> _statusMessageLog;
        private List<FilterItemViewModel> _availableFilters;
        internal static _7thWrapperLib.LoaderContext _context;

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

        public MyModsViewModel ModsViewModel { get; set; }

        public CatalogViewModel CatalogViewModel { get; set; }

        private static Dictionary<string, _7thWrapperLib.ModInfo> _infoCache = new Dictionary<string, _7thWrapperLib.ModInfo>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, Process> _alsoLaunchProcesses = new Dictionary<string, Process>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, _7HPlugin> _plugins = new Dictionary<string, _7HPlugin>(StringComparer.InvariantCultureIgnoreCase);
        private Visibility _loadingGifVisibility;

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

                    ReloadAvailableFilters();

                    if ((TabIndex)_selectedTabIndex == TabIndex.MyMods)
                    {
                        UpdateModPreviewInfo(ModsViewModel.GetSelectedMod());
                    }
                    else
                    {
                        UpdateModPreviewInfo(CatalogViewModel.GetSelectedMod());
                    }
                }
            }
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

        #endregion

        public MainWindowViewModel()
        {
            SearchText = "";

            ModsViewModel = new MyModsViewModel();
            ModsViewModel.SelectedModChanged += ModsViewModel_SelectedModChanged;

            CatalogViewModel = new CatalogViewModel();
            CatalogViewModel.SelectedModChanged += CatalogViewModel_SelectedModChanged;

            LoadingGifVisibility = Visibility.Hidden;
        }

        private void CatalogViewModel_SelectedModChanged(object sender, CatalogModItemViewModel selected)
        {
            UpdateModPreviewInfo(selected);
        }

        private void ModsViewModel_SelectedModChanged(object sender, InstalledModViewModel selected)
        {
            UpdateModPreviewInfo(selected);
        }

        private void UpdateModPreviewInfo(InstalledModViewModel selected)
        {
            if (selected == null)
            {
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

            LoadingGifVisibility = Visibility.Visible;

            string pathToImage = Sys.ImageCache.GetImagePath(selected.InstallInfo.CachedDetails.LatestVersion.PreviewImage, selected.InstallInfo.CachedDetails.ID);
            PreviewModImageSource = pathToImage == null ? null : new Uri(pathToImage);
        }

        private void UpdateModPreviewInfo(CatalogModItemViewModel selected)
        {
            if (selected == null)
            {
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

            LoadingGifVisibility = Visibility.Visible;

            string pathToImage = Sys.ImageCache.GetImagePath(selected.Mod.LatestVersion.PreviewImage, selected.Mod.ID);

            CatalogModItemViewModel currentSelection = CatalogViewModel.GetSelectedMod();

            if (selected.Mod.ID == currentSelection?.Mod.ID)
            {
                PreviewModImageSource = pathToImage == null ? null : new Uri(pathToImage);
            }
        }

        public void InitViewModel()
        {
            Sys.MessageReceived += Sys_MessageReceived;
            Sys.StatusChanged += new EventHandler<ModStatusEventArgs>(Sys_StatusChanged);

            // Set the Downloads Interface so Sys can use Download methods defined in the CatalogViewModel
            Sys.Downloads = CatalogViewModel;

            StatusMessage = $"{App.GetAppName()} started: app v{App.GetAppVersion().ToString()} - Sys v{Sys.Version.ToString()}";

            MegaIros.Logger = Logger.Info;

            GeneralSettingsViewModel.AutoDetectSystemPaths();

            CopyDllAndUpdaterExe(); // TODO: change or fix app updater process

            LoadCatalogXmlFile();

            InitLoaderContext();

            ThemeSettingsViewModel.LoadThemeFromFile();

            InitActiveProfile();


            // check if the settings window should be showed (on first start or if errors in settings)
            // ... note that VersionUpgradeCompleted will be 0 when Sys.Settings is initialized for the first time.
            bool showSettings = false;
            if (Sys.Settings.VersionUpgradeCompleted < Sys.Version)
            {
                showSettings = true;
            }
            else
            {
                var errors = Sys.Settings.VerifySettings();
                if (errors.Any())
                {
                    string msg = "The following errors were found in your configuration:\n" +
                                 string.Join("\n", errors) + "\n" +
                                 "The settings window will now be displayed so you can fix them.";
                    MessageBox.Show(msg, "Config Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    showSettings = true;
                }
            }

            if (showSettings)
            {
                ShowGeneralSettingsWindow();
            }

            TryAutoImportMods();

            CatalogViewModel.CheckForCatalogUpdatesAsync(new CatCheckOptions());

            CatalogViewModel.ReloadModList();

            ReloadAvailableFilters();

            // this deletes any temp images that were extracted from IRO archives
            // ... the temp images are used for the configure mod window
            // ... the app does not release the file lock on the images at runtime of the app so 
            // ... this will ensure the images from last app session are deleted
            ConfigureModViewModel.DeleteTempFolder();

            // TODO: check for app updates
        }

        private void Sys_StatusChanged(object sender, ModStatusEventArgs e)
        {
            CatalogViewModel.UpdateModDetails(e.ModID);

            if (e.Status == ModStatus.Installed)
            {
                // remove newly installed mod from info cache incase it is stale or the install location changed
                InstalledItem mod = Sys.Library.GetItem(e.ModID);
                string mfile = mod.LatestInstalled.InstalledLocation;
                _infoCache.Remove(mfile);
                ModsViewModel.ReloadModList(ModsViewModel.GetSelectedMod()?.InstallInfo.ModID, SearchText, CheckedCategories, CheckedTags);
            }

            if (e.Status == ModStatus.Installed && e.OldStatus != ModStatus.Installed && Sys.Settings.Options.HasFlag(GeneralOptions.AutoActiveNewMods))
                ModsViewModel.ToggleActivateMod(e.ModID);
            if (e.OldStatus == ModStatus.Installed && e.Status == ModStatus.NotInstalled && Sys.ActiveProfile.Items.Any(i => i.ModID.Equals(e.ModID)))
                ModsViewModel.ToggleActivateMod(e.ModID);

            // update mod preview info page if the mod is currently selected
            if ((TabIndex)SelectedTabIndex == TabIndex.MyMods)
            {
                InstalledModViewModel currentlySelected = ModsViewModel.GetSelectedMod();

                if (currentlySelected?.InstallInfo.ModID == e.ModID)
                {
                    UpdateModPreviewInfo(currentlySelected);
                }
            }
            else
            {
                CatalogModItemViewModel currentlySelected = CatalogViewModel.GetSelectedMod();

                if (currentlySelected?.Mod.ID == e.ModID)
                {
                    UpdateModPreviewInfo(currentlySelected);
                }
            }
        }

        /// <summary>
        /// Used to cleanup any resources (e.g. when shutting down app).
        /// Saves profile, Unsubscribes from Events, and nullifys certain objects.
        /// </summary>
        internal void CleanUp()
        {
            SaveProfile();
            Sys.Save();

            ModsViewModel.SelectedModChanged -= ModsViewModel_SelectedModChanged;
            ModsViewModel.ModList.Clear();
            ModsViewModel.ModList = null;
            ModsViewModel = null;

            CatalogViewModel.SelectedModChanged -= CatalogViewModel_SelectedModChanged;
            CatalogViewModel.CatalogModList.Clear();
            CatalogViewModel = null;
        }

        private static void InitLoaderContext()
        {
            _context = new _7thWrapperLib.LoaderContext()
            {
                VarAliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            };

            string varFile = "7thHeaven.var";


            if (File.Exists(varFile))
            {
                foreach (string line in File.ReadAllLines(varFile))
                {
                    string[] parts = line.Split(new[] { '=' }, 2);

                    if (parts.Length == 2)
                        _context.VarAliases[parts[0]] = parts[1];
                }
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

        private void TryAutoImportMods()
        {
            if (Sys.Settings.Options.HasFlag(GeneralOptions.AutoImportMods) && Directory.Exists(Sys.Settings.LibraryLocation))
            {
                foreach (string folder in Directory.GetDirectories(Sys.Settings.LibraryLocation))
                {
                    string name = Path.GetFileName(folder);
                    if (!name.EndsWith("temp", StringComparison.InvariantCultureIgnoreCase) && !Sys.Library.PendingDelete.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                    {
                        if (!Sys.Library.Items.SelectMany(ii => ii.Versions).Any(v => v.InstalledLocation.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Log.Write("Trying to auto-import file " + folder);
                            try
                            {
                                string modName = ImportModViewModel.ParseNameFromFileOrFolder(Path.GetFileNameWithoutExtension(folder));
                                ImportModViewModel.ImportMod(folder, modName, false, true);
                            }
                            catch (Exception ex)
                            {
                                Sys.Message(new WMessage() { Text = "Mod " + name + " failed to import: " + ex.ToString() });
                                continue;
                            }
                            Sys.Message(new WMessage() { Text = "Auto imported mod " + name });
                        }
                    }
                }

                foreach (string iro in Directory.GetFiles(Sys.Settings.LibraryLocation, "*.iro"))
                {
                    string name = Path.GetFileName(iro);
                    if (!name.EndsWith("temp", StringComparison.InvariantCultureIgnoreCase) && !Sys.Library.PendingDelete.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                    {
                        if (!Sys.Library.Items.SelectMany(ii => ii.Versions).Any(v => v.InstalledLocation.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Log.Write($"Trying to auto-import file {iro}");
                            try
                            {
                                string modName = ImportModViewModel.ParseNameFromFileOrFolder(Path.GetFileNameWithoutExtension(iro));
                                ImportModViewModel.ImportMod(iro, modName, true, true);
                            }
                            catch (_7thWrapperLib.IrosArcException)
                            {
                                Sys.Message(new WMessage() { Text = $"Could not import .iro mod {Path.GetFileNameWithoutExtension(iro)}, file is corrupt" });
                                continue;
                            }

                            Sys.Message(new WMessage() { Text = $"Auto imported mod {name}" });
                        }
                    }
                }
            }

            // validate imported mod files exist - remove them if they do not exit
            foreach (InstalledItem mod in Sys.Library.Items.ToArray())
            {
                string fn = Path.Combine(Sys.Settings.LibraryLocation, mod.LatestInstalled.InstalledLocation);
                if (!File.Exists(fn) && !Directory.Exists(fn))
                {
                    Sys.Library.Items.Remove(mod);
                    Mod details = mod.CachedDetails ?? new Mod();
                    Sys.Message(new WMessage { Text = $"Could not find mod {details.Name} - has it been deleted? Removed." });
                }
            }

            Sys.Library.AttemptDeletions();
            ModsViewModel.ReloadModList(null, SearchText, CheckedCategories, CheckedTags);
        }

        public void RefreshProfile()
        {
            if (Sys.ActiveProfile == null)
            {
                return;
            }

            CurrentProfile = Sys.Settings.CurrentProfile;

            // reload list of active mods for the profile
            ModsViewModel.ReloadModList(null, SearchText, CheckedCategories, CheckedTags);
        }

        internal void SaveProfile()
        {
            if (Sys.ActiveProfile != null)
            {
                try
                {
                    using (FileStream fs = new FileStream(Sys.PathToCurrentProfileFile, FileMode.Create))
                    {
                        Util.Serialize(Sys.ActiveProfile, fs);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to save profile ...");
                }
            }
        }

        private void Sys_MessageReceived(object sender, MessageEventArgs e)
        {
            string receivedMessage = e.Message.Text;

            if (!String.IsNullOrEmpty(e.Message.Link))
            {
                receivedMessage += $" - {e.Message.Link}";
            }

            StatusMessage = receivedMessage;
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

        internal static _7thWrapperLib.ModInfo GetModInfo(InstalledItem ii)
        {
            InstalledVersion inst = ii.LatestInstalled;
            string mfile = Path.Combine(Sys.Settings.LibraryLocation, inst.InstalledLocation);

            _7thWrapperLib.ModInfo info;

            if (!_infoCache.TryGetValue(mfile, out info))
            {
                if (mfile.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (var arc = new _7thWrapperLib.IrosArc(mfile))
                        if (arc.HasFile("mod.xml"))
                        {
                            var doc = new System.Xml.XmlDocument();
                            doc.Load(arc.GetData("mod.xml"));
                            info = new _7thWrapperLib.ModInfo(doc, _context);
                        }
                }
                else
                {
                    string file = Path.Combine(mfile, "mod.xml");
                    if (File.Exists(file))
                        info = new _7thWrapperLib.ModInfo(file, _context);
                }
                _infoCache.Add(mfile, info);
            }
            return info;
        }

        internal static bool CheckAllowedActivate(Guid modID)
        {
            if (Sys.Library.CodeAllowed.Contains(modID)) return true;

            InstalledItem mod = Sys.Library.GetItem(modID);
            InstalledVersion inst = mod.LatestInstalled;
            string mfile = Path.Combine(Sys.Settings.LibraryLocation, inst.InstalledLocation);
            bool hasCode;
            var modInfo = GetModInfo(mod);

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
            }

            if (!hasCode) return true;

            string msg = "This mod ({0}) contains code/patches that could change FF7.exe. Are you sure you want to activate and run this mod?\n" +
                "Only choose YES if you trust the author of this mod to run code/programs on your computer!";
            msg = String.Format(msg, mod.CachedDetails.Name);

            if (MessageBox.Show(msg, "Allow mod to run?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return false;
            }

            Sys.Library.CodeAllowed.Add(modID);
            Sys.Save();
            return true;
        }

        internal static bool SanityCheckSettings()
        {
            List<string> changes = new List<string>();
            foreach (var constraint in GetConstraints())
            {
                if (!constraint.Verify(out string msg))
                {
                    MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (msg != null)
                {
                    changes.Add(msg);
                }
            }

            if (changes.Any())
            {
                MessageBox.Show($"The following settings have been changed to make these mods compatible:\n{String.Join("\n", changes)}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return true;
        }

        internal static List<Constraint> GetConstraints()
        {
            List<Constraint> constraints = new List<Constraint>();
            foreach (ProfileItem pItem in Sys.ActiveProfile.Items)
            {
                InstalledItem inst = Sys.Library.GetItem(pItem.ModID);
                _7thWrapperLib.ModInfo info = GetModInfo(inst);

                if (info == null)
                {
                    continue;
                }

                foreach (var cSetting in info.Compatibility.Settings)
                {
                    if (!String.IsNullOrWhiteSpace(cSetting.MyID))
                    {
                        var setting = pItem.Settings.Find(s => s.ID.Equals(cSetting.MyID, StringComparison.InvariantCultureIgnoreCase));
                        if ((setting == null) || (setting.Value != cSetting.MyValue)) continue;
                    }

                    ProfileItem oItem = Sys.ActiveProfile.Items.Find(i => i.ModID.Equals(cSetting.ModID));
                    if (oItem == null) continue;

                    InstalledItem oInst = Sys.Library.GetItem(cSetting.ModID);
                    Constraint ct = constraints.Find(c => c.ModID.Equals(cSetting.ModID) && c.Setting.Equals(cSetting.TheirID, StringComparison.InvariantCultureIgnoreCase));
                    if (ct == null)
                    {
                        ct = new Constraint() { ModID = cSetting.ModID, Setting = cSetting.TheirID };
                        constraints.Add(ct);
                    }

                    ct.ParticipatingMods.Add(inst.CachedDetails.Name);
                    if (cSetting.Require.HasValue)
                    {
                        ct.Require.Add(cSetting.Require.Value);
                    }

                    foreach (var f in cSetting.Forbid)
                    {
                        ct.Forbid.Add(f);
                    }
                }

                foreach (var setting in info.Options)
                {
                    Constraint ct = constraints.Find(c => c.ModID.Equals(pItem.ModID) && c.Setting.Equals(setting.ID, StringComparison.InvariantCultureIgnoreCase));
                    if (ct == null)
                    {
                        ct = new Constraint() { ModID = pItem.ModID, Setting = setting.ID };
                        constraints.Add(ct);
                    }
                    ct.Option = setting;
                }

            }

            return constraints;
        }

        private bool VerifyOrdering()
        {
            var details = Sys.ActiveProfile
                             .Items
                             .Select(i => Sys.Library.GetItem(i.ModID))
                             .Select(ii => new { Mod = ii, Info = GetModInfo(ii) })
                             .ToDictionary(a => a.Mod.ModID, a => a);

            List<string> problems = new List<string>();

            foreach (int i in Enumerable.Range(0, Sys.ActiveProfile.Items.Count))
            {
                ProfileItem mod = Sys.ActiveProfile.Items[i];
                var info = details[mod.ModID].Info;

                if (info == null)
                {
                    continue;
                }

                foreach (Guid after in info.OrderAfter)
                {
                    if (Sys.ActiveProfile.Items.Skip(i).Any(pi => pi.ModID.Equals(after)))
                    {
                        problems.Add($"Mod {details[mod.ModID].Mod.CachedDetails.Name} is meant to come BELOW mod {details[after].Mod.CachedDetails.Name} in the load order");
                    }
                }

                foreach (Guid before in info.OrderBefore)
                {
                    if (Sys.ActiveProfile.Items.Take(i).Any(pi => pi.ModID.Equals(before)))
                    {
                        problems.Add($"Mod {details[mod.ModID].Mod.CachedDetails.Name} is meant to come ABOVE mod {details[before].Mod.CachedDetails.Name} in the load order");
                    }
                }
            }

            if (problems.Any())
            {
                if (MessageBox.Show($"The following mods will not work properly in the current order:\n{String.Join("\n", problems)}\nDo you want to continue anyway?", "Load Order Incompatible", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return false;
            }

            return true;
        }

        private bool SanityCheckCompatibility()
        {
            List<InstalledItem> profInst = Sys.ActiveProfile.Items.Select(pi => Sys.Library.GetItem(pi.ModID)).ToList();

            foreach (InstalledItem item in profInst)
            {
                var info = GetModInfo(item);

                if (info == null)
                {
                    continue;
                }

                foreach (var req in info.Compatibility.Requires)
                {
                    var rInst = profInst.Find(i => i.ModID.Equals(req.ModID));
                    if (rInst == null)
                    {
                        MessageBox.Show(String.Format("Mod {0} requires you to activate {1} as well.", item.CachedDetails.Name, req.Description));
                        return false;
                    }
                    else if (req.Versions.Any() && !req.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version))
                    {
                        MessageBox.Show(String.Format("Mod {0} requires you to activate {1}, but you do not have a compatible version installed. Try updating it?", item.CachedDetails.Name, rInst.CachedDetails.Name));
                        return false;
                    }
                }

                foreach (var forbid in info.Compatibility.Forbids)
                {
                    var rInst = profInst.Find(i => i.ModID.Equals(forbid.ModID));
                    if (rInst == null)
                    {
                        continue; //good!
                    }

                    if (forbid.Versions.Any() && forbid.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version))
                    {
                        MessageBox.Show($"Mod {item.CachedDetails.Name} is not compatible with the version of {rInst.CachedDetails.Name} you have installed. Try updating it?");
                        return false;
                    }
                    else
                    {
                        MessageBox.Show($"Mod {item.CachedDetails.Name} is not compatible with {rInst.CachedDetails.Name}. You will need to disable it.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// getsLaunches FF7 exe 
        /// </summary>
        /// <param name="varDump"></param>
        /// <param name="debug"></param>
        public void LaunchGame(bool varDump, bool debug)
        {

            if (!SanityCheckCompatibility()) return;
            if (!SanityCheckSettings()) return;
            if (!VerifyOrdering()) return;

            string lib = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "7thWrapperLib.dll");
            if (Sys.ActiveProfile == null)
            {
                MessageBox.Show("Create a profile first");
                return;
            }

            if (!File.Exists(Sys.Settings.FF7Exe))
            {
                MessageBox.Show("FF7.exe not found. You may need to configure 7H using the Workshop/Settings menu.");
                return;
            }

            if (Sys.ActiveProfile.Items.Count == 0)
            {
                MessageBox.Show("No mods have been activated. The game will now launch as 'vanilla'");

                LaunchFF7Exe();
                return;
            }

            string ff7Folder = Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string pathToDataFolder = Path.Combine(ff7Folder, "data");

            _7thWrapperLib.RuntimeProfile runtimeProfiles = new _7thWrapperLib.RuntimeProfile()
            {
                MonitorPaths = new List<string>() {
                    pathToDataFolder,
                    Sys.Settings.AaliFolder,
                    Sys.Settings.MovieFolder,
                },
                ModPath = Sys.Settings.LibraryLocation,
                OpenGLConfig = Sys.ActiveProfile.OpenGLConfig,
                FF7Path = ff7Folder,
                gameFiles = Directory.GetFiles(ff7Folder, "*.*", SearchOption.AllDirectories),
                Mods = Sys.ActiveProfile.Items.Select(i => i.GetRuntime(_context))
                                              .Where(i => i != null)
                                              .ToList()
            };

            runtimeProfiles.MonitorPaths.AddRange(Sys.Settings.ExtraFolders.Where(s => s.Length > 0).Select(s => Path.Combine(ff7Folder, s)));


            if (varDump)
            {
                runtimeProfiles.MonitorVars = _context.VarAliases.Select(kv => new Tuple<string, string>(kv.Key, kv.Value)).ToList();

                string turboLogProcName = "TurBoLog.exe";
                ProcessStartInfo psi = new ProcessStartInfo(turboLogProcName)
                {
                    WorkingDirectory = Path.GetDirectoryName(turboLogProcName)
                };
                Process aproc = Process.Start(psi);

                _alsoLaunchProcesses.Add(turboLogProcName, aproc);
                aproc.EnableRaisingEvents = true;
                aproc.Exited += (o, e) => _alsoLaunchProcesses.Remove(turboLogProcName);
            }

            // launch other processes set in settings
            foreach (string al in Sys.Settings.AlsoLaunch.Where(s => !String.IsNullOrWhiteSpace(s)))
            {
                if (!_alsoLaunchProcesses.ContainsKey(al))
                {
                    string lal = al;
                    ProcessStartInfo psi = new ProcessStartInfo(lal)
                    {
                        WorkingDirectory = Path.GetDirectoryName(lal)
                    };
                    Process aproc = Process.Start(psi);

                    _alsoLaunchProcesses.Add(lal, aproc);
                    aproc.EnableRaisingEvents = true;
                    aproc.Exited += (_o, _e) => _alsoLaunchProcesses.Remove(lal);
                }
            }

            // copy EasyHook.dll to FF7
            string dir = Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string source = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string f1 = Path.Combine(dir, "EasyHook.dll");
            if (!File.Exists(f1))
                File.Copy(Path.Combine(source, "EasyHook.dll"), f1);

            string f2 = Path.Combine(dir, "EasyHook32.dll");
            if (!File.Exists(f2))
                File.Copy(Path.Combine(source, "EasyHook32.dll"), f2);


            // setup log file if debugging
            if (debug)
            {
                runtimeProfiles.Options |= _7thWrapperLib.RuntimeOptions.DetailedLog;
                runtimeProfiles.LogFile = Path.Combine(Sys.SysFolder, "log.txt");
            }

            int pid;
            try
            {
                _7thWrapperLib.RuntimeParams parms = new _7thWrapperLib.RuntimeParams
                {
                    ProfileFile = Path.GetTempFileName()
                };

                using (var fs = new FileStream(parms.ProfileFile, FileMode.Create))
                    Util.SerializeBinary(runtimeProfiles, fs);

                // Add 640x480 and High DPI compatibility flags if set in settings
                if (Sys.Settings.Options.HasFlag(GeneralOptions.SetEXECompatFlags))
                {
                    RegistryKey ff7CompatKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                    ff7CompatKey?.SetValue(Sys.Settings.FF7Exe, "~ 640X480 HIGHDPIAWARE");
                }

                EasyHook.RemoteHooking.CreateAndInject(Sys.Settings.FF7Exe, String.Empty, 0, lib, null, out pid, parms);

                var proc = Process.GetProcessById(pid);
                if (proc != null)
                {
                    proc.EnableRaisingEvents = true;
                    if (debug)
                    {
                        proc.Exited += (o, e) =>
                        {
                            Process.Start(runtimeProfiles.LogFile);
                        };
                    }
                }

                /// load plugins for mods
                foreach (var mod in runtimeProfiles.Mods)
                {
                    if (mod.LoadPlugins.Any())
                    {
                        mod.Startup();
                        foreach (string dll in mod.GetLoadPlugins())
                        {
                            _7HPlugin plugin;
                            if (!_plugins.TryGetValue(dll, out plugin))
                            {
                                System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom(dll);

                                plugin = asm.GetType("_7thHeaven.Plugin")
                                            .GetConstructor(Type.EmptyTypes)
                                            .Invoke(null) as _7HPlugin;
                                _plugins.Add(dll, plugin);
                            }
                            plugin.Start(mod);
                        }
                    }
                }

                // wire up process to stop plugins when proc has exited
                proc.Exited += (o, e) =>
                {
                    foreach (var plugin in _plugins.Values)
                        plugin.Stop();
                };
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageBox.Show(e.ToString(), "Error starting FF7");

                return;
            }
        }

        /// <summary>
        /// Launches FF7.exe without loading any mods.
        /// </summary>
        internal static void LaunchFF7Exe()
        {
            // remove the flag for 640x480 when playing vanilla since Easy Hook is not being used
            if (Sys.Settings.Options.HasFlag(GeneralOptions.SetEXECompatFlags))
            {
                RegistryKey ff7CompatKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);

                try
                {
                    ff7CompatKey?.DeleteValue(Sys.Settings.FF7Exe);
                }
                catch (Exception e)
                {
                    // will fail if already deleted
                }
            }

            try
            {
                Process.Start(Sys.Settings.FF7Exe);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Sys.Message(new WMessage("Failed to start FF7 process..."));
            }
        }

        internal void DoSearch()
        {
            if ((TabIndex)SelectedTabIndex == TabIndex.BrowseCatalog)
            {
                CatalogViewModel.ClearRememberedSearchTextAndCategories();
                CatalogViewModel.ReloadModList(CatalogViewModel.GetSelectedMod()?.Mod?.ID, SearchText, CheckedCategories, CheckedTags);
            }
            else
            {
                ModsViewModel.ClearRememberedSearchTextAndCategories();
                ModsViewModel.ReloadModList(ModsViewModel.GetSelectedMod()?.InstallInfo?.ModID, SearchText, CheckedCategories, CheckedTags);
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
        /// Opens a window to input a new profile name.
        /// if new profile is created, then current profile is saved before switching to new profile.
        /// </summary>
        internal void CreateNewProfile()
        {
            string profileName = OpenProfileViewModel.InputNewProfileName();

            if (profileName == null)
            {
                return; // user canceled inputting a profile name
            }

            SaveProfile();
            Sys.ActiveProfile = new Profile();
            Sys.Settings.CurrentProfile = profileName;
            RefreshProfile();
            SaveProfile();

            Sys.Message(new WMessage() { Text = $"Successfully created new profile {profileName}!" });
        }

        internal void ShowOpenProfileWindow()
        {
            OpenProfileWindow profileWindow = new OpenProfileWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            bool? dialogResult = profileWindow.ShowDialog();

            if (!dialogResult.GetValueOrDefault(false))
            {
                // user did not click OK so don't switch profiles
                return;
            }

            if (!string.IsNullOrWhiteSpace(profileWindow.ViewModel.SelectedProfile))
            {
                SaveProfile(); // save current profile before switching

                Sys.Settings.CurrentProfile = profileWindow.ViewModel.SelectedProfile;
                Sys.ActiveProfile = Util.Deserialize<Profile>(Sys.PathToCurrentProfileFile);

                RefreshProfile();
                Sys.Message(new WMessage($"Loaded profile {CurrentProfile}"));
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
            settingsWindow.ShowDialog();
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
                categories = Sys.Catalog.Mods.Where(c => !string.IsNullOrEmpty(c.Category) || !string.IsNullOrEmpty(c.LatestVersion.Category))
                                             .Select(c => c.Category ?? c.LatestVersion.Category)
                                             .Distinct()
                                             .ToList();
            }
            else
            {
                categories = Sys.Library.Items.Where(c => !string.IsNullOrEmpty(c.CachedDetails.Category) || !string.IsNullOrEmpty(c.CachedDetails.LatestVersion.Category))
                                              .Select(c => c.CachedDetails.Category ?? c.CachedDetails.LatestVersion.Category)
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
            if ((TabIndex)SelectedTabIndex == TabIndex.BrowseCatalog)
            {
                CatalogViewModel.ReloadModList(CatalogViewModel.GetSelectedMod()?.Mod?.ID, SearchText, CheckedCategories, CheckedTags);
            }
            else
            {
                ModsViewModel.ReloadModList(ModsViewModel.GetSelectedMod()?.InstallInfo?.ModID, SearchText, CheckedCategories, CheckedTags);
            }

            ReloadAvailableFilters();
        }

        internal void ReloadAvailableFilters()
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
            List<FilterItemViewModel> oldItems = AvailableFilters.ToList();

            foreach (FilterItemViewModel item in newList)
            {
                if (oldItems.Any(c => c.Name == item.Name && c.IsChecked && c.FilterType != FilterItemType.Separator))
                {
                    item.IsChecked = true;
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
    }

    internal class CatCheckOptions
    {
        public bool ForceCheck { get; set; }
    }

}
