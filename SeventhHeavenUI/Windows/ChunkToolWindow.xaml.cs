using SeventhHeaven.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for ChunkToolWindow.xaml
    /// </summary>
    public partial class ChunkToolWindow : Window
    {
        ChunkToolViewModel ViewModel { get; set; }

        public ChunkToolWindow()
        {
            InitializeComponent();

            ViewModel = new ChunkToolViewModel();
            this.DataContext = ViewModel;
        }

        private void btnBrowseFlevel_Click(object sender, RoutedEventArgs e)
        {
            string pathToFile = BrowseForFlevelFile();

            if (!string.IsNullOrEmpty(pathToFile))
            {
                ViewModel.PathToFlevelFile = pathToFile;
            }
        }

        private void btnBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            string pathToFolder = BrowseForFolder();

            if (!string.IsNullOrEmpty(pathToFolder))
            {
                ViewModel.PathToOutputFolder = pathToFolder;
            }
        }

        private void btnExtract_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.BeginExtract();
        }

        private string BrowseForFlevelFile()
        {
            using (OpenFileDialog fileBrowserDialog = new OpenFileDialog())
            {
                fileBrowserDialog.Filter = "FLevel.lgp|FLevel.lgp";
                fileBrowserDialog.Title = "Select FLevel.lgp file";
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
