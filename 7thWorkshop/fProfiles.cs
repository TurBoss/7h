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
    public partial class fProfiles : Form {
        public fProfiles() {
            InitializeComponent();
        }

        public string SelectedProfile { get { return lbProfiles.SelectedItem as string; } }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
            if (lbProfiles.SelectedIndex >= 0) {
                string file = System.IO.Path.Combine(Sys.SysFolder, "profiles", lbProfiles.SelectedItem.ToString()) + ".xml";
                System.IO.File.Delete(file);
                lbProfiles.Items.RemoveAt(lbProfiles.SelectedIndex);
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            if (lbProfiles.SelectedIndex >= 0) {
                string file = System.IO.Path.Combine(Sys.SysFolder, "profiles", lbProfiles.SelectedItem.ToString()) + ".xml";
                string name = Microsoft.VisualBasic.Interaction.InputBox("Profile name:", "Enter new Profile name", "New Profile");
                System.IO.File.Copy(file, System.IO.Path.Combine(Sys.SysFolder, "profiles", name) + ".xml");
                lbProfiles.Items.Add(name);
            }
        }

        private void fProfiles_Load(object sender, EventArgs e) {
            foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.Combine(Sys.SysFolder, "profiles"), "*.xml")) {
                lbProfiles.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }
        }
    }
}
