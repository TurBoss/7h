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
    public partial class fChunks : Form {
        public fChunks() {
            InitializeComponent();
        }

        private void bFLevel_Click(object sender, EventArgs e) {
            if (ofFLevel.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtFLevel.Text = ofFLevel.FileName;
        }

        private void bOutput_Click(object sender, EventArgs e) {
            if (fbOutput.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtOutput.Text = fbOutput.SelectedPath;
        }

        private class ExtractArgs {
            public string Input, Output;
            public int[] Chunks;
        }

        private void bGo_Click(object sender, EventArgs e) {
            BackgroundWorker bw = new BackgroundWorker() { WorkerReportsProgress = true };
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync(new ExtractArgs() {
                Input = txtFLevel.Text,
                Output = txtOutput.Text,
                Chunks = txtChunks.Text.Split(',').Select(s => int.Parse(s.Trim())).ToArray()
            });
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Complete!");
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            pbChunk.Value = e.ProgressPercentage;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e) {
            ExtractArgs ea = (ExtractArgs)e.Argument;
            using (var fs = new System.IO.FileStream(ea.Input, System.IO.FileMode.Open)) {
                var df = ProcMonParser.FF7Files.LoadLGP(fs, ea.Input);
                int file=0;
                foreach (var item in df.Items) {
                    (sender as BackgroundWorker).ReportProgress(100 * file++ / df.Items.Count);
                    if (System.IO.Path.GetExtension(item.Name).Length == 0) {
                        byte[] ff = new byte[item.Length-24];
                        fs.Position = item.Start + 24;
                        fs.Read(ff, 0, ff.Length);
                        var chunks = _7thWrapperLib.FieldFile.Unchunk(ff);
                        if (chunks.Count > 0)
                            foreach (int i in ea.Chunks) {
                                string fn = System.IO.Path.Combine(ea.Output, System.IO.Path.GetFileNameWithoutExtension(item.Name) + ".chunk." + i);
                                System.IO.File.WriteAllBytes(fn, chunks[i - 1]);
                            }
                    }
                }
            }
        }
    }
}
