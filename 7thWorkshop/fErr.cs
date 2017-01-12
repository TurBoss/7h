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
    public partial class fErr : Form {
        public fErr() {
            InitializeComponent();
        }

        public static void Display(Exception ex) {
            foreach (Form f in Application.OpenForms)
                f.Hide();
            fErr form = new fErr();
            form.txtErr.Text = ex.ToString();
            form.ShowDialog();
        }

        private void fErr_FormClosed(object sender, FormClosedEventArgs e) {
            Application.Exit();
        }
    }
}
