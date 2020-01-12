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

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveSettings();
            this.Close();
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
    }
}
