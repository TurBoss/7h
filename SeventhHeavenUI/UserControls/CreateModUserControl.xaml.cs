using SeventhHeaven.Classes;
using SeventhHeaven.Windows;
using SeventhHeavenUI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SeventhHeaven.UserControls
{
    /// <summary>
    /// Interaction logic for CreateModUserControl.xaml
    /// </summary>
    public partial class CreateModUserControl : UserControl
    {
        ModCreationViewModel ViewModel { get; set; }
        public CreateModUserControl()
        {
            InitializeComponent();

            ViewModel = new ModCreationViewModel();
            this.DataContext = ViewModel;
        }

        private void btnBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            string pathToFile = FileDialogHelper.BrowseForFile("*.png,*.jpg,*.gif,*.jpeg|*.*", "Select image to use");


            if (!string.IsNullOrEmpty(pathToFile))
            {
                ViewModel.PreviewImageInput = System.IO.Path.GetFileName(pathToFile);
                MessageDialogWindow.Show($"Make sure to copy {ViewModel.PreviewImageInput} to the root folder of your mod so the preview image loads correctly.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GenerateModOutput();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string pathToFile = FileDialogHelper.OpenSaveDialog("mod .xml|*.xml", "Save mod xml");

            if (!string.IsNullOrEmpty(pathToFile))
            {
                ViewModel.SaveModXml(pathToFile);
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            string pathToFile = FileDialogHelper.BrowseForFile("mod .xml|*.xml", "Select mod.xml to Load");
        
            if (!string.IsNullOrEmpty(pathToFile))
            {
                ViewModel.LoadModXml(pathToFile);
            }
        }
    }
}
