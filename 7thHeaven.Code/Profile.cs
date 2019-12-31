/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iros._7th.Workshop {
    public class Profile {
        public List<ProfileItem> Items { get; set; }
        public string OpenGLConfig { get; set; }

        public Profile() {
            Items = new List<ProfileItem>();
        }

        public IEnumerable<string> GetDetails() {
            foreach (var item in Items) {
                var mod = Sys.Library.GetItem(item.ModID);
                if (mod != null) {
                    var details = mod.CachedDetails;
                    if (details != null) {
                        yield return String.Format("# {0}", details.Name);
                        yield return String.Format("\tID: {0}", details.ID.ToString());
                        yield return String.Format("\tVersion: {0}", mod.LatestInstalled.VersionDetails.Version.ToString());
                    } else
                        yield return String.Format("\tModID {0}", mod.ModID.ToString());

                    foreach (var config in item.Settings) {
                        yield return String.Format("\t{0} = {1}", config.ID, config.Value.ToString());
                    }
                }
            }
        }
    }

    public class ProfileItem {
        public Guid ModID { get; set; }
        public List<ProfileSetting> Settings { get; set; }

        private static string[] _comparison = new[] { "=", "!=", "<", ">", "<=", ">=" };
        private static List<string> _comparisonL = _comparison.ToList();

        private bool IsConfigActive(string spec) {
            if (String.IsNullOrWhiteSpace(spec)) return true;

            string[] parts = spec.Split(_comparison, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            parts = new[] { parts[0].Trim(), spec.Substring(parts[0].Length, spec.Length - parts[1].Length - parts[0].Length), parts[1].Trim() };

            var conf = Settings.Find(c => c.ID.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase));
            if (conf == null) return false;

            int val;
            if (!int.TryParse(parts[2], out val)) return false;

            switch (_comparisonL.IndexOf(parts[1])) {
                case 0:
                    return val == conf.Value;
                case 1:
                    return val != conf.Value;
                case 2:
                    return val < conf.Value;
                case 3:
                    return val > conf.Value;
                case 4:
                    return val <= conf.Value;
                case 5:
                    return val >= conf.Value;
                default:
                    return false;
            }
        }

        private bool IsActive(_7thWrapperLib.ActiveWhen aw) {
            if (aw == null) return true;
            return aw.IsActive(IsConfigActive);
        }

        public _7thWrapperLib.RuntimeMod GetRuntime(_7thWrapperLib.LoaderContext context) {
            var mod = Sys.Library.GetItem(ModID);
            if (mod == null) return null;
            string location = System.IO.Path.Combine(Sys.Settings.LibraryLocation, mod.LatestInstalled.InstalledLocation);
            _7thWrapperLib.ModInfo modinfo = null;
            if (mod.LatestInstalled.InstalledLocation.EndsWith(".iro")) {
                using (var arc = new _7thWrapperLib.IrosArc(location)) {
                    if (arc.HasFile("mod.xml")) {
                        var doc = new System.Xml.XmlDocument();
                        doc.Load(arc.GetData("mod.xml"));
                        modinfo = new _7thWrapperLib.ModInfo(doc, context);
                    }
                }
            } else {
                string mfile = System.IO.Path.Combine(location, "mod.xml");
                if (System.IO.File.Exists(mfile))
                    modinfo = new _7thWrapperLib.ModInfo(mfile, context);
            }
            modinfo = modinfo ?? new _7thWrapperLib.ModInfo();

            foreach (var opt in modinfo.Options) {
                if (!Settings.Any(s => s.ID.Equals(opt.ID, StringComparison.InvariantCultureIgnoreCase)))
                    Settings.Add(new ProfileSetting() { ID = opt.ID, Value = opt.Default });
            }

            return new _7thWrapperLib.RuntimeMod(
                location,
                modinfo.Conditionals.Where(f => IsActive(f.ActiveWhen)),
                modinfo.ModFolders.Where(f => IsActive(f.ActiveWhen)).Select(f => f.Folder),
                modinfo
                );
        }
    }

    public class ProfileSetting {
        public string ID { get; set; }
        public int Value { get; set; }
    }
}
