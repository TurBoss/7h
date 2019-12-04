using SeventhHeaven.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for ImportModWindow.xaml
    /// </summary>
    public partial class ImportModWindow : Window
    {
        ImportModViewModel ViewModel { get; set; }

        public ImportModWindow()
        {
            InitializeComponent();

            ViewModel = new ImportModViewModel();
            this.DataContext = ViewModel;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ImportModFromWindow();
        }

        private void btnBrowseIro_Click(object sender, RoutedEventArgs e)
        {
            string selectedFile = BrowseForIroFile();

            if (!string.IsNullOrEmpty(selectedFile))
            {
                ViewModel.PathToIroArchiveInput = selectedFile;
                ViewModel.ModNameInput = Path.GetFileNameWithoutExtension(selectedFile);
            }
        }

        private void btnBrowseModFolder_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = BrowseForFolder();

            if (!string.IsNullOrEmpty(selectedFolder))
            {
                ViewModel.PathToModFolderInput = selectedFolder;
                ViewModel.ModNameInput = Path.GetFileName(selectedFolder);
            }
        }

        private void btnBrowseBatchFolder_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = BrowseForFolder();

            if (!string.IsNullOrEmpty(selectedFolder))
            {
                ViewModel.PathToBatchFolderInput = selectedFolder;
            }
        }

        private string BrowseForIroFile()
        {
            using (OpenFileDialog fileBrowserDialog = new OpenFileDialog())
            {
                fileBrowserDialog.Filter = "iro archive (*.iro)|*.iro";
                fileBrowserDialog.Title = "Select .iro Archive File";

                DialogResult result = fileBrowserDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    return fileBrowserDialog.FileName;
                }
            }

            return "";
        }

        private string BrowseForFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    return folderBrowserDialog.SelectedPath;
                }
            }

            return "";
        }
    }
}
