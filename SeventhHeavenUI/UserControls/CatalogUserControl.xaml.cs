using SeventhHeavenUI.ViewModels;
using System.Windows.Controls;

namespace SeventhHeaven.UserControls
{
    /// <summary>
    /// Interaction logic for CatalogUserControl.xaml
    /// </summary>
    public partial class CatalogUserControl : UserControl
    {
        CatalogViewModel ViewModel { get; set; }

        public CatalogUserControl()
        {
            InitializeComponent();
        }

        public void SetDataContext(CatalogViewModel viewModel)
        {
            ViewModel = viewModel;
            this.DataContext = ViewModel;
        }

        private void lstCatalogMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.RaiseSelectedModChanged(sender, (lstCatalogMods.SelectedItem as CatalogModItemViewModel));
        }

        private void btnRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.ReloadModList();
        }
    }
}
