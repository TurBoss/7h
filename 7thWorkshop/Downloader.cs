/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iros._7th.Workshop {
    class Downloader {
    }

    public class PendingDownload {
        public string Source { get; set; }
        public string Destination { get; set; }
        public long Complete { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }

        public void Start() {
            //System.Net.WebClient
        }
    }
}
