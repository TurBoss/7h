using _7thHeaven.Code;
using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeaven.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

        public delegate void OnRefreshListRequested();
        public event OnRefreshListRequested RefreshListRequested;

        private object listLock = new object();
        
        /// <summary>
        /// Keep track of load order (row index) of mods
        /// </summary>
        Dictionary<Guid, int> modLoadOrders = new Dictionary<Guid, int>();

        private ObservableCollection<InstalledModViewModel> _modList;

        /// <summary>
        /// List of installed mods (includes active mods in the currently active profile)
        /// </summary>
        public ObservableCollection<InstalledModViewModel> ModList
        {
            get
            {
                // guarantee the property never returns null
                if (_modList == null)
                {
                    _modList = new ObservableCollection<InstalledModViewModel>();
                }

                return _modList;
            }
            set
            {
                _modList = value;
                NotifyPropertyChanged();
            }
        }

        internal ReloadListOption _previousReloadOptions;

        public MyModsViewModel()
        {
            _previousReloadOptions = new ReloadListOption();
        }

        /// <summary>
        /// Invokes the <see cref="SelectedModChanged"/> Event if not null.
        /// </summary>
        internal void RaiseSelectedModChanged(object sender, InstalledModViewModel selected)
        {
            SelectedModChanged?.Invoke(this, selected);
        }

        internal void ClearRememberedSearchTextAndCategories()
        {
            _previousReloadOptions = new ReloadListOption();
        }

        internal void RefreshModList()
        {
            ClearRememberedSearchTextAndCategories();
            ReloadModList(GetSelectedMod()?.InstallInfo.ModID);
            RefreshListRequested?.Invoke();
        }

        /// <summary>
        /// Loads installed and active mods into <see cref="ModList"/> from <see cref="Sys.Library"/> and <see cref="Sys.ActiveProfile"/>
        /// </summary>
        internal void ReloadModList(Guid? modToSelect = null, string searchText = "", IEnumerable<FilterItemViewModel> categories = null, IEnumerable<FilterItemViewModel> tags = null)
        {
            // make sure to remove any deleted items and auto import mods first
            Sys.ActiveProfile.RemoveDeletedItems();
            Sys.TryAutoImportMods();


            // if there are no mods installed then just clear the list and return since no extra filtering work needs to be done
            if (Sys.Library.Items.Count == 0)
            {
                ClearModList();
                lock (listLock)
                {
                    ModList = new ObservableCollection<InstalledModViewModel>();
                }

                RaiseSelectedModChanged(this, null); // raise selected change event so mod preview info panel is cleared
                return;
            }

            categories = _previousReloadOptions.SetOrGetPreviousCategories(categories);
            searchText = _previousReloadOptions.SetOrGetPreviousSearchText(searchText);
            tags = _previousReloadOptions.SetOrGetPreviousTags(tags);

            List<InstalledModViewModel> allMods = new List<InstalledModViewModel>();

            foreach (ProfileItem item in Sys.ActiveProfile.Items)
            {
                InstalledItem mod = Sys.Library.GetItem(item.ModID);

                if (mod != null)
                {
                    bool includeMod = DoesModMatchSearchCriteria(searchText, categories, tags, mod.CachedDetails);

                    if (includeMod)
                    {
                        InstalledModViewModel activeMod = new InstalledModViewModel(mod, item);
                        activeMod.ActivationChanged += ActiveMod_ActivationChanged;
                        allMods.Add(activeMod);
                    }
                }
            }

            foreach (InstalledItem item in Sys.Library.Items.ToList())
            {
                bool isAdded = allMods.Any(m => m.InstallInfo.ModID == item.ModID && m.InstallInfo.LatestInstalled.InstalledLocation == item.LatestInstalled.InstalledLocation);

                bool includeMod = DoesModMatchSearchCriteria(searchText, categories, tags, item.CachedDetails);

                // ensure installed mod is included under the active profile if it is missing so you can toggle activation later
                ProfileItem profileItem = Sys.ActiveProfile.GetItem(item.ModID);
                if (profileItem == null)
                {
                    profileItem = new ProfileItem() { ModID = item.ModID, Name = item.CachedDetails.Name, Settings = new List<ProfileSetting>(), IsModActive = false };
                    Sys.ActiveProfile.AddItem(profileItem);
                }

                if (!isAdded && includeMod)
                {
                    InstalledModViewModel installedMod = new InstalledModViewModel(item, profileItem);
                    installedMod.ActivationChanged += ActiveMod_ActivationChanged;
                    allMods.Add(installedMod);
                }
            }

            if (modToSelect != null)
            {
                int index = allMods.FindIndex(m => m.InstallInfo.ModID == modToSelect);

                if (index >= 0)
                {
                    allMods[index].IsSelected = true;
                }
                else if (allMods.Count > 0)
                {
                    allMods[0].IsSelected = true;
                }
            }
            else
            {
                if (allMods.Count > 0)
                {
                    allMods[0].IsSelected = true;
                }
            }


            int installedModCount = Sys.Library.Items.Count;
            bool isFilteredBySearch = (allMods.Count != installedModCount);

            if (allMods.Count == 0)
            {
                Sys.Message(new WMessage("No results found", true));

                if (isFilteredBySearch && !string.IsNullOrWhiteSpace(searchText))
                {
                    // when searching by text clear the list to visually show to the user no results were found
                    ClearModList();
                }

                return;
            }


            if (!isFilteredBySearch)
            {
                bool storeLoadOrder = (allMods.Count == ModList.Count);
                SetPreviousLoadOrder(allMods, storeLoadOrder);
            }

            ClearModList();

            lock (listLock)
            {
                if (allMods.Any(m => m.SortOrder != InstalledModViewModel.defaultSortOrder))
                {
                    ModList = new ObservableCollection<InstalledModViewModel>(allMods.OrderBy(m => m.SortOrder));
                }
                else
                {
                    ModList = new ObservableCollection<InstalledModViewModel>(allMods);
                }
            }
        }

        /// <summary>
        /// Gets sort order for <see cref="ModList"/> and applies the same sort order to <paramref name="allMods"/> based on the Mod ID.
        /// </summary>
        private void SetPreviousLoadOrder(List<InstalledModViewModel> allMods, bool storeLoadOrder)
        {

            if (storeLoadOrder)
            {
                SetModLoadOrders();
            }

            if (modLoadOrders.Count > 0)
            {
                foreach (var mod in allMods)
                {
                    if (modLoadOrders.TryGetValue(mod.InstallInfo.ModID, out int expectedRowIdx))
                    {
                        mod.SortOrder = expectedRowIdx;
                    }
                }
            }
        }

        private void SetModLoadOrders()
        {
            modLoadOrders = new Dictionary<Guid, int>();
            int rowIdx = 0;
            lock (listLock)
            {
                foreach (var mod in ModList.ToList())
                {
                    if (!modLoadOrders.ContainsKey(mod.InstallInfo.ModID))
                    {
                        modLoadOrders.Add(mod.InstallInfo.ModID, rowIdx);
                    }

                    rowIdx++;
                }
            }
        }

        /// <summary>
        /// Invokes ReloadModList on the UI Thread (useful when having to modify collection from background thread)
        /// </summary>
        /// <param name="modToSelect"></param>
        /// <param name="searchText"></param>
        /// <param name="categories"></param>
        /// <param name="tags"></param>
        internal void ReloadModListFromUIThread(Guid? modToSelect = null, string searchText = "", IEnumerable<FilterItemViewModel> categories = null, IEnumerable<FilterItemViewModel> tags = null)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ReloadModList(modToSelect, searchText, categories, tags);
            });
        }

        private static bool DoesModMatchSearchCriteria(string searchText, IEnumerable<FilterItemViewModel> categories, IEnumerable<FilterItemViewModel> tags, Mod mod)
        {
            bool isSearchTextRelevant = !string.IsNullOrWhiteSpace(searchText) && mod.SearchRelevance(searchText) > 0;

            if (categories.Count() > 0 && tags.Count() > 0)
            {
                return FilterItemViewModel.FilterByCategory(mod, categories) || FilterItemViewModel.FilterByTags(mod, tags) || isSearchTextRelevant;
            }
            else if (categories.Count() > 0)
            {
                return FilterItemViewModel.FilterByCategory(mod, categories) || isSearchTextRelevant;
            }
            else if (tags.Count() > 0)
            {
                return FilterItemViewModel.FilterByTags(mod, tags) || isSearchTextRelevant;
            }
            else
            {
                return isSearchTextRelevant || string.IsNullOrWhiteSpace(searchText); // returns all if search text is empty and no category/tags are checked
            }

        }

        internal void ClearModList()
        {
            lock (listLock)
            {
                foreach (var item in ModList)
                {
                    item.ActivationChanged -= ActiveMod_ActivationChanged;
                }

                ModList.Clear();
            }

            modLoadOrders.Clear();
        }

        private void ActiveMod_ActivationChanged(object sender, InstalledModViewModel selected)
        {
            ToggleActivateMod(selected.InstallInfo.ModID, false);
        }

        internal void ShowImportModWindow()
        {
            ImportModWindow modWindow = new ImportModWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            modWindow.ShowDialog();
            ReloadModList(GetSelectedMod()?.InstallInfo?.ModID);
        }

        /// <summary>
        /// Returns selected view model in <see cref="ModList"/>.
        /// </summary>
        public InstalledModViewModel GetSelectedMod()
        {
            InstalledModViewModel selected = null;

            lock (listLock)
            {
                selected = ModList.Where(m => m.IsSelected).LastOrDefault();

                // due to virtualization, IsSelected could be set on multiple items... 
                // ... so we will deselect the other items to avoid problems of multiple items being selected
                if (ModList.Where(m => m.IsSelected).Count() > 1)
                {
                    foreach (var mod in ModList.Where(m => m.IsSelected && m.InstallInfo.ModID != selected.InstallInfo.ModID))
                    {
                        mod.IsSelected = false;
                    }
                }
            }

            return selected;
        }

        internal void ToggleActivateMod(Guid modID, bool reloadList = true)
        {
            if (String.IsNullOrWhiteSpace(Sys.Settings.CurrentProfile)) return;

            ProfileItem item = Sys.ActiveProfile.Items.Find(i => i.ModID.Equals(modID));

            // item == null - activate mod for current profile
            // item != null - deactivate mod for current profile

            if (item == null || item?.IsModActive == false)
            {
                HashSet<Guid> examined = new HashSet<Guid>();
                List<InstalledItem> pulledIn = new List<InstalledItem>();
                List<ProfileItem> remove = new List<ProfileItem>();
                List<string> missing = new List<string>();
                List<InstalledItem> badVersion = new List<InstalledItem>();
                Stack<InstalledItem> toExamine = new Stack<InstalledItem>();

                // verify the mod in the profile still exists in the library.
                InstalledItem installedMod = Sys.Library.GetItem(modID);
                if (installedMod == null)
                {
                    return;
                }

                toExamine.Push(installedMod);

                while (toExamine.Any())
                {
                    examined.Add(toExamine.Peek().ModID);
                    var info = toExamine.Pop().GetModInfo();

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
                                if (Sys.ActiveProfile.ActiveItems.Find(pi => pi.ModID.Equals(req.ModID)) == null)
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
                            var pItem = Sys.ActiveProfile.ActiveItems.Find(i => i.ModID.Equals(forbid.ModID));
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

                foreach (var active in Sys.ActiveProfile.ActiveItems.Except(remove))
                {
                    var info = Sys.Library.GetItem(active.ModID)?.GetModInfo();
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
                                    MessageDialogWindow.Show(String.Format(MainWindowViewModel._forbidMain, Sys.Library.GetItem(active.ModID).CachedDetails.Name), "Warning");
                                else
                                    MessageDialogWindow.Show(String.Format(MainWindowViewModel._forbidDependent, Sys.Library.GetItem(mID).CachedDetails.Name, Sys.Library.GetItem(active.ModID).CachedDetails.Name), "Warning");
                                return;
                            }
                        }
                    }
                }

                if (missing.Any())
                {
                    MessageDialogWindow.Show(String.Format(MainWindowViewModel._msgReqMissing, String.Join("\n", missing)), "Missing Requirements");
                }
                if (badVersion.Any())
                {
                    MessageDialogWindow.Show(String.Format(MainWindowViewModel._msgBadVer, String.Join("\n", badVersion.Select(ii => ii.CachedDetails.Name))), "Unsupported Version");
                }
                if (pulledIn.Any())
                {
                    MessageDialogWindow.Show(String.Format(MainWindowViewModel._msgRequired, String.Join("\n", pulledIn.Select(ii => ii.CachedDetails.Name))), "Missing Required Active Mods");
                }
                if (remove.Any())
                {
                    MessageDialogWindow.Show(String.Format(MainWindowViewModel._msgRemove, String.Join("\n", remove.Select(pi => Sys.Library.GetItem(pi.ModID).CachedDetails.Name))), "Deactivate Mods Warning");
                }

                DoActivate(modID, reloadList);

                foreach (InstalledItem req in pulledIn)
                {
                    DoActivate(req.ModID, reloadList);
                    if (reloadList) 
                        Sys.Ping(req.ModID);
                }

                foreach (ProfileItem pi in remove)
                {
                    DoDeactivate(pi, reloadList);
                    if (reloadList) 
                        Sys.Ping(pi.ModID);
                }

                GameLauncher.SanityCheckSettings();
            }
            else
            {
                DoDeactivate(item, reloadList);
            }

            if (reloadList) 
                Sys.Ping(modID);
        }

        private void DoDeactivate(ProfileItem item, bool reloadList = true)
        {
            item.IsModActive = false;

            if (reloadList)
            {
                ReloadModList(item.ModID);
            }
        }

        private void DoActivate(Guid modID, bool reloadList = true)
        {
            if (!MainWindowViewModel.CheckAllowedActivate(modID)) return;

            var mod = Sys.ActiveProfile.Items.FirstOrDefault(m => m.ModID == modID);

            if (mod != null)
            {
                // already exists in list so just mark as active
                mod.IsModActive = true;
            }
            else
            {
                var installedItem = Sys.Library.GetItem(modID);

                var item = new ProfileItem() { ModID = modID, Name = installedItem?.CachedDetails.Name, Settings = new List<ProfileSetting>(), IsModActive = true };
                Sys.ActiveProfile.AddItem(item);
            }


            if (reloadList)
            {
                ReloadModListFromUIThread(modID);
            }
        }

        internal void DeactivateAllActiveMods()
        {
            foreach (InstalledModViewModel installedMod in ModList.Where(m => m.IsActive).ToList())
            {
                ToggleActivateMod(installedMod.ActiveModInfo.ModID, reloadList: false); // reload list at the end
            }

            ReloadModList();
        }

        internal void ActivateAllMods()
        {
            foreach (InstalledModViewModel installedMod in ModList.Where(m => !m.IsActive).ToList())
            {
                ToggleActivateMod(installedMod.InstallInfo.ModID, reloadList: false); // reload list at the end
            }

            ReloadModList();
        }

        internal void UninstallMod(InstalledModViewModel installedModViewModel)
        {
            InstalledItem installed = Sys.Library.GetItem(installedModViewModel.InstallInfo.ModID);

            if (installed == null)
            {
                Logger.Warn("could not get InstalledItem for mod.");
                return;
            }

            Install.Uninstall(installed);
            ReloadModList();
        }

        /// <summary>
        /// Change the sort order of an active mod
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="change"></param>
        public void ReorderProfileItem(InstalledModViewModel mod, int change)
        {
            // validate mod has not been removed from filesystem
            if (!mod.InstallInfo.ModExistsOnFileSystem())
            {
                Sys.ValidateAndRemoveDeletedMods();
                Sys.Message(new WMessage($"Can not re-order mod. It seems {mod.Name} has been removed from the file system.", true));
                ReloadModListFromUIThread();
                return;
            }

            int index = ModList.IndexOf(mod);

            int newindex = index + change;

            if (newindex < 0 || newindex >= ModList.Count)
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                // ensure to modify observable collection on UI thread 
                lock (listLock)
                {
                    ModList.RemoveAt(index);
                    ModList.Insert(newindex, mod);
                }

                Sys.ActiveProfile.Items = ModList.Select(m => m.ActiveModInfo).ToList();

                ReloadModList(mod.InstallInfo.CachedDetails.ID);
            });
        }

        public void SendModToBottom(InstalledModViewModel mod)
        {
            // validate mod has not been removed from filesystem
            if (!mod.InstallInfo.ModExistsOnFileSystem())
            {
                Sys.ValidateAndRemoveDeletedMods();
                Sys.Message(new WMessage($"Can not re-order mod. It seems {mod.Name} has been removed from the file system.", true));
                ReloadModListFromUIThread();
                return;
            }

            int index = ModList.IndexOf(mod);

            App.Current.Dispatcher.Invoke(() =>
            {
                // ensure to modify observable collection on UI thread 
                lock (listLock)
                {
                    ModList.RemoveAt(index);
                    ModList.Add(mod);
                }

                Sys.ActiveProfile.Items = ModList.Select(m => m.ActiveModInfo).ToList();

                ReloadModList(mod.InstallInfo.CachedDetails.ID);
            });
        }

        public void SendModToTop(InstalledModViewModel mod)
        {
            // validate mod has not been removed from filesystem
            if (!mod.InstallInfo.ModExistsOnFileSystem())
            {
                Sys.ValidateAndRemoveDeletedMods();
                Sys.Message(new WMessage($"Can not re-order mod. It seems {mod.Name} has been removed from the file system.", true));
                ReloadModListFromUIThread();
                return;
            }

            int index = ModList.IndexOf(mod);

            App.Current.Dispatcher.Invoke(() =>
            {
                // ensure to modify observable collection on UI thread 
                lock (listLock)
                {
                    ModList.RemoveAt(index);
                    ModList.Insert(0, mod);
                }

                Sys.ActiveProfile.Items = ModList.Select(m => m.ActiveModInfo).ToList();

                ReloadModList(mod.InstallInfo.CachedDetails.ID);
            });
        }

        /// <summary>
        /// Opens Configure Mod Window to make changes to mod settings.
        /// </summary>
        /// <param name="modToConfigure"></param>
        internal void ShowConfigureModWindow(InstalledModViewModel modToConfigure)
        {
            // validate mod has not been removed from filesystem
            if (!modToConfigure.InstallInfo.ModExistsOnFileSystem())
            {
                Sys.ValidateAndRemoveDeletedMods();
                Sys.Message(new WMessage($"Can not configure mod. It seems {modToConfigure.Name} has been removed from the file system.", true));
                ReloadModListFromUIThread();
                return;
            }

            InstalledVersion installed = Sys.Library.GetItem(modToConfigure.InstallInfo.ModID)?.LatestInstalled;
            _7thWrapperLib.ModInfo info = null;
            Func<string, string> imageReader;
            Func<string, Stream> audioReader;

            string config_temp_folder = Path.Combine(Sys.PathToTempFolder, "configmod");
            string pathToModXml = Path.Combine(Sys.Settings.LibraryLocation, installed.InstalledLocation);

            // wire up the imageReader and audioReader to extract/read files from the .iro archive
            IDisposable arcToDispose = null;
            if (pathToModXml.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase))
            {
                var arc = new _7thWrapperLib.IrosArc(pathToModXml);
                if (arc.HasFile("mod.xml"))
                {
                    var doc = new System.Xml.XmlDocument();
                    doc.Load(arc.GetData("mod.xml"));
                    info = new _7thWrapperLib.ModInfo(doc, Sys._context);
                }

                imageReader = s =>
                {
                    if (arc.HasFile(s))
                    {
                        string tempImgPath = Path.Combine(config_temp_folder, s);

                        Directory.CreateDirectory(config_temp_folder);

                        string fullImgDirectoryPath = Path.GetDirectoryName(tempImgPath);

                        if (!Directory.Exists(fullImgDirectoryPath))
                        {
                            Directory.CreateDirectory(fullImgDirectoryPath);
                        }

                        if (File.Exists(tempImgPath))
                        {
                            return tempImgPath;
                        }

                        // extract img from IRO arc to temp img location
                        using (Stream imgStream = arc.GetData(s))
                        {
                            try
                            {
                                using (FileStream fileStream = new FileStream(tempImgPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                                {
                                    fileStream.Seek(0, SeekOrigin.Begin);
                                    imgStream.CopyTo(fileStream);
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Warn(e, "Failed to write image to file");
                                return null;
                            }
                        }

                        return tempImgPath;
                    }

                    return null;
                };

                audioReader = s =>
                {
                    if (arc.HasFile(s))
                        arc.GetData(s);
                    return null;
                };

                arcToDispose = arc;
            }
            else
            {
                // if mod is in a folder than wire up imageReader and audioReader to return path to file and audio file stream
                string file = Path.Combine(pathToModXml, "mod.xml");
                if (File.Exists(file))
                    info = new _7thWrapperLib.ModInfo(file, Sys._context);

                imageReader = s =>
                {
                    string ifile = Path.Combine(pathToModXml, s);
                    if (File.Exists(ifile))
                    {
                        return ifile;
                    }

                    return null;
                };

                audioReader = s =>
                {
                    string ifile = Path.Combine(pathToModXml, s);
                    if (File.Exists(ifile))
                        return new FileStream(ifile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    return null;
                };
            }

            using (arcToDispose)
            {
                info = info ?? new _7thWrapperLib.ModInfo();
                if (info.Options.Count == 0)
                {
                    MessageDialogWindow.Show("There are no options to configure for this mod.", "No Options", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                List<Constraint> modConstraints = GameLauncher.GetConstraints().Where(c => c.ModID.Equals(modToConfigure.InstallInfo.ModID)).ToList();

                ConfigureModWindow modWindow = new ConfigureModWindow();
                modWindow.ViewModel.Init(info, imageReader, audioReader, modToConfigure.ActiveModInfo, modConstraints, pathToModXml);

                // Open dialog for configuring settings - if true is returned then the settings are saved
                bool? dialogResult = modWindow.ShowDialog();
                if (dialogResult.GetValueOrDefault(false) == true)
                {
                    modToConfigure.ActiveModInfo.Settings = modWindow.ViewModel.GetSettings();

                    GameLauncher.SanityCheckSettings();
                }

                modWindow.ViewModel.CleanUp();
                modWindow.ViewModel = null;
            }
        }

        /// <summary>
        /// This will sort <see cref="ModList"/> by categories based on <see cref="ModLoadOrder"/>.
        /// Mod order constraints in mod.xml is taken into account and takes precedence over the cateogry load sort
        /// </summary>
        public void AutoSortBasedOnCategory()
        {
            List<InstalledModViewModel> sortedList = ModList.OrderBy(s => ModLoadOrder.Get(s.Category))
                                                            .ThenBy(m => m.Name)
                                                            .ToList();

            // loop over and check mod info for ordering of before/after other mods
            int i = 0;
            foreach (var installedMod in sortedList.ToList())
            {
                _7thWrapperLib.ModInfo info = installedMod.InstallInfo.GetModInfo();

                foreach (Guid after in info.OrderAfter)
                {
                    int afterIndex = sortedList.FindIndex(m => m.InstallInfo.ModID.Equals(after));

                    if (afterIndex > i)
                    {
                        // move mod to come after this guid
                        sortedList.RemoveAt(i);
                        sortedList.Insert(afterIndex, installedMod);
                    }
                }

                foreach (Guid before in info.OrderBefore)
                {
                    int beforeIndex = sortedList.FindIndex(m => m.InstallInfo.ModID.Equals(before));

                    if (beforeIndex < i)
                    {
                        // move mod to come before this guid
                        sortedList.RemoveAt(i);
                        sortedList.Insert(beforeIndex-1, installedMod);
                    }
                }

                i++;
            }

            ClearModList();

            lock (listLock)
            {
                ModList = new ObservableCollection<InstalledModViewModel>(sortedList);
            }

            // update active profile with new sort order
            Sys.ActiveProfile.Items = ModList.Select(m => m.ActiveModInfo).ToList();

            ReloadModList(GetSelectedMod()?.InstallInfo?.ModID);
        }
    }
}
