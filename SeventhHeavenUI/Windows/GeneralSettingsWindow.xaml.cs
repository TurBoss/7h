using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using System.Windows;
using System.Windows.Forms;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for GeneralSettingsWindow.xaml
    /// </summary>
    public partial class GeneralSettingsWindow : Window
    {
        public GeneralSettingsViewModel ViewModel { get; set; }

        public GeneralSettingsWindow()
        {
            InitializeComponent();

            ViewModel = new GeneralSettingsViewModel();
            this.DataContext = ViewModel;

            ViewModel.LoadSettings();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            bool settingsSaved = ViewModel.SaveSettings();

            if (settingsSaved)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnFf7Exe_Click(object sender, RoutedEventArgs e)
        {
            string exePath = FileDialogHelper.BrowseForFile("exe file (*.exe)|*.exe", "Select FF7.exe");

            if (!string.IsNullOrEmpty(exePath))
            {
                ViewModel.FF7ExePathInput = exePath;
            }
        }

        private void btnMovies_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FileDialogHelper.BrowseForFolder();

            if (!string.IsNullOrEmpty(folderPath))
            {
                ViewModel.MoviesPathInput = folderPath;
            }
        }

        private void btnTextures_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FileDialogHelper.BrowseForFolder();

            if (!string.IsNullOrEmpty(folderPath))
            {
                ViewModel.TexturesPathInput = folderPath;
            }
        }

        private void btnLibrary_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FileDialogHelper.BrowseForFolder();

            if (!string.IsNullOrEmpty(folderPath))
            {
                ViewModel.LibraryPathInput = folderPath;
            }
        }
    }
}
