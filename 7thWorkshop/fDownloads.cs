/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iros._7th.Workshop {
    partial class fDownloads : Form, IDownloader {

        public fDownloads() {
            InitializeComponent();
        }

        private enum DownloadMode {
            Download,
            Install,
        }

        private class DownloadItem {
            public Install.InstallProcedure IProc;
            public ListViewItem LVI;
            public double PCComplete;

            public DateTime LastCalc;
            public long LastBytes;

            public Action PerformCancel;
            public Action OnCancel;

            public DownloadItem() {
                LastCalc = DateTime.Now;
            }
        }

        /*
        public void DownloadString(string url, string description, Install.InstallProcedure iproc) {
            var lvi = lvDownloads.Items.Add(description);
            lvi.Tag = new DownloadItem() { IProc = iproc, LVI = lvi };
            lvi.SubItems.Add("DummyProgress");
            lvi.SubItems.Add("Calculating...");
            var wc = new System.Net.WebClient();
            wc.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
            wc.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
            wc.DownloadStringAsync(new Uri(url), lvi.Tag);
        }
         */

        private void CompleteIProc(DownloadItem item, AsyncCompletedEventArgs e) {
            this.Invoke((Delegate)(Action)(() => {
                lvDownloads.Items.Remove(item.LVI);
                item.IProc.DownloadComplete(e);
            }));
        }

        void wc_DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e) {
            var item = (DownloadItem)e.UserState;
            ProcessDownloadComplete(item, e);
        }

        private Dictionary<string, Iros.Mega.MegaIros> _megaFolders = new Dictionary<string, Mega.MegaIros>(StringComparer.InvariantCultureIgnoreCase);

        public void Download(string link, string file, string description, Install.InstallProcedure iproc, Action onCancel) {
            Download(new[] { link }, file, description, iproc, onCancel);
        }
        public void Download(IEnumerable<string> links, string file, string description, Install.InstallProcedure iproc, Action onCancel) {
            string link = links.First();
            LocationType type; string location;
            if (!LocationUtil.Parse(link, out type, out location)) return;

            if (!this.Visible) this.Show();
            this.BringToFront();

            if (links.Count() > 1) {
                onCancel = () => {
                    Log.Write(String.Format("Downloading {0} - switching to backup url {1}", file, links.ElementAt(1)));
                    Download(links.Skip(1), file, description, iproc, onCancel);
                };
            }

            var lvi = lvDownloads.Items.Add(description);
            lvi.Tag = new DownloadItem() { IProc = iproc, LVI = lvi, OnCancel = onCancel };
            lvi.SubItems.Add("DummyProgress");
            lvi.SubItems.Add("Calculating...");
            switch (type) {
                case LocationType.Url:
                    var wc = new System.Net.WebClient();
                    (lvi.Tag as DownloadItem).PerformCancel = wc.CancelAsync;
                    wc.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(_wc_DownloadFileCompleted);
                    wc.DownloadFileAsync(new Uri(location), file, lvi.Tag);
                    break;
                case LocationType.GDrive:
                    var gd = new GDrive();
                    (lvi.Tag as DownloadItem).PerformCancel = gd.CancelAsync;
                    gd.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(_wc_DownloadProgressChanged);
                    gd.DownloadFileCompleted += new AsyncCompletedEventHandler(_wc_DownloadFileCompleted);
                    gd.Download(location, file, lvi.Tag);
                    break;
                case LocationType.MegaSharedFolder:
                    string[] parts = location.Split(',');
                    Iros.Mega.MegaIros mega;
                    if (!_megaFolders.TryGetValue(parts[0], out mega) || mega.Dead) {
                        _megaFolders[parts[0]] = mega = new Mega.MegaIros(parts[0], String.Empty);
                    }
                    DownloadItem item = (DownloadItem)lvi.Tag;
                    Mega.MegaIros.Transfer tfr = null;
                    tfr = mega.Download(parts[1], parts[2], file, () => {
                        this.Invoke((Delegate)(Action)(() => {
                            switch (tfr.State) {
                                case Mega.MegaIros.TransferState.Complete:
                                    ProcessDownloadComplete(item, new AsyncCompletedEventArgs(null, false, item));
                                    break;
                                case Mega.MegaIros.TransferState.Failed:
                                    lvDownloads.Items.Remove(lvi);
                                    Sys.Message(new WMessage() { Text = "Error downloading " + lvi.Text });
                                    if (onCancel != null) onCancel();
                                    break;
                                case Mega.MegaIros.TransferState.Canceled:
                                    lvDownloads.Items.Remove(lvi);
                                    Sys.Message(new WMessage() { Text = String.Format("{0} was canceled", lvi.Text) });
                                    break;
                                default:
                                    DoDownloadProgress(item, (int)(100 * tfr.Complete / tfr.Size), tfr.Complete);
                                    break;
                            }
                        }));
                    });
                    mega.ConfirmStartTransfer();
                    (lvi.Tag as DownloadItem).PerformCancel = () => mega.CancelDownload(tfr);
                    break;
            }

        }

        public static void SetDoubleBuffered(System.Windows.Forms.Control c) {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }

        private void fDownloads_Load(object sender, EventArgs e) {
            SetDoubleBuffered(lvDownloads);
        }

        private void ProcessDownloadComplete(DownloadItem item, AsyncCompletedEventArgs e) {
            item.IProc.Error = ex => {
                this.Invoke((Delegate)(Action)(() => {
                    lvDownloads.Items.Remove(item.LVI);
                    Sys.Message(new WMessage() { Text = "Error " + item.LVI.Text + ": " + e.ToString() });
                }));
            };
            item.IProc.Complete = () => CompleteIProc(item, e);
            item.LVI.SubItems[2].Text = "Installing";
            item.PCComplete = 0;
            item.IProc.SetPCComplete = i => {
                if (item.PCComplete != i) {
                    item.PCComplete = i;
                    this.Invoke((Delegate)(Action)lvDownloads.Invalidate);
                }
            };
            item.IProc.Schedule();
        }

        void _wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            var item = (DownloadItem)e.UserState;
            if (e.Cancelled) {
                if (item.OnCancel != null) item.OnCancel();
                lvDownloads.Items.Remove(item.LVI);
            } else if (e.Error != null) {
                if (item.OnCancel != null) item.OnCancel();
                lvDownloads.Items.Remove(item.LVI);
                string msg = "Error " + item.LVI.Text + e.Error.Message;                
                Sys.Message(new WMessage() { Text = msg });
            } else
                ProcessDownloadComplete(item, e);
        }

        private void DoDownloadProgress(DownloadItem item, int percentDone, long bytesReceived) {
            if (item.PCComplete != percentDone) {
                item.PCComplete = percentDone;
                lvDownloads.Invalidate();
            }
            var interval = DateTime.Now - item.LastCalc;
            if ((interval.TotalSeconds >= 5)) {
                if (bytesReceived > 0) {
                    string kbs = (((bytesReceived - item.LastBytes) / 1024.0) / interval.TotalSeconds).ToString("0.0") + "KB/s";
                    //System.Diagnostics.Debug.WriteLine("{0} bytes in {1} seconds = {2}", item.BytesSinceLast, interval.TotalSeconds, kbs);
                    item.LVI.SubItems[2].Text = kbs;
                    item.LastBytes = bytesReceived;
                }
                item.LastCalc = DateTime.Now;
                lvDownloads.Invalidate();
            }
        }

        void _wc_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e) {
            var item = (DownloadItem)e.UserState;
            int prog = e.ProgressPercentage;
            if ((e.TotalBytesToReceive < 0) && (sender is GDrive)) {
                prog = (int)(100 * e.BytesReceived / (sender as GDrive).GetContentLength());
            }
            DoDownloadProgress(item, prog, e.BytesReceived);
        }

        private void lvDownloads_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) {
            if (e.ColumnIndex == 1) {
                var item = (DownloadItem)e.Item.Tag;
                var rect = e.Bounds;
                rect.Inflate(-2, -2);
//                rect.Offset(1, 1);
                e.Graphics.DrawRectangle(Pens.Black, rect);
                rect.Inflate(-1, -1);
                rect.Offset(1, 1);
                rect.Width = (int)((rect.Width - 1) * item.PCComplete / 100);
                rect.Height -= 1;
                e.Graphics.FillRectangle(SystemBrushes.HotTrack, rect);
            } else
                e.DrawDefault = true;
        }

        private void lvDownloads_DrawItem(object sender, DrawListViewItemEventArgs e) {
            //e.DrawDefault = true;
        }

        private void lvDownloads_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) {
            e.DrawDefault = true;
        }

        private void fDownloads_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e) {
            if (lvDownloads.SelectedItems.Count > 0) {
                var cancel = lvDownloads.SelectedItems.OfType<ListViewItem>().ToArray();
                foreach (var lvi in cancel) {
                    var item = lvi.Tag as DownloadItem;
                    if (item.PerformCancel != null) item.PerformCancel();
                }
            }
        }
    }
}
