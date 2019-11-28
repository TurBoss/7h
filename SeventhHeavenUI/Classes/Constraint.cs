using Iros._7th.Workshop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeaven.Classes
{
    internal class Constraint
    {
        public Guid ModID { get; set; }
        public string Setting { get; set; }
        public List<int> Require { get; set; }
        public HashSet<int> Forbid { get; set; }
        public HashSet<string> ParticipatingMods { get; set; }
        public _7thWrapperLib.ConfigOption Option { get; set; }

        public Constraint()
        {
            Forbid = new HashSet<int>();
            Require = new List<int>();
            ParticipatingMods = new HashSet<string>();
        }

        public bool Verify(out string message)
        {
            message = null;
            if (Option == null) return true; //setting no longer exists, constraints are irrelevant?
            var pItem = Sys.ActiveProfile.Items.Find(pi => pi.ModID.Equals(ModID));
            var inst = Sys.Library.GetItem(pItem.ModID);
            var setting = pItem.Settings.Find(s => s.ID.Equals(Setting, StringComparison.InvariantCultureIgnoreCase));
            if (setting == null)
            {
                setting = new ProfileSetting() { ID = Setting, Value = Option.Default };
                pItem.Settings.Add(setting);
            }
            if (Require.Any() && (Require.Min() != Require.Max()))
            {
                message = String.Format("Mod {0}, setting {1} - no compatible option can be found. The following mods all restrict how it can be configured: {2}", inst.CachedDetails.Name, Option.Name, String.Join(",", ParticipatingMods));
                return false;
            }

            if (Require.Any() && Forbid.Contains(Require[0]))
            {
                message = String.Format("Mod {0}, setting {1} - no compatible option can be found. The following mods all restrict how it can be configured: {2}", inst.CachedDetails.Name, Option.Name, String.Join(",", ParticipatingMods));
                return false;
            }
            if (Option.Values.All(o => Forbid.Contains(o.Value)))
            {
                message = String.Format("Mod {0}, setting {1} - no compatible option can be found. The following mods all restrict how it can be configured: {2}", inst.CachedDetails.Name, Option.Name, String.Join(",", ParticipatingMods));
                return false;
            }
            if (Require.Any() && (setting.Value != Require[0]))
            {
                var opt = Option.Values.Find(v => v.Value == Require[0]);
                if (opt == null)
                {
                    message = String.Format("Mod {0}, setting {1} - no compatible option can be found. The following mods all restrict how it can be configured: {2}", inst.CachedDetails.Name, Option.Name, String.Join(",", ParticipatingMods));
                    return false;
                }
                setting.Value = Require[0];
                message = String.Format("Mod {0} - changed setting {1} to {2}", inst.CachedDetails.Name, Option.Name, opt.Name);
            }
            else if (Forbid.Contains(setting.Value))
            {
                setting.Value = Option.Values.First(v => !Forbid.Contains(v.Value)).Value;
                var opt = Option.Values.Find(v => v.Value == setting.Value);
                message = String.Format("Mod {0} - changed setting {1} to {2}", inst.CachedDetails.Name, Option.Name, opt.Name);
            }
            return true;
        }
    }
}
