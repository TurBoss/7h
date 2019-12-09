using SeventhHeaven.Classes;
using SeventhHeaven.ViewModels;
using SeventhHeavenUI;
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
    /// Interaction logic for ThemeSettingsWindow.xaml
    /// </summary>
    public partial class ThemeSettingsWindow : Window
    {
        ThemeSettingsViewModel ViewModel { get; set; }

        public ThemeSettingsWindow()
        {
            InitializeComponent();

            ViewModel = new ThemeSettingsViewModel();
            this.DataContext = ViewModel;
        }

        private void cboThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.ChangeTheme();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveTheme();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.ApplyCustomTheme();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyCustomTheme();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ThemeSettingsViewModel.LoadThemeFromFile(); // reload theme.xml on cancel in-case any unsaved changes are made
            this.Close();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            string themeFile = FileDialogHelper.BrowseForFile("theme file (.xml)|*.xml", "Select Custom Theme .xml File");

            if (!string.IsNullOrWhiteSpace(themeFile))
            {
                ViewModel.ApplyThemeFromFile(themeFile);
            }
        }

        private void menuSaveOptions_Closed(object sender, RoutedEventArgs e)
        {
            btnSaveAs.IsEnabled = true;
        }

        private void menuExport_Click(object sender, RoutedEventArgs e)
        {
            string saveFile = FileDialogHelper.OpenSaveDialog("theme file (.xml)|*.xml", "Save Custom Theme .xml File");

            if (!string.IsNullOrWhiteSpace(saveFile))
            {
                ViewModel.SaveTheme(saveFile);
            }
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (!menuSaveOptions.IsOpen)
            {
                menuSaveOptions.PlacementTarget = btnSave;
                menuSaveOptions.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                menuSaveOptions.IsOpen = true;
                btnSaveAs.IsEnabled = false;
            }
        }

        private void windowTheme_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ThemeSettingsViewModel.LoadThemeFromFile(); // reload theme.xml on close in-case any unsaved changes are made
        }
    }
}
