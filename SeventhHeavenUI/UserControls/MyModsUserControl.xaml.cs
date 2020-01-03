using Iros._7th.Workshop;
using SeventhHeaven.Windows;
using SeventhHeavenUI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SeventhHeaven.UserControls
{
    /// <summary>
    /// Interaction logic for MyModsUserControl.xaml
    /// </summary>
    public partial class MyModsUserControl : UserControl
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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

        /// <summary>
        /// Returns true if a mod is selected in the list.
        /// Returns false and shows messagebox warning user that no mod is selected otherwise;
        /// </summary>
        private bool IsModSelected()
        {
            if (lstMods.SelectedItem == null)
            {
                Sys.Message(new WMessage("Select a mod first.", true));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the selected mod is active.
        /// Returns false and shows messagebox warning user that selected mod is not active otherwise;
        /// </summary>
        private bool IsActiveModSelected(string notActiveMessage)
        {
            if (lstMods.SelectedItem == null)
            {
                return false;
            }

            if (!(lstMods.SelectedItem as InstalledModViewModel).IsActive)
            {
                Sys.Message(new WMessage(notActiveMessage, true));
                return false;
            }

            return true;
        }

        private void lstMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMods.SelectedItem == null)
            {
                return;
            }

            ViewModel.RaiseSelectedModChanged(sender, (lstMods.SelectedItem as InstalledModViewModel));
        }

        private void btnRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.RefreshModList();
            RecalculateColumnWidths();
        }

        private void btnDeactivateAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.DeactivateAllActiveMods();
        }

        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
            {
                return;
            }

            InstalledModViewModel selected = (lstMods.SelectedItem as InstalledModViewModel);

            MessageDialogWindow messageDialog = new MessageDialogWindow("Uninstall Warning", $"Are you sure you want to delete {selected.Name}?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            messageDialog.ShowDialog();

            if (messageDialog.ViewModel.Result == MessageBoxResult.Yes)
            {
                ViewModel.UninstallMod(selected);
            }

        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
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

            ViewModel.ReorderProfileItem((lstMods.SelectedItem as InstalledModViewModel), 1);
        }

        private void btnMoveTop_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
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

            ViewModel.SendModToBottom((lstMods.SelectedItem as InstalledModViewModel));
        }

        private void btnActivateAll_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ActivateAllMods();
        }

        private void btnConfigure_Click(object sender, RoutedEventArgs e)
        {
            if (!IsModSelected())
            {
                return;
            }

            if (!IsActiveModSelected("Mod is not active. Only activated mods can be configured."))
            {
                return;
            }

            ViewModel.ShowConfigureModWindow((lstMods.SelectedItem as InstalledModViewModel));
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowImportModWindow();
        }

        internal void RecalculateColumnWidths(double listWidth)
        {
            double staticColumnWidth = 40 + 90 + 100 + 75; // sum of columns with static widths
            double padding = 6;

            if (listWidth == 0)
            {
                return; // ActualWidth could be zero if list has not been rendered yet
            }


            // account for the scroll bar being visible and add extra padding
            ScrollViewer sv = FindVisualChild<ScrollViewer>(lstMods);
            Visibility? scrollVis = sv?.ComputedVerticalScrollBarVisibility;

            if (scrollVis.GetValueOrDefault(Visibility.Collapsed) == Visibility.Visible)
            {
                padding = 24;
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
            catch (System.Exception e)
            {
                Logger.Warn(e, "failed to resize columns");
            }
        }

        internal void RecalculateColumnWidths()
        {
            RecalculateColumnWidths(lstMods.ActualWidth);
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstMods.SelectedItem != null)
            {
                ViewModel.ToggleActivateMod((lstMods.SelectedItem as InstalledModViewModel).InstallInfo.ModID);
            }
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

        /// <summary>
        /// Initiates the Drag/Drop re-ordering of active mods
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstMods_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (e.OriginalSource is CheckBox)
                {
                    return; // do not do drag/drop since user is clicking on checkbox to activate mod
                }

                if ((e.OriginalSource as FrameworkElement)?.DataContext is InstalledModViewModel)
                {
                    ListViewItem lbi = FindVisualParent<ListViewItem>(((DependencyObject)e.OriginalSource));

                    if (lbi != null)
                    {
                        DragDrop.DoDragDrop(lbi, lbi.DataContext, DragDropEffects.Move);
                    }
                }
            }
        }

        /// <summary>
        /// Re-orders active mods after Drag/Drop has been "dropped"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstMods_Drop(object sender, DragEventArgs e)
        {
            try
            {
                InstalledModViewModel droppedData = e.Data.GetData(typeof(InstalledModViewModel)) as InstalledModViewModel;
                InstalledModViewModel target = ((FrameworkElement)e.OriginalSource)?.DataContext as InstalledModViewModel;

                int removedIdx = lstMods.Items.IndexOf(droppedData);
                int targetIdx = lstMods.Items.IndexOf(target);

                int delta = targetIdx - removedIdx;

                ViewModel.ReorderProfileItem(droppedData, delta);
            }
            catch (System.Exception ex)
            {
                Logger.Warn("Failed to drag/drop mods");
                Logger.Error(ex);
            }
        }

        private T FindVisualParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;

            return FindVisualParent<T>(parentObject);
        }

        private void btnAutoSort_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AutoSortBasedOnCategory();
        }
    }
}
