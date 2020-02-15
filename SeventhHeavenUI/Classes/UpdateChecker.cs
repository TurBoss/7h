using _7thHeaven.Code;
using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeavenUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeaven.Classes
{
    /// <summary>
    /// Use <see cref="Instance"/> of this class to check for updates
    /// </summary>
    public class UpdateChecker
    {
        public const string linkToVersionXml = "https://pastebin.com/raw/TriEpAcK";
        private static UpdateChecker _instance;

        public delegate void OnUpdateChecked(bool wasSuccessful);
        public event OnUpdateChecked UpdateCheckCompleted;

        public static UpdateChecker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UpdateChecker();

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        /// <summary>
        /// Downloads the file at <see cref="linkToVersionXml"/> and attempts to deserialize it into <see cref="Sys.LastCheckedVersion"/>.
        /// Uses the download queue so this happens asynchronously. <see cref="UpdateCheckCompleted"/> is raised when the download is complete.
        /// </summary>
        public void CheckForUpdates()
        {
            string path = Path.Combine(Sys.PathToTempFolder, "version.xml");
            Directory.CreateDirectory(Sys.PathToTempFolder);

            DownloadItem download = new DownloadItem()
            {
                Links = new List<string>() { LocationUtil.FormatHttpUrl(linkToVersionXml) },
                SaveFilePath = path,
                Category = DownloadCategory.AppUpdate,
                ItemName = $"Checking for updates at {linkToVersionXml}"
            };

            download.IProc = new Install.InstallProcedureCallback(e =>
            {
                bool success = (e.Error == null && e.Cancelled == false);

                if (success)
                {
                    try
                    {
                        Sys.LastCheckedVersion = Util.Deserialize<AvailableUpdate>(path);
                    }
                    catch (Exception ex)
                    {
                        Sys.LastCheckedVersion = Sys.LastCheckedVersion ?? new AvailableUpdate(); // if currently null then initialize new instance
                        Sys.Message(new WMessage() { Text = $"Failed to check for updates at {linkToVersionXml}: {ex.Message}", LoggedException = ex });
                    }
                }
                else
                {
                    Sys.LastCheckedVersion = Sys.LastCheckedVersion ?? new AvailableUpdate();
                    Sys.Message(new WMessage() { Text = $"Failed to check for updates at {linkToVersionXml}", LoggedException = e.Error });
                }

                UpdateCheckCompleted?.Invoke(success);
            });

            Sys.Downloads.AddToDownloadQueue(download);
        }

        public static bool IsNewVersionAvailable(AvailableUpdate availableUpdate)
        {
            if (Version.TryParse(availableUpdate.Version, out Version availableVersion))
            {
                return availableVersion.CompareTo(App.GetAppVersion()) > 0;
            }

            return false;
        }
    }

}
