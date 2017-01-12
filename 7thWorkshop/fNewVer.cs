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
    public partial class fNewVer : Form {
        public fNewVer() {
            InitializeComponent();
        }

        public void Init(decimal ver) {
            lNewVer.Text = "New version available: " + ver.ToString();
        }
    }
}
