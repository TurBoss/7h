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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeventhHeaven.UserControls
{
    /// <summary>
    /// Interaction logic for PackIroUserControl.xaml
    /// </summary>
    public partial class PatchIroUserControl : UserControl
    {
        PatchIroViewModel ViewModel { get; set; }

        public PatchIroUserControl()
        {
            InitializeComponent();

            ViewModel = new PatchIroViewModel();
            this.DataContext = ViewModel;
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Validate())
            {
                ViewModel.PatchIro();
            }
        }

        private void btnBrowseNewIro_Click(object sender, RoutedEventArgs e)
        {
            string saveFile = FileDialogHelper.BrowseForFile("IRO files|*.iro", "Select .iro");

            if (!string.IsNullOrEmpty(saveFile))
            {
                ViewModel.PathToNewIroFile = saveFile;
            }
        }

        private void btnBrowseOriginalIro_Click(object sender, RoutedEventArgs e)
        {
            string sourceFolder = FileDialogHelper.BrowseForFile("IRO files|*.iro", "Select .iro");

            if (!string.IsNullOrEmpty(sourceFolder))
            {
                ViewModel.PathToOriginalIroFile = sourceFolder;
            }
        }

        private void btnBrowseIrop_Click(object sender, RoutedEventArgs e)
        {
            string saveFile = FileDialogHelper.OpenSaveDialog("IRO patches|*.irop", "Save As .irop");

            if (!string.IsNullOrEmpty(saveFile))
            {
                ViewModel.PathToIropFile = saveFile;
            }
        }
    }
}
