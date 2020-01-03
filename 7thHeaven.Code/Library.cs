/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iros._7th.Workshop
{
    public class Library
    {
        private Dictionary<Guid, InstalledItem> _lookup;

        public UpdateType DefaultUpdate { get; set; }
        public List<InstalledItem> Items { get; set; }

        private object _pendingLock = new object();
        public List<string> PendingDelete { get; set; }

        public void QueuePendingDelete(IEnumerable<string> files)
        {
            lock (_pendingLock)
                PendingDelete.AddRange(files);
        }

        public InstalledItem GetItem(Guid modID)
        {
            if (_lookup == null)
            {
                _lookup = Items.ToDictionary(i => i.ModID, i => i);
            }
            InstalledItem ii;
            _lookup.TryGetValue(modID, out ii);
            return ii;
        }

        public void AddInstall(InstalledItem ii)
        {
            var exist = GetItem(ii.ModID); //forces lookup creation
            if (exist != null) Items.Remove(exist);
            Items.Add(ii);
            _lookup[ii.ModID] = ii;
            PendingDelete.RemoveAll(s => s.Equals(ii.LatestInstalled.InstalledLocation, StringComparison.InvariantCultureIgnoreCase));
            Sys.SetStatus(ii.ModID, ModStatus.Installed);
            Sys.Save();
        }

        public void RemoveInstall(InstalledItem ii)
        {
            var exist = GetItem(ii.ModID); //forces lookup creation
            if (exist != null) Items.Remove(exist);
            _lookup.Remove(ii.ModID);
            Sys.SetStatus(ii.ModID, ModStatus.NotInstalled);
            Sys.Save();
        }

        public void AttemptDeletions()
        {
            lock (_pendingLock)
            {
                int oldCount = PendingDelete.Count;
                for (int i = PendingDelete.Count - 1; i >= 0; i--)
                {
                    string path = Path.Combine(Sys.Settings.LibraryLocation, PendingDelete[i]);
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        else if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                    }
                    catch (IOException)
                    {
                        System.Diagnostics.Debug.Write(String.Format("File {0} could not be accessed - in use? Leaving in list.", path));
                        continue;
                    }
                    System.Diagnostics.Debug.Write(String.Format("File {0} successfully deleted.", path));
                    PendingDelete.RemoveAt(i);
                }

                if (oldCount != PendingDelete.Count) Sys.Save();
            }
        }

        public Library()
        {
            DefaultUpdate = UpdateType.Notify;
            Items = new List<InstalledItem>();
            PendingDelete = new List<string>();
        }
    }

    public enum UpdateType
    {
        Notify,
        Install,
        Ignore,
    }

    public class InstalledItem
    {
        public Guid ModID { get; set; }
        public Mod CachedDetails { get; set; }
        public string CachePreview { get; set; }
        public UpdateType UpdateType { get; set; }

        public List<InstalledVersion> Versions { get; set; }
        public InstalledVersion LatestInstalled { get { return Versions.OrderBy(v => v.VersionDetails.Version).Last(); } }

        public int SavedSortOrder { get; set; }
    }

    public class InstalledVersion
    {
        public ModVersion VersionDetails { get; set; }
        public string InstalledLocation { get; set; }

        public Stream GetData(string name)
        {
            string path = Path.Combine(Sys.Settings.LibraryLocation, InstalledLocation);

            if (InstalledLocation.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase) && File.Exists(path))
            {
                using (var arc = new _7thWrapperLib.IrosArc(path))
                    return arc.GetData(name);
            }
            else
            {
                path = Path.Combine(path, name);
                if (File.Exists(path))
                    return new FileStream(path, FileMode.Open, FileAccess.Read);
            }

            return null;
        }

        public bool HasData(string name)
        {
            using (Stream s = GetData(name))
            {
                return s != null;
            }
        }
    }

    public enum ModStatus
    {
        NotInstalled,
        Downloading,
        Installed,
        Updating,
        InfoChanged
    }
}
