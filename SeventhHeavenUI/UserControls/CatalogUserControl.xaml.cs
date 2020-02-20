using _7thHeaven.Code;
using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        Dictionary<string, double> MinColumnWidths = new Dictionary<string, double>()
        {
            { "Name", 100 },
            { "Author", 60 },
            { "Released", 90 },
            { "Category", 100 },
            { "Size", 70 },
            { "Inst.", 30 }
        };

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

        internal void RecalculateDownloadColumnWidths(double listWidth)
        {
            double staticColumnWidth = 90 + 90; // sum of columns with static widths
            double padding = 8;

            if (listWidth == 0)
            {
                return; // ActualWidth could be zero if list has not been rendered yet
            }

            // account for the scroll bar being visible and add extra padding
            ScrollViewer sv = FindVisualChild<ScrollViewer>(lstDownloads);
            Visibility? scrollVis = sv?.ComputedVerticalScrollBarVisibility;

            if (scrollVis.GetValueOrDefault() == Visibility.Visible)
            {
                padding = 26;
            }


            double remainingWidth = listWidth - staticColumnWidth - padding;

            double nameWidth = (0.66) * remainingWidth; // Download Name takes 66% of remaining width
            double progressWidth = (0.33) * remainingWidth; // Progress takes up 33% of remaining width

            double minNameWidth = 100; // don't resize columns less than the minimums
            double minProgressWidth = 60;

            try
            {
                if (nameWidth < listWidth && nameWidth > minNameWidth)
                {
                    colDownloadName.Width = nameWidth;
                }
                else if (nameWidth <= minNameWidth)
                {
                    colDownloadName.Width = minNameWidth;
                }

                if (progressWidth < listWidth && progressWidth > minProgressWidth)
                {
                    colProgress.Width = progressWidth;
                }
                else if (nameWidth <= minProgressWidth)
                {
                    colProgress.Width = minProgressWidth;
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e, "failed to resize download list columns");
            }
        }

        internal void RecalculateColumnWidths(double listWidth)
        {
            var currentColumnSettings = GetColumnSettings();

            double staticColumnWidth = currentColumnSettings.Where(c => !c.AutoResize).Sum(s => s.Width); // sum of columns with static widths
            double padding = 8;

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

        internal void SetSortColumn(string sortColumn, ListSortDirection sortDirection)
        {
            if (sortColumn != null && MinColumnWidths.ContainsKey(sortColumn))
            {
                Sort(sortColumn, sortDirection);
            }
        }

        internal void RecalculateColumnWidths()
        {
            RecalculateColumnWidths(lstCatalogMods.ActualWidth);
            RecalculateDownloadColumnWidths(lstCatalogMods.ActualWidth);
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
                switch (headerClicked.Content)
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

            if (sortBy == nameof(CatalogModItemViewModel.ReleaseDate) || sortBy == "Released")
            {
                DateTimeComparer sorter = new DateTimeComparer()
                {
                    SortDirection = direction
                };
                (dataView as ListCollectionView).CustomSort = sorter;
            }
            else if (sortBy == nameof(CatalogModItemViewModel.DownloadSize) || sortBy == "Size")
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

        internal List<ColumnInfo> GetColumnSettings()
        {
            GridViewColumnCollection columns = (lstCatalogMods.View as GridView).Columns;

            if (columns == null)
            {
                return null;
            }

            List<string> columnsThatAutoResize = new List<string>() { "Name", "Author" };

            return columns.Select(c => new ColumnInfo()
            {
                Name = (c.Header as GridViewColumnHeader).Content as string,
                Width = c.ActualWidth,
                AutoResize = columnsThatAutoResize.Contains((c.Header as GridViewColumnHeader).Content as string)
            }).ToList();
        }

        /// <summary>
        /// Ensure column is not resized to less than the defined minimum width.
        /// Sets width to minimum width if less than minimum.
        /// </summary>
        private void GridViewColumnHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            string columnName = (e.OriginalSource as GridViewColumnHeader).Content as string;

            if (e.NewSize.Width < MinColumnWidths[columnName])
            {
                e.Handled = true;
                ((GridViewColumnHeader)sender).Column.Width = MinColumnWidths[columnName];
            }
        }

        internal void SaveUsersColumnSettings()
        {
            Sys.Settings.UserColumnSettings.BrowseCatalogColumns = GetColumnSettings();

            if (_lastHeaderClicked != null)
            {
                Sys.Settings.UserColumnSettings.SortColumn = _lastHeaderClicked.Content as string;
            }
            Sys.Settings.UserColumnSettings.SortDirection = (int)_lastDirection;
        }

        private void btnResetColumns_Click(object sender, RoutedEventArgs e)
        {
            List<ColumnInfo> defaultColumns = ColumnSettings.GetDefaultSettings().BrowseCatalogColumns;
            ApplyColumnSettings(defaultColumns);
        }

        internal void ApplyColumnSettings(List<ColumnInfo> newColumns)
        {
            GridViewColumnCollection columns = (lstCatalogMods.View as GridView).Columns;

            for (int i = 0; i < newColumns.Count; i++)
            {
                // get the current index of the column
                int colIndex = columns.ToList().FindIndex(c => ((c.Header as GridViewColumnHeader).Content as string) == newColumns[i].Name);

                // apply the new width if the column does not auto resize
                if (!newColumns[i].AutoResize)
                {
                    columns[colIndex].Width = newColumns[i].Width;
                }

                // move the column to expected index
                columns.Move(colIndex, i);
            }

            RecalculateColumnWidths(); // call this to have auto resize columns recalculate
        }
    }
}
