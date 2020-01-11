using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using SeventhHeavenUI;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for GameLaunchWindow.xaml
    /// </summary>
    public partial class GameLaunchWindow : Window
    {
        public GameLaunchViewModel ViewModel { get; set; }


        public GameLaunchWindow(bool variableDump, bool debugLogging)
        {
            InitializeComponent();

            ViewModel = new GameLaunchViewModel(variableDump, debugLogging);
            this.DataContext = ViewModel;

            btnOk.Visibility = Visibility.Collapsed;
        }

        public static GameLaunchViewModel Show(bool variableDump, bool debugLogging, bool noMods = false)
        {
            GameLaunchWindow launchWindow = new GameLaunchWindow(variableDump, debugLogging);
            launchWindow.ViewModel.IsLaunchingWithNoMods = noMods;


            launchWindow.ShowDialog();
            return launchWindow.ViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LogStatus("### Launching Final Fantasy VII ###");

            // starts the game launch process asynchronously when window loads
            Task<bool> task = ViewModel.BeginLaunchProcessAsync();

            // setup async task to closing window after completion or keeping window open after error
            task.ContinueWith((taskResult) =>
            {
                if (taskResult.IsFaulted)
                {
                    ViewModel.LogStatus($"Unknown error when launching the game: {taskResult.Exception.GetBaseException().Message}");
                    ShowOkButton();
                    return;
                }

                if (!taskResult.Result)
                {
                    ViewModel.LogStatus("Failed to launch FF7. View the above log for details.");
                    ShowOkButton();
                }
                else
                {
                    Thread.Sleep(4000); // wait a few seconds before closing window on success
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                }
            });
        }

        private void ShowOkButton()
        {
            btnOk.Visibility = Visibility.Visible;
            progBar.Visibility = Visibility.Collapsed;
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
        }
    }
}
