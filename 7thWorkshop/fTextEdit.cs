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
    public partial class fTextEdit : Form {
        public fTextEdit() {
            InitializeComponent();
        }

        public static bool Edit(string caption, ref string text) {
            fTextEdit t = new fTextEdit() { Text = caption };
            t.txtText.Text = text;
            if (t.ShowDialog() == DialogResult.OK) {
                text = t.txtText.Text;
                return true;
            }
            return false;
        }
    }
}
