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
using System.Windows.Shapes;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for OpenProfileWindow.xaml
    /// </summary>
    public partial class OpenProfileWindow : Window
    {
        public OpenProfileViewModel ViewModel { get; set; }

        public OpenProfileWindow()
        {
            InitializeComponent();

            ViewModel = new OpenProfileViewModel();
            this.DataContext = ViewModel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void menuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!IsProfileSelected())
            {
                return;
            }

            string selected = (string)lstProfiles.SelectedItem;

            if (MessageBox.Show($"Are you sure you want to delete the selected profile ({selected})?", "Delete Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                ViewModel.DeleteProfile(selected);
            }
        }

        private void menuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!IsProfileSelected())
            {
                return;
            }

            ViewModel.CopyProfile((string)lstProfiles.SelectedItem);
        }

        private void lstProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedProfile = (string)lstProfiles.SelectedItem;
        }

        private bool IsProfileSelected()
        {
            if (lstProfiles.SelectedItem == null)
            {
                MessageBox.Show("Select a Profile first.", "No Profile Selected", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }
    }
}
