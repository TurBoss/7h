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
    /// ViewModel to contain interaction logic for the 'My Mods' tab user control.
    /// </summary>
    public class MyModsViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public delegate void OnSelectionChanged(object sender, InstalledModViewModel selected);
        public event OnSelectionChanged SelectedModChanged;

        private List<InstalledModViewModel> _modList;

        /// <summary>
        /// List of installed mods (includes active mods in the currently active profile)
        /// </summary>
        public List<InstalledModViewModel> ModList
        {
            get
            {
                // guarantee the property never returns null
                if (_modList == null)
                {
                    _modList = new List<InstalledModViewModel>();
                }

                return _modList;
            }
            set
            {
                _modList = value;
                NotifyPropertyChanged();
            }
        }

        public MyModsViewModel()
        {
        }

        /// <summary>
        /// Invokes the <see cref="SelectedModChanged"/> Event if not null.
        /// </summary>
        internal void RaiseSelectedModChanged(object sender, InstalledModViewModel selected)
        {
            SelectedModChanged?.Invoke(this, selected);
        }

        /// <summary>
        /// Loads installed and active mods into <see cref="ModList"/> from <see cref="Sys.Library"/> and <see cref="Sys.ActiveProfile"/>
        /// </summary>
        internal void ReloadModList()
        {
            Sys.ActiveProfile.Items.RemoveAll(i => Sys.Library.GetItem(i.ModID) == null);

            List<InstalledModViewModel> allMods = new List<InstalledModViewModel>();

            foreach (ProfileItem item in Sys.ActiveProfile.Items)
            {
                InstalledItem mod = Sys.Library.GetItem(item.ModID);

                if (mod != null)
                {
                    allMods.Add(new InstalledModViewModel(mod, item));
                }
            }

            foreach (InstalledItem item in Sys.Library.Items)
            {
                bool isActive = allMods.Any(m => m.InstallInfo.ModID == item.ModID && m.InstallInfo.LatestInstalled.InstalledLocation == item.LatestInstalled.InstalledLocation);

                if (!isActive)
                {
                    allMods.Add(new InstalledModViewModel(item, null));
                }
            }

            ModList = allMods;
        }

        /// <summary>
        /// Loads active mods into <see cref="ModList"/> from <see cref="Sys.ActiveProfile"/>
        /// </summary>
        /// <param name="clearList"> removes any active mods in <see cref="ModList"/> before reloading </param>
        internal void ReloadActiveMods(bool clearList)
        {
            Sys.ActiveProfile.Items.RemoveAll(i => Sys.Library.GetItem(i.ModID) == null);

            if (clearList)
            {
                ModList.RemoveAll(m => m.IsActive);
            }

            List<InstalledModViewModel> activeMods = new List<InstalledModViewModel>();

            foreach (ProfileItem item in Sys.ActiveProfile.Items)
            {
                InstalledItem mod = Sys.Library.GetItem(item.ModID);

                if (mod != null)
                {
                    activeMods.Add(new InstalledModViewModel(mod, item));
                }
            }

            ModList.AddRange(activeMods);
            NotifyPropertyChanged(nameof(ModList));
        }

        /// <summary>
        /// Returns selected view model in <see cref="ModList"/>.
        /// </summary>
        public InstalledModViewModel GetSelectedMod()
        {
            InstalledModViewModel selected = ModList.Where(m => m.IsSelected).LastOrDefault();

            // due to virtualization, IsSelected could be set on multiple items... 
            // ... so we will deselect the other items to avoid problems of multiple items being selected
            if (ModList.Where(m => m.IsSelected).Count() > 1)
            {
                foreach (var mod in ModList.Where(m => m.IsSelected && m.InstallInfo.ModID != selected.InstallInfo.ModID))
                {
                    mod.IsSelected = false;
                }
            }

            return selected;
        }
    }
}
