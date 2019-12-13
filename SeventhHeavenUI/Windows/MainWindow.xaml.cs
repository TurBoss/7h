using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeaven.Windows;
using SeventhHeavenUI.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace SeventhHeavenUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal const string _warningMessage = "Are you sure you want to play FF7 in this mode? Playing FF7 this way can cause extreme slowness and use a lot of disk space.\n\nYou should only use this option while troubleshooting a single active mod so that you can troubleshoot and provide bug reports to the mod author and/or to the 7H developers.";

        internal MainWindowViewModel ViewModel { get; set; }

        private int _currentTabIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();
            this.DataContext = ViewModel;

            ctrlMyMods.SetDataContext(ViewModel.ModsViewModel);
            ctrlCatalog.SetDataContext(ViewModel.CatalogViewModel);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.InitViewModel();

            _currentTabIndex = tabCtrlMain.SelectedIndex;

            if (Sys.Settings.MainWindow != null)
            {
                var loc = new System.Drawing.Point((int)Sys.Settings.MainWindow.X, (int)Sys.Settings.MainWindow.Y);

                if (Screen.AllScreens.Any(s => s.Bounds.Contains(loc)))
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    System.Windows.Application.Current.MainWindow.Left = Sys.Settings.MainWindow.X;
                    System.Windows.Application.Current.MainWindow.Top = Sys.Settings.MainWindow.Y;
                }

                Width = Sys.Settings.MainWindow.W;
                Height = Sys.Settings.MainWindow.H;
                WindowState = Sys.Settings.MainWindow.State;
            }

            ProcessCommandLineArgs();

            ctrlMyMods.RecalculateColumnWidths();
        }

        private void ProcessCommandLineArgs()
        {
            foreach (string parm in Environment.GetCommandLineArgs())
            {
                if (parm.StartsWith("iros://", StringComparison.InvariantCultureIgnoreCase))
                {
                    //TODO: ProcessIrosLink(parm);
                }
                else if (parm.StartsWith("/OPENIRO:", StringComparison.InvariantCultureIgnoreCase))
                {
                    string irofile = parm.Substring(9);
                    string irofilenoext = System.IO.Path.GetFileNameWithoutExtension(irofile);
                    Log.Write("Importing IRO from Windows " + irofile);
                    try
                    {
                        //TODO: fImportMod.ImportMod(irofile, irofilenoext, true, false);
                    }
                    catch (Exception ex)
                    {
                        Sys.Message(new WMessage() { Text = "Mod " + irofilenoext + " failed to import: " + ex.ToString() });
                        continue;
                    }
                    Sys.Message(new WMessage() { Text = "Auto imported mod " + irofilenoext });
                    System.Windows.MessageBox.Show("The mod " + irofilenoext + " has been added to your Library.", "Import Mod from Windows");
                    //TODO: Add an IRO "Unpack" option here
                }
                else if (parm.StartsWith("/PROFILE:", StringComparison.InvariantCultureIgnoreCase))
                {
                    Sys.Settings.CurrentProfile = parm.Substring(9);
                    Sys.ActiveProfile = Util.Deserialize<Profile>(Sys.PathToCurrentProfileFile);
                    ViewModel.RefreshProfile();
                }
                else if (parm.Equals("/LAUNCH", StringComparison.InvariantCultureIgnoreCase))
                {
                    //TODO: bLaunch.PerformClick();
                }
                else if (parm.Equals("/QUIT", StringComparison.InvariantCultureIgnoreCase))
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Sys.Settings.MainWindow = new SavedWindow()
            {
                X = (int)System.Windows.Application.Current.MainWindow.Left,
                Y = (int)System.Windows.Application.Current.MainWindow.Top,
                W = (int)ActualWidth,
                H = (int)ActualHeight,
                State = WindowState
            };

            ViewModel.CleanUp();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LaunchGame(varDump: false, debug: false);
        }

        private void txtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.DoSearch();
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!menuSettings.IsOpen)
            {
                menuSettings.PlacementTarget = btnSettings;
                menuSettings.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menuSettings.IsOpen = true;
                btnSettings.IsEnabled = false;
            }
        }

        private void btnTools_Click(object sender, RoutedEventArgs e)
        {
            if (!menuTools.IsOpen)
            {
                menuTools.PlacementTarget = btnTools;
                menuTools.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menuTools.IsOpen = true;
                btnTools.IsEnabled = false;
            }
        }

        private void menuTools_Closed(object sender, RoutedEventArgs e)
        {
            btnTools.IsEnabled = true;
        }

        private void menuSettings_Closed(object sender, RoutedEventArgs e)
        {
            btnSettings.IsEnabled = true;
        }

        private void btnPlayOptions_Click(object sender, RoutedEventArgs e)
        {
            if (!menuPlayOptions.IsOpen)
            {
                menuPlayOptions.PlacementTarget = btnPlayOptions;
                menuPlayOptions.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menuPlayOptions.IsOpen = true;
                btnPlayOptions.IsEnabled = false;
            }
        }

        private void menuPlayVariableDump_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show(_warningMessage, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                ViewModel.LaunchGame(varDump: true, debug: false);
            }
        }

        private void menuPlayDebugLog_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show(_warningMessage, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                ViewModel.LaunchGame(varDump: false, debug: true);
            }
        }

        private void menuPlayOptions_Closed(object sender, RoutedEventArgs e)
        {
            btnPlayOptions.IsEnabled = true;
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LaunchHelpPage();
        }

        private void btnOpenModLink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenPreviewModLink();
        }

        private void btnOpenModReadme_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenPreviewModReadMe();
        }

        private void menuItemGeneralSettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowGeneralSettingsWindow();
        }

        private void menuItemNewProfile_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateNewProfile();
        }

        private void menuItemProfileDetails_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuItemOpenProfile_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowOpenProfileWindow();
        }

        private void menuItemChunkTool_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowChunkToolWindow();
        }

        private void btnFilters_Click(object sender, RoutedEventArgs e)
        {
            if (!popupFilters.IsOpen)
            {
                btnFilters.IsEnabled = false; // disable button while it is opened
                popupFilters.IsOpen = true;
            }
        }

        private void popupFilters_Closed(object sender, EventArgs e)
        {
            btnFilters.IsEnabled = true;
            ViewModel.ApplyFiltersAndReloadList();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ctrlMyMods.RecalculateColumnWidths();
            ctrlCatalog.RecalculateColumnWidths();
        }

        private void menuItemIroCreation_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowIroToolsWindow();
        }

        private void menuItemThemeSettings_Click(object sender, RoutedEventArgs e)
        {
            var window = new ThemeSettingsWindow() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            window.ShowDialog();
        }

        private void tabCtrlMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int tabSelectedIndex = tabCtrlMain.SelectedIndex;

            if (tabCtrlMain.SelectedIndex == 0 && _currentTabIndex != tabSelectedIndex)
            {
                _currentTabIndex = tabSelectedIndex;
                ctrlMyMods.RecalculateColumnWidths(ctrlCatalog.lstCatalogMods.ActualWidth);
            }
            else if (_currentTabIndex != tabSelectedIndex)
            {
                _currentTabIndex = tabSelectedIndex;
                ctrlCatalog.RecalculateColumnWidths(ctrlMyMods.lstMods.ActualWidth);
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadingGifVisibility = Visibility.Hidden;
        }

        private void menuItemAaliSettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowGLConfigWindow();
        }

        private void menuPlayWithMods_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LaunchGame(varDump: false, debug: false);
        }

        private void menuPlayWithoutMods_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.LaunchFF7Exe();
        }
    }
}
