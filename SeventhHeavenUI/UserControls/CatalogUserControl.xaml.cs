using Iros._7th.Workshop;
using SeventhHeavenUI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SeventhHeaven.UserControls
{
    /// <summary>
    /// Interaction logic for CatalogUserControl.xaml
    /// </summary>
    public partial class CatalogUserControl : UserControl
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
            ViewModel.RefreshCatalogList();
            RecalculateColumnWidths();
        }

        private void btnDownload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstCatalogMods.SelectedItem == null)
            {
                Sys.Message(new WMessage("Select a mod to download first.", true));
                return;
            }

            ViewModel.DownloadMod((lstCatalogMods.SelectedItem as CatalogModItemViewModel));
        }

        private void menuItemCancelDownload_Click(object sender, RoutedEventArgs e)
        {
            if (lstDownloads.SelectedItem == null)
            {
                Sys.Message(new WMessage("No Download selected.", true));
                return;
            }

            ViewModel.CancelDownload((lstDownloads.SelectedItem as DownloadItemViewModel));
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstCatalogMods.SelectedItem != null)
            {
                ViewModel.DownloadMod((lstCatalogMods.SelectedItem as CatalogModItemViewModel));
            }
        }

        internal void RecalculateColumnWidths(double listWidth)
        {
            double scrollBarWidth = 15;
            double staticColumnWidth = 40 + 90 + 90 + 60; // sum of columns with static widths

            if (listWidth == 0)
            {
                return; // ActualWidth could be zero if list has not been rendered yet
            }

            var scrollBarVis = ScrollViewer.GetVerticalScrollBarVisibility(lstCatalogMods);

            if (scrollBarVis == ScrollBarVisibility.Visible)
            {
                scrollBarWidth = 15;
            }

            double remainingWidth = listWidth - staticColumnWidth - scrollBarWidth;

            double nameWidth = (0.66) * remainingWidth; // Name takes 66% of remaining width
            double authorWidth = (0.33) * remainingWidth; // Author takes up 33% of remaining width

            double minNameWidth = 100; // don't resize columns less than the minimums
            double minAuthorWidth = 60;

            try
            {
                if (nameWidth < listWidth && nameWidth > minNameWidth)
                {
                    colName.Width = nameWidth;
                }

                if (authorWidth < listWidth && authorWidth > minAuthorWidth)
                {
                    colAuthor.Width = authorWidth;
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e, "failed to resize columns");
            }
        }

        internal void RecalculateColumnWidths()
        {
            RecalculateColumnWidths(lstCatalogMods.ActualWidth);
        }
    }
}
