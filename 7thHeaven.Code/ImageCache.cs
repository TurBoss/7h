/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iros._7th.Workshop {

    public class ImageCacheEntry {
        public string Url { get; set; }
        public string File { get; set; }
        public DateTime Updated { get; set; }
    }

    public class ImageCacheData {
        public List<ImageCacheEntry> Entries { get; set; }
    }

    public class ImageCache {
        private string _folder;
        private Dictionary<string, ImageCacheEntry> _entries;
        private HashSet<string> _usedFiles;

        public ImageCache(string folder) {
            System.IO.Directory.CreateDirectory(folder);
            string file = System.IO.Path.Combine(folder, "cache.xml");
            if (System.IO.File.Exists(file)) {
                ImageCacheData data = Util.Deserialize<ImageCacheData>(file);
                _entries = data.Entries.ToDictionary(e => e.Url, e => e, StringComparer.InvariantCultureIgnoreCase);
            } else
                _entries = new Dictionary<string, ImageCacheEntry>(StringComparer.InvariantCultureIgnoreCase);
            _usedFiles = new HashSet<string>(_entries.Select(e => e.Value.File), StringComparer.InvariantCultureIgnoreCase);
            _folder = folder;
        }

        public void Save() {
            string file = System.IO.Path.Combine(_folder, "cache.xml");
            lock (_entries) {
                var data = new ImageCacheData() { Entries = _entries.Values.ToList() };
                using (var fs = new System.IO.FileStream(file, System.IO.FileMode.Create))
                    Util.Serialize(data, fs);
            }
        }

        private void TriggerDownload(string url, Guid modID) {
            ImageCacheEntry e;
            lock (_entries) {
                if (!_entries.TryGetValue(url, out e)) {
                    e = new ImageCacheEntry();
                    e.Url = url;
                    _entries[url] = e;
                }
            }
            e.Updated = DateTime.Now;
            string file = e.File;
            if (String.IsNullOrEmpty(file)) {
                string prefix = url.GetHashCode().ToString("x");
                int count = 0;
                do {
                    file = prefix + count.ToString();
                    count++;
                } while(_usedFiles.Contains(file));
                _usedFiles.Add(file);
            }
            Sys.Downloads.Download("iros://Url/" + url.Replace("://", "$"), System.IO.Path.Combine(_folder, file + ".tmp"), "Downloading preview image", new Install.InstallProcedureCallback(ae => {
                if (ae.Error == null) {
                    lock (_entries) {
                        e.File = file;
                        string f = System.IO.Path.Combine(_folder, file);
                        if (System.IO.File.Exists(f)) System.IO.File.Delete(f);
                        System.IO.File.Move(System.IO.Path.Combine(_folder, file + ".tmp"), f);
                    }
                } else {
                    System.Diagnostics.Debug.WriteLine("ImageCache:Download error: " + ae.Error.ToString());
                }
                Sys.Ping(modID);
            }), null);
        }

        public void InsertManual(string url, byte[] data) {
            ImageCacheEntry e;
            lock (_entries) {
                if (!_entries.TryGetValue(url, out e)) {
                    e = new ImageCacheEntry();
                    e.Url = url;
                    _entries[url] = e;
                }
            }
            e.Updated = DateTime.Now;
            if (String.IsNullOrWhiteSpace(e.File)) {
                string prefix = url.GetHashCode().ToString("x");
                int count = 0;
                do {
                    e.File = prefix + count.ToString();
                    count++;
                } while (_usedFiles.Contains(e.File));
                _usedFiles.Add(e.File);
            }
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(_folder, e.File), data);
        }

        public System.Drawing.Image Get(string url, Guid modID) {
            if (String.IsNullOrWhiteSpace(url)) return null;
            System.Drawing.Image img = null;
            ImageCacheEntry e;
            lock (_entries) {
                if (_entries.TryGetValue(url, out e) && e.File != null) {
                    string file = System.IO.Path.Combine(_folder, e.File);
                    if (System.IO.File.Exists(file)) {
                        try {
                            img = new System.Drawing.Bitmap(new System.IO.MemoryStream(System.IO.File.ReadAllBytes(file)));
                        } catch {
                            img = null;
                        }
                    }
                    if (e.Updated < DateTime.Now.AddDays(-1)) {
                        TriggerDownload(url, modID);
                    } else if (img == null) return null;
                } else
                    TriggerDownload(url, modID);
            }
            if (img == null) img = _7thHeaven.Code.Workshop.Loader;
            return img;
        }

        /// <summary>
        /// Returns absolute path to the cached image file if it exists.
        /// Triggers the image download if it does not exist.
        /// </summary>
        /// <param name="url"> image download url</param>
        /// <param name="modID"> ID of Mod for cached image to look up </param>
        public string GetImagePath(string url, Guid modID)
        {
            if (String.IsNullOrWhiteSpace(url)) return null;

            bool gotValue = false;
            string pathToImage = null;
            ImageCacheEntry e;

            lock (_entries)
            {
                gotValue = _entries.TryGetValue(url, out e);
            }

            if (gotValue && e.File != null)
            {
                string file = Path.Combine(_folder, e.File);
                if (File.Exists(file))
                {
                    pathToImage = file;
                }

                // re-download image if older than a day to keep cache updated
                if (e.Updated < DateTime.Now.AddDays(-1))
                {
                    TriggerDownload(url, modID);
                }
            }
            else
            {
                TriggerDownload(url, modID);
            }

            return pathToImage;
        }
    }
}
