using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeavenUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _catFile;
        private string _statusMessage;
        private string _currentProfile;
        private _7thWrapperLib.LoaderContext _context;

        public string WindowTitle
        {
            get
            {
                return $"{App.GetAppName()} v{App.GetAppVersion().ToString()} - Ultimate Mod Manager for Final Fantasy 7 [{CurrentProfile}]";
            }
        }

        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                Logger.Info(_statusMessage);
                NotifyPropertyChanged();
            }
        }

        public string CurrentProfile
        {
            get
            {
                return _currentProfile;
            }
            set
            {
                _currentProfile = value;
                NotifyPropertyChanged(nameof(WindowTitle));
            }
        }

        public MainWindowViewModel()
        {
            CurrentProfile = "Default Profile";
            StatusMessage = "Welcome to 7th Heaven ...";
        }

        public void InitViewModel()
        {
            StatusMessage = $"{App.GetAppName()} started: app v{App.GetAppVersion().ToString()} - dll v{Sys.Version.ToString()}";

            MegaIros.Logger = Logger.Info;

            CopyDllAndUpdaterExe();

            LoadCatalogXmlFile();

            InitLoaderContext();

            Sys.MessageReceived += Sys_MessageReceived;

            InitActiveProfile();
        }

        private void InitLoaderContext()
        {
            _context = new _7thWrapperLib.LoaderContext()
            {
                VarAliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            };

            string varFile = Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location, ".var");

            if (File.Exists(varFile))
            {
                foreach (string line in File.ReadAllLines(varFile))
                {
                    string[] parts = line.Split(new[] { '=' }, 2);

                    if (parts.Length == 2)
                        _context.VarAliases[parts[0]] = parts[1];
                }
            }
        }

        private void InitActiveProfile()
        {
            Sys.ActiveProfile = null;

            Directory.CreateDirectory(Path.Combine(Sys.SysFolder, "profiles"));

            if (!String.IsNullOrWhiteSpace(Sys.Settings.CurrentProfile) && File.Exists(Sys.ProfileFile))
            {
                try
                {
                    Sys.ActiveProfile = Util.Deserialize<Profile>(Sys.ProfileFile);
                    RefreshProfile();
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Failed to load current profile xml ... Setting current profile to null");
                    Sys.Settings.CurrentProfile = null;
                }
            }

            if (Sys.ActiveProfile == null)
            {
                Sys.ActiveProfile = new Profile();
                Sys.Settings.CurrentProfile = "Default";
                Sys.Save();

                CurrentProfile = Sys.Settings.CurrentProfile;
            }
        }

        public void RefreshProfile()
        {
            if (Sys.ActiveProfile == null)
            {
                return;
            }

            CurrentProfile = Sys.Settings.CurrentProfile;

            // TODO: reload list of active mods ???
        }

        internal void SaveProfile()
        {
            if (Sys.ActiveProfile != null)
            {
                try
                {
                    using (FileStream fs = new FileStream(Sys.ProfileFile, System.IO.FileMode.Create))
                    {
                        Util.Serialize(Sys.ActiveProfile, fs);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to save profile ...");
                }
            }
        }

        private void Sys_MessageReceived(object sender, MessageEventArgs e)
        {
            string receivedMessage = e.Message.Text;

            if (!String.IsNullOrEmpty(e.Message.Link))
            {
                receivedMessage += $" - {e.Message.Link}";
            }

            StatusMessage = receivedMessage;
        }

        private void LoadCatalogXmlFile()
        {
            try
            {
                _catFile = Path.Combine(Sys.SysFolder, "catalog.xml");
                Sys.Catalog = Util.Deserialize<Catalog>(_catFile);
            }
            catch (Exception e)
            {
                Logger.Warn(e, "failed to load catalog.xml - initializing empty catalog ...");
                Sys.Catalog = new Catalog();
            }
        }

        private static void CopyDllAndUpdaterExe()
        {
            try
            {
                string src = Path.Combine(Sys._7HFolder, "SharpCompressU.cpy");
                string dst = Path.Combine(Sys._7HFolder, "SharpCompressU.dll");

                if (File.Exists(dst))
                {
                    File.Delete(dst);
                    File.Copy(src, dst);
                }

                src = Path.Combine(Sys._7HFolder, "Updater.cpy");
                dst = Path.Combine(Sys._7HFolder, "Updater.exe");
                if (File.Exists(dst))
                {
                    File.Delete(dst);
                    File.Copy(src, dst);
                }
            }
            catch (IOException iex)
            {
                Logger.Warn(iex);
            }
            catch (UnauthorizedAccessException uae)
            {
                Logger.Warn(uae);
            }
        }
    }
}
