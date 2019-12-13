using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using System.IO;
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
            string initialDir = "";

            if (File.Exists(ViewModel.FF7ExePathInput))
            {
                initialDir = Path.GetDirectoryName(ViewModel.FF7ExePathInput);
            }

            string exePath = FileDialogHelper.BrowseForFile("exe file (*.exe)|*.exe", "Select FF7.exe", initialDir);

            if (!string.IsNullOrEmpty(exePath))
            {
                ViewModel.FF7ExePathInput = exePath;
            }
        }

        private void btnMovies_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FileDialogHelper.BrowseForFolder("Select Movies Folder", ViewModel.MoviesPathInput);

            if (!string.IsNullOrEmpty(folderPath))
            {
                ViewModel.MoviesPathInput = folderPath;
            }
        }

        private void btnTextures_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FileDialogHelper.BrowseForFolder("Select Textures Folder", ViewModel.TexturesPathInput);

            if (!string.IsNullOrEmpty(folderPath))
            {
                ViewModel.TexturesPathInput = folderPath;
            }
        }

        private void btnLibrary_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FileDialogHelper.BrowseForFolder("Select 7th Heaven Library Folder", ViewModel.LibraryPathInput);

            if (!string.IsNullOrEmpty(folderPath))
            {
                ViewModel.LibraryPathInput = folderPath;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // reference: https://stackoverflow.com/questions/11232438/single-line-wpf-textbox-horizontal-scroll-to-end
            txtMovies.CaretIndex = ViewModel.MoviesPathInput.Length;
            var rect = txtMovies.GetRectFromCharacterIndex(txtMovies.CaretIndex);
            txtMovies.ScrollToHorizontalOffset(rect.Right);

            txtTextures.CaretIndex = ViewModel.TexturesPathInput.Length;
            rect = txtTextures.GetRectFromCharacterIndex(txtTextures.CaretIndex);
            txtTextures.ScrollToHorizontalOffset(rect.Right);

            txtLibrary.CaretIndex = ViewModel.LibraryPathInput.Length;
            rect = txtLibrary.GetRectFromCharacterIndex(txtLibrary.CaretIndex);
            txtLibrary.ScrollToHorizontalOffset(rect.Right);

            txtFf7Exe.CaretIndex = ViewModel.FF7ExePathInput.Length;
            rect = txtFf7Exe.GetRectFromCharacterIndex(txtFf7Exe.CaretIndex);
            txtFf7Exe.ScrollToHorizontalOffset(rect.Right);
        }
    }
}
