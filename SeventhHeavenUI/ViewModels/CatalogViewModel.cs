using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
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

        private List<CatalogModItemViewModel> _catalogModList;
        private ObservableCollection<DownloadItemViewModel> _downloadList;

        private List<FilterItemViewModel> _previousCategoryFilters;
        private string _previousSearchText;

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
        internal void ReloadModList(string searchText = "", IEnumerable<FilterItemViewModel> categories = null)
        {
            List<Mod> results;

            if (categories == null && _previousCategoryFilters != null)
            {
                categories = _previousCategoryFilters;
            }
            else if (categories == null && _previousCategoryFilters == null)
            {
                categories = new List<FilterItemViewModel>();
                _previousCategoryFilters = categories.ToList();
            }
            else if (categories != null)
            {
                _previousCategoryFilters = categories.ToList();
            }

            if (string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(_previousSearchText))
            {
                searchText = _previousSearchText;
            }
            else if (string.IsNullOrEmpty(searchText) && string.IsNullOrEmpty(_previousSearchText))
            {
                searchText = "";
                _previousSearchText = "";
            }
            else if (!string.IsNullOrEmpty(searchText))
            {
                _previousSearchText = searchText;
            }


            if (String.IsNullOrEmpty(searchText))
            {
                results = Sys.Catalog.Mods.Where(m => FilterByCategory(m, categories)).ToList();
            }
            else
            {
                results = Sys.Catalog.Mods.Where(m => FilterByCategory(m, categories))
                                          .Select(m => new { Mod = m, Relevance = m.SearchRelevance(searchText) })
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

            // make sure to set CatalogModList on the UI thread
            // ... due to uncaught exception that can be thrown when modifying on background thread
            App.Current.Dispatcher.Invoke(() =>
            {
                lock (_listLock)
                {
                    CatalogModList.Clear();
                    CatalogModList = newList;
                }
            });
        }

        /// <summary>
        /// Returns true if <paramref name="mod"/>.Category is found in <paramref name="categories"/>.
        /// Returns true if <paramref name="categories"/> is empty.
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="categories"></param>
        /// <returns></returns>
        bool FilterByCategory(Mod mod, IEnumerable<FilterItemViewModel> categories)
        {
            string modCategory = mod.Category ?? mod.LatestVersion.Category;

            return categories.Count() == 0 ||
                   categories.Any(c => c.Name == modCategory) ||
                   (categories.Any(c => c.Name == "Unknown") && string.IsNullOrEmpty(modCategory));
        }

        internal void ClearRememberedSearchTextAndCategories()
        {
            _previousSearchText = "";
            _previousCategoryFilters = null;
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

        #region Methods Related to Downloads

        internal void CancelDownload(DownloadItemViewModel downloadItemViewModel)
        {
            downloadItemViewModel?.PerformCancel();
            RemoveFromDownloadList(downloadItemViewModel);
            Sys.Message(new WMessage($"Canceled download: {downloadItemViewModel.ItemName}"));
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

            Action onError = null;

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
                        newDownload.PerformCancel = wc.CancelAsync;
                        wc.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
                        wc.DownloadFileCompleted += new AsyncCompletedEventHandler(_wc_DownloadFileCompleted);
                        wc.DownloadFileAsync(new Uri(location), file, newDownload);
                    }

                    break;

                case LocationType.GDrive:
                    var gd = new GDrive();
                    newDownload.PerformCancel = gd.CancelAsync;
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
                    newDownload.PerformCancel = () => mega.CancelDownload(tfr);
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
                string msg = "Error " + item.ItemName + e.Error.Message;
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
            RemoveFromDownloadList(item);

            item.IProc.DownloadComplete(e);
            NotifyPropertyChanged();
        }

        private void ProcessDownloadComplete(DownloadItemViewModel item, AsyncCompletedEventArgs e)
        {
            item.IProc.Error = ex =>
            {
                RemoveFromDownloadList(item);
                Sys.Message(new WMessage() { Text = $"Error {item.ItemName}: {e.ToString()}" });
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
