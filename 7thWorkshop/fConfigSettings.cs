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
    public partial class fConfigSettings : Form {
        public fConfigSettings() {
            InitializeComponent();
        }

        private ConfigSettings.Settings _settings;
        private ConfigSettings.ConfigSpec _spec;
        private string _file;

        public void Init(string cfgSpec, string cfgFile) {
            _settings = new ConfigSettings.Settings(System.IO.File.ReadAllLines(cfgFile));
            _spec = Util.Deserialize<ConfigSettings.ConfigSpec>(cfgSpec);
            _file = cfgFile;

            foreach (var items in _spec.Settings.GroupBy(s => s.Group)) {
                tcSettings.TabPages.Add(items.Key);
                var tc = tcSettings.TabPages[tcSettings.TabPages.Count - 1];
                tcSettings.PerformLayout();
                int y = 0;
                foreach (var setting in items) {
                    Panel p = new Panel();
                    tc.Controls.Add(p);
                    p.SetBounds(0, y, tc.ClientSize.Width, 60);
                    p.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    setting.Load(p, _settings);
                    y += p.Height;
                }
            }
        }

        private void bOK_Click(object sender, EventArgs e) {
            foreach (var item in _spec.Settings) item.Save(_settings);
            try {
                System.IO.File.WriteAllLines(_file, _settings.GetOutput());
                MessageBox.Show("OpenGL settings saved.");
            } catch(System.UnauthorizedAccessException) {
                MessageBox.Show("Could not write the ff7_opengl.cfg file. Check that it is not set to read only, and that FF7 is installed in a folder you have full write access to.");
            }
        }
    }
}
