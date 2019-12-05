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

    public class ModStatusEventArgs : EventArgs
    {
        public ModStatus OldStatus { get; set; }
        public ModStatus Status { get; set; }
        public Guid ModID { get; set; }
    }

    public class MessageEventArgs : EventArgs
    {
        public WMessage Message { get; set; }
    }

    public class LinkEventArgs : EventArgs
    {
        public string Link { get; set; }
    }

    public class WMessage
    {
        public WMessage()
        {
        }

        public WMessage(string message)
        {
            Text = message;
        }

        public string Text { get; set; }
        public string Link { get; set; }
        //TODO ....


    }

    public interface IDownloader
    {
        void Download(string link, string file, string description, Install.InstallProcedure iproc, Action onCancel);
        void Download(IEnumerable<string> links, string file, string description, Install.InstallProcedure iproc, Action onCancel);
        void BringToFront();
    }

    public static class Sys
    {
        public const decimal Version = 1.59m;

        private static Dictionary<Type, object> _single = new Dictionary<Type, object>();

        public static object CatalogLock = new object();

        public static T GetSingle<T>()
        {
            object o;
            _single.TryGetValue(typeof(T), out o);
            return (T)o;
        }

        public static void SetSingle<T>(T t)
        {
            _single[typeof(T)] = t;
        }

        public static Settings Settings { get; private set; }
        public static string _7HFolder { get; private set; }
        public static string SysFolder { get; private set; }

        public static string PathToCurrentProfileFile
        {
            get
            {
                return Path.Combine(PathToProfiles, $"{Settings.CurrentProfile}.xml");
            }
        }

        public static string PathToProfiles
        {
            get
            {
                return Path.Combine(SysFolder, "profiles");
            }
        }

        public static string PathToTempFolder
        {
            get
            {
                return Path.Combine(SysFolder, "temp");
            }
        }

        public static Catalog Catalog { get; set; }
        public static Library Library { get; set; }
        public static ImageCache ImageCache { get; private set; }
        public static IDownloader Downloads { get; set; }

        public static Profile ActiveProfile { get; set; }

        public static event EventHandler<ModStatusEventArgs> StatusChanged;
        public static event EventHandler<MessageEventArgs> MessageReceived;
        public static event EventHandler<LinkEventArgs> GotoLink;

        private static Dictionary<Guid, ModStatus> _statuses;
        private static List<WMessage> _pendingMessages = new List<WMessage>();

        public static void TriggerLink(string link)
        {
            var e = new LinkEventArgs() { Link = link };
            GotoLink(null, e);
        }

        public static void Message(WMessage m)
        {
            if (MessageReceived != null)
            {
                foreach (var pending in _pendingMessages)
                    MessageReceived(null, new MessageEventArgs() { Message = pending });
                _pendingMessages.Clear();
                MessageReceived(null, new MessageEventArgs() { Message = m });
            }
            else
                _pendingMessages.Add(m);
        }

        public static void Ping(Guid modID)
        {
            ModStatus st;
            _statuses.TryGetValue(modID, out st);
            ModStatusEventArgs e = new ModStatusEventArgs() { ModID = modID, Status = st, OldStatus = st };
            StatusChanged?.Invoke(null, e);
        }
        public static void SetStatus(Guid modID, ModStatus status)
        {
            ModStatus olds;
            _statuses.TryGetValue(modID, out olds);
            _statuses[modID] = status;
            ModStatusEventArgs e = new ModStatusEventArgs() { ModID = modID, Status = status, OldStatus = olds };
            StatusChanged?.Invoke(null, e);
        }
        public static void RevertStatus(Guid modID)
        {
            var lib = Library.GetItem(modID);
            SetStatus(modID, lib == null ? ModStatus.NotInstalled : ModStatus.Installed);
        }
        public static ModStatus GetStatus(Guid modID)
        {
            ModStatus s;
            _statuses.TryGetValue(modID, out s);
            return s;
        }

        public static void Save()
        {
            string lfile = Path.Combine(SysFolder, "library.xml");
            string sfile = Path.Combine(SysFolder, "settings.xml");

            Directory.CreateDirectory(Path.GetDirectoryName(lfile));

            using (var fs = new FileStream(lfile, FileMode.Create))
                Util.Serialize(Library, fs);


            using (var fs = new FileStream(sfile, FileMode.Create))
                Util.Serialize(Settings, fs);

            ImageCache.Save();
        }

        /// <summary>
        /// Updates <see cref="Catalog"/> to <paramref name="newCatalog"/>; 
        /// thread safe by locking the object
        /// </summary>
        /// <param name="newCatalog"></param>
        public static void SetNewCatalog(Catalog newCatalog)
        {
            lock (CatalogLock)
            {
                Catalog = newCatalog;
            }
        }

        /// <summary>
        /// Returns a Mod in <see cref="Catalog"/>
        /// ... uses <see cref="CatalogLock"/> to ensure the catalog does not change
        /// when multiple threads are accessing/modifying it at once.
        /// </summary>
        /// <param name="modId"></param>
        /// <returns></returns>
        public static Mod GetModFromCatalog(Guid modId)
        {
            Mod m = null;

            lock (CatalogLock)
            {
                m = Catalog.GetMod(modId);
            }

            return m;
        }

        static Sys()
        {
            string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _7HFolder = appPath;

            SysFolder = Path.Combine(appPath, "7thWorkshop");
            Directory.CreateDirectory(SysFolder);


            string sfile = Path.Combine(SysFolder, "settings.xml");
            if (File.Exists(sfile))
            {
                try
                {
                    Settings = Util.Deserialize<Settings>(sfile);
                }
                catch
                {
                    Sys.Message(new WMessage() { Text = "Error loading settings - please configure 7H using the Workshop/Settings menu" });
                }
            }
            if (Settings == null)
            {
                Settings = new Settings();
                Settings.Options = GeneralOptions.AutoImportMods | GeneralOptions.SetEXECompatFlags;
                Settings.SubscribedUrls.Add("iros://Url/http$pastebin.com/raw.php?i=dDBkYkDu");
                Settings.SubscribedUrls.Add("iros://Url/http$pastebin.com/raw.php?i=QBGsgGK6");
            }

            string lfile = Path.Combine(SysFolder, "library.xml");
            if (File.Exists(lfile))
            {
                try
                {
                    Library = Util.Deserialize<Library>(lfile);
                }
                catch
                {
                    Sys.Message(new WMessage() { Text = "Error loading library file" });
                }
            }

            if (Library == null)
                Library = new Library();

            _statuses = Library.Items.ToDictionary(i => i.ModID, _ => ModStatus.Installed);

            ImageCache = new ImageCache(System.IO.Path.Combine(SysFolder, "cache"));
        }
    }
}
