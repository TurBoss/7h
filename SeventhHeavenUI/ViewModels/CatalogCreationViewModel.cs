using _7thHeaven.Code;
using _7thWrapperLib;
using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeavenUI.ViewModels
{

    public class CatalogCreationViewModel : ViewModelBase
    {
        private string _nameInput;
        private string _idInput;
        private string _authorInput;
        private string _descriptionInput;
        private string _categoryInput;
        private List<string> _categoryList;
        private string _versionInput;
        private string _previewImageInput;
        private string _infoLinkInput;
        private string _releaseNotesInput;
        private string _modOutput;
        private string _releaseDateInput;
        private ObservableCollection<CatalogModItemViewModel> _catalogModList;
        private CatalogModItemViewModel _selectedMod;

        private Mod _modToEdit;
        private ObservableCollection<DownloadLinkViewModel> _downloadLinkList;
        private string _tagsInput;

        public string IDInput
        {
            get
            {
                return _idInput;
            }
            set
            {
                _idInput = value;
                NotifyPropertyChanged();
            }
        }

        public string NameInput
        {
            get
            {
                return _nameInput;
            }
            set
            {
                _nameInput = value;
                NotifyPropertyChanged();
            }
        }

        public string AuthorInput
        {
            get
            {
                return _authorInput;
            }
            set
            {
                _authorInput = value;
                NotifyPropertyChanged();
            }
        }

        public string DescriptionInput
        {
            get
            {
                return _descriptionInput;
            }
            set
            {
                _descriptionInput = value;
                NotifyPropertyChanged();
            }
        }

        public string CategoryInput
        {
            get
            {
                return _categoryInput;
            }
            set
            {
                _categoryInput = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> CategoryList
        {
            get
            {
                if (_categoryList == null)
                {
                    _categoryList = ModLoadOrder.Orders.Keys.OrderBy(s => s).ToList();
                }

                return _categoryList;
            }
        }

        public string VersionInput
        {
            get
            {
                return _versionInput;
            }
            set
            {
                _versionInput = value;
                NotifyPropertyChanged();
            }
        }

        public string PreviewImageInput
        {
            get
            {
                return _previewImageInput;
            }
            set
            {
                _previewImageInput = value;
                NotifyPropertyChanged();
            }
        }

        public string InfoLinkInput
        {
            get
            {
                return _infoLinkInput;
            }
            set
            {
                _infoLinkInput = value;
                NotifyPropertyChanged();
            }
        }

        public string ReleaseNotesInput
        {
            get
            {
                return _releaseNotesInput;
            }
            set
            {
                _releaseNotesInput = value;
                NotifyPropertyChanged();
            }
        }

        public string ReleaseDateInput
        {
            get
            {
                return _releaseDateInput;
            }
            set
            {
                _releaseDateInput = value;
                NotifyPropertyChanged();
            }
        }

        public string TagsInput
        {
            get
            {
                return _tagsInput;
            }
            set
            {
                _tagsInput = value;
                NotifyPropertyChanged();
            }
        }


        public ObservableCollection<DownloadLinkViewModel> DownloadLinkList
        {
            get
            {
                if (_downloadLinkList == null)
                {
                    _downloadLinkList = new ObservableCollection<DownloadLinkViewModel>();
                }

                return _downloadLinkList;
            }
            set
            {
                _downloadLinkList = value;
                NotifyPropertyChanged();
            }
        }

        public string CatalogOutput
        {
            get
            {
                return _modOutput;
            }
            set
            {
                _modOutput = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<CatalogModItemViewModel> CatalogModList
        {
            get
            {
                return _catalogModList;
            }
            set
            {
                _catalogModList = value;
                NotifyPropertyChanged();
            }
        }

        public CatalogModItemViewModel SelectedMod
        {
            get
            {
                return _selectedMod;
            }
            set
            {
                if (_modToEdit != null && _selectedMod != null)
                {
                    // save back to selected mod before switching
                    _selectedMod.Mod.Name = NameInput;
                    _selectedMod.Name = NameInput;

                    _selectedMod.Mod.Author = AuthorInput;
                    _selectedMod.Author = AuthorInput;

                    _selectedMod.Mod.Category = CategoryInput;
                    _selectedMod.Category = CategoryInput;

                    _selectedMod.Mod.Tags = TagsInput.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    _selectedMod.Mod.Description = DescriptionInput;
                    _selectedMod.Mod.Link = InfoLinkInput;

                    if (Guid.TryParse(IDInput, out Guid parsedID))
                    {
                        _selectedMod.Mod.ID = parsedID;
                    }

                    DateTime.TryParse(ReleaseDateInput, out DateTime parsedDate);
                    decimal.TryParse(VersionInput, System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.CultureInfo(""), out decimal parsedVersion);

                    _selectedMod.Mod.LatestVersion = new ModVersion()
                    {
                        Links = DownloadLinkList.Where(d => !string.IsNullOrEmpty(d.SourceLinkInput)).Select(s => s.GetFormattedLink()).ToList(),
                        ReleaseDate = parsedDate,
                        Version = parsedVersion,
                        PreviewImage = PreviewImageInput,
                        ReleaseNotes = ReleaseNotesInput,
                        CompatibleGameVersions = GameVersions.All
                    };

                    _selectedMod.ReleaseDate = parsedDate.ToString("MM/dd/yyyy");
                }

                _selectedMod = value;

                if (_selectedMod != null)
                {
                    _modToEdit = _selectedMod.Mod;

                    NameInput = _modToEdit.Name;
                    AuthorInput = _modToEdit.Author;
                    IDInput = _modToEdit.ID.ToString();
                    CategoryInput = _modToEdit.Category;
                    TagsInput = string.Join("\n", _modToEdit.Tags);
                    DescriptionInput = _modToEdit.Description;
                    InfoLinkInput = _modToEdit.Link;
                    ReleaseNotesInput = _modToEdit.LatestVersion.ReleaseNotes;
                    PreviewImageInput = _modToEdit.LatestVersion.PreviewImage;
                    VersionInput = _modToEdit.LatestVersion.Version.ToString();
                    ReleaseDateInput = _modToEdit.LatestVersion.ReleaseDate.ToString("MM/dd/yyyy");

                    DownloadLinkList.Clear();
                    foreach (string link in _modToEdit.LatestVersion.Links)
                    {
                        if (LocationUtil.TryParse(link, out LocationType linkKind, out string url))
                        {
                            DownloadLinkList.Add(new DownloadLinkViewModel(linkKind.ToString(), url));
                        }
                    }

                }

                NotifyPropertyChanged();
            }
        }

        public CatalogCreationViewModel()
        {
            CatalogModList = new ObservableCollection<CatalogModItemViewModel>();
            VersionInput = "1.00";
            TagsInput = "";
        }

        public string GenerateCatalogOutput()
        {
            Catalog generated = new Catalog()
            {
                Mods = CatalogModList.Select(m => m.Mod).ToList(),
                Name = ""
            };

            return Util.Serialize(generated).Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        }

        internal void LoadModXml(string pathToModXml)
        {
            try
            {
                ModInfo mod = new ModInfo(pathToModXml, Sys._context);

                IDInput = mod.ID.ToString();
                NameInput = mod.Name;
                CategoryInput = mod.Category;
                AuthorInput = mod.Author;
                DescriptionInput = mod.Description;
                VersionInput = mod.Version.ToString();
                PreviewImageInput = "";
                InfoLinkInput = mod.Link;
                ReleaseNotesInput = mod.ReleaseNotes;
                ReleaseDateInput = mod.ReleaseDate.ToString("MM/dd/yyyy");
                DownloadLinkList.Clear();
                _modToEdit = null;
            }
            catch (Exception e)
            {
                Sys.Message(new WMessage($"Could not load mod xml: {e.Message}", true)
                {
                    LoggedException = e
                });
            }
        }

        internal void LoadModXmlFromIro(string pathToIroFile)
        {
            try
            {
                Mod parsedMod = new ModImporter().ParseModXmlFromSource(pathToIroFile);

                if (parsedMod == null)
                {
                    Sys.Message(new WMessage($"Could not load mod xml from .iro file", true));
                    return;
                }

                IDInput = parsedMod.ID.ToString();
                NameInput = parsedMod.Name;
                CategoryInput = parsedMod.Category;
                AuthorInput = parsedMod.Author;
                DescriptionInput = parsedMod.Description;
                VersionInput = parsedMod.LatestVersion.Version.ToString();
                PreviewImageInput = "";
                InfoLinkInput = parsedMod.Link;
                ReleaseNotesInput = parsedMod.LatestVersion.ReleaseNotes;
                ReleaseDateInput = parsedMod.LatestVersion.ReleaseDate.ToString("MM/dd/yyyy");
                DownloadLinkList.Clear();
                _modToEdit = null;
            }
            catch (Exception e)
            {
                Sys.Message(new WMessage($"Could not load mod xml from .iro: {e.Message}", true)
                {
                    LoggedException = e
                });
            }
        }

        internal void SaveCatalogXml(string pathToFile)
        {
            try
            {
                File.WriteAllText(pathToFile, GenerateCatalogOutput());
                Sys.Message(new WMessage($"Successfully saved to {pathToFile}", true));
            }
            catch (Exception e)
            {
                Sys.Message(new WMessage($"Could not save catalog xml: {e.Message}", true)
                {
                    LoggedException = e
                });
            }
        }

        internal void LoadCatalogXml(string pathToFile)
        {
            try
            {
                Catalog catalog = Util.Deserialize<Catalog>(pathToFile);
                CatalogModList = new ObservableCollection<CatalogModItemViewModel>(catalog.Mods.Select(m => new CatalogModItemViewModel(m)));
                SelectedMod = null;
                ClearInputFields();
                CatalogOutput = "";
            }
            catch (Exception e)
            {
                Sys.Message(new WMessage($"Could not load catalog xml: {e.Message}", true)
                {
                    LoggedException = e
                });
            }
        }

        private void ClearInputFields()
        {
            IDInput = Guid.NewGuid().ToString();
            NameInput = "";
            CategoryInput = "Unknown";
            AuthorInput = "";
            DescriptionInput = "";
            VersionInput = "1.0";
            PreviewImageInput = "";
            InfoLinkInput = "";
            ReleaseNotesInput = "";
            ReleaseDateInput = DateTime.Now.ToString("MM/dd/yyyy");
            DownloadLinkList.Clear();
        }

        internal void AddModToList()
        {
            if (!Guid.TryParse(IDInput, out Guid parsedID))
            {
                return;
            }

            if (CatalogModList.Any(c => c.Mod.ID == parsedID))
            {
                return;
            }

            if (string.IsNullOrEmpty(VersionInput))
            {
                VersionInput = "1.00";
            }

            DateTime.TryParse(ReleaseDateInput, out DateTime parsedDate);
            decimal.TryParse(VersionInput, System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.CultureInfo(""), out decimal parsedVersion);

            Mod newMod = new Mod()
            {
                ID = parsedID,
                Name = NameInput,
                Author = AuthorInput,
                Category = CategoryInput,
                Tags = TagsInput.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                Description = DescriptionInput,
                Link = InfoLinkInput,
                LatestVersion = new ModVersion()
                {
                    Links = DownloadLinkList.Where(d => !string.IsNullOrEmpty(d.SourceLinkInput)).Select(d => d.GetFormattedLink()).ToList(),
                    ReleaseDate = parsedDate,
                    Version = parsedVersion,
                    CompatibleGameVersions = GameVersions.All,
                    PreviewImage = PreviewImageInput
                }
            };

            CatalogModList.Add(new CatalogModItemViewModel(newMod));

            _modToEdit = null;
            SelectedMod = CatalogModList[0];
        }

        internal void AddEmptyDownloadLink()
        {
            if (DownloadLinkList.Any(d => string.IsNullOrEmpty(d.SourceLinkInput)))
            {
                return; // don't add a new link since there is an empty one in the list that can be filled out
            }

            DownloadLinkList.Add(new DownloadLinkViewModel(LocationType.GDrive.ToString(), ""));
        }

    }
}
