using _7thHeaven.Code;
using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using SeventhHeaven.Windows;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using NLog;
using System.IO;

namespace SeventhLauncher
{


    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static MainWindow _instance;

        public static MainWindow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MainWindow();

                return _instance;
            }
        }

        public delegate void OnProgressChanged(string message);
        public event OnProgressChanged ProgressChanged;

        public delegate void OnLaunchCompleted(bool wasSuccessful);
        public event OnLaunchCompleted LaunchCompleted;


        private List<Mod> _missingMods;
        internal GeneralSettingsViewModel SettingsVM { get; set; }
        internal MainWindowViewModel MainWindowVM { get; set; }
        internal DownloadItemViewModel DownloadVM { get; set; }
        internal MyModsViewModel ModsVM { get; set; }
        public CatalogViewModel CatalogVM { get; set; }

        public bool show_content { get; private set; }
        public bool show_options { get; private set; }
        public bool is_dev { get; private set; }
        public string download_msg { get; private set; }

        public MainWindow()
        {

            InitializeComponent();

            SettingsVM = new GeneralSettingsViewModel();
            MainWindowVM = new MainWindowViewModel();
            ModsVM = new MyModsViewModel();
            CatalogVM = new CatalogViewModel();

            this.DataContext = MainWindowVM;

            this.ctrlMyMods.SetDataContext(ModsVM);
            this.ctrlCatalog.SetDataContext(CatalogVM);



            this.show_content = false;
            this.show_options = false;
            this.is_dev = true;
            this.download_msg = "";

            this.ctrlMyMods.Visibility = Visibility.Hidden;
            this.ctrlCatalog.Visibility = Visibility.Hidden;

            this.profile_button.Visibility = Visibility.Hidden;
            this.launcher_button.Visibility = Visibility.Hidden;
            this.ffnx_button.Visibility = Visibility.Hidden;
            this.controller_button.Visibility = Visibility.Hidden;
            this.system_button.Visibility = Visibility.Hidden;
            this.content_button.Visibility = Visibility.Hidden;

            this.download_bar.Visibility = Visibility.Hidden;
            this.download_text.Visibility = Visibility.Hidden;
            this.play_button.IsEnabled = false;

        }

        public void Convert98()
        {

            Console.WriteLine("Initializing coneversion to 98 version");

            // GameConverter converter = new GameConverter(Path.GetDirectoryName(Sys.Settings.FF7Exe));
            // converter.MessageSent += GameConverter_MessageSent;



            string initialDir = "";

            if (File.Exists(SettingsVM.FF7ExePathInput))
            {
                initialDir = Path.GetDirectoryName(SettingsVM.FF7ExePathInput);
            }

            // string exePath = FileDialogHelper.BrowseForFile("*.exe|*.exe", ResourceHelper.Get(StringKey.SelectFf7Exe), initialDir);

            string exePath = initialDir;


            if (!string.IsNullOrEmpty(exePath))
            {
                FileInfo fileSelected = new FileInfo(exePath);
                if (fileSelected.Name.Equals("ff7_en.exe", System.StringComparison.InvariantCultureIgnoreCase) || fileSelected.Name.Equals("FF7_Launcher.exe", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    // User selected the exe's for Steam release so we try to auto copy the 1.02 patched exe and select it for them
                    string targetPathToFf7Exe = Path.Combine(fileSelected.DirectoryName, "ff7.exe");
                    string copyOrSelectMessage = ResourceHelper.Get(StringKey.Selected);

                    if (!File.Exists(targetPathToFf7Exe))
                    {
                        // use game converter to copy files over
                        var gc = new GameConverter(fileSelected.DirectoryName);
                        if (!gc.CopyFF7ExeToGame())
                        {

                            Console.WriteLine("ERROR CONVERTING!");

                            // MessageDialogWindow.Show(ResourceHelper.Get(StringKey.ThisExeIsUsedForSteamReleaseFailedToCopyExe),
                            //                         ResourceHelper.Get(StringKey.ErrorIncorrectExe),
                            //                         MessageBoxButton.OK,
                            //                         MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            Console.WriteLine("CONVERT OK");
                        }

                        copyOrSelectMessage = ResourceHelper.Get(StringKey.CopiedAndSelected);
                    }

                    SettingsVM.FF7ExePathInput = targetPathToFf7Exe;

                    MessageDialogWindow.Show(string.Format(ResourceHelper.Get(StringKey.ThisExeIsUsedForSteamReleaseCopiedSelectedForYou), copyOrSelectMessage),
                                             ResourceHelper.Get(StringKey.ErrorIncorrectExe),
                                             MessageBoxButton.OK,
                                             MessageBoxImage.Warning);
                    Console.WriteLine("ok");
                    return;
                }

                if (fileSelected.Name.Equals("FF7Config.exe", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    // MessageDialogWindow.Show(ResourceHelper.Get(StringKey.ThisExeIsUsedForConfiguringFf7Settings),
                    //                         ResourceHelper.Get(StringKey.ErrorIncorrectExe),
                    //                         MessageBoxButton.OK,
                    //                         MessageBoxImage.Error);

                    Console.WriteLine("Wrong exe");

                    return;
                }

                SettingsVM.FF7ExePathInput = exePath;
            }
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("init done");
            MainWindowVM.InitViewModel();
            var main_loop = Main();
        }



        private async Task Main()
        {
            await LoopCheck();
        }


        private async Task LoopCheck()
        {

            while (true)
            {


                GetGameContent();

                if (!is_dev)
                {
                    MainWindowVM.MyMods.AutoSortBasedOnCategory();
                    MainWindowVM.MyMods.ActivateAllMods();
                }


                int total_mods = Sys.Catalog.Mods.Count();
                int downloading_mods = MainWindowVM.CatalogMods.DownloadList.Count;
                int installed_mods = Sys.ActiveProfile.Items.Count();

                if (isOkToPlay())
                {
                    this.play_button.Content = "Play";
                    this.play_button.IsEnabled = true;
                    this.download_bar.Visibility = Visibility.Hidden;
                    this.download_text.Visibility = Visibility.Hidden;
                }
                else if (downloading_mods > 0)
                {
                    this.play_button.Content = "Downloading";
                    this.download_bar.Visibility = Visibility.Visible;
                    this.download_text.Visibility = Visibility.Visible;
                }
                else
                {
                    this.play_button.Content = "Install";
                    this.download_bar.Visibility = Visibility.Hidden;
                    this.download_text.Visibility = Visibility.Hidden;
                }

                DownloadItemViewModel viewModel = null;
                viewModel = MainWindowVM.CatalogMods.DownloadList.FirstOrDefault();

                string download_name = "";
                string download_speed = "";
                double download_percent = 0.0;
                string download_time = "";

                if (viewModel != null)
                {
                    download_name = viewModel.ItemName;
                    download_speed = viewModel.DownloadSpeed;
                    download_percent = viewModel.PercentComplete;
                    download_time = viewModel.RemainingTime;
                }


                download_msg = String.Format("Downloading {0}/{1} {2}% {3} {4}", installed_mods, total_mods, download_percent, download_speed, download_time);

                this.download_bar.Value = download_percent;
                this.download_text.Text = download_msg;

                Console.WriteLine("LOOP");
                await Task.Delay(1000);
            }
        }

        public void GetGameContent()
        {
            //Console.WriteLine("Checking for missing content");

            Catalog catalog = Sys.Catalog;
            Profile profile = Sys.ActiveProfile;

            if (_missingMods == null)
            {
                _missingMods = new List<Mod>();
            }

            //Console.WriteLine("Checking for missing mods");
            foreach (Mod mod in catalog.Mods)
            {
                ProfileItem v = profile.GetItem(mod.ID);

                if (v == null)
                {
                    if (!_missingMods.Contains(mod))
                    {
                        _missingMods.Add(mod);
                    }
                }
            }
        }

        public void GetMissingContent()
        {

            GetGameContent();

            if (_missingMods != null)
            {
                // Console.WriteLine("Installing content");
                foreach (Mod missingMod in _missingMods)
                {
                    //Console.WriteLine(missingMod.ID);

                    ModStatus _mod_status = Sys.GetStatus(missingMod.ID);
                    //Console.WriteLine(_mod_status.ToString());

                    Install.DownloadAndInstall(missingMod, false);
                }
            }

            MainWindowVM.MyMods.AutoSortBasedOnCategory();
            MainWindowVM.MyMods.ScanForModUpdates();
        }

        public bool isOkToPlay()
        {
            bool install_ok = true;

            Catalog catalog = Sys.Catalog;
            Profile profile = Sys.ActiveProfile;

            foreach (Mod mod in catalog.Mods)
            {
                ProfileItem v = profile.GetItem(mod.ID);
                Console.WriteLine(v);
                if (v == null)
                {
                    install_ok = false;
                }
            }

            return install_ok;
        }


        private void options_button_Click(object sender, RoutedEventArgs e)
        {
            this.show_options = !this.show_options;


            if (this.show_options)
            {

                this.profile_button.Visibility = Visibility.Visible;
                this.launcher_button.Visibility = Visibility.Visible;
                this.ffnx_button.Visibility = Visibility.Visible;
                this.controller_button.Visibility = Visibility.Visible;
                this.system_button.Visibility = Visibility.Visible;
                this.content_button.Visibility = Visibility.Visible;
            }
            else
            {
                this.profile_button.Visibility = Visibility.Hidden;
                this.launcher_button.Visibility = Visibility.Hidden;
                this.ffnx_button.Visibility = Visibility.Hidden;
                this.controller_button.Visibility = Visibility.Hidden;
                this.system_button.Visibility = Visibility.Hidden;
                this.content_button.Visibility = Visibility.Hidden;

                if (this.is_dev)
                {
                    this.ctrlMyMods.Visibility = Visibility.Hidden;
                }
                this.ctrlCatalog.Visibility = Visibility.Hidden;

                this.show_content = this.show_options;
            }


        }


        internal void RaiseProgressChanged(string messageToLog, NLog.LogLevel logLevel = null)
        {
            if (logLevel == null)
            {
                logLevel = NLog.LogLevel.Info;
            }

            Logger.Log(logLevel, messageToLog);
            ProgressChanged?.Invoke(messageToLog);
        }

        internal void RaiseLaunchCompleted(bool didLaunch)
        {
            LaunchCompleted?.Invoke(didLaunch);
        }


        private void play_button_Click(object sender, RoutedEventArgs e)
        {
            // launch the downloads or the game
            if (!this.isOkToPlay())
            {
                this.play_button.IsEnabled = false;

                GetMissingContent();
            }
            else
            {
                MainWindowVM.LaunchGame(variableDump: false, debugLogging: false);
                if (!this.is_dev) quit_app();

            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MainWindowVM.CatalogMods.DownloadList.Count > 0)
            {
                var result = MessageDialogWindow.Show(ResourceHelper.Get(StringKey.AreYouSureYouWantToExitPendingDownloads), ResourceHelper.Get(StringKey.ConfirmExit), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result.Result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            quit_app();
        }

        private void quit_app()
        {

            Sys.Settings.MainWindow = new SavedWindow()
            {
                X = (int)System.Windows.Application.Current.MainWindow.Left,
                Y = (int)System.Windows.Application.Current.MainWindow.Top,
                W = (int)ActualWidth,
                H = (int)ActualHeight,
                State = WindowState
            };

            //  this.ctrlCatalog.SaveUsersColumnSettings();

            Sys.Save();

        }

        private void quit_button_Click(object sender, RoutedEventArgs e)
        {
            quit_app();
        }

        private void convert_button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("HOLA?");
            this.Convert98();
        }
        private void profile_button_Click(object sender, RoutedEventArgs e)
        {

            MainWindowVM.ShowProfilesWindow();
        }

        private void controller_button_Click(object sender, RoutedEventArgs e)
        {
            new ControlMappingWindow().ShowDialog();
        }

        private void system_button_Click(object sender, RoutedEventArgs e)
        {
            MainWindowVM.ShowGeneralSettingsWindow();
        }

        private void ffnx_button_Click(object sender, RoutedEventArgs e)
        {

            // MessageDialogWindow.Show("HOLA", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

            // Check if there are errors in the FFNx.toml file and show to the user the output
            Exception FFNxConfigError = Sys.FFNxConfig.GetLastError();
            if (FFNxConfigError != null)
            {
                MessageDialogWindow.Show("Something went wrong while parsing \"" + Sys.PathToFFNxToml + "\".\n\n" + Encoding.UTF8.GetString(Encoding.Default.GetBytes(FFNxConfigError.Message)), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            if (File.Exists(Sys.PathToFFNxToml))
            {
                Sys.FFNxConfig.Reload();

                try
                {
                    // CustomGLWindow gLWindow = new CustomGLWindow();
                    // if (gLWindow.Init(Sys.PathToGameDriverUiXml(Sys.Settings.AppLanguage)))
                    // {
                    //     gLWindow.ShowDialog();
                    // }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    MessageDialogWindow.Show(ResourceHelper.Get(StringKey.PleaseUpdateFFNx), ResourceHelper.Get(StringKey.Error), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageDialogWindow messageDialog = new MessageDialogWindow(ResourceHelper.Get(StringKey.MissingPath), $"FFNx.toml {ResourceHelper.Get(StringKey.FileNotFound)}\n\nPlease run the game at least once and try again.", MessageBoxButton.OK, MessageBoxImage.Warning);
                messageDialog.ShowDialog();
            }


            //MainWindowVM.ShowGameDriverConfigWindow();
        }

        private void launcher_button_Click(object sender, RoutedEventArgs e)
        {
            MainWindowVM.ShowGameLaunchSettingsWindow();
        }

        private void content_button_Click(object sender, RoutedEventArgs e)
        {



            this.show_content = !this.show_content;


            if (this.show_content)
            {
                if (this.is_dev)
                {
                    this.ctrlMyMods.Visibility = Visibility.Visible;
                }

                this.ctrlCatalog.Visibility = Visibility.Visible;
            }
            else
            {
                if (this.is_dev)
                {
                    this.ctrlMyMods.Visibility = Visibility.Hidden;
                }
                this.ctrlCatalog.Visibility = Visibility.Hidden;
            }
        }
    }
}
