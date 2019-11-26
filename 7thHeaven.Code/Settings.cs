/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Iros._7th.Workshop {

    [Flags]
    public enum GeneralOptions {
        None = 0,
        KeepOldVersions = 0x1,
        AutoActiveNewMods = 0x2,
        AutoImportMods = 0x4,
        CheckForUpdates = 0x8,
        BypassCompatibility = 0x10,
        SetEXECompatFlags = 0x20,
    }

    [Flags]
    public enum InterfaceOptions {
        None = 0,
        ProfileCollapse = 0x1,
    }

    public class SavedWindow {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public WindowState State { get; set; }
    }

    public class Subscription {
        public DateTime LastSuccessfulCheck { get; set; }
        public int FailureCount { get; set; }
        public string Url { get; set; }
    }

    public class Settings {

        public IEnumerable<string> VerifySettings() {
            bool validexe = System.IO.File.Exists(FF7Exe);
            if (!validexe) yield return "FF7Exe " + FF7Exe + " not found";
            foreach (string al in AlsoLaunch.Where(s => !String.IsNullOrWhiteSpace(s)))
                if (!System.IO.File.Exists(al)) yield return "AlsoLaunch " + al + " not found";
            if (!System.IO.Directory.Exists(MovieFolder)) yield return "MovieFolder " + MovieFolder + " not found";
            if (!System.IO.Directory.Exists(AaliFolder)) yield return "Aali Modpath " + AaliFolder + " not found";
            if (validexe) {
                string ff7folder = System.IO.Path.GetDirectoryName(FF7Exe);
                foreach (string extra in ExtraFolders.Where(s => !String.IsNullOrWhiteSpace(s))) {
                    string path = System.IO.Path.Combine(ff7folder, extra);
                    if (!System.IO.Directory.Exists(path)) yield return "Extra folder " + path + " not found";
                }
            }
        }

        public List<string> SubscribedUrls { get; set; }
        public List<string> ExtraFolders { get; set; }
        public List<Subscription> Subscriptions { get; set; }

        public string LibraryLocation { get; set; }
        
        public string FF7Exe { get; set; }
        [System.Xml.Serialization.XmlElement("AlsoLaunch")]
        public List<string> AlsoLaunch { get; set; }
        public string AaliFolder { get; set; }
        public string MovieFolder { get; set; }

        public DateTime LastUpdateCheck { get; set; }
        public GeneralOptions Options { get; set; }
        public InterfaceOptions IntOptions { get; set; }
        public string CurrentProfile { get; set; }
        public SavedWindow MainWindow { get; set; }
        public string AutoUpdateSource { get; set; }
        public decimal AutoUpdateOffered { get; set; }

        public decimal VersionUpgradeCompleted { get; set; }

        public Settings() {
            SubscribedUrls = new List<string>();
            ExtraFolders = new List<string>();
            AlsoLaunch = new List<string>();
            Subscriptions = new List<Subscription>();
            AutoUpdateSource = "#F!yBlHTYiJ!SFpmT2xII7iXcgXAmNYLJg";
        }
    }
}
