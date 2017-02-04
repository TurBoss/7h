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
using System.Diagnostics;

namespace Iros._7th.Workshop {
    public partial class fReadme : Form {
        public fReadme() {
            InitializeComponent();
        }

        public static void Display(string text, string caption = null) {
            fReadme form = new fReadme();
            form.webBrowser1.DocumentText = text.ToString();
            if (!String.IsNullOrWhiteSpace(caption)) form.Text = caption;
            form.ShowDialog();
        }
        public void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.ToString() != "about:blank")
            {
                //cancel the current event
                e.Cancel = true;

                //this opens the URL in the user's default browser
                Process.Start(e.Url.ToString());
            }
        }
    }
}
