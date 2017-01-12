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
    partial class pMessage : UserControl {
        public event EventHandler Close;

        public pMessage() {
            InitializeComponent();
        }

        public void Init(WMessage msg, int width) {
            label1.Text = msg.Text;
            if (!String.IsNullOrWhiteSpace(msg.Link)) {
                label1.Font = new System.Drawing.Font(label1.Font, FontStyle.Underline);
                label1.ForeColor = Color.Blue;
                label1.Tag = msg.Link;
                label1.Cursor = Cursors.Hand;
            }
            this.Width = width;
            using (var g = Graphics.FromHwnd(label1.Handle)) {
                var size = g.MeasureString(label1.Text, label1.Font, label1.ClientSize.Width, StringFormat.GenericDefault);
                this.Height = Math.Max((int)(size.Height + label1.Padding.Vertical), bClose.Height + bClose.Margin.Vertical) + 1 + this.Padding.Vertical;
            }
        }

        private void bClose_Click(object sender, EventArgs e) {
            Close(this, e);
        }

        private void label1_Click(object sender, EventArgs e) {
            if (label1.Tag != null) {
                string link = label1.Tag.ToString();
                if (!String.IsNullOrEmpty(link)) Sys.TriggerLink(link);
                /*
                if (link.StartsWith("iros://", StringComparison.InvariantCultureIgnoreCase))
                    Sys.TriggerGotoMod(Guid.Parse(link.Substring(7)));
                else
                    System.Diagnostics.Process.Start(link);
                 */
            }
        }

    }
}
