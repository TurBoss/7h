using SeventhHeavenUI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SeventhHeaven.UserControls
{
    /// <summary>
    /// Interaction logic for MyModsUserControl.xaml
    /// </summary>
    public partial class MyModsUserControl : UserControl
    {
        public MyModsViewModel ViewModel { get; set; }

        public MyModsUserControl()
        {
            InitializeComponent();
        }

        public void SetDataContext(MyModsViewModel viewModel)
        {
            ViewModel = viewModel;
            this.DataContext = ViewModel;
        }

        private void lstMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.RaiseSelectedModChanged(sender, (lstMods.SelectedItem as InstalledModViewModel));
        }

        private void btnRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.ReloadModList();
        }

        private void btnDeactivateAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.ToggleDeactivationForAllMods();
        }

        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
            {
                return;
            }

            ViewModel.UninstallMod((lstMods.SelectedItem as InstalledModViewModel));
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
            {
                return;
            }

            if (!IsActiveModSelected())
            {
                return;
            }

            ViewModel.ReorderProfileItem((lstMods.SelectedItem as InstalledModViewModel), -1);
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
            {
                return;
            }

            if (!IsActiveModSelected())
            {
                return;
            }

            ViewModel.ReorderProfileItem((lstMods.SelectedItem as InstalledModViewModel), 1);
        }

        private bool IsModSelected()
        {
            if (lstMods.SelectedItem == null)
            {
                MessageBox.Show("Select a mod first.", "No Mod Selected", MessageBoxButton.OK);
                return false;
            }

            return true;
        }

        private bool IsActiveModSelected()
        {
            if (lstMods.SelectedItem == null)
            {
                return false;
            }

            if (!(lstMods.SelectedItem as InstalledModViewModel).IsActive)
            {
                MessageBox.Show("Mod is not active. Only activated mods can be re-ordered.", "Cannot Move Inactive Mod", MessageBoxButton.OK);
                return false;
            }

            return true;
        }

        private void btnMoveTop_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
            {
                return;
            }

            if (!IsActiveModSelected())
            {
                return;
            }

            ViewModel.SendModToTop((lstMods.SelectedItem as InstalledModViewModel));
        }

        private void btnSendBottom_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
            {
                return;
            }

            if (!IsActiveModSelected())
            {
                return;
            }

            ViewModel.SendModToBottom((lstMods.SelectedItem as InstalledModViewModel));
        }
    }
}
