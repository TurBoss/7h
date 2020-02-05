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
        public delegate void OnImportProgressChanged(string message, double percentComplete);
        public event OnImportProgressChanged ImportProgressChanged;

        public void Import(string source, string name, bool iroMode, bool noCopy)
        {
            Mod m = ParseModXmlFromSource(source); // this will increment the progress changed value up to 40%

            if (string.IsNullOrWhiteSpace(m.Name))
            {
                m.Name = name;
            }

            // validate mod with same ID doesn't exist in library
            InstalledItem existingItem = Sys.Library.GetItem(m.ID);
            if (existingItem != null)
            {
                throw new DuplicateModException($"A mod ({existingItem.CachedDetails.Name}) with the same ID already exists in your library.");
            }

            string copyLocation;
            if (noCopy)
                copyLocation = Path.GetFileName(source);
            else
                copyLocation = String.Format("{0}_{1}", m.ID, name);

            if (!iroMode)
            {
                if (!noCopy)
                {
                    int i = 1;
                    string[] allFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                    foreach (string file in allFiles)
                    {
                        string part = file.Substring(source.Length).Trim('\\', '/');
                        string dest = Path.Combine(Sys.Settings.LibraryLocation, copyLocation, part);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        File.Copy(file, dest, true);

                        double newProgress = 45.0 + (((double)i / allFiles.Length) * 45); // start at 45 and eventually increment to 90 (i.e. 45 + 45 = 90)
                        RaiseProgressChanged($"Copying files from folder {i} / {allFiles.Length}", newProgress);
                        i++;
                    }
                }
            }
            else
            {
                if (!noCopy)
                {
                    RaiseProgressChanged("Copying .iro file to library", 75);
                    copyLocation += ".iro";
                    File.Copy(source, Path.Combine(Sys.Settings.LibraryLocation, copyLocation), true);
                }
            }

            RaiseProgressChanged("Finalizing import", 95);

            Sys.Library.AddInstall(new InstalledItem()
            {
                CachedDetails = m,
                CachePreview = String.Empty,
                ModID = m.ID,
                UpdateType = UpdateType.Ignore,
                Versions = new List<InstalledVersion>() { new InstalledVersion() { VersionDetails = m.LatestVersion, InstalledLocation = copyLocation } },
            });

            Sys.ActiveProfile.AddItem(new ProfileItem() { ModID = m.ID, Name = m.Name, Settings = new List<ProfileSetting>(), IsModActive = false });
            RaiseProgressChanged("Import complete", 100);
        }

        private Mod ParseModXmlFromSource(string sourceFileOrFolder)
        {
            Mod parsedMod = new Mod()
            {
                Author = String.Empty,
                Description = "Imported mod",
                Category = "Unknown",
                ID = Guid.NewGuid(),
                Link = String.Empty,
                Tags = new List<string>(),
                Name = "",
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

            if (string.IsNullOrWhiteSpace(sourceFileOrFolder))
            {
                return parsedMod;
            }

            if (!File.Exists(sourceFileOrFolder) && !Directory.Exists(sourceFileOrFolder))
            {
                return parsedMod;
            }

            bool isIroFile = sourceFileOrFolder.EndsWith(".iro");
            System.Xml.XmlDocument doc = null;
            Func<string, byte[]> getData = null;
            _7thWrapperLib.IrosArc arc = null;

            if (isIroFile)
            {
                RaiseProgressChanged("Getting mod.xml data from .iro", 10);

                arc = new _7thWrapperLib.IrosArc(sourceFileOrFolder, patchable: false, (i, fileCount) =>
                {
                    double newProgress = 10.0 + ((double)i / fileCount) * 30.0;
                    RaiseProgressChanged($"Scanning .iro archive files {i} / {fileCount}", newProgress);
                });

                if (arc.HasFile("mod.xml"))
                {
                    doc = new System.Xml.XmlDocument();
                    doc.Load(arc.GetData("mod.xml"));
                }

                getData = s =>
                {
                    return arc.HasFile(s) ? arc.GetBytes(s) : null;
                };
            }
            else
            {
                string pathToModXml = Path.Combine(sourceFileOrFolder, "mod.xml");

                RaiseProgressChanged("Getting mod.xml data from file", 10);
                if (File.Exists(pathToModXml))
                {
                    doc = new System.Xml.XmlDocument();
                    doc.Load(pathToModXml);
                }

                getData = s =>
                {
                    string file = Path.Combine(sourceFileOrFolder, s);
                    if (File.Exists(file)) return File.ReadAllBytes(file);
                    return null;
                };
            }


            if (doc != null)
            {
                RaiseProgressChanged("Parsing information from mod.xml", 40);

                //If mod.xml contains an ID GUID, then use that instead of generating random one
                string modidstr = doc.SelectSingleNode("/ModInfo/ID").NodeTextS();
                if (!string.IsNullOrWhiteSpace(modidstr))
                {
                    try
                    {
                        parsedMod.ID = new Guid(modidstr);
                    }
                    catch (Exception e)
                    {
                        Sys.Message(new WMessage("Invalid GUID found for Mod ID ... Using random guid.", WMessageLogLevel.LogOnly, e));
                        parsedMod.ID = Guid.NewGuid();
                    }
                }

                parsedMod.Name = doc.SelectSingleNode("/ModInfo/Name").NodeTextS();

                parsedMod.Author = doc.SelectSingleNode("/ModInfo/Author").NodeTextS();
                parsedMod.Link = doc.SelectSingleNode("/ModInfo/Link").NodeTextS();
                parsedMod.Description = doc.SelectSingleNode("/ModInfo/Description").NodeTextS();
                parsedMod.Category = doc.SelectSingleNode("/ModInfo/Category").NodeTextS();
                parsedMod.LatestVersion.ReleaseNotes = doc.SelectSingleNode("/ModInfo/ReleaseNotes").NodeTextS();

                if (DateTime.TryParse(doc.SelectSingleNode("/ModInfo/ReleaseDate").NodeTextS(), out DateTime parsedDate))
                {
                    parsedMod.LatestVersion.ReleaseDate = parsedDate;
                }

                if (decimal.TryParse(doc.SelectSingleNode("/ModInfo/Version").NodeTextS().Replace(',', '.'), out decimal ver))
                {
                    parsedMod.LatestVersion.Version = ver;
                }

                var pv = doc.SelectSingleNode("/ModInfo/PreviewFile");
                if (pv != null)
                {
                    // add the preview file to image cache and set the url prefixed with iros://Preview/Auto since it came from auto-import
                    byte[] data = getData(pv.InnerText);
                    if (data != null)
                    {
                        string url = "iros://Preview/Auto/" + parsedMod.ID.ToString();
                        parsedMod.LatestVersion.PreviewImage = url;
                        Sys.ImageCache.InsertManual(url, data);
                    }
                }
            }

            if (arc != null)
            {
                arc.Dispose();
            }

            return parsedMod;
        }

        public static void ImportMod(string source, string name, bool iroMode, bool noCopy)
        {
            new ModImporter().Import(source, name, iroMode, noCopy);
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

        public void RaiseProgressChanged(string message, double percentComplete)
        {
            ImportProgressChanged?.Invoke(message, percentComplete);
        }
    }
}
