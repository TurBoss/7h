using _7thHeaven.Code;
using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using SeventhHeaven.Classes;
using SeventhHeaven.Windows;
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
            @"This mod requires the following mods to also be installed, but they could not be found in any catalog:
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
        /// Ordered by Catalog Subscription, Category, Name
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

            // map the order of the subscriptions so the results can honor the list order when sorting
            Dictionary<string, int> subscriptionOrder = new Dictionary<string, int>();
            for (int i = 0; i < Sys.Settings.Subscriptions.Count; i++)
            {
                subscriptionOrder[Sys.Settings.Subscriptions[i].Url] = i;
            }

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
                    bool isRelevant = m.SearchRelevance(searchText) > 0;

                    if (categories.Count() > 0 && tags.Count() > 0)
                    {
                        return FilterItemViewModel.FilterByCategory(m, categories) || FilterItemViewModel.FilterByTags(m, tags) || isRelevant;
                    }
                    else if (categories.Count() > 0)
                    {
                        return FilterItemViewModel.FilterByCategory(m, categories) || isRelevant;
                    }
                    else if (tags.Count() > 0)
                    {
                        return FilterItemViewModel.FilterByTags(m, tags) || isRelevant;
                    }
                    else
                    {
                        return isRelevant;
                    }
                }).ToList();
            }

            List<CatalogModItemViewModel> newList = new List<CatalogModItemViewModel>();

            foreach (Mod m in results.OrderBy(k =>
                                        {
                                            subscriptionOrder.TryGetValue(k.SourceCatalogUrl, out int sortOrder);
                                            return sortOrder;
                                        })
                                     .ThenBy(k => k.Category)
                                     .ThenBy(k => k.Name))
            {
                CatalogModItemViewModel item = new CatalogModItemViewModel(m);
                newList.Add(item);
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

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        SetCatalogList(newList);
                    }

                    return;
                }

                SetCatalogList(newList);
            });
        }

        private void SetCatalogList(List<CatalogModItemViewModel> newList)
        {
            lock (_listLock)
            {
                CatalogModList.Clear();
                CatalogModList = newList;
            }
        }

        internal void ClearRememberedSearchTextAndCategories()
        {
            _previousReloadOptions = new ReloadListOption();
        }

        internal void RefreshCatalogList()
        {
            ClearRememberedSearchTextAndCategories();
            ForceCheckCatalogUpdateAsync();
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
            object countLock = new object();

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
                    if (File.Exists(catFile))
                    {
                        File.Delete(catFile);
                    }
                    ReloadModList();
                    return;
                }

                foreach (string subUrl in Sys.Settings.SubscribedUrls.ToArray())
                {
                    Subscription sub = Sys.Settings.Subscriptions.Find(s => s.Url.Equals(subUrl, StringComparison.InvariantCultureIgnoreCase));
                    if (sub == null)
                    {
                        sub = new Subscription() { Url = subUrl, FailureCount = 0, LastSuccessfulCheck = DateTime.MinValue };
                        Sys.Settings.Subscriptions.Add(sub);
                    }

                    if ((sub.LastSuccessfulCheck < DateTime.Now.AddDays(-1)) || options.ForceCheck)
                    {
                        Logger.Info($"Checking subscription {sub.Url}");

                        string uniqueFileName = $"cattemp{Path.GetRandomFileName()}.xml"; // save temp catalog update to unique filename so multiple catalog updates can download async
                        string path = Path.Combine(Sys.SysFolder, "temp", uniqueFileName);

                        DownloadItem download = new DownloadItem()
                        {
                            Links = new List<string>() { subUrl },
                            SaveFilePath = path,
                            Category = DownloadCategory.Catalog,
                            ItemName = $"Checking catalog {subUrl}"
                        };

                        download.IProc = new Install.InstallProcedureCallback(e =>
                        {
                            bool success = (e.Error == null && e.Cancelled == false);
                            subUpdateCount++;

                            if (success)
                            {
                                try
                                {
                                    Catalog c = Util.Deserialize<Catalog>(path);

                                    // set the catalog name of where the mod came from for filtering later
                                    string sourceCatalogName = sub.Name;
                                    if (string.IsNullOrWhiteSpace(sourceCatalogName))
                                    {
                                        sourceCatalogName = c.Name;
                                    }

                                    c.Mods.ForEach(m =>
                                    {
                                        m.SourceCatalogName = sourceCatalogName;
                                        m.SourceCatalogUrl = subUrl;
                                    });


                                    lock (Sys.CatalogLock) // put a lock on the Catalog so multiple threads can only merge one at a time
                                    {
                                        Sys.Catalog = Catalog.Merge(Sys.Catalog, c, out pingIDs);

                                        using (FileStream fs = new FileStream(catFile, FileMode.Create))
                                        {
                                            Util.Serialize(Sys.Catalog, fs);
                                        }
                                    }

                                    Sys.Message(new WMessage() { Text = $"Updated catalog from {subUrl}" });

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
                                    Sys.Message(new WMessage() { Text = $"Failed to load subscription {subUrl}: {ex.Message}" });
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
                            bool isDoneDownloading = false;

                            lock (countLock)
                            {
                                isDoneDownloading = (subUpdateCount == subTotalCount);
                            }

                            if (isDoneDownloading)
                            {
                                ReloadModList(GetSelectedMod()?.Mod.ID);
                                ScanForModUpdates();
                                RefreshListRequested?.Invoke();
                            }

                        });

                        Sys.Downloads.AddToDownloadQueue(download);
                    }
                    else
                    {
                        lock (countLock)
                        {
                            subTotalCount -= 1; // This catalog does not have to be updated
                        }
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
            if (downloadItemViewModel?.Download?.PerformCancel != null)
            {
                downloadItemViewModel.Download.PerformCancel?.Invoke(); // PerformCancel will happen during download and internally calls OnCancel
            }
            else
            {
                downloadItemViewModel.Download.OnCancel?.Invoke(); // invoke OnCancel when downloads are pending so the full cancellation process can happen
            }

            RemoveFromDownloadList(downloadItemViewModel?.Download);
            Sys.Message(new WMessage($"Canceled {downloadItemViewModel?.ItemName}"));
        }

        internal void DownloadMod(CatalogModItemViewModel catalogModItemViewModel)
        {
            Mod modToDownload = catalogModItemViewModel.Mod;
            ModStatus status = Sys.GetStatus(modToDownload.ID);

            if (status == ModStatus.Downloading)
            {
                MessageDialogWindow.Show($"{modToDownload.Name} is already downloading!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (status == ModStatus.Updating)
            {
                MessageDialogWindow.Show($"{modToDownload.Name} is already updating!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (status == ModStatus.Installed)
            {
                MessageDialogWindow.Show($"{modToDownload.Name} is already downloaded and installed!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                if (MessageDialogWindow.Show(String.Format(_msgDownloadReq, String.Join("\n", required.Select(m => m.Name))), "Requirements", MessageBoxButton.YesNo, MessageBoxImage.Question).Result == MessageBoxResult.Yes)
                {
                    foreach (Mod rMod in required)
                    {
                        Install.DownloadAndInstall(rMod);
                    }
                }
            }

            if (notFound.Any())
            {
                MessageDialogWindow.Show(String.Format(_msgMissingReq, String.Join("\n", notFound)), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Download(string link, DownloadItem downloadInfo)
        {
            Download(new List<string>() { link }, downloadInfo);
        }

        public void Download(IEnumerable<string> links, DownloadItem downloadInfo)
        {
            downloadInfo.HasStarted = true;

            string link = links.First();
            LocationType type;
            string location;

            if (!LocationUtil.TryParse(link, out type, out location)) return;

            Action onError = () =>
            {
                RemoveFromDownloadList(downloadInfo);
                downloadInfo.IProc.Error?.Invoke(new Exception($"Failed {downloadInfo.ItemName}"));
            };

            if (links.Count() > 1)
            {
                onError = () =>
                {
                    Sys.Message(new WMessage($"Downloading {downloadInfo.SaveFilePath} - switching to backup url {links.ElementAt(1)}"));
                    Download(links.Skip(1), downloadInfo);
                };
            }

            downloadInfo.OnError = onError;

            try
            {
                switch (type)
                {
                    case LocationType.Url:
                        using (var wc = new System.Net.WebClient())
                        {
                            downloadInfo.PerformCancel = () =>
                            {
                                wc.CancelAsync();
                                downloadInfo.OnCancel?.Invoke();
                            };
                            wc.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
                            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(_wc_DownloadFileCompleted);
                            wc.DownloadFileAsync(new Uri(location), downloadInfo.SaveFilePath, downloadInfo);
                        }

                        break;

                    case LocationType.GDrive:
                        var gd = new GDrive();
                        downloadInfo.PerformCancel = () =>
                        {
                            gd.CancelAsync();
                            downloadInfo.OnCancel?.Invoke();
                        };
                        gd.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
                        gd.DownloadFileCompleted += new AsyncCompletedEventHandler(_wc_DownloadFileCompleted);
                        gd.Download(location, downloadInfo.SaveFilePath, downloadInfo);
                        break;

                    case LocationType.MegaSharedFolder:
                        string[] parts = location.Split(',');

                        MegaIros mega;

                        if (!_megaFolders.TryGetValue(parts[0], out mega) || mega.Dead)
                        {
                            _megaFolders[parts[0]] = mega = new MegaIros(parts[0], String.Empty);
                        }
                        MegaIros.Transfer tfr = null;

                        tfr = mega.Download(parts[1], parts[2], downloadInfo.SaveFilePath, () =>
                        {
                            switch (tfr.State)
                            {
                                case MegaIros.TransferState.Complete:
                                    ProcessDownloadComplete(downloadInfo, new AsyncCompletedEventArgs(null, false, downloadInfo));
                                    break;

                                case MegaIros.TransferState.Failed:
                                    Sys.Message(new WMessage() { Text = "Error downloading " + downloadInfo.ItemName });
                                    downloadInfo.OnError?.Invoke();
                                    break;

                                case MegaIros.TransferState.Canceled:
                                    RemoveFromDownloadList(downloadInfo);
                                    Sys.Message(new WMessage() { Text = $"{downloadInfo.ItemName} was canceled" });
                                    break;

                                default:
                                    UpdateDownloadProgress(downloadInfo, (int)(100 * tfr.Complete / tfr.Size), tfr.Complete, tfr.Size);
                                    break;
                            }
                        });

                        mega.ConfirmStartTransfer();
                        downloadInfo.PerformCancel = () =>
                        {
                            mega.CancelDownload(tfr);
                            downloadInfo.OnCancel?.Invoke();
                        };
                        break;
                }
            }
            catch (Exception e)
            {
                string msg = $"Error {downloadInfo.ItemName} - {e.Message}";
                Sys.Message(new WMessage(msg, WMessageLogLevel.Error, e));
                downloadInfo.OnError?.Invoke();
            }


        }

        public void AddToDownloadQueue(DownloadItem newDownload)
        {
            int downloadCount = 0;

            if (newDownload.OnCancel == null)
            {
                newDownload.OnCancel = () =>
                {
                    RemoveFromDownloadList(newDownload);
                };
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                lock (_downloadLock)
                {
                    if (DownloadList.All(d => d.Download.UniqueId != newDownload.UniqueId))
                    {
                        DownloadList.Add(new DownloadItemViewModel(newDownload));
                    }

                    downloadCount = DownloadList.Count;
                }
            });


            if (downloadCount == 1)
            {
                // only item in queue so start downloading right away
                Download(newDownload.Links, newDownload);
            }
            else if (downloadCount > 1)
            {
                // allow images and catalogs to download while mod is downloading so it does not halt queue of catalog/image refreshes
                DownloadItem nextDownload = null;
                bool isAlreadyDownloadingImageOrCat = false;

                lock (_downloadLock)
                {
                    if (DownloadList[0].Download.Category == DownloadCategory.Mod && DownloadList[0].Download.HasStarted)
                    {
                        isAlreadyDownloadingImageOrCat = DownloadList.Any(d => d.Download.Category != DownloadCategory.Mod && d.Download.HasStarted);
                        nextDownload = DownloadList.FirstOrDefault(d => d.Download.Category != DownloadCategory.Mod && !d.Download.HasStarted)?.Download;
                    }
                }

                //start the next catalog/image download if no other catalogs/images are being downloaded
                if (!isAlreadyDownloadingImageOrCat && nextDownload != null)
                {
                    Download(nextDownload.Links, nextDownload);
                }
            }
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
            DownloadItem item = (DownloadItem)e.UserState;
            if (e.Cancelled)
            {
                item.PerformCancel?.Invoke();
                RemoveFromDownloadList(item);
            }
            else if (e.Error != null)
            {
                string msg = $"Error {item.ItemName} - {e.Error.GetBaseException().Message}";
                Sys.Message(new WMessage(msg, WMessageLogLevel.Error, e.Error.GetBaseException()));

                item.OnError?.Invoke();
            }
            else
            {
                ProcessDownloadComplete(item, e);
            }
        }

        private void RemoveFromDownloadList(DownloadItem item)
        {
            int downloadCount = 0;
            bool isDownloadingMod = false;


            App.Current.Dispatcher.Invoke(() =>
            {
                lock (_downloadLock)
                {
                    if (DownloadList.Any(d => d.Download.UniqueId == item.UniqueId))
                    {
                        DownloadItemViewModel viewModel = DownloadList.FirstOrDefault(d => d.Download.UniqueId == item.UniqueId);
                        DownloadList.Remove(viewModel);
                    }

                    downloadCount = DownloadList.Count;
                    isDownloadingMod = DownloadList.Any(d => d.Download.Category == DownloadCategory.Mod && d.Download.HasStarted);
                }
            });

            if (downloadCount > 0)
            {
                // start next download in queue
                DownloadItem nextDownload = null;

                lock (_downloadLock)
                {
                    if (!isDownloadingMod)
                    {
                        // no mod is currently downloading so get the next download in queue
                        nextDownload = DownloadList.FirstOrDefault(d => !d.Download.HasStarted)?.Download;
                    }
                    else
                    {
                        // a mod is downloading so get next catalog/image to download
                        nextDownload = DownloadList.FirstOrDefault(d => d.Download.Category != DownloadCategory.Mod && !d.Download.HasStarted)?.Download;
                    }
                }

                if (nextDownload != null)
                {
                    Download(nextDownload.Links, nextDownload);
                }
            }
        }

        void _wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            DownloadItem item = (DownloadItem)e.UserState;

            long totalBytes = e.TotalBytesToReceive;

            if (item.Category == DownloadCategory.Image && e.TotalBytesToReceive > 3 * 1000000)
            {
                Logger.Warn("preview image greater than 3MB, cancelling download");
                item.PerformCancel?.Invoke();
                return;
            }

            int prog = e.ProgressPercentage;
            if ((e.TotalBytesToReceive < 0) && (sender is GDrive))
            {
                totalBytes = (sender as GDrive).GetContentLength();
                prog = (int)(100 * e.BytesReceived / totalBytes);
            }

            UpdateDownloadProgress(item, prog, e.BytesReceived, totalBytes);
        }

        private void CompleteIProc(DownloadItem item, AsyncCompletedEventArgs e)
        {
            item.IProc.DownloadComplete(e);
            RemoveFromDownloadList(item);
        }

        private void ProcessDownloadComplete(DownloadItem item, AsyncCompletedEventArgs e)
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

        private void UpdateDownloadProgress(DownloadItem item, int percentDone, long bytesReceived, long totalBytes)
        {
            DownloadItemViewModel viewModel = DownloadList.FirstOrDefault(d => d.Download.UniqueId == item.UniqueId);

            if (viewModel == null)
            {
                return;
            }

            viewModel.PercentComplete = percentDone;

            TimeSpan interval = DateTime.Now - item.LastCalc;

            if ((interval.TotalSeconds >= 3))
            {
                if (bytesReceived > 0)
                {
                    double estimatedSpeed = (((bytesReceived - item.LastBytes) / 1024.0) / interval.TotalSeconds); // estimated speed in KB/s

                    if ((estimatedSpeed / 1024.0) > 1.0)
                    {
                        // show speed in MB/s
                        viewModel.DownloadSpeed = (estimatedSpeed / 1024.0).ToString("0.0") + "MB/s";
                    }
                    else
                    {
                        // show speed in KB/s
                        viewModel.DownloadSpeed = estimatedSpeed.ToString("0.0") + "KB/s";
                    }

                    double estimatedSecondsLeft = ((totalBytes - bytesReceived) / 1024.0) / estimatedSpeed; // divide bytes by 1024 to get total KB

                    if ((estimatedSecondsLeft / 60) > 60)
                    {
                        // show in hours
                        viewModel.RemainingTime = $"{(estimatedSecondsLeft / 60) / 60: 0.0} hours";
                    }
                    else if (estimatedSecondsLeft > 60)
                    {
                        // show in minutes
                        viewModel.RemainingTime = $"{estimatedSecondsLeft / 60: 0.0} min";
                    }
                    else
                    {
                        viewModel.RemainingTime = $"{estimatedSecondsLeft: 0.0} sec";
                    }

                    item.LastBytes = bytesReceived;
                }

                item.LastCalc = DateTime.Now;
            }
        }

        #endregion
    }
}
