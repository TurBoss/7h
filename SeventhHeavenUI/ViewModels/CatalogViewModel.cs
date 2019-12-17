using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using SeventhHeaven.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class CatalogViewModel : ViewModelBase, IDownloader
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string _msgDownloadReq =
    @"This mod also requires you to download the following mods:
{0}
Download and install them?";

        private const string _msgMissingReq =
            @"This mod requires the following mods to also be installed, but I cannot find them:
{0}
It may not work properly unless you find and install the requirements.";

        public delegate void OnSelectionChanged(object sender, CatalogModItemViewModel selected);
        public event OnSelectionChanged SelectedModChanged;

        public delegate void OnRefreshListRequested();
        public event OnRefreshListRequested RefreshListRequested;

        private List<CatalogModItemViewModel> _catalogModList;
        private ObservableCollection<DownloadItemViewModel> _downloadList;

        internal ReloadListOption _previousReloadOptions;

        private Dictionary<string, MegaIros> _megaFolders = new Dictionary<string, MegaIros>(StringComparer.InvariantCultureIgnoreCase);

        private object _listLock = new object();
        private object _downloadLock = new object();

        /// <summary>
        /// List of installed mods (includes active mods in the currently active profile)
        /// </summary>
        public List<CatalogModItemViewModel> CatalogModList
        {
            get
            {
                // guarantee the property never returns null
                if (_catalogModList == null)
                {
                    _catalogModList = new List<CatalogModItemViewModel>();
                }

                return _catalogModList;
            }
            set
            {
                _catalogModList = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<DownloadItemViewModel> DownloadList
        {
            get
            {
                // guarantee the property never returns null
                if (_downloadList == null)
                {
                    _downloadList = new ObservableCollection<DownloadItemViewModel>();
                }

                return _downloadList;
            }
            set
            {
                _downloadList = value;
                NotifyPropertyChanged();
            }
        }

        public CatalogViewModel()
        {
            DownloadList = new ObservableCollection<DownloadItemViewModel>();
            _previousReloadOptions = new ReloadListOption();
        }

        /// <summary>
        /// Invokes the <see cref="SelectedModChanged"/> Event if not null.
        /// </summary>
        internal void RaiseSelectedModChanged(object sender, CatalogModItemViewModel selected)
        {
            SelectedModChanged?.Invoke(this, selected);
        }

        /// <summary>
        /// Loads available mods from catalogs into <see cref="CatalogModList"/> from <see cref="Sys.Catalog.Mods"/>
        /// </summary>
        /// <param name="searchText"> empty string returns all mods </param>
        internal void ReloadModList(Guid? modToSelect = null, string searchText = "", IEnumerable<FilterItemViewModel> categories = null, IEnumerable<FilterItemViewModel> tags = null)
        {
            // if there are no mods in the catalog then just clear the list and return since no extra filtering work needs to be done
            if (Sys.Catalog.Mods.Count == 0)
            {
                // make sure to set CatalogModList on the UI thread
                // ... due to uncaught exception that can be thrown when modifying on background thread
                App.Current.Dispatcher.Invoke(() =>
                {
                    lock (_listLock)
                    {
                        CatalogModList.Clear();
                        CatalogModList = new List<CatalogModItemViewModel>();
                    }
                });

                return;
            }


            List<Mod> results;

            categories = _previousReloadOptions.SetOrGetPreviousCategories(categories);
            searchText = _previousReloadOptions.SetOrGetPreviousSearchText(searchText);
            tags = _previousReloadOptions.SetOrGetPreviousTags(tags);

            if (String.IsNullOrEmpty(searchText))
            {
                results = Sys.Catalog.Mods.Where(m =>
                {
                    if (categories.Count() > 0 && tags.Count() > 0)
                    {
                        return FilterItemViewModel.FilterByCategory(m, categories) || FilterItemViewModel.FilterByTags(m, tags);
                    }
                    else if (categories.Count() > 0)
                    {
                        return FilterItemViewModel.FilterByCategory(m, categories);
                    }
                    else
                    {
                        return FilterItemViewModel.FilterByTags(m, tags);
                    }
                }).ToList();
            }
            else
            {
                results = Sys.Catalog.Mods.Where(m =>
                {
                    if (categories.Count() > 0 && tags.Count() > 0)
                    {
                        return FilterItemViewModel.FilterByCategory(m, categories) || FilterItemViewModel.FilterByTags(m, tags);
                    }
                    else if (categories.Count() > 0)
                    {
                        return FilterItemViewModel.FilterByCategory(m, categories);
                    }
                    else
                    {
                        return FilterItemViewModel.FilterByTags(m, tags);
                    }
                }).Select(m => new { Mod = m, Relevance = m.SearchRelevance(searchText) })
                  .Where(a => a.Relevance > 0)
                  .OrderByDescending(a => a.Relevance)
                  .Select(a => a.Mod)
                  .ToList();
            }

            List<CatalogModItemViewModel> newList = new List<CatalogModItemViewModel>();

            foreach (Mod m in results)
            {
                CatalogModItemViewModel item = new CatalogModItemViewModel(m);
                newList.Add(item);
            }

            if (categories.Count() > 0)
            {
                // order by category then relevance
                newList = newList.OrderBy(m => m.Category).ThenByDescending(m => m.Mod.SearchRelevance(searchText)).ToList();
            }

            if (modToSelect != null)
            {
                int index = newList.FindIndex(m => m.Mod.ID == modToSelect);

                if (index >= 0)
                {
                    newList[index].IsSelected = true;
                }
                else if (newList.Count > 0)
                {
                    newList[0].IsSelected = true;
                }
            }
            else
            {
                if (newList.Count > 0)
                {
                    newList[0].IsSelected = true;
                }
            }

            // make sure to set CatalogModList on the UI thread
            // ... due to uncaught exception that can be thrown when modifying on background thread
            App.Current.Dispatcher.Invoke(() =>
            {
                if (newList.Count == 0)
                {
                    Sys.Message(new WMessage("No results found", true));
                    return;
                }

                lock (_listLock)
                {
                    CatalogModList.Clear();
                    CatalogModList = newList;
                }
            });
        }

        internal void ClearRememberedSearchTextAndCategories()
        {
            _previousReloadOptions = new ReloadListOption();
        }

        internal void RefreshCatalogList()
        {
            ClearRememberedSearchTextAndCategories();
            ForceCheckCatalogUpdateAsync();
            RefreshListRequested?.Invoke();
        }

        /// <summary>
        /// Returns selected view model in <see cref="CatalogModList"/>.
        /// </summary>
        public CatalogModItemViewModel GetSelectedMod()
        {
            CatalogModItemViewModel selected = null;
            int selectedCount = 0;

            lock (_listLock)
            {
                selected = CatalogModList.Where(m => m.IsSelected).LastOrDefault();
                selectedCount = CatalogModList.Where(m => m.IsSelected).Count();
            }

            // due to virtualization, IsSelected could be set on multiple items... 
            // ... so we will deselect the other items to avoid problems of multiple items being selected
            if (selectedCount > 1)
            {
                lock (_listLock)
                {
                    foreach (var mod in CatalogModList.Where(m => m.IsSelected && m.Mod.ID != selected.Mod.ID))
                    {
                        mod.IsSelected = false;
                    }
                }
            }

            return selected;
        }

        internal Task CheckForCatalogUpdatesAsync(object state)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                List<Guid> pingIDs = null;
                var options = (CatCheckOptions)state;
                string catFile = Path.Combine(Sys.SysFolder, "catalog.xml");

                Directory.CreateDirectory(Path.Combine(Sys.SysFolder, "temp"));

                int subTotalCount = Sys.Settings.SubscribedUrls.Count; // amount of subscriptions to update
                int subUpdateCount = 0; // amount of subscriptions updated

                if (options.ForceCheck)
                {
                    // on force check, initialize a new catalog to ignore any cached items
                    Sys.SetNewCatalog(new Catalog());
                }

                if (Sys.Settings.SubscribedUrls.Count == 0)
                {
                    ReloadModList();
                    return;
                }

                foreach (string subscribe in Sys.Settings.SubscribedUrls.ToArray())
                {
                    Subscription sub = Sys.Settings.Subscriptions.Find(s => s.Url.Equals(subscribe, StringComparison.InvariantCultureIgnoreCase));
                    if (sub == null)
                    {
                        sub = new Subscription() { Url = subscribe, FailureCount = 0, LastSuccessfulCheck = DateTime.MinValue };
                        Sys.Settings.Subscriptions.Add(sub);
                    }

                    if ((sub.LastSuccessfulCheck < DateTime.Now.AddDays(-1)) || options.ForceCheck)
                    {
                        Logger.Info($"Checking subscription {sub.Url}");

                        string uniqueFileName = $"cattemp{Path.GetRandomFileName()}.xml"; // save temp catalog update to unique filename so multiple catalog updates can download async
                        string path = Path.Combine(Sys.SysFolder, "temp", uniqueFileName);

                        Sys.Downloads.Download(subscribe, path, $"Checking catalog {subscribe}", new Install.InstallProcedureCallback(e =>
                        {
                            bool success = (e.Error == null && e.Cancelled == false);
                            subUpdateCount++;

                            if (success)
                            {
                                try
                                {
                                    Catalog c = Util.Deserialize<Catalog>(path);

                                    lock (Sys.CatalogLock) // put a lock on the Catalog so multiple threads can only merge one at a time
                                    {
                                        Sys.Catalog = Catalog.Merge(Sys.Catalog, c, out pingIDs);

                                        using (FileStream fs = new FileStream(catFile, FileMode.Create))
                                        {
                                            Util.Serialize(Sys.Catalog, fs);
                                        }
                                    }

                                    Sys.Message(new WMessage() { Text = $"Updated catalog from {subscribe}" });

                                    sub.LastSuccessfulCheck = DateTime.Now;
                                    sub.FailureCount = 0;

                                    foreach (Guid id in pingIDs)
                                    {
                                        Sys.Ping(id);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);

                                    sub.FailureCount++;
                                    Sys.Message(new WMessage() { Text = $"Failed to load subscription {subscribe}: {ex.Message}" });
                                }
                                finally
                                {
                                    // delete temp catalog
                                    if (File.Exists(path))
                                    {
                                        File.Delete(path);
                                    }
                                }
                            }
                            else
                            {
                                Logger.Warn(e.Error?.Message, "catalog download failed");
                                sub.FailureCount++;
                            }

                            // reload the UI list of catalog mods and scan for any mod updates once all subs have been attempted to download
                            if (subUpdateCount == subTotalCount)
                            {
                                ReloadModList(GetSelectedMod()?.Mod.ID);
                                ScanForModUpdates();
                            }

                        }), null);
                    }
                    else
                    {
                        subTotalCount -= 1; // This catalog does not have to be updated
                    }
                }
            });

            return t;
        }

        internal void ForceCheckCatalogUpdateAsync()
        {
            Task t = CheckForCatalogUpdatesAsync(new CatCheckOptions() { ForceCheck = true });

            t.ContinueWith((taskResult) =>
            {
                if (taskResult.IsFaulted)
                {
                    Logger.Warn(taskResult.Exception);
                }
            });
        }

        private void ScanForModUpdates()
        {
            foreach (InstalledItem inst in Sys.Library.Items)
            {
                Mod cat = Sys.GetModFromCatalog(inst.ModID);

                if (cat != null && cat.LatestVersion.Version > inst.Versions.Max(v => v.VersionDetails.Version))
                {
                    switch (inst.UpdateType)
                    {
                        case UpdateType.Notify:
                            Sys.Message(new WMessage() { Text = $"New version of {cat.Name} available", Link = "iros://" + cat.ID.ToString() });
                            Sys.Ping(inst.ModID);
                            break;

                        case UpdateType.Install:
                            Install.DownloadAndInstall(cat);
                            break;
                    }
                }
            }
        }

        #region Methods Related to Downloads

        internal void CancelDownload(DownloadItemViewModel downloadItemViewModel)
        {
            downloadItemViewModel?.PerformCancel();
            RemoveFromDownloadList(downloadItemViewModel);
            Sys.Message(new WMessage($"Canceled {downloadItemViewModel.ItemName}"));
        }

        internal void DownloadMod(CatalogModItemViewModel catalogModItemViewModel)
        {
            Mod modToDownload = catalogModItemViewModel.Mod;
            ModStatus status = Sys.GetStatus(modToDownload.ID);

            if (status == ModStatus.Downloading)
            {
                MessageBox.Show($"{modToDownload.Name} is already downloading!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (status == ModStatus.Updating)
            {
                MessageBox.Show($"{modToDownload.Name} is already updating!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (status == ModStatus.Installed)
            {
                MessageBox.Show($"{modToDownload.Name} is already downloaded and installed!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Sys.GetStatus(modToDownload.ID) == ModStatus.NotInstalled)
            {
                Install.DownloadAndInstall(modToDownload);
            }

            List<Mod> required = new List<Mod>();
            List<string> notFound = new List<string>();
            foreach (ModRequirement req in modToDownload.Requirements)
            {
                InstalledItem inst = Sys.Library.GetItem(req.ModID);

                if (inst != null)
                {
                    continue;
                }

                Mod rMod = Sys.GetModFromCatalog(req.ModID);

                if (rMod != null)
                    required.Add(rMod);
                else
                    notFound.Add(req.Description);
            }

            if (required.Any())
            {
                if (MessageBox.Show(String.Format(_msgDownloadReq, String.Join("\n", required.Select(m => m.Name))), "Requirements", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (Mod rMod in required)
                    {
                        Install.DownloadAndInstall(rMod);
                    }
                }
            }

            if (notFound.Any())
            {
                MessageBox.Show(String.Format(_msgMissingReq, String.Join("\n", notFound)), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Download(string link, string file, string description, Install.InstallProcedure iproc, Action onCancel)
        {
            Download(new[] { link }, file, description, iproc, onCancel);
        }

        public void Download(IEnumerable<string> links, string file, string description, Install.InstallProcedure iproc, Action onCancel)
        {
            string link = links.First();
            LocationType type;
            string location;

            if (!LocationUtil.Parse(link, out type, out location)) return;

            Action onError = () =>
            {
                iproc.Error?.Invoke(new Exception($"Failed {description}"));
            };

            if (links.Count() > 1)
            {
                onError = () =>
                {
                    Log.Write($"Downloading {file} - switching to backup url {links.ElementAt(1)}");
                    Download(links.Skip(1), file, description, iproc, onCancel);
                };
            }

            DownloadItemViewModel newDownload = new DownloadItemViewModel()
            {
                ItemName = description,
                IProc = iproc,
                OnCancel = onCancel,
                OnError = onError,
                DownloadSpeed = "Calculating ..."
            };


            // invoking on current App dispatcher to update the list from UI instead of background thread....
            AddToDownloadList(newDownload);

            switch (type)
            {
                case LocationType.Url:
                    using (var wc = new System.Net.WebClient())
                    {
                        newDownload.PerformCancel = () =>
                        {
                            wc.CancelAsync();
                            newDownload.OnCancel?.Invoke();
                        };
                        wc.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
                        wc.DownloadFileCompleted += new AsyncCompletedEventHandler(_wc_DownloadFileCompleted);
                        wc.DownloadFileAsync(new Uri(location), file, newDownload);
                    }

                    break;

                case LocationType.GDrive:
                    var gd = new GDrive();
                    newDownload.PerformCancel = () => 
                    {
                        gd.CancelAsync();
                        newDownload.OnCancel?.Invoke();
                    };
                    gd.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
                    gd.DownloadFileCompleted += new AsyncCompletedEventHandler(_wc_DownloadFileCompleted);
                    gd.Download(location, file, newDownload);
                    break;

                case LocationType.MegaSharedFolder:
                    string[] parts = location.Split(',');

                    MegaIros mega;

                    if (!_megaFolders.TryGetValue(parts[0], out mega) || mega.Dead)
                    {
                        _megaFolders[parts[0]] = mega = new MegaIros(parts[0], String.Empty);
                    }
                    DownloadItemViewModel item = newDownload;
                    MegaIros.Transfer tfr = null;

                    tfr = mega.Download(parts[1], parts[2], file, () =>
                    {
                        switch (tfr.State)
                        {
                            case MegaIros.TransferState.Complete:
                                ProcessDownloadComplete(item, new AsyncCompletedEventArgs(null, false, item));
                                break;

                            case MegaIros.TransferState.Failed:
                                RemoveFromDownloadList(item);
                                Sys.Message(new WMessage() { Text = "Error downloading " + item.ItemName });
                                onCancel?.Invoke();
                                break;

                            case MegaIros.TransferState.Canceled:
                                RemoveFromDownloadList(item);
                                Sys.Message(new WMessage() { Text = $"{item.ItemName} was canceled" });
                                break;

                            default:
                                UpdateDownloadProgress(item, (int)(100 * tfr.Complete / tfr.Size), tfr.Complete);
                                break;
                        }
                    });

                    mega.ConfirmStartTransfer();
                    newDownload.PerformCancel = () =>
                    {
                        mega.CancelDownload(tfr);
                        newDownload.OnCancel?.Invoke();
                    };
                    break;
            }


        }

        private void AddToDownloadList(DownloadItemViewModel newDownload)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                lock (_downloadLock)
                {
                    if (DownloadList.All(d => d.UniqueId != newDownload.UniqueId))
                    {
                        DownloadList.Add(newDownload);
                    }
                }
            });
        }

        internal void UpdateModDetails(Guid modID)
        {
            CatalogModItemViewModel foundMod = null;

            lock (_listLock)
            {
                foundMod = CatalogModList.Where(m => m.Mod.ID == modID).FirstOrDefault();
            }

            foundMod?.UpdateDetails();
        }

        void _wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadItemViewModel item = (DownloadItemViewModel)e.UserState;
            if (e.Cancelled)
            {
                item.OnCancel?.Invoke();
                RemoveFromDownloadList(item);
            }
            else if (e.Error != null)
            {
                item.OnError?.Invoke();
                RemoveFromDownloadList(item);
                string msg = $"Error {item.ItemName} - {e.Error.Message}";
                Sys.Message(new WMessage() { Text = msg });
            }
            else
            {
                ProcessDownloadComplete(item, e);
            }
        }

        private void RemoveFromDownloadList(DownloadItemViewModel item)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                lock (_downloadLock)
                {
                    if (DownloadList.Any(d => d.UniqueId == item.UniqueId))
                    {
                        DownloadList.Remove(item);
                    }
                }
            });
        }

        void _wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            DownloadItemViewModel item = (DownloadItemViewModel)e.UserState;
            int prog = e.ProgressPercentage;
            if ((e.TotalBytesToReceive < 0) && (sender is GDrive))
            {
                prog = (int)(100 * e.BytesReceived / (sender as GDrive).GetContentLength());
            }
            UpdateDownloadProgress(item, prog, e.BytesReceived);
        }

        private void CompleteIProc(DownloadItemViewModel item, AsyncCompletedEventArgs e)
        {
            item.IProc.DownloadComplete(e);
            RemoveFromDownloadList(item);
        }

        private void ProcessDownloadComplete(DownloadItemViewModel item, AsyncCompletedEventArgs e)
        {
            // wire-up error action to also remove the item from the download list
            Action<Exception> existingErrorAction = item.IProc.Error;
            item.IProc.Error = ex =>
            {
                existingErrorAction(ex);
                RemoveFromDownloadList(item);
            };

            item.IProc.Complete = () => CompleteIProc(item, e);

            item.PercentComplete = 0;
            item.IProc.SetPCComplete = i =>
            {
                if (item.PercentComplete != i)
                {
                    item.PercentComplete = i;
                }
            };

            item.IProc.Schedule();
        }

        private void UpdateDownloadProgress(DownloadItemViewModel item, int percentDone, long bytesReceived)
        {
            if (item.PercentComplete != percentDone)
            {
                item.PercentComplete = percentDone;
            }

            TimeSpan interval = DateTime.Now - item.LastCalc;

            if ((interval.TotalSeconds >= 5))
            {
                if (bytesReceived > 0)
                {
                    item.DownloadSpeed = (((bytesReceived - item.LastBytes) / 1024.0) / interval.TotalSeconds).ToString("0.0") + "KB/s";
                    item.LastBytes = bytesReceived;
                }

                item.LastCalc = DateTime.Now;
            }
        }


        public void BringToFront()
        {
            return;
        }

        #endregion
    }
}
