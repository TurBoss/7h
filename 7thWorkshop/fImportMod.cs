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
    public partial class fImportMod : Form {
        public fImportMod() {
            InitializeComponent();
        }

        private void bIro_Click(object sender, EventArgs e) {
            if (ofIro.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                txtIro.Text = ofIro.FileName;
                txtName.Text = System.IO.Path.GetFileNameWithoutExtension(txtIro.Text);
            }
        }

        private void bFolder_Click(object sender, EventArgs e) {
            if (flFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                txtFolder.Text = flFolder.SelectedPath;
                txtName.Text = System.IO.Path.GetFileName(txtFolder.Text);
            }
        }

        public static void ImportMod(string source, string name, bool iroMode, bool noCopy) {
            Mod m = new Mod() {
                Author = String.Empty,
                Description = "Imported mod",
                ID = Guid.NewGuid(),
                Link = String.Empty,
                Tags = new List<string>(),
                Name = name,
                LatestVersion = new ModVersion() {
                    CompatibleGameVersions = GameVersions.All,
                    Links = new List<string>(),
                    PreviewImage = String.Empty,
                    ReleaseDate = DateTime.Now,
                    ReleaseNotes = String.Empty,
                    Version = 1.00m,
                }
            };

            string location;
            if (noCopy)
                location = System.IO.Path.GetFileName(source);
            else
                location = String.Format("{0}_{1}", m.ID, name);
            System.Xml.XmlDocument doc = null;
            Func<string, byte[]> getData = null;
            if (!iroMode) {
                if (!noCopy) {
                    foreach (string file in System.IO.Directory.GetFiles(source, "*", System.IO.SearchOption.AllDirectories)) {
                        string part = file.Substring(source.Length).Trim('\\', '/');
                        string dest = System.IO.Path.Combine(Sys.Settings.LibraryLocation, location, part);
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dest));
                        System.IO.File.Copy(file, dest, true);
                    }
                }
                string mx = System.IO.Path.Combine(Sys.Settings.LibraryLocation, location, "mod.xml");
                if (System.IO.File.Exists(mx)) {
                    doc = new System.Xml.XmlDocument();
                    doc.Load(mx);
                }
                getData = s => {
                    string file = System.IO.Path.Combine(Sys.Settings.LibraryLocation, location, s);
                    if (System.IO.File.Exists(file)) return System.IO.File.ReadAllBytes(file);
                    return null;
                };
                //System.IO.Directory.Move(txtFolder.Text, System.IO.Path.Combine(Sys.Settings.LibraryLocation, location));
            } else {
                if (!noCopy) {
                    location = location + ".iro";
                    System.IO.File.Copy(source, System.IO.Path.Combine(Sys.Settings.LibraryLocation, location), true);
                }
                var arc = new _7thWrapperLib.IrosArc(source);
                if (arc.HasFile("mod.xml")) {
                    doc = new System.Xml.XmlDocument();
                    doc.Load(arc.GetData("mod.xml"));
                }
                getData = s => arc.HasFile(s) ? arc.GetBytes(s) : null;
            }

            if (doc != null) {
                m.Author = doc.SelectSingleNode("/ModInfo/Author").NodeTextS();
                m.Link = doc.SelectSingleNode("/ModInfo/Link").NodeTextS();
                m.Description = doc.SelectSingleNode("/ModInfo/Description").NodeTextS();
                decimal ver;
                if (decimal.TryParse(doc.SelectSingleNode("/ModInfo/Version").NodeTextS().Replace(',', '.'), out ver)) m.LatestVersion.Version = ver;
                var pv = doc.SelectSingleNode("/ModInfo/PreviewFile");
                if (pv != null) {
                    byte[] data = getData(pv.InnerText);
                    if (data != null) {
                        string url = "iros://Preview/Auto/" + m.ID.ToString();
                        m.LatestVersion.PreviewImage = url;
                        Sys.ImageCache.InsertManual(url, data);
                    }
                }
            }

            Sys.Library.AddInstall(new InstalledItem() {
                CachedDetails = m,
                CachePreview = String.Empty,
                ModID = m.ID,
                UpdateType = UpdateType.Ignore,
                Versions = new List<InstalledVersion>() { new InstalledVersion() { VersionDetails = m.LatestVersion, InstalledLocation = location } },
            });
        }

        private void bOK_Click(object sender, EventArgs e) {
            switch (tcSource.SelectedIndex) {
                case 0:
                    ImportMod(txtIro.Text, txtName.Text, true, false);
                    break;
                case 1:
                    ImportMod(txtFolder.Text, txtName.Text, false, false);
                    break;
                case 2:
                    foreach (string iro in System.IO.Directory.GetFiles(txtBatch.Text, "*.iro")) {
                        ImportMod(iro, System.IO.Path.GetFileNameWithoutExtension(iro), true, false);
                    }
                    foreach (string dir in System.IO.Directory.GetDirectories(txtBatch.Text)) {
                        ImportMod(dir, System.IO.Path.GetFileNameWithoutExtension(dir), false, false);
                    }
                    break;
            }
        }

        private void bBatch_Click(object sender, EventArgs e) {
            if (flFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtBatch.Text = flFolder.SelectedPath;
        }

        private void tcSource_SelectedIndexChanged(object sender, EventArgs e) {
            txtName.Enabled = tcSource.SelectedIndex != 2;
        }
    }
}
