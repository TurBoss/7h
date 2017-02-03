/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iros._7th.Workshop {
    public partial class fProfile : Form {
        public fProfile() {
            //InitializeComponent();
        }

        public static void Display(string text, string caption = null) {
            fReadme form = new fReadme();
            //form.textBox.DocumentText = text.ToString();
            if (!String.IsNullOrWhiteSpace(caption)) form.Text = caption;
            form.ShowDialog();
        }
    }
}
