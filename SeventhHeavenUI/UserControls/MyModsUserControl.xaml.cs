using SeventhHeavenUI.ViewModels;
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
    }
}
