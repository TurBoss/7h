using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SeventhHeaven.UserControls
{
    /// <summary>
    /// Interaction logic for CatalogUserControl.xaml
    /// </summary>
    public partial class CatalogUserControl : UserControl
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        CatalogViewModel ViewModel { get; set; }
        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

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
            double padding = 8;
            double staticColumnWidth = 90 + 100 + 60 + 40; // sum of columns with static widths

            if (listWidth == 0)
            {
                return; // ActualWidth could be zero if list has not been rendered yet
            }

            // account for the scroll bar being visible and add extra padding
            ScrollViewer sv = FindVisualChild<ScrollViewer>(lstCatalogMods);
            Visibility? scrollVis = sv?.ComputedVerticalScrollBarVisibility;

            if (scrollVis.GetValueOrDefault() == Visibility.Visible)
            {
                padding = 26;
            }


            double remainingWidth = listWidth - staticColumnWidth - padding;

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

        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked == null || headerClicked?.Role == GridViewColumnHeaderRole.Padding)
            {
                return;
            }


            if (headerClicked != _lastHeaderClicked)
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                if (_lastDirection == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                }
            }

            string propertyNameToSortBy = "";

            if (headerClicked.Column.DisplayMemberBinding != null)
            {
                Binding headerBinding = headerClicked.Column.DisplayMemberBinding.ProvideValue(null) as Binding;
                
                if (headerBinding == null)
                {
                    return;
                }

                propertyNameToSortBy = headerBinding.Path?.Path;
            }
            else
            {
                switch (headerClicked.Column.Header)
                {
                    case "Inst.":
                        propertyNameToSortBy = "IsInstalled";
                        break;

                    default:
                        return; // exit if unknown
                }
            }


            Sort(propertyNameToSortBy, direction);

            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(lstCatalogMods.ItemsSource);

            if (dataView == null)
            {
                return;
            }

            dataView.SortDescriptions.Clear();
            (dataView as ListCollectionView).CustomSort = null;

            if (sortBy == nameof(CatalogModItemViewModel.ReleaseDate))
            {
                DateTimeComparer sorter = new DateTimeComparer()
                {
                    SortDirection = direction
                };
                (dataView as ListCollectionView).CustomSort = sorter;
            }
            else if (sortBy == nameof(CatalogModItemViewModel.DownloadSize))
            {
                SortDescription sd = new SortDescription(nameof(CatalogModItemViewModel.DownloadSizeInBytes), direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
            else
            {
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }
    }
}
