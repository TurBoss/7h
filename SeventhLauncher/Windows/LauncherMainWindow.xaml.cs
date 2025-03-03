using _7thHeaven.Code;
using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using SeventhHeaven.Windows;
using SeventhHeavenUI.ViewModels;
using SeventhLauncher.ViewModels;
using SeventhLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using NLog;
using System.IO;
using System.Diagnostics;

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
        internal LauncherMainWindowViewModel LauncherMainWindowVM { get; set; }
        internal DownloadItemViewModel DownloadVM { get; set; }
        internal MyModsViewModel ModsVM { get; set; }
        public CatalogViewModel CatalogVM { get; set; }

        public bool show_content { get; private set; }
        public bool show_options { get; private set; }
        public bool is_dev { get; private set; }
        public bool enable_var_dump { get; private set; }
        public bool enable_debug_log { get; private set; }
        public string download_msg { get; private set; }

        public MainWindow()
        {

            InitializeComponent();

            SettingsVM = new GeneralSettingsViewModel();
            LauncherMainWindowVM = new LauncherMainWindowViewModel();
            // ModsVM = new MyModsViewModel();
            // CatalogVM = new CatalogViewModel();

            this.DataContext = LauncherMainWindowVM;

            this.ctrlMyMods.SetDataContext(LauncherMainWindowVM.MyMods);
            this.ctrlCatalog.SetDataContext(LauncherMainWindowVM.CatalogMods);



            this.show_content = false;
            this.show_options = false;
            //this.is_dev = false;
            this.is_dev = true;

            this.enable_debug_log = false;
            this.enable_var_dump = false;

            this.download_msg = "";


            this.main_tab.Visibility = Visibility.Hidden;
            this.mods_tab.Visibility = Visibility.Hidden;
            this.catalog_tab.Visibility = Visibility.Hidden;
            this.options_tab.Visibility = Visibility.Hidden;

            this.dev_install_button.Visibility = Visibility.Hidden;
            this.develop_button.Visibility = Visibility.Hidden;
            this.variable_dump_checkbox.Visibility = Visibility.Hidden;
            this.debug_log_checkbox.Visibility = Visibility.Hidden;

            if (this.is_dev == true)
            {
                this.dev_install_button.Visibility = Visibility.Visible;
                this.develop_button.Visibility = Visibility.Visible;
//                this.variable_dump_checkbox.Visibility = Visibility.Visible;
//                this.debug_log_checkbox.Visibility = Visibility.Visible;
            }

            /*
            this.profile_button.Visibility = Visibility.Hidden;
            this.launcher_button.Visibility = Visibility.Hidden;
            this.ffnx_button.Visibility = Visibility.Hidden;
            this.controller_button.Visibility = Visibility.Hidden;
            this.system_button.Visibility = Visibility.Hidden;
            */

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

                   LauncherMessageDialogWindow.Show(string.Format(ResourceHelper.Get(StringKey.ThisExeIsUsedForSteamReleaseCopiedSelectedForYou), copyOrSelectMessage),
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
                        
            LauncherMainWindowVM.InitViewModel();

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

                if (    this.is_dev == false)
                {
                    LauncherMainWindowVM.MyMods.AutoSortBasedOnCategory();
                    LauncherMainWindowVM.MyMods.ActivateAllMods();
                }


                int total_mods = Sys.Catalog.Mods.Count();
                int downloading_mods = LauncherMainWindowVM.CatalogMods.DownloadList.Count;
                int installed_mods = Sys.ActiveProfile.Items.Count();

                if (this.isOkToPlay())
                {
                    this.play_button.Content = "Play";
                    this.play_button.IsEnabled = true;
                    this.download_bar.Visibility = Visibility.Hidden;
                    this.download_text.Visibility = Visibility.Hidden;
                }
                
                else if (downloading_mods > 0)
                {
                    this.play_button.Content = "Downloading";
                    this.play_button.IsEnabled = false;
                    this.download_bar.Visibility = Visibility.Visible;
                    this.download_text.Visibility = Visibility.Visible;
                }
                else
                {
                    this.play_button.Content = "Install";
                    this.play_button.IsEnabled = true;
                    this.download_bar.Visibility = Visibility.Hidden;
                    this.download_text.Visibility = Visibility.Hidden;
                }

                DownloadItemViewModel viewModel = null;
                viewModel = LauncherMainWindowVM.CatalogMods.DownloadList.FirstOrDefault();
                

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

                //Console.WriteLine("LOOP");
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

            // LauncherMainWindowVM.MyMods.AutoSortBasedOnCategory();
            LauncherMainWindowVM.MyMods.ScanForModUpdates();
        }

        public bool isOkToPlay()
        {

            Debug.WriteLine("CHEK GAME");

            bool install_ok = true;

            if (is_dev)
            {
                return install_ok;
            }

            Catalog catalog = Sys.Catalog;
            Profile profile = Sys.ActiveProfile;

            // check catalog mods vs profile mods
            foreach (Mod mod in catalog.Mods)
            {
                ProfileItem v = profile.GetItem(mod.ID);
                //Console.WriteLine(v);

                if (v == null)
                {
                    install_ok = false;
                    break;
                }
            }

            // check if there are more mods than the mod on the catalog
            foreach (ProfileItem profile_mod in profile.Items)
            {
                Mod catalog_mod = catalog.GetMod(profile_mod.ModID);
                if (catalog_mod != null)
                {
                    if (this.is_dev == false)
                    {
                        profile_mod.IsModActive = true;
                    }
                }
                else
                {
                    if (this.is_dev == true)
                    { 
                        Debug.Write("DEVELOPER MODE ENABLED:Mod extra found: ");
                        Debug.WriteLine(profile_mod.Name);
                        install_ok = true;
                    }
                    else
                    {
                        if (profile_mod.IsModActive == true)
                        {
                            Debug.Write("Desactivando mod externo ");
                            Debug.WriteLine(profile_mod.Name);

                            LauncherMainWindowVM.MyMods.ToggleActivateMod(profile_mod.ModID, true);

                            // profile_mod.IsModActive = false;

                        }
                    }

                }
            }

            if (this.is_dev == false)
            {
                LauncherMainWindowVM.MyMods.ReloadModList();
            }

            return install_ok;
        }


        private void options_button_Click(object sender, RoutedEventArgs e)
        {

            this.main_control.SelectedItem = this.options_tab;

            /*

            this.show_options = !this.show_options;


            if (this.show_options)
            {

                this.profile_button.Visibility = Visibility.Visible;
                this.launcher_button.Visibility = Visibility.Visible;
                this.ffnx_button.Visibility = Visibility.Visible;
                this.controller_button.Visibility = Visibility.Visible;
                this.system_button.Visibility = Visibility.Visible;
                 
                if (this.is_dev == true)
                {
                    this.develop_button.Visibility = Visibility.Visible;
                }
            }
            else
            {
                this.profile_button.Visibility = Visibility.Hidden;
                this.launcher_button.Visibility = Visibility.Hidden;
                this.ffnx_button.Visibility = Visibility.Hidden;
                this.controller_button.Visibility = Visibility.Hidden;
                this.system_button.Visibility = Visibility.Hidden;

                if (this.is_dev == true)
                {
                    this.develop_button.Visibility = Visibility.Hidden;
                    this.develop_button.IsChecked = false;
                }

                this.show_content = this.show_options;
            }


            */


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
                LauncherMainWindowVM.LaunchGame(variableDump: enable_var_dump, debugLogging: enable_var_dump);

                 // if (!this.is_dev) quit_app();

            }
        }
        private void dev_install_button_Click(object sender, RoutedEventArgs e)
        {
            if (is_dev) {
                GetMissingContent();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {


            if (GameLauncher.IsFF7Running())
            {
                e.Cancel = true;
                return;
            }

            if (Sys.IsImporting)
            {
                MessageDialogWindow.Show(ResourceHelper.Get(StringKey.CannotExitWhileImporting), ResourceHelper.Get(StringKey.Warning), MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Cancel = true;
                return;
            }

            if (LauncherMainWindowVM.CatalogMods.DownloadList.Count > 0)
            {
                var result = MessageDialogWindow.Show(ResourceHelper.Get(StringKey.AreYouSureYouWantToExitPendingDownloads), ResourceHelper.Get(StringKey.ConfirmExit), MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result.Result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            Sys.Settings.MainWindow = new SavedWindow()
            {
                X = (int)System.Windows.Application.Current.MainWindow.Left,
                Y = (int)System.Windows.Application.Current.MainWindow.Top,
                W = (int)ActualWidth,
                H = (int)ActualHeight,
                State = WindowState
            };

            Sys.Settings.UserColumnSettings.MyModsColumns = ctrlMyMods.GetColumnSettings();
            ctrlCatalog.SaveUsersColumnSettings();

            LauncherMainWindowVM.CleanUp();


        }

        private void quit_app()
        {

            // Sys.Settings.MainWindow = new SavedWindow()
            //{
            //     X = (int)System.Windows.Application.Current.MainWindow.Left,
            //     Y = (int)System.Windows.Application.Current.MainWindow.Top,
            //     W = (int)ActualWidth,
            //     H = (int)ActualHeight,
            //     State = WindowState
            // };

            //  this.ctrlCatalog.SaveUsersColumnSettings();

            Sys.Save();
            Environment.Exit(0);

        }

        private void quit_button_Click(object sender, RoutedEventArgs e)
        {
            quit_app();
        }

        private void convert_button_Click(object sender, RoutedEventArgs e)
        {
            // Console.WriteLine("HOLA?");
            // this.Convert98();
        }
        private void profile_button_Click(object sender, RoutedEventArgs e)
        {

            LauncherMainWindowVM.ShowProfilesWindow();
        }

        private void controller_button_Click(object sender, RoutedEventArgs e)
        {
            new ControlMappingWindow().ShowDialog();
        }

        private void system_button_Click(object sender, RoutedEventArgs e)
        {
            LauncherGeneralSettingsWindow settingsWindow = new LauncherGeneralSettingsWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            bool? didSave = settingsWindow.ShowDialog();

            //if (didSave.GetValueOrDefault(false))
            //{
            //    if (settingsWindow.ViewModel.SubscriptionsChanged)
            //    {
            //        CatalogMods.ForceCheckCatalogUpdateAsync();
            //    }
//
            //    if (settingsWindow.ViewModel.HasChangedInstalledModUpdateTypes && SelectedTabIndex == (int)TabIndex.MyMods)
            //    {
            //        MyMods.ScanForModUpdates();
            //        UpdateModPreviewInfo(MyMods.GetSelectedMod(), true);
            //    }
            //}
        }

       

        private void launcher_button_Click(object sender, RoutedEventArgs e)
        {
            LauncherMainWindowVM.ShowGameLaunchSettingsWindow();
        }


        private void develop_button_Checked(object sender, RoutedEventArgs e)
        {
            // Debug.WriteLine("CHECK");
            this.main_tab.Visibility = Visibility.Visible;
            this.mods_tab.Visibility = Visibility.Visible;
            this.catalog_tab.Visibility = Visibility.Visible;
            this.options_tab.Visibility = Visibility.Visible;

            // this.develop_button.Visibility = Visibility.Visible;
            this.variable_dump_checkbox.Visibility = Visibility.Visible;
            this.debug_log_checkbox.Visibility = Visibility.Visible;
        }
        private void develop_button_Unchecked(object sender, RoutedEventArgs e)
        {
            // Debug.WriteLine("UN CHECK");
            this.main_tab.Visibility = Visibility.Hidden;
            this.mods_tab.Visibility = Visibility.Hidden;
            this.catalog_tab.Visibility = Visibility.Hidden;
            this.options_tab.Visibility = Visibility.Hidden;

            // this.develop_button.Visibility = Visibility.Hidden;
            this.variable_dump_checkbox.Visibility = Visibility.Hidden;
            this.debug_log_checkbox.Visibility = Visibility.Hidden;

        }

        private void back_button_Click(object sender, RoutedEventArgs e)
        {
            this.main_control.SelectedItem = this.main_tab;
        }

        private void variable_dump_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            enable_var_dump = true;
        }

        private void debug_log_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            enable_debug_log = true;
        }

        private void variable_dump_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            enable_var_dump = false;
        }

        private void debug_log_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            enable_debug_log = false;
        }

    }
}
