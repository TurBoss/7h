using Iros._7th;
using Iros._7th.Workshop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace _7thHeaven.Code
{
    public class ModImporter
    {
        public static void ImportMod(string source, string name, bool iroMode, bool noCopy)
        {
            Mod m = new Mod()
            {
                Author = String.Empty,
                Description = "Imported mod",
                Category = "Unknown",
                ID = Guid.NewGuid(),
                Link = String.Empty,
                Tags = new List<string>(),
                Name = name,
                LatestVersion = new ModVersion()
                {
                    CompatibleGameVersions = GameVersions.All,
                    Links = new List<string>(),
                    PreviewImage = String.Empty,
                    ReleaseDate = DateTime.Now,
                    ReleaseNotes = String.Empty,
                    Version = 1.00m,
                }
            };

            string location;
            if (noCopy)
                location = Path.GetFileName(source);
            else
                location = String.Format("{0}_{1}", m.ID, name);

            System.Xml.XmlDocument doc = null;
            Func<string, byte[]> getData = null;
            if (!iroMode)
            {
                if (!noCopy)
                {
                    foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                    {
                        string part = file.Substring(source.Length).Trim('\\', '/');
                        string dest = Path.Combine(Sys.Settings.LibraryLocation, location, part);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        File.Copy(file, dest, true);
                    }
                }
                string mx = Path.Combine(Sys.Settings.LibraryLocation, location, "mod.xml");

                if (File.Exists(mx))
                {
                    doc = new System.Xml.XmlDocument();
                    doc.Load(mx);
                }

                getData = s =>
                {
                    string file = Path.Combine(Sys.Settings.LibraryLocation, location, s);
                    if (File.Exists(file)) return File.ReadAllBytes(file);
                    return null;
                };
            }
            else
            {
                if (!noCopy)
                {
                    location += ".iro";
                    File.Copy(source, Path.Combine(Sys.Settings.LibraryLocation, location), true);
                }
                var arc = new _7thWrapperLib.IrosArc(source);
                if (arc.HasFile("mod.xml"))
                {
                    doc = new System.Xml.XmlDocument();
                    doc.Load(arc.GetData("mod.xml"));
                }
                getData = s => arc.HasFile(s) ? arc.GetBytes(s) : null;
            }

            if (doc != null)
            {
                //If mod.xml contains an ID GUID, then use that instead of generating random one
                string modidstr = doc.SelectSingleNode("/ModInfo/ID").NodeTextS();
                if (!string.IsNullOrWhiteSpace(modidstr))
                {
                    try
                    {
                        m.ID = new Guid(modidstr);
                    }
                    catch (Exception e)
                    {
                        Sys.Message(new WMessage("Invalid GUID found for Mod ID ... Using random guid.", WMessageLogLevel.LogOnly, e));
                        m.ID = Guid.NewGuid();
                    }
                }

                m.Name = doc.SelectSingleNode("/ModInfo/Name").NodeTextS();

                if (string.IsNullOrWhiteSpace(m.Name))
                {
                    m.Name = name;
                }

                m.Author = doc.SelectSingleNode("/ModInfo/Author").NodeTextS();
                m.Link = doc.SelectSingleNode("/ModInfo/Link").NodeTextS();
                m.Description = doc.SelectSingleNode("/ModInfo/Description").NodeTextS();
                m.Category = doc.SelectSingleNode("/ModInfo/Category").NodeTextS();
                m.LatestVersion.ReleaseNotes = doc.SelectSingleNode("/ModInfo/ReleaseNotes").NodeTextS();

                if (DateTime.TryParse(doc.SelectSingleNode("/ModInfo/ReleaseDate").NodeTextS(), out DateTime parsedDate))
                {
                    m.LatestVersion.ReleaseDate = parsedDate;
                }

                if (decimal.TryParse(doc.SelectSingleNode("/ModInfo/Version").NodeTextS().Replace(',', '.'), out decimal ver))
                {
                    m.LatestVersion.Version = ver;
                }

                var pv = doc.SelectSingleNode("/ModInfo/PreviewFile");
                if (pv != null)
                {
                    byte[] data = getData(pv.InnerText);
                    if (data != null)
                    {
                        string url = "iros://Preview/Auto/" + m.ID.ToString();
                        m.LatestVersion.PreviewImage = url;
                        Sys.ImageCache.InsertManual(url, data);
                    }
                }
            }

            Sys.Library.AddInstall(new InstalledItem()
            {
                CachedDetails = m,
                CachePreview = String.Empty,
                ModID = m.ID,
                UpdateType = UpdateType.Ignore,
                Versions = new List<InstalledVersion>() { new InstalledVersion() { VersionDetails = m.LatestVersion, InstalledLocation = location } },
            });

            Sys.ActiveProfile.AddItem(new ProfileItem() { ModID = m.ID, Name = m.Name, Settings = new List<ProfileSetting>(), IsModActive = false });
        }

        /// <summary>
        /// Removes guid from passed in string and replaces underscores '_' with spaces if guid is found in string.
        /// </summary>
        /// <param name="name"> name of mod with possible Guid in string </param>
        /// <returns> if Guid found then returns parsed string; other wise returns <paramref name="name"/></returns>
        public static string ParseNameFromFileOrFolder(string name)
        {
            string parsedName = name;

            Regex regex = new Regex(@"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}");
            Match match = regex.Match(name);

            if (match.Success)
            {
                int index = name.IndexOf(match.Value) + match.Length;
                parsedName = name.Substring(index);
                parsedName = parsedName.Replace("_", " ").Trim();
            }

            return parsedName;
        }
    }
}
