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
                    InstalledModViewModel activeMod = new InstalledModViewModel(mod, item);
                    activeMod.ActivationChanged += ActiveMod_ActivationChanged;
                    allMods.Add(activeMod);
                }
            }

            foreach (InstalledItem item in Sys.Library.Items)
            {
                bool isActive = allMods.Any(m => m.InstallInfo.ModID == item.ModID && m.InstallInfo.LatestInstalled.InstalledLocation == item.LatestInstalled.InstalledLocation);

                if (!isActive)
                {
                    InstalledModViewModel installedMod = new InstalledModViewModel(item, null);
                    installedMod.ActivationChanged += ActiveMod_ActivationChanged;
                    allMods.Add(installedMod);
                }
            }

            ModList = allMods;
        }

        private void ActiveMod_ActivationChanged(object sender, InstalledModViewModel selected)
        {
            ToggleActivateMod(selected.InstallInfo.ModID);
            ReloadModList();
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

        internal void ToggleActivateMod(Guid modID)
        {
            if (String.IsNullOrWhiteSpace(Sys.Settings.CurrentProfile)) return;

            ProfileItem item = Sys.ActiveProfile.Items.Find(i => i.ModID.Equals(modID));

            // item == null - activate mod for current profile
            // item != null - deactivate mod for current profile

            if (item == null)
            {
                HashSet<Guid> examined = new HashSet<Guid>();
                List<InstalledItem> pulledIn = new List<InstalledItem>();
                List<ProfileItem> remove = new List<ProfileItem>();
                List<string> missing = new List<string>();
                List<InstalledItem> badVersion = new List<InstalledItem>();
                Stack<InstalledItem> toExamine = new Stack<InstalledItem>();
                toExamine.Push(Sys.Library.GetItem(modID));

                while (toExamine.Any())
                {
                    examined.Add(toExamine.Peek().ModID);
                    var info = MainWindowViewModel.GetModInfo(toExamine.Pop());

                    if (info == null)
                    {
                        continue;
                    }


                    foreach (var req in info.Compatibility.Requires)
                    {
                        if (!examined.Contains(req.ModID))
                        {
                            var inst = Sys.Library.GetItem(req.ModID);
                            if (inst == null)
                                missing.Add(req.Description);
                            else if (req.Versions.Any() && !req.Versions.Contains(inst.Versions.Last().VersionDetails.Version))
                                badVersion.Add(inst);
                            else
                            {
                                if (Sys.ActiveProfile.Items.Find(pi => pi.ModID.Equals(req.ModID)) == null)
                                    pulledIn.Add(inst);
                                toExamine.Push(inst);
                            }
                        }
                    }

                    foreach (var forbid in info.Compatibility.Forbids)
                    {
                        if (!examined.Contains(forbid.ModID))
                        {
                            examined.Add(forbid.ModID);
                            var pItem = Sys.ActiveProfile.Items.Find(i => i.ModID.Equals(forbid.ModID));
                            if (pItem != null)
                            {
                                if (!forbid.Versions.Any() || forbid.Versions.Contains(Sys.Library.GetItem(pItem.ModID).LatestInstalled.VersionDetails.Version))
                                {
                                    remove.Add(pItem);
                                }
                            }
                        }
                    }
                }

                foreach (var active in Sys.ActiveProfile.Items.Except(remove))
                {
                    var info = MainWindowViewModel.GetModInfo(Sys.Library.GetItem(active.ModID));
                    if (info != null)
                    {
                        foreach (Guid mID in pulledIn.Select(ii => ii.ModID).Concat(new[] { modID }))
                        {
                            var forbid = info.Compatibility.Forbids.Find(f => f.ModID.Equals(mID));

                            if (forbid == null)
                            {
                                continue;
                            }

                            if (!forbid.Versions.Any() || forbid.Versions.Contains(Sys.Library.GetItem(mID).LatestInstalled.VersionDetails.Version))
                            {
                                if (mID.Equals(modID))
                                    MessageBox.Show(String.Format(MainWindowViewModel._forbidMain, Sys.Library.GetItem(active.ModID).CachedDetails.Name));
                                else
                                    MessageBox.Show(String.Format(MainWindowViewModel._forbidDependent, Sys.Library.GetItem(mID).CachedDetails.Name, Sys.Library.GetItem(active.ModID).CachedDetails.Name));
                                return;
                            }
                        }
                    }
                }

                if (missing.Any())
                {
                    MessageBox.Show(String.Format(MainWindowViewModel._msgReqMissing, String.Join("\n", missing)));
                }
                if (badVersion.Any())
                {
                    MessageBox.Show(String.Format(MainWindowViewModel._msgBadVer, String.Join("\n", badVersion.Select(ii => ii.CachedDetails.Name))));
                }
                if (pulledIn.Any())
                {
                    MessageBox.Show(String.Format(MainWindowViewModel._msgRequired, String.Join("\n", pulledIn.Select(ii => ii.CachedDetails.Name))));
                }
                if (remove.Any())
                {
                    MessageBox.Show(String.Format(MainWindowViewModel._msgRemove, String.Join("\n", remove.Select(pi => Sys.Library.GetItem(pi.ModID).CachedDetails.Name))));
                }

                DoActivate(modID);

                foreach (InstalledItem req in pulledIn)
                {
                    DoActivate(req.ModID);
                    Sys.Ping(req.ModID);
                }
                foreach (ProfileItem pi in remove)
                {
                    DoDeactivate(pi);
                    Sys.Ping(pi.ModID);
                }

                MainWindowViewModel.SanityCheckSettings();
            }
            else
            {
                DoDeactivate(item);
            }

            Sys.Ping(modID);
        }

        private void DoDeactivate(ProfileItem item)
        {
            Sys.ActiveProfile.Items.Remove(item);
            ReloadActiveMods(true);
        }

        private void DoActivate(Guid modID)
        {
            if (!MainWindowViewModel.CheckAllowedActivate(modID)) return;

            var item = new ProfileItem() { ModID = modID, Settings = new List<ProfileSetting>() };
            Sys.ActiveProfile.Items.Add(item);

            ReloadActiveMods(true);
        }
    }
}
