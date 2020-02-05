/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iros._7th.Workshop
{
    public class Catalog
    {
        private Dictionary<Guid, Mod> _lookup;

        [System.Xml.Serialization.XmlAttribute("Name")]
        public string Name { get; set; }

        public List<Mod> Mods { get; set; }

        public Catalog()
        {
            Mods = new List<Mod>();
        }
        //https://mega.co.nz/#F!DYQWTZ7B!FWil2nif2HQF_Dr7RxvOLA

        public Mod GetMod(Guid modID)
        {
            if (_lookup == null)
            {
                _lookup = Mods.ToDictionary(m => m.ID, m => m);
            }
            Mod mod;
            _lookup.TryGetValue(modID, out mod);
            return mod;
        }

        public static Catalog Merge(Catalog c1, Catalog c2, out List<Guid> pingIDs)
        {
            Dictionary<Guid, Mod> mods = c1.Mods.ToDictionary(m => m.ID, m => m);
            pingIDs = new List<Guid>();

            foreach (var mod in c2.Mods)
            {
                Mod m;
                if (mods.TryGetValue(mod.ID, out m))
                {
                    if (mod.LatestVersion.Version > m.LatestVersion.Version || mod.MetaVersion > m.MetaVersion)
                    {
                        mods[mod.ID] = mod;
                        pingIDs.Add(mod.ID);
                    }
                }
                else
                {
                    mods[mod.ID] = mod;
                }
            }

            return new Catalog() { Mods = mods.Values.ToList() };
        }
    }

    public class Mod
    {
        public Guid ID { get; set; }
        public string Author { get; set; }
        public string Link { get; set; }
        public ModVersion LatestVersion { get; set; }
        public decimal MetaVersion { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string Category { get; set; }

        public string SourceCatalogName { get; set; }

        public string SourceCatalogUrl { get; set; }

        public List<string> Tags { get; set; }
        [System.Xml.Serialization.XmlElement("Patch")]
        public List<ModPatch> Patches { get; set; }
        [System.Xml.Serialization.XmlElement("Requirement")]
        public List<ModRequirement> Requirements { get; set; }

        public IEnumerable<ModPatch> GetPatchesFromTo(decimal from, decimal to)
        {
            foreach (var patch in Patches)
            {
                if (patch.VerTo == to && patch.VerFrom.Split(',').Select(s => decimal.Parse(s)).Contains(from))
                    yield return patch;
            }
        }

        public int SearchRelevance(string text)
        {
            if (Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0) return 200;
            if (Description.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0) return 100;
            if (Author.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0) return 50;
            if (Category?.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0) return 25;
            return 0;
        }

        /// <summary>
        /// Compares properties to another Mod to see if 
        /// name, description, category, author, or link is different.
        /// </summary>
        /// <param name="other"></param>
        /// <returns> true if properties are different; false if they are equal. </returns>
        internal bool HasChanges(Mod other)
        {
            return (this.Name != other.Name ||
                    this.Description != other.Description ||
                    this.Category != other.Category ||
                    this.Author != other.Author ||
                    this.Link != other.Link);
        }
    }

    public class ModRequirement
    {
        public string Description { get; set; }
        public Guid ModID { get; set; }
    }

    public class ModPatch
    {
        [System.Xml.Serialization.XmlAttribute("VerFrom")]
        public string VerFrom { get; set; }
        [System.Xml.Serialization.XmlAttribute("VerTo")]
        public decimal VerTo { get; set; }
        [System.Xml.Serialization.XmlText]
        public string Link { get; set; }
        [System.Xml.Serialization.XmlAttribute("DownloadSize")]
        public int DownloadSize { get; set; }

    }

    public enum LocationType
    {
        INVALID,
        Url,
        MegaSharedFolder, //Format: SharedFolderLink,FileIDString,HintFileName
        GDrive,
    }

    public static class LocationUtil
    {
        public static bool Parse(string link, out LocationType type, out string url)
        {
            if (link.StartsWith("iros://", StringComparison.InvariantCultureIgnoreCase)) link = link.Substring(7);
            string[] parts = link.Split(new[] { '/' }, 2);
            type = LocationType.INVALID; url = null;

            if (parts.Length < 2) return false;
            if (!Enum.TryParse<LocationType>(parts[0], out type)) return false;

            url = parts[1];
            int dpos = url.IndexOf('$');
            if (dpos >= 0) url = url.Substring(0, dpos) + "://" + url.Substring(dpos + 1);
            return true;
        }
    }

    [Flags]
    public enum GameVersions
    {
        Original = 0x1,
        Rerelease = 0x2,
        Steam = 0x4,
        All = 0xff
    }

    public class ModVersion
    {
        public int DownloadSize { get; set; }
        [System.Xml.Serialization.XmlElement("Link")]
        public List<string> Links { get; set; }
        [System.Xml.Serialization.XmlElement("ApplyPatch")]
        public List<string> PatchLinks { get; set; }
        public string ExtractSubFolder { get; set; }
        public string ExtractInto { get; set; }

        public decimal Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public GameVersions CompatibleGameVersions { get; set; }

        public string PreviewImage { get; set; }
        public string ReleaseNotes { get; set; }

    }
}
