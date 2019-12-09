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
                ViewModel.ChangeTheme();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.ChangeTheme();
        }
    }
}
