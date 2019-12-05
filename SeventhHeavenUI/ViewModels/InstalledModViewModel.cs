using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeavenUI.ViewModels
{
    /// <summary>
    /// Represents a list item in the <see cref="MyModsViewModel"/> used to display
    /// installed and active mods in the 'My Mods' tab.
    /// </summary>
    public class InstalledModViewModel : ViewModelBase
    {
        private string _name;
        private string _author;
        private string _version;
        private string _category;
        private bool _isSelected;
        private ProfileItem _activeModInfo;
        private InstalledItem _installInfo;
        private string _releaseDate;

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
                return ActiveModInfo != null;
            }
            set
            {
                ActivationChanged?.Invoke(this, this);
            }
        }

        public InstalledModViewModel(InstalledItem installedItem, ProfileItem profileItem)
        {
            InstallInfo = installedItem;
            ActiveModInfo = profileItem;

            Name = InstallInfo.CachedDetails.Name;
            Author = InstallInfo.CachedDetails.Author;
            Version = InstallInfo.CachedDetails.LatestVersion.Version.ToString();
            ReleaseDate = InstallInfo.CachedDetails.LatestVersion.ReleaseDate.ToString(Sys.Settings.DateTimeStringFormat);

            // if latest version does not have category then use category from Mod
            Category = string.IsNullOrWhiteSpace(InstallInfo.CachedDetails.LatestVersion.Category) ? InstallInfo.CachedDetails.Category : InstallInfo.CachedDetails.LatestVersion.Category;
        }

    }
}
