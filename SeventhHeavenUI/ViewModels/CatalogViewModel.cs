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

namespace SeventhHeavenUI.ViewModels
{
    /// <summary>
    /// ViewModel to contain interaction logic for the 'My Mods' tab user control.
    /// </summary>
    public class CatalogViewModel : ViewModelBase, IDownloader
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public delegate void OnSelectionChanged(object sender, InstalledModViewModel selected);
        public event OnSelectionChanged SelectedModChanged;

        private List<InstalledModViewModel> _modList;
        private ObservableCollection<DownloadItemViewModel> _downloadList;

        private Dictionary<string, MegaIros> _megaFolders = new Dictionary<string, MegaIros>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// List of installed mods (includes active mods in the currently active profile)
        /// </summary>
        public List<InstalledModViewModel> CatalogModList
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
        internal void RaiseSelectedModChanged(object sender, InstalledModViewModel selected)
        {
            SelectedModChanged?.Invoke(this, selected);
        }

        /// <summary>
        /// Loads installed and active mods into <see cref="CatalogModList"/> from <see cref="Sys.Library"/> and <see cref="Sys.ActiveProfile"/>
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

            CatalogModList = allMods;
        }

        /// <summary>
        /// Loads active mods into <see cref="CatalogModList"/> from <see cref="Sys.ActiveProfile"/>
        /// </summary>
        /// <param name="clearList"> removes any active mods in <see cref="CatalogModList"/> before reloading </param>
        internal void ReloadActiveMods(bool clearList)
        {
            Sys.ActiveProfile.Items.RemoveAll(i => Sys.Library.GetItem(i.ModID) == null);

            if (clearList)
            {
                CatalogModList.RemoveAll(m => m.IsActive);
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

            CatalogModList.AddRange(activeMods);
            NotifyPropertyChanged(nameof(CatalogModList));
        }

        /// <summary>
        /// Returns selected view model in <see cref="CatalogModList"/>.
        /// </summary>
        public InstalledModViewModel GetSelectedMod()
        {
            InstalledModViewModel selected = CatalogModList.Where(m => m.IsSelected).LastOrDefault();

            // due to virtualization, IsSelected could be set on multiple items... 
            // ... so we will deselect the other items to avoid problems of multiple items being selected
            if (CatalogModList.Where(m => m.IsSelected).Count() > 1)
            {
                foreach (var mod in CatalogModList.Where(m => m.IsSelected && m.InstallInfo.ModID != selected.InstallInfo.ModID))
                {
                    mod.IsSelected = false;
                }
            }

            return selected;
        }


        #region Methods Related to Downloads

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

            if (links.Count() > 1)
            {
                onCancel = () => {
                    Log.Write($"Downloading {file} - switching to backup url {links.ElementAt(1)}");
                    Download(links.Skip(1), file, description, iproc, onCancel);
                };
            }

            DownloadItemViewModel newDownload = new DownloadItemViewModel()
            {
                ItemName = description,
                IProc = iproc,
                OnCancel = onCancel,
                DownloadSpeed = "Calculating ..."
            };

            DownloadList.Add(newDownload);

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

                    tfr = mega.Download(parts[1], parts[2], file, () => {
                        switch (tfr.State)
                        {
                            case MegaIros.TransferState.Complete:
                                ProcessDownloadComplete(item, new AsyncCompletedEventArgs(null, false, item));
                                break;

                            case MegaIros.TransferState.Failed:
                                DownloadList.Remove(item);

                                Sys.Message(new WMessage() { Text = "Error downloading " + item.ItemName });
                                onCancel?.Invoke();
                                break;

                            case MegaIros.TransferState.Canceled:
                                DownloadList.Remove(item);
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

        void _wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadItemViewModel item = (DownloadItemViewModel)e.UserState;
            if (e.Cancelled)
            {
                item.OnCancel?.Invoke();
                DownloadList.Remove(item);
            }
            else if (e.Error != null)
            {
                item.OnCancel?.Invoke();
                DownloadList.Remove(item);
                string msg = "Error " + item.ItemName + e.Error.Message;
                Sys.Message(new WMessage() { Text = msg });
            }
            else
            {
                ProcessDownloadComplete(item, e);
            }
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
            DownloadList.Remove(item);
            item.IProc.DownloadComplete(e);
            NotifyPropertyChanged();
        }

        private void ProcessDownloadComplete(DownloadItemViewModel item, AsyncCompletedEventArgs e)
        {
            item.IProc.Error = ex => {
                DownloadList.Remove(item);
                Sys.Message(new WMessage() { Text = $"Error {item.ItemName}: {e.ToString()}" });
            };

            item.IProc.Complete = () => CompleteIProc(item, e);

            item.PercentComplete = 0;
            item.IProc.SetPCComplete = i => {
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
