using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeavenUI;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.ViewModels
{


    public class ImportModViewModel : ViewModelBase
    {
        enum ImportTabIndex
        {
            FromIro,
            FromFolder,
            BatchImport
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _pathToIroArchiveInput;
        private string _pathToModFolderInput;
        private string _pathToBatchFolderInput;
        private string _helpText;
        private string _modNameInput;
        private int _selectedTabIndex;
        private bool _isImporting;

        public string PathToIroArchiveInput
        {
            get
            {
                return _pathToIroArchiveInput;
            }
            set
            {
                _pathToIroArchiveInput = value;
                NotifyPropertyChanged();
            }
        }

        public string PathToModFolderInput
        {
            get
            {
                return _pathToModFolderInput;
            }
            set
            {
                _pathToModFolderInput = value;
                NotifyPropertyChanged();
            }
        }

        public string PathToBatchFolderInput
        {
            get
            {
                return _pathToBatchFolderInput;
            }
            set
            {
                _pathToBatchFolderInput = value;
                NotifyPropertyChanged();
            }
        }

        public string HelpText
        {
            get
            {
                return _helpText;
            }
            set
            {
                _helpText = value;
                NotifyPropertyChanged();
            }
        }

        public string ModNameInput
        {
            get
            {
                return _modNameInput;
            }
            set
            {
                _modNameInput = value;
                NotifyPropertyChanged();
            }
        }

        public bool ModNameInputIsEnabled
        {
            get
            {
                return (ImportTabIndex)SelectedTabIndex != ImportTabIndex.BatchImport;
            }
        }

        public bool IsImporting
        {
            get
            {
                return _isImporting;
            }
            set
            {
                _isImporting = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotImporting));
            }
        }

        public bool IsNotImporting
        {
            get
            {
                return !_isImporting;
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
                _selectedTabIndex = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ModNameInputIsEnabled));
                UpdateHelpText();
            }
        }

        public ImportModViewModel()
        {
            SelectedTabIndex = 0;
            ModNameInput = "";
            UpdateHelpText();
        }

        private void UpdateHelpText()
        {
            if ((ImportTabIndex)SelectedTabIndex == ImportTabIndex.FromIro)
            {
                HelpText = "Select a .iro archive file to import into the library folder.";
            }
            else if ((ImportTabIndex)SelectedTabIndex == ImportTabIndex.FromFolder)
            {
                HelpText = "Select a folder that contain mod files. The mod file(s) will be copied into the library folder";
            }
            else
            {
                HelpText = "Select a folder that contains .iro mod files and mod folders. All mod files found will be copied into the library folder.";
            }
        }

        public static void ImportMod(string source, string name, bool iroMode, bool noCopy)
        {
            Mod m = new Mod()
            {
                Author = String.Empty,
                Description = "Imported mod",
                Category = "Unknown",
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
                m.Name = doc.SelectSingleNode("/ModInfo/Name").NodeTextS();

                if (string.IsNullOrWhiteSpace(m.Name))
                {
                    m.Name = name;
                }

                m.Author = doc.SelectSingleNode("/ModInfo/Author").NodeTextS();
                m.Link = doc.SelectSingleNode("/ModInfo/Link").NodeTextS();
                m.Description = doc.SelectSingleNode("/ModInfo/Description").NodeTextS();
                m.Category = doc.SelectSingleNode("/ModInfo/Category").NodeTextS();
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

        public void ImportModFromWindow()
        {
            IsImporting = true;
            Sys.Message(new WMessage("Importing mod(s)... Please wait ..."));

            Task t = Task.Factory.StartNew(() =>
            {
                switch ((ImportTabIndex)SelectedTabIndex)
                {
                    case ImportTabIndex.FromIro:
                        TryImportFromIroArchive();
                        break;
                    case ImportTabIndex.FromFolder:
                        TryImportFromFolder();
                        break;
                    case ImportTabIndex.BatchImport:
                        TryBatchImport();
                        break;
                }

                IsImporting = false;
            });

            t.ContinueWith((taskResult) => 
            {
                if (taskResult.IsFaulted)
                {
                    Logger.Error(taskResult.Exception);
                }
            });
        }

        private void TryImportFromIroArchive()
        {
            if (string.IsNullOrWhiteSpace(PathToIroArchiveInput))
            {
                MessageBox.Show("Enter a path to a .iro archive file", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(ModNameInput))
            {
                MessageBox.Show("Enter a name for the mod.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(PathToIroArchiveInput))
            {
                MessageBox.Show(".iro archive file does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                ImportMod(PathToIroArchiveInput, ModNameInput, true, false);
                Sys.Message(new WMessage($"Successfully imported {ModNameInput}!"));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageBox.Show("Failed to import mod. The error has been logged", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TryImportFromFolder()
        {
            if (string.IsNullOrWhiteSpace(PathToModFolderInput))
            {
                MessageBox.Show("Enter a path to a folder containing mod files.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(ModNameInput))
            {
                MessageBox.Show("Enter a name for the mod", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(PathToModFolderInput))
            {
                MessageBox.Show("Directory does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                ImportMod(PathToModFolderInput, ModNameInput, false, false);
                Sys.Message(new WMessage($"Successfully imported {ModNameInput}!"));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageBox.Show("Failed to import mod. The error has been logged", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void TryBatchImport()
        {
            if (string.IsNullOrWhiteSpace(PathToBatchFolderInput))
            {
                MessageBox.Show("Enter a path to a folder containing .iro mod files and/or mod folders", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(PathToBatchFolderInput))
            {
                MessageBox.Show("Directory does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int modImportCount = 0;

                foreach (string iro in Directory.GetFiles(PathToBatchFolderInput, "*.iro"))
                {
                    ImportMod(iro, Path.GetFileNameWithoutExtension(iro), true, false);
                    modImportCount++;
                }

                foreach (string dir in Directory.GetDirectories(PathToBatchFolderInput))
                {
                    ImportMod(dir, Path.GetFileNameWithoutExtension(dir), false, false);
                    modImportCount++;
                }

                Sys.Message(new WMessage($"Successfully imported {modImportCount} mod(s)!"));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageBox.Show("Failed to import mod(s). The error has been logged", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Removes guid from passed in string and replaces underscores '_' with spaces if guid is found in string.
        /// </summary>
        /// <param name="name"> name of mod with possible Guid in string </param>
        /// <returns> if Guid found then returns parsed string; other wise returns <paramref name="name"/></returns>
        internal static string ParseNameFromFileOrFolder(string name)
        {
            string parsedName = name;

            Regex regex = new Regex(@"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}");
            Match match = regex.Match(name);

            if (match.Success)
            {
                int index = name.IndexOf(match.Value) + match.Length;
                parsedName = name.Substring(index);
                parsedName = parsedName.Replace("_", " ").Trim();
            }

            return parsedName;
        }
    }
}
