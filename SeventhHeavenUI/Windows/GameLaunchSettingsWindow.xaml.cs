using SeventhHeaven.ViewModels;
using System.Windows;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for GameLaunchSettingsWindow.xaml
    /// </summary>
    public partial class GameLaunchSettingsWindow : Window
    {
        GameLaunchSettingsViewModel ViewModel { get; set; }

        public GameLaunchSettingsWindow()
        {
            InitializeComponent();

            ViewModel = new GameLaunchSettingsViewModel();
            this.DataContext = ViewModel;
        }

        private void btnTestAudio_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestAudio(AudioChannel.Center);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SaveSettings())
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Control_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as FrameworkElement) != null)
            {
                ViewModel.StatusMessage = (sender as FrameworkElement).ToolTip as string;
            }
        }

        private void menuItemTestLeft_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestAudio(AudioChannel.Left);
        }

        private void menuItemTestRight_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TestAudio(AudioChannel.Right);
        }

        private void menuAudioTest_Closed(object sender, RoutedEventArgs e)
        {
            btnAudioOptions.IsEnabled = true; // ensure button is re-enabled after context menu closes
        }

        private void btnAudioOptions_Click(object sender, RoutedEventArgs e)
        {
            if (!menuAudioTest.IsOpen)
            {
                menuAudioTest.PlacementTarget = btnAudioOptions;
                menuAudioTest.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menuAudioTest.IsOpen = true;
                btnAudioOptions.IsEnabled = false; // disable the button while the menu is open until context menu is closed
            }
        }
    }
}
