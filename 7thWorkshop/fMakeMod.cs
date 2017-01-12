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
    public partial class fMakeMod : Form {
        public fMakeMod() {
            InitializeComponent();
        }

        private Guid _guid;

        private void bReset_Click(object sender, EventArgs e) {
            txtExtractInto.Text = txtInfoPage.Text = txtName.Text = txtDescription.Text = txtAuthor.Text = txtTags.Text = txtPreview.Text = txtExtract.Text = txtLinkData.Text = String.Empty;
            udVersion.Value = 1.00m;
            cbLinkType.SelectedIndex = 0;
            _guid = Guid.NewGuid();
        }

        private void fMakeMod_Load(object sender, EventArgs e) {
            cbLinkType.Items.AddRange(Enum.GetValues(typeof(LocationType)).OfType<LocationType>().Cast<object>().ToArray());
            bReset.PerformClick();
        }

        private void bGenerate_Click(object sender, EventArgs e) {
            Mod m = new Mod() {
                Author = txtAuthor.Text,
                Description = txtDescription.Text,
                ID = _guid,
                Link = txtInfoPage.Text,
                Name = txtName.Text,
                Tags = txtTags.Lines.ToList(),
                LatestVersion = new ModVersion() {
                    CompatibleGameVersions = GameVersions.All,
                    ExtractSubFolder = txtExtract.Text,
                    ExtractInto = txtExtractInto.Text,
                    Links = new List<string>() { "iros://" + cbLinkType.SelectedItem.ToString() + "/" + txtLinkData.Text.Replace("://", "$") },
                    ReleaseDate = DateTime.Now,
                    Version = udVersion.Value,
                    PreviewImage = txtPreview.Text,
                }
            };

            txtOutput.Text = Util.Serialize(m);
        }

        private void button1_Click(object sender, EventArgs e) {
            Mega.MegaIros mega = new Mega.MegaIros(txtMegaFolder.Text, String.Empty);
            txtMegaLinks.Text = "";
            foreach (int i in Enumerable.Range(0, 15)) {
                System.Threading.Thread.Sleep(1000);
                if (mega.GetNodes().Any()) break;
            }
            Log.Write("Mega:Link generator: found " + mega.GetNodes().Count() + " nodes");
            if (!mega.GetNodes().Any()) return;
            foreach (var n in mega.GetNodes()) {
                txtMegaLinks.Text += String.Format("iros://MegaSharedFolder/{0},{1},{2}\r\n", txtMegaFolder.Text, Iros.Mega.Base64.btoa(n.Handle), n.Name);
            }
        }
    }
}
