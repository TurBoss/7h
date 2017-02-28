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
    public partial class fSettings : Form {
        public fSettings() {
            InitializeComponent();
        }

        private bool WriteLinkReg(Microsoft.Win32.RegistryKey key) {
            string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var iros = key.CreateSubKey("iros");
            if (iros == null) return false;
            var icon = key.CreateSubKey("DefaultIcon");
            var shell = iros.CreateSubKey("shell");
            var open = shell.CreateSubKey("open");
            var command = open.CreateSubKey("command");
            iros.SetValue(String.Empty, "URL: Alert Protocol");
            iros.SetValue("URL Protocol", String.Empty);
            icon.SetValue(String.Empty, app + ",1");
            command.SetValue(String.Empty, "\"" + app + "\" \"%1\"");
            return true;
        }

        private bool WriteLinkReg() {
            var key = Microsoft.Win32.Registry.ClassesRoot;
            bool global = WriteLinkReg(key);
            if (!global) {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes");
                global = WriteLinkReg(key);
            }
            return global;
        }

        private void fSettings_Load(object sender, EventArgs e) {
            if (String.IsNullOrEmpty(Sys.Settings.FF7Exe)) {
                if (MessageBox.Show("No settings configured. Try to autodetect sensible settings?", "Setup", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    string ff7 = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Square Soft, Inc.\Final Fantasy VII", "AppPath", null);
                    if (!String.IsNullOrEmpty(ff7)) {
                        Sys.Settings.AaliFolder = ff7 + @"mods\Textures";
                        Sys.Settings.FF7Exe = ff7 + @"FF7.exe";
                        Sys.Settings.LibraryLocation = ff7 + @"mods/7th Heaven";

                    }
                }
            }
            
            txtSubscriptions.Lines = Sys.Settings.SubscribedUrls.ToArray();
            txtLibrary.Text = Sys.Settings.LibraryLocation;
            txtFF7.Text = Sys.Settings.FF7Exe;
            txtAlsoLaunch.Lines = Sys.Settings.AlsoLaunch.ToArray();
            txtAali.Text = Sys.Settings.AaliFolder;

            for (int i = 0; i < clOptions.Items.Count; i++)
                clOptions.SetItemChecked(i, (((int)Sys.Settings.Options) & (1 << i)) != 0);

            if (Sys.Settings.VersionUpgradeCompleted < Sys.Version) {
                if (String.IsNullOrWhiteSpace(Sys.Settings.MovieFolder)) {
                    Sys.Settings.MovieFolder = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Square Soft, Inc.\Final Fantasy VII", "MoviePath", null);
                }

                if (MessageBox.Show("Would you like to set up subscription links with Windows?", "Link setup", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    try {
                        if (!WriteLinkReg()) throw new Exception("Could not create keys");
                    } catch (Exception ex) {
                        MessageBox.Show("Unable to register links: " + ex.ToString());
                    }
                }
                if (Sys.Settings.VersionUpgradeCompleted < 1.21m) Sys.Settings.ExtraFolders.Add("music");
                if (Sys.Settings.VersionUpgradeCompleted < 1.29m) Sys.Settings.Options |= GeneralOptions.CheckForUpdates;
            }
            txtExtraFolders.Lines = Sys.Settings.ExtraFolders.ToArray();
            txtMovie.Text = Sys.Settings.MovieFolder;
            Sys.Settings.VersionUpgradeCompleted = Sys.Version;
        }

        private void bOK_Click(object sender, EventArgs e) {

            Sys.Settings.SubscribedUrls = txtSubscriptions.Lines.ToList();
            Sys.Settings.ExtraFolders = txtExtraFolders.Lines.ToList();
            Sys.Settings.AlsoLaunch = txtAlsoLaunch.Lines.ToList();

            Sys.Settings.FF7Exe = txtFF7.Text;
            Sys.Settings.AaliFolder = txtAali.Text;
            Sys.Settings.LibraryLocation = txtLibrary.Text;
            Sys.Settings.MovieFolder = txtMovie.Text;

            int opts = 0;
            for (int i = 0; i < clOptions.Items.Count; i++)
                if (clOptions.GetItemChecked(i)) opts |= (1 << i);
            Sys.Settings.Options = (GeneralOptions)opts;

            System.IO.Directory.CreateDirectory(Sys.Settings.LibraryLocation);
        }

        private void bLibrary_Click(object sender, EventArgs e) {
            if (flBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtLibrary.Text = flBrowser.SelectedPath;
        }

        private void bAali_Click(object sender, EventArgs e) {
            if (flBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtAali.Text = flBrowser.SelectedPath;
        }

        private void bFF7_Click(object sender, EventArgs e) {
            if (ofExe.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtFF7.Text = ofExe.FileName;
        }

        private void bAlsoLaunch_Click(object sender, EventArgs e) {
            if (ofExe.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtAlsoLaunch.Text = ofExe.FileName;
        }

        private void bMovie_Click(object sender, EventArgs e) {
            if (flBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtMovie.Text = flBrowser.SelectedPath;
        }
    }
}
