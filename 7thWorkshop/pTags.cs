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
    public partial class pTags : UserControl {

        public event EventHandler SelectionChanged;

        private HashSet<string> _selected;

        public IEnumerable<string> Selected { get { return _selected; } }

        public void Init() {
            if (Sys.Catalog == null) return;
            clTags.Items.Clear();
            foreach (string tag in Sys.Catalog.Mods.SelectMany(m => m.Tags).Concat(Sys.Library.Items.SelectMany(i => i.CachedDetails.Tags)).Distinct(StringComparer.InvariantCultureIgnoreCase))
                clTags.Items.Add(tag);
            _selected = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public pTags() {
            InitializeComponent();
            Init();
        }

        private void bClear_Click(object sender, EventArgs e) {
            for (int i = 0; i < clTags.Items.Count; i++)
                clTags.SetItemChecked(i, false);
            SelectionChanged(this, EventArgs.Empty);
        }

        private void clTags_ItemCheck(object sender, ItemCheckEventArgs e) {
            if (e.NewValue == CheckState.Checked)
                _selected.Add((string)clTags.Items[e.Index]);
            else
                _selected.Remove((string)clTags.Items[e.Index]);
            SelectionChanged(this, EventArgs.Empty);
        }

        private void bAll_Click(object sender, EventArgs e) {
            for (int i = 0; i < clTags.Items.Count; i++)
                clTags.SetItemChecked(i, true);
            SelectionChanged(this, EventArgs.Empty);
        }
    }
}
