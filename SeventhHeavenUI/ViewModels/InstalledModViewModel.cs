using _7thHeaven.Code;
using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeavenUI.ViewModels
{
    /// <summary>
    /// Represents a list item in the <see cref="MyModsViewModel"/> used to display
    /// installed and active mods in the 'My Mods' tab.
    /// </summary>
    public class InstalledModViewModel : ViewModelBase
    {
        public const int defaultSortOrder = 9999;

        private string _name;
        private string _author;
        private string _version;
        private string _category;
        private bool _isSelected;
        private ProfileItem _activeModInfo;
        private InstalledItem _installInfo;
        private string _releaseDate;
        private int _sortOrder;
        private List<string> _categoryList;
        private Thickness _borderThickness;

        public delegate void OnActivationChanged(object sender, InstalledModViewModel selected);
        public event OnActivationChanged ActivationChanged;


        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                _author = value;
                NotifyPropertyChanged();
            }
        }

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                NotifyPropertyChanged();
            }
        }

        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
                UpdateCategoryInLibrary();
                NotifyPropertyChanged();
            }
        }

        private void UpdateCategoryInLibrary()
        {
            var installedMod = Sys.Library.GetItem(InstallInfo.CachedDetails.ID);

            if (installedMod != null)
            {
                installedMod.CachedDetails.Category = Category;
            }
        }

        public List<string> CategoryList
        {
            get
            {
                if (_categoryList == null)
                    _categoryList = ModLoadOrder.Orders.Keys.OrderBy(s => s).ToList();

                return _categoryList;
            }
            set
            {
                _categoryList = value;
                NotifyPropertyChanged();
            }
        }


        public string ReleaseDate
        {
            get
            {
                return _releaseDate;
            }
            set
            {
                _releaseDate = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Returns info about the active mod and its setting 
        /// when the mod is active for the current profile.
        /// Will return null if installed mod is not active
        /// </summary>
        public ProfileItem ActiveModInfo
        {
            get
            {
                return _activeModInfo;
            }
            set
            {
                _activeModInfo = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsActive));
            }
        }

        public InstalledItem InstallInfo
        {
            get
            {
                return _installInfo;
            }
            set
            {
                _installInfo = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsActive
        {
            get
            {
                return ActiveModInfo != null && ActiveModInfo.IsModActive;
            }
            set
            {
                ActivationChanged?.Invoke(this, this);
            }
        }

        public int SortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                _sortOrder = value;
                NotifyPropertyChanged();
            }
        }

        public Thickness BorderThickness
        {
            get
            {
                return _borderThickness;
            }
            set
            {
                _borderThickness = value;
                NotifyPropertyChanged();
            }
        }

        public InstalledModViewModel(InstalledItem installedItem, ProfileItem profileItem)
        {
            InstallInfo = installedItem;
            ActiveModInfo = profileItem;

            Name = InstallInfo.CachedDetails.Name;
            Author = InstallInfo.CachedDetails.Author;
            Category = InstallInfo.CachedDetails.Category;
            Version = InstallInfo.CachedDetails.LatestVersion.Version.ToString();
            ReleaseDate = InstallInfo.CachedDetails.LatestVersion.ReleaseDate.ToString(Sys.Settings.DateTimeStringFormat);

            SortOrder = defaultSortOrder;

            if (string.IsNullOrWhiteSpace(Category))
            {
                Category = ModCategory.Unknown.ToString();
            }

            BorderThickness = new Thickness(0);
        }

        public override string ToString()
        {
            return $"{SortOrder}; {InstallInfo?.ModID} = {Name}";
        }

    }
}
