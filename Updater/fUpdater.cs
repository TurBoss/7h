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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Updater {
    public partial class fUpdater : Form {
        private string _sys, _7h;

        protected void Process(object _) {
            try {
                string ufile = System.IO.Path.Combine(_sys, "AutoUpdate.zip");
                if (!System.IO.File.Exists(ufile)) {
                    MessageLaunchAndExit("Couldn't find update file."); return;
                }
                byte[] buffer = new byte[0x10000];
                using (SharpCompress.Archives.Zip.ZipArchive zip = SharpCompress.Archives.Zip.ZipArchive.Open(ufile)) {
                    foreach (var entry in zip.Entries.Where(e => !e.IsDirectory)) {
                        using (var s = entry.OpenEntryStream()) {
                            Status("Updating " + entry.Key);
                            foreach (int i in Enumerable.Range(0, 5)) {
                                try {
                                    using (var fs = new System.IO.FileStream(System.IO.Path.Combine(_7h, entry.Key), System.IO.FileMode.Create)) {
                                        int len;
                                        while ((len = s.Read(buffer, 0, buffer.Length)) != 0)
                                            fs.Write(buffer, 0, len);
                                    }
                                    break;
                                } catch (System.IO.IOException) {
                                    if (i < 4) {
                                        Status( entry.Key + " ...file in use; waiting.");
                                        System.Threading.Thread.Sleep(1000);
                                        continue;
                                    }
                                }
                                if (i == 4) {
                                    MessageLaunchAndExit("Update incomplete - could not update all files");
                                }
                            }
                        }
                    }
                }
                Status("Update complete, removing download");
                System.IO.File.Delete(ufile);
                MessageLaunchAndExit("Update complete!");
            } catch (Exception ex) {
                MessageLaunchAndExit("Error applying update: " + ex.ToString());
            }
        }

        private void Status(string msg) {
            Action a = () => {
                int i = txtStatus.Text.Length;
                txtStatus.Text += msg + "\r\n";
                txtStatus.SelectionStart = i;
                txtStatus.ScrollToCaret();
            };
            Invoke((Delegate)a);
        }
        private void MessageLaunchAndExit(string msg) {
            if (InvokeRequired) {
                this.Invoke((Delegate)(Action)(() => MessageLaunchAndExit(msg)));
            } else {
                MessageBox.Show(msg);
                System.Diagnostics.Process.Start(System.IO.Path.Combine(_7h, "7thHeaven.exe"));
                Application.Exit();
            }
        }

        public fUpdater() {
            InitializeComponent();
        }

        private void fUpdater_Load(object sender, EventArgs e) {

            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); 
            _sys = System.IO.Path.Combine(appPath, "7thWorkshop");
            _7h = appPath;

            System.Threading.ThreadPool.QueueUserWorkItem(Process);
        }
    }
}
