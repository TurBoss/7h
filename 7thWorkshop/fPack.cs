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
    public partial class fPack : Form {
        public fPack() {
            InitializeComponent();
        }

        private void bSourceF_Click(object sender, EventArgs e) {
            if (flFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtSourceF.Text = flFolder.SelectedPath;
        }

        private void bIroDest_Click(object sender, EventArgs e) {
            if (sfIro.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtIroDest.Text = sfIro.FileName;
        }

        private void bSourceIro_Click(object sender, EventArgs e) {
            if (ofIro.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtSourceIro.Text = ofIro.FileName;
        }

        private void bDestF_Click(object sender, EventArgs e) {
            if (flFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtDestF.Text = flFolder.SelectedPath;
        }

        private void IroProgress(double d, string s) {
            Action a = () => {
                PB.Value = (int)(100 * d);
                lStatus.Text = s;
            };
            Invoke((Delegate)a);
        }

        private void bPack_Click(object sender, EventArgs e) {
            var pbw = new BackgroundWorker();
            pbw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(pbw_RunWorkerCompleted);
            pbw.DoWork += new DoWorkEventHandler(pbw_DoWork);
            pbw.RunWorkerAsync(new IroOp() { Folder = txtSourceF.Text, Iro = txtIroDest.Text, Compress = (_7thWrapperLib.CompressType)cbCompress.SelectedItem });
            bPack.Enabled = false;
        }

        void pbw_DoWork(object sender, DoWorkEventArgs e) {
            IroOp io = e.Argument as IroOp;
            BackgroundWorker bw = sender as BackgroundWorker;
            var files = System.IO.Directory.GetFiles(io.Folder, "*", System.IO.SearchOption.AllDirectories)
                .Select(s => s.Substring(io.Folder.Length).Trim('\\', '/'))
                .ToList();
            using (var fs = new System.IO.FileStream(io.Iro, System.IO.FileMode.Create))
                _7thWrapperLib.IrosArc.Create(fs, files.Select(s => _7thWrapperLib.IrosArc.ArchiveCreateEntry.FromDisk(io.Folder, s)), _7thWrapperLib.ArchiveFlags.None, io.Compress, IroProgress);
        }

        void pbw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            bPack.Enabled = true;
            PB.Value = 0;
        }

        private class IroOp {
            public string Iro;
            public string Folder;
            public string[] Delete;
            public _7thWrapperLib.CompressType Compress;
        }

        private void bUnpack_Click(object sender, EventArgs e) {
            var bw = new BackgroundWorker();
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerAsync(new IroOp() { Folder = txtDestF.Text, Iro = txtSourceIro.Text });
            bUnpack.Enabled = false;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e) {
            IroOp io = e.Argument as IroOp;
            BackgroundWorker bw = sender as BackgroundWorker;
            using (var arc = new _7thWrapperLib.IrosArc(io.Iro)) {
                var files = arc.AllFileNames().ToList();
                int count = 0;
                foreach (string file in files) {
                    string path = System.IO.Path.Combine(io.Folder, file);
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                    System.IO.File.WriteAllBytes(path, arc.GetBytes(file));
                    count++;
                    IroProgress(1.0 * count / files.Count, file);
                }
            }
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            bUnpack.Enabled = true;
            PB.Value = 0;
        }

        private void fPack_Load(object sender, EventArgs e) {
            cbCompress.Items.AddRange(Enum.GetValues(typeof(_7thWrapperLib.CompressType)).Cast<object>().ToArray());
            cbCompress.SelectedIndex = 0;
            cbPatchCompress.Items.AddRange(Enum.GetValues(typeof(_7thWrapperLib.CompressType)).Cast<object>().ToArray());
            cbPatchCompress.SelectedIndex = 0;
            cbPatchACompress.Items.AddRange(Enum.GetValues(typeof(_7thWrapperLib.CompressType)).Cast<object>().ToArray());
            cbPatchACompress.SelectedIndex = 0;
        }

        private void bPatchOrig_Click(object sender, EventArgs e) {
            if (ofIro.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPatchOrig.Text = ofIro.FileName;
        }

        private void bPatchNew_Click(object sender, EventArgs e) {
            if (ofIro.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPatchNew.Text = ofIro.FileName;
        }

        private void bPatchSave_Click(object sender, EventArgs e) {
            if (sfIrop.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPatchSave.Text = sfIrop.FileName;
        }

        private class PatchOp {
            public string OrigFile, NewFile, SaveFile;
            public _7thWrapperLib.CompressType Compress;
        }

        private void bGoPatch_Click(object sender, EventArgs e) {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_Patch;
            bw.RunWorkerAsync(new PatchOp() {
                OrigFile = txtPatchOrig.Text,
                NewFile = txtPatchNew.Text,
                SaveFile = txtPatchSave.Text,
                Compress = (_7thWrapperLib.CompressType)cbPatchCompress.SelectedItem
            });
        }

        void bw_Patch(object sender, DoWorkEventArgs e) {
            PatchOp patch = (PatchOp)e.Argument;
            using (var orig = new _7thWrapperLib.IrosArc(patch.OrigFile, true)) {
                using (var newiro = new _7thWrapperLib.IrosArc(patch.NewFile)) {
                    using (var fs = new System.IO.FileStream(patch.SaveFile, System.IO.FileMode.Create))
                        _7thWrapperLib.IrosPatcher.Create(orig, newiro, fs, patch.Compress, IroProgress);
                }
            }
        }

        private void bPatchASrc_Click(object sender, EventArgs e) {
            if (flFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPatchASrc.Text = flFolder.SelectedPath;
        }

        private void bPatchASave_Click(object sender, EventArgs e) {
            if (sfIrop.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPatchASave.Text = sfIrop.FileName;
        }

        private void bGoPatchA_Click(object sender, EventArgs e) {
            var pbw = new BackgroundWorker();
            pbw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PatchA_RunWorkerCompleted);
            pbw.DoWork += new DoWorkEventHandler(PatchA_DoWork);
            pbw.RunWorkerAsync(new IroOp() { 
                Folder = txtPatchASrc.Text, 
                Iro = txtPatchASave.Text, 
                Compress = (_7thWrapperLib.CompressType)cbPatchACompress.SelectedItem,
                Delete = txtPatchDelete.Lines
            });
            bPack.Enabled = false;
        }

        void PatchA_DoWork(object sender, DoWorkEventArgs e) {
            IroOp io = e.Argument as IroOp;
            BackgroundWorker bw = sender as BackgroundWorker;
            var files = System.IO.Directory.GetFiles(io.Folder, "*", System.IO.SearchOption.AllDirectories)
                .Select(s => s.Substring(io.Folder.Length).Trim('\\', '/'))
                .Select(s => _7thWrapperLib.IrosArc.ArchiveCreateEntry.FromDisk(io.Folder, s))
                .ToList();

            if (io.Delete.Any()) {
                byte[] deldata = System.Text.Encoding.Unicode.GetBytes(String.Join("\n", io.Delete));
                files.Add(new _7thWrapperLib.IrosArc.ArchiveCreateEntry() {
                    Filename = "%IrosPatch:Deleted",
                    GetData = () => deldata
                });
            }

            using (var fs = new System.IO.FileStream(io.Iro, System.IO.FileMode.Create))
                _7thWrapperLib.IrosArc.Create(fs, files, _7thWrapperLib.ArchiveFlags.Patch, io.Compress, IroProgress);
        }

        void PatchA_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            bPack.Enabled = true;
            PB.Value = 0;
        }
    }
}
