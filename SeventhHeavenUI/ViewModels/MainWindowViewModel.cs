using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SeventhHeavenUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Data Members And Properties

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _catFile;
        private string _statusMessage;
        private string _currentProfile;
        private _7thWrapperLib.LoaderContext _context;

        private string _previewModAuthor;
        private string _previewModName;
        private string _previewModDescription;
        private string _previewModLink;
        private Uri _previewModImageSource;

        public MyModsViewModel ModsViewModel { get; set; }

        public string WindowTitle
        {
            get
            {
                return $"{App.GetAppName()} v{App.GetAppVersion().ToString()} - Ultimate Mod Manager for Final Fantasy 7 [{CurrentProfile}]";
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

        public Uri PreviewModImageSource
        {
            get
            {
                return _previewModImageSource;
            }
            set
            {
                _previewModImageSource = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public MainWindowViewModel()
        {
            CurrentProfile = "Default Profile";
            StatusMessage = "Welcome to 7th Heaven ...";

            ModsViewModel = new MyModsViewModel();
            ModsViewModel.SelectedModChanged += ModsViewModel_SelectedModChanged;
        }

        private void ModsViewModel_SelectedModChanged(object sender, InstalledModViewModel selected)
        {
            UpdateModPreviewInfo(selected);
        }

        private void UpdateModPreviewInfo(InstalledModViewModel selected)
        {
            PreviewModAuthor = selected.Author;
            PreviewModName = selected.Name;
            PreviewModDescription = selected.InstallInfo.CachedDetails.Description;
            PreviewModLink = selected.InstallInfo.CachedDetails.Link;
            PreviewModImageSource = new Uri(Sys.ImageCache.GetImagePath(selected.InstallInfo.CachedDetails.LatestVersion.PreviewImage, selected.InstallInfo.CachedDetails.ID));
        }

        public void InitViewModel()
        {
            StatusMessage = $"{App.GetAppName()} started: app v{App.GetAppVersion().ToString()} - dll v{Sys.Version.ToString()}";

            MegaIros.Logger = Logger.Info;

            CopyDllAndUpdaterExe();

            LoadCatalogXmlFile();

            InitLoaderContext();

            Sys.MessageReceived += Sys_MessageReceived;

            InitActiveProfile();
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
        }

        private void InitLoaderContext()
        {
            _context = new _7thWrapperLib.LoaderContext()
            {
                VarAliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            };

            string varFile = Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location, ".var");

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

            if (!String.IsNullOrWhiteSpace(Sys.Settings.CurrentProfile) && File.Exists(Sys.ProfileFile))
            {
                try
                {
                    Sys.ActiveProfile = Util.Deserialize<Profile>(Sys.ProfileFile);
                    RefreshProfile();
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Failed to load current profile xml ... Setting current profile to null");
                    Sys.Settings.CurrentProfile = null;
                }
            }

            TryAutoImportMods();

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
                                ImportMod(folder, Path.GetFileNameWithoutExtension(folder), false, true);
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
                                ImportMod(iro, Path.GetFileNameWithoutExtension(iro), true, true);
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
        }

        public void RefreshProfile()
        {
            if (Sys.ActiveProfile == null)
            {
                return;
            }

            CurrentProfile = Sys.Settings.CurrentProfile;

            // reload list of active mods for the profile
            ModsViewModel.ReloadModList();
        }

        internal void SaveProfile()
        {
            if (Sys.ActiveProfile != null)
            {
                try
                {
                    using (FileStream fs = new FileStream(Sys.ProfileFile, System.IO.FileMode.Create))
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
                Sys.Catalog = Util.Deserialize<Catalog>(_catFile);
            }
            catch (Exception e)
            {
                Logger.Warn(e, "failed to load catalog.xml - initializing empty catalog ...");
                Sys.Catalog = new Catalog();
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

        public static void ImportMod(string source, string name, bool iroMode, bool noCopy)
        {
            // TODO: move to Mod Import Window
            Mod m = new Mod()
            {
                Author = String.Empty,
                Description = "Imported mod",
                ID = Guid.NewGuid(),
                Link = String.Empty,
                Tags = new List<string>(),
                Name = name,
                LatestVersion = new ModVersion()
                {
                    CompatibleGameVersions = GameVersions.All,
                    Links = new List<string>(),
                    PreviewImage = String.Empty,
                    ReleaseDate = DateTime.Now,
                    ReleaseNotes = String.Empty,
                    Version = 1.00m,
                }
            };

            string location;
            if (noCopy)
                location = Path.GetFileName(source);
            else
                location = String.Format("{0}_{1}", m.ID, name);

            System.Xml.XmlDocument doc = null;
            Func<string, byte[]> getData = null;
            if (!iroMode)
            {
                if (!noCopy)
                {
                    foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                    {
                        string part = file.Substring(source.Length).Trim('\\', '/');
                        string dest = Path.Combine(Sys.Settings.LibraryLocation, location, part);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        File.Copy(file, dest, true);
                    }
                }
                string mx = Path.Combine(Sys.Settings.LibraryLocation, location, "mod.xml");

                if (File.Exists(mx))
                {
                    doc = new System.Xml.XmlDocument();
                    doc.Load(mx);
                }

                getData = s =>
                {
                    string file = Path.Combine(Sys.Settings.LibraryLocation, location, s);
                    if (File.Exists(file)) return File.ReadAllBytes(file);
                    return null;
                };
            }
            else
            {
                if (!noCopy)
                {
                    location += ".iro";
                    File.Copy(source, Path.Combine(Sys.Settings.LibraryLocation, location), true);
                }
                var arc = new _7thWrapperLib.IrosArc(source);
                if (arc.HasFile("mod.xml"))
                {
                    doc = new System.Xml.XmlDocument();
                    doc.Load(arc.GetData("mod.xml"));
                }
                getData = s => arc.HasFile(s) ? arc.GetBytes(s) : null;
            }

            if (doc != null)
            {
                m.Author = doc.SelectSingleNode("/ModInfo/Author").NodeTextS();
                m.Link = doc.SelectSingleNode("/ModInfo/Link").NodeTextS();
                m.Description = doc.SelectSingleNode("/ModInfo/Description").NodeTextS();
                decimal ver;
                if (decimal.TryParse(doc.SelectSingleNode("/ModInfo/Version").NodeTextS().Replace(',', '.'), out ver)) m.LatestVersion.Version = ver;
                var pv = doc.SelectSingleNode("/ModInfo/PreviewFile");
                if (pv != null)
                {
                    byte[] data = getData(pv.InnerText);
                    if (data != null)
                    {
                        string url = "iros://Preview/Auto/" + m.ID.ToString();
                        m.LatestVersion.PreviewImage = url;
                        Sys.ImageCache.InsertManual(url, data);
                    }
                }
            }

            Sys.Library.AddInstall(new InstalledItem()
            {
                CachedDetails = m,
                CachePreview = String.Empty,
                ModID = m.ID,
                UpdateType = UpdateType.Ignore,
                Versions = new List<InstalledVersion>() { new InstalledVersion() { VersionDetails = m.LatestVersion, InstalledLocation = location } },
            });
        }

    }
}
