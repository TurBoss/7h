using SeventhHeavenUI.ViewModels;
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

        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GenerateModOutput();
        }
    }
}
