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
        private List<string> _linkKindList;
        private string _linkKindInput;
        private string _sourceLinkInput;
        private string _modOutput;
        private string _releaseDateInput;
        private ObservableCollection<CatalogModItemViewModel> _catalogModList;
        private CatalogModItemViewModel _selectedMod;

        private Mod _modToEdit;

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

        public List<string> LinkKindList
        {
            get
            {
                if (_linkKindList == null)
                {
                    _linkKindList = new List<string>()
                    {
                        LocationType.GDrive.ToString(),
                        LocationType.MegaSharedFolder.ToString(),
                        LocationType.Url.ToString(),
                    };
                }

                return _linkKindList;
            }
        }

        public string LinkKindInput
        {
            get
            {
                return _linkKindInput;
            }
            set
            {
                _linkKindInput = value;
                NotifyPropertyChanged();
            }
        }

        public string SourceLinkInput
        {
            get
            {
                return _sourceLinkInput;
            }
            set
            {
                _sourceLinkInput = value;
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
                        Links = new List<string>() { $"iros://{LinkKindInput}/{SourceLinkInput?.Replace("://", "$")}" },
                        ReleaseDate = parsedDate,
                        Version = parsedVersion,
                        PreviewImage = PreviewImageInput,
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
                    DescriptionInput = _modToEdit.Description;
                    InfoLinkInput = _modToEdit.Link;
                    PreviewImageInput = _modToEdit.LatestVersion.PreviewImage;
                    VersionInput = _modToEdit.LatestVersion.Version.ToString();
                    ReleaseDateInput = _modToEdit.LatestVersion.ReleaseDate.ToString("MM/dd/yyyy");
                
                    if (LocationUtil.TryParse(_modToEdit.LatestVersion.Links[0], out LocationType linkKind, out string url))
                    {
                        LinkKindInput = linkKind.ToString();
                        SourceLinkInput = url;
                    }
                }

                NotifyPropertyChanged();
            }
        }

        public CatalogCreationViewModel()
        {
            CatalogModList = new ObservableCollection<CatalogModItemViewModel>();
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

        internal void LoadModXml(string pathToFile)
        {
            try
            {
                ModInfo mod = new ModInfo(pathToFile, Sys._context);

                IDInput = mod.ID.ToString();
                NameInput = mod.Name;
                CategoryInput = mod.Category;
                AuthorInput = mod.Author;
                DescriptionInput = mod.Description;
                VersionInput = mod.Version.ToString();
                PreviewImageInput = mod.PreviewFile;
                InfoLinkInput = mod.Link;
                ReleaseNotesInput = mod.ReleaseNotes;
                ReleaseDateInput = mod.ReleaseDate.ToString("MM/dd/yyyy");
            }
            catch (Exception e)
            {
                Sys.Message(new WMessage($"Could not load mod xml: {e.Message}", true)
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
            LinkKindInput = "GDrive";
            ReleaseNotesInput = "";
            ReleaseDateInput = DateTime.Now.ToString("MM/dd/yyyy");
            SourceLinkInput = "";
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

            DateTime.TryParse(ReleaseDateInput, out DateTime parsedDate);
            decimal.TryParse(VersionInput, System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.CultureInfo(""), out decimal parsedVersion);


            Mod newMod = new Mod()
            {
                ID = parsedID,
                Name = NameInput,
                Author = AuthorInput,
                Category = CategoryInput,
                Description = DescriptionInput,
                Link = InfoLinkInput,
                LatestVersion = new ModVersion()
                {
                    Links = new List<string>() { $"iros://{LinkKindInput}/{SourceLinkInput?.Replace("://", "$")}" },
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

    }
}
