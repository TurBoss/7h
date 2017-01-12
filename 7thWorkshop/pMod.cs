/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iros._7th.Workshop {

    enum ModBarAction {
        Activate,
        Configure,
        Up,
        Down,
        Readme,
    }

    partial class pMod : UserControl {

        public enum ModBarState {
            None,
            Activate,
            Full,
        }

        public pMod() {
            InitializeComponent();
        }

        private Color[] _stateColors = new[] { 
            Color.FromArgb(255, 224, 192), //not installed, Download
            Color.FromArgb(192, 255, 192), //installed 
            Color.FromArgb(192, 255, 255), //installed - update
        };

        private Mod _mod;
        private bool _showInstall, _showDownloadSize;

        public Action<pMod, ModBarAction> DoAction;

        public const int COLLAPSED_HEIGHT = 65;

        public Guid ModID {
            get { return _mod.ID; }
        }

        private string GetDLSize(int size) {
            if (size <= 0)
                return String.Empty;
            else if (size < 100 * 1024) {
                return String.Format("{0:0.0}MB", size / 1024m);
            } else if (size < 500 * 1024) {
                return String.Format("{0:0}MB", size / 1024m);
            } else {
                return String.Format("{0:0.0}GB", size / (1024m * 1024m));
            }
        }

        public void UpdateDetails() {
            _mod = Sys.Catalog.GetMod(_mod.ID) ?? _mod;
            gMain.Text = _mod.Name;
            var image = Sys.ImageCache.Get(_mod.LatestVersion.PreviewImage, _mod.ID);
            PB.SizeMode = (image != null) && ((image.Width > PB.Width || image.Height > PB.Height)) ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;
            PB.Image = image;
            
            var cat = Sys.Catalog.GetMod(_mod.ID);
            var inst = Sys.Library.GetItem(_mod.ID);
            bAction.Enabled = true;
            switch (Sys.GetStatus(_mod.ID)) {
                case ModStatus.NotInstalled:
                    bAction.BackColor = _stateColors[0];
                    bAction.Text = "Download";
                    bAction.Tag = 1;
                    break;
                case ModStatus.Downloading:
                    bAction.BackColor = _stateColors[0];
                    bAction.Text = "Downloading...";
                    bAction.Tag = 0;
                    break;
                case ModStatus.Installed:
                    if (cat != null && inst.Versions.Max(v => v.VersionDetails.Version) < cat.LatestVersion.Version) {
                        bAction.BackColor = _stateColors[2];
                        bAction.Text = "Update";
                        bAction.Tag = 2;
                    } else {
                        bAction.BackColor = _stateColors[1];
                        bAction.Text = "Installed"; //???
                        bAction.Enabled = false;
                        bAction.Tag = 3;
                    }
                    break;
                case ModStatus.Updating:
                    bAction.BackColor = _stateColors[2];
                    bAction.Text = "Updating...";
                    bAction.Tag = 0;
                    break;
            }
            var mod = _mod;
            if (_showInstall && inst != null)
                mod = inst.CachedDetails;
            var ver = _mod.LatestVersion;
            if (_showInstall && inst != null)
                ver = inst.Versions.OrderBy(v => v.VersionDetails.Version).Last().VersionDetails;

            bActivate.Text = Sys.ActiveProfile.Items.Any(i => i.ModID.Equals(ModID)) ? "Deactivate" : "Activate";
            rtInfo.Text =
                (_showDownloadSize && ver.DownloadSize > 0 ? "Size: " + GetDLSize(ver.DownloadSize) + "\r\n" : "") +
                "Author: " + mod.Author + "\r\n" +
                "Version: " + ver.Version.ToString() + " released on " + ver.ReleaseDate.ToShortDateString() + "\r\n" +
                "Link: " + (mod.Link ?? String.Empty) + "\r\n" +
                mod.Description;
        }

        public void Init(Mod m, ModBarState barState, bool showInstall, bool showDownloadSize) {
            gMain.Text = m.Name;
            _showInstall = showInstall;
            _showDownloadSize = showDownloadSize;
            _mod = m;
            pProfile.Visible = barState != ModBarState.None;
            switch (barState) {
                case ModBarState.None:
                    pProfile.Visible = false;
                    break;
                case ModBarState.Activate:
                    pProfile.Visible = true;
                    bUp.Visible = bDown.Visible = bConfigure.Visible = false;
                    bReadme.Visible = true;
                    break;
                case ModBarState.Full:
                    pProfile.Visible = true;
                    bUp.Visible = bDown.Visible = bConfigure.Visible = true;
                    bReadme.Visible = false;
                    break;
            }
            this.PerformLayout();
            UpdateDetails();
        }

        private const string _msgDownloadReq =
            @"This mod also requires you to download the following mods:
{0}
Download and install them?";

        private const string _msgMissingReq =
            @"This mod requires the following mods to also be installed, but I cannot find them:
{0}
It may not work properly unless you find and install the requirements.";

        private void bAction_Click(object sender, EventArgs e) {
            if (bAction.Tag == null) return;
            switch ((int)bAction.Tag) {
                case 1: //Download mod
                    if (Sys.GetStatus(_mod.ID) == ModStatus.NotInstalled) {
                        Install.DownloadAndInstall(_mod);
                    } else
                        UpdateDetails();

                    List<Mod> required = new List<Mod>();
                    List<string> notFound = new List<string>();
                    foreach (var req in _mod.Requirements) {
                        var inst = Sys.Library.GetItem(req.ModID);
                        if (inst != null) continue;
                        var rMod = Sys.Catalog.GetMod(req.ModID);
                        if (rMod != null)
                            required.Add(rMod);
                        else
                            notFound.Add(req.Description);
                    }

                    if (required.Any()) {
                        if (MessageBox.Show(String.Format(_msgDownloadReq, String.Join("\n", required.Select(m => m.Name))), "Requirements", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            foreach (var rMod in required)
                                Install.DownloadAndInstall(rMod);
                    }
                    if (notFound.Any()) {
                        MessageBox.Show(String.Format(_msgMissingReq, String.Join("\n", notFound)));
                    }

                    break;
                case 2:
                    var cat = Sys.Catalog.GetMod(_mod.ID);
                    var lib = Sys.Library.GetItem(_mod.ID);
                    if (lib.Versions.Max(v => v.VersionDetails.Version) < cat.LatestVersion.Version) {
                        Install.DownloadAndInstall(cat);
                        Sys.Downloads.BringToFront();
                    } else
                        UpdateDetails();
                    break;
            }
        }

        private void bActivate_Click(object sender, EventArgs e) {
            DoAction(this, ModBarAction.Activate);
        }

        private void bConfigure_Click(object sender, EventArgs e) {
            DoAction(this, ModBarAction.Configure);
        }

        private void bUp_Click(object sender, EventArgs e) {
            DoAction(this, ModBarAction.Up);
        }

        private void bDown_Click(object sender, EventArgs e) {
            DoAction(this, ModBarAction.Down);
        }

        private void rtInfo_LinkClicked(object sender, LinkClickedEventArgs e) {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void bReadme_Click(object sender, EventArgs e) {
            DoAction(this, ModBarAction.Readme);
        }

    }
}
