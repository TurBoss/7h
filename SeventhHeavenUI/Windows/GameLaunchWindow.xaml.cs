using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using SeventhHeavenUI;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for GameLaunchWindow.xaml
    /// </summary>
    public partial class GameLaunchWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public GameLaunchViewModel ViewModel { get; set; }


        public GameLaunchWindow(bool variableDump, bool debugLogging)
        {
            InitializeComponent();

            ViewModel = new GameLaunchViewModel(variableDump, debugLogging);
            this.DataContext = ViewModel;

            btnOk.Visibility = Visibility.Collapsed;
            txtLog.MaxLines = int.MaxValue;
        }

        public static GameLaunchViewModel Show(bool variableDump, bool debugLogging, bool noMods = false)
        {
            GameLaunchWindow launchWindow = new GameLaunchWindow(variableDump, debugLogging);
            launchWindow.ViewModel.IsLaunchingWithNoMods = noMods;

            if (!Sys.Settings.GameLaunchSettings.ShowLauncherWindow)
            {
                launchWindow.WindowState = WindowState.Minimized;
                launchWindow.Visibility = Visibility.Hidden;
                launchWindow.ShowInTaskbar = false;
                
                GameLauncher.Instance.ProgressChanged += LauncherInstance_ProgressChanged;
                launchWindow.Show();
            }
            else 
            { 
                launchWindow.ShowDialog();
            }

            return launchWindow.ViewModel;
        }

        private static void LauncherInstance_ProgressChanged(string message)
        {
            Sys.Message(new WMessage(message, WMessageLogLevel.StatusOnly));
        }

        /// <summary>
        /// Begin the game launch process once Window is loaded
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LogStatus("### Launching Final Fantasy VII ###");
            App.ForceUpdateUI();

            // Wait 1 second before starting actual launch process so the window is fully loaded
            var sleepTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
            });

            sleepTask.ContinueWith((result) =>
            {
                StartGameLaunchProcessAsync();
            });
        }

        /// <summary>
        /// starts the game launcher process asynchronously
        /// </summary>
        private Task<bool> StartGameLaunchProcessAsync()
        {
            Task<bool> task = ViewModel.BeginLaunchProcessAsync();

            // setup async task to closing window after completion or keeping window open after error
            task.ContinueWith((taskResult) =>
            {
                if (taskResult.IsFaulted)
                {
                    Logger.Error(taskResult.Exception);
                    ViewModel.LogStatus($"Unknown error when launching the game: {taskResult.Exception.GetBaseException().Message}");
                    ShowOkButton();
                    return;
                }

                if (taskResult.IsCanceled)
                {
                    ViewModel.LogStatus("game launch process canceled");
                    return;
                }

                if (!taskResult.Result)
                {
                    ViewModel.LogStatus("Failed to launch FF7. View the above log for details.");
                    ShowOkButton();
                }
                else
                {
                    Sys.Message(new WMessage("Successfully launched FF7!"));
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                }
            });

            return task;
        }

        private void ShowOkButton()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                btnOk.Visibility = Visibility.Visible;
                progBar.Visibility = Visibility.Collapsed;

                if (Sys.Settings.GameLaunchSettings.ShowLauncherWindow == false)
                {
                    // ensure window is closed if user has setting not to show launcher window
                    this.Close();
                }
            });
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Scrolls the textbox to the end when text changes
        /// </summary>
        private void txtLog_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            txtLog.ScrollToEnd();
            App.ForceUpdateUI();
        }
    }
}
