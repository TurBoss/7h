using SeventhHeaven.Classes;
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
            string pathToFile = FileDialogHelper.BrowseForFile("FLevel.lgp|FLevel.lgp", "Select FLevel.lgp file");

            if (!string.IsNullOrEmpty(pathToFile))
            {
                ViewModel.PathToFlevelFile = pathToFile;
            }
        }

        private void btnBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            string pathToFolder = FileDialogHelper.BrowseForFolder();

            if (!string.IsNullOrEmpty(pathToFolder))
            {
                ViewModel.PathToOutputFolder = pathToFolder;
            }
        }

        private void btnExtract_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.BeginExtract();
        }

    }
}
