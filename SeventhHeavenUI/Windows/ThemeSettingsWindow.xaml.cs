using SeventhHeaven.Classes;
using SeventhHeaven.Classes.Themes;
using SeventhHeaven.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SeventhHeaven.Windows
{
    /// <summary>
    /// Interaction logic for ThemeSettingsWindow.xaml
    /// </summary>
    public partial class ThemeSettingsWindow : Window
    {
        public ThemeSettingsViewModel ViewModel { get; set; }

        public ThemeSettingsWindow()
        {
            InitializeComponent();

            ViewModel = new ThemeSettingsViewModel();
            this.DataContext = ViewModel;
        }

        private void cboThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.ChangeTheme();
            InitColorPickerBackgrounds();
        }

        /// <summary>
        /// Sets the SelectedColor of the ColorPicker controls to the ViewModel properties
        /// </summary>
        private void InitColorPickerBackgrounds()
        {
            try
            {
                pickerAppBg.SelectedColor = (Color)ColorConverter.ConvertFromString(ViewModel.AppBackgroundText);
                pickerSecondBg.SelectedColor = (Color)ColorConverter.ConvertFromString(ViewModel.SecondaryBackgroundText);
                pickerDisabledBg.SelectedColor = (Color)ColorConverter.ConvertFromString(ViewModel.ControlDisabledBgText);
                pickerDisabledFg.SelectedColor = (Color)ColorConverter.ConvertFromString(ViewModel.ControlDisabledFgText);
                pickerControlBg.SelectedColor = (Color)ColorConverter.ConvertFromString(ViewModel.ControlBackgroundText);
                pickerControlFg.SelectedColor = (Color)ColorConverter.ConvertFromString(ViewModel.ControlForegroundText);
                pickerMouseOver.SelectedColor = (Color)ColorConverter.ConvertFromString(ViewModel.ControlMouseOverText);
                pickerPressed.SelectedColor = (Color)ColorConverter.ConvertFromString(ViewModel.ControlPressedText);
            }
            catch (Exception)
            {
                // color was invalid - the viewmodel will handle displaying error message to user so ignore this exception
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveTheme();
            this.Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.ApplyCustomTheme();
                InitColorPickerBackgrounds(); // when the user types a color in then we have to make sure the color pickers get updated to match the new color
                SetSelectedThemeToCustom();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyCustomTheme();
            InitColorPickerBackgrounds(); // when the user types a color in then we have to make sure the color pickers get updated to match the new color
            SetSelectedThemeToCustom();
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

            // make sure image is reset
            try
            {
                ITheme themeSettings = ThemeSettingsViewModel.GetThemeSettingsFromFile();
                byte[] imageBytes = Convert.FromBase64String(themeSettings.BackgroundImageBase64);
                ViewModel.CurrentImageTheme = imageBytes;
            }
            catch (Exception)
            {
                ViewModel.CurrentImageTheme = null;
            }
        }

        private void SetSelectedThemeToCustom()
        {
            if (ViewModel.SelectedThemeText != "Custom")
                ViewModel.SelectedThemeText = "Custom";
        }

        #region Color Picker Color Changed Events

        private void pickerAppBg_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ViewModel.ColorChanged(nameof(ViewModel.AppBackgroundText), e.NewValue);
        }
        private void pickerSecondBg_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ViewModel.ColorChanged(nameof(ViewModel.SecondaryBackgroundText), e.NewValue);
        }
        private void pickerDisabledBg_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ViewModel.ColorChanged(nameof(ViewModel.ControlDisabledBgText), e.NewValue);
        }
        private void pickerDisabledFg_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ViewModel.ColorChanged(nameof(ViewModel.ControlDisabledFgText), e.NewValue);
        }
        private void pickerControlBg_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ViewModel.ColorChanged(nameof(ViewModel.ControlBackgroundText), e.NewValue);
        }
        private void pickerControlFg_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ViewModel.ColorChanged(nameof(ViewModel.ControlForegroundText), e.NewValue);
        }
        private void pickerMouseOver_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ViewModel.ColorChanged(nameof(ViewModel.ControlMouseOverText), e.NewValue);
        }
        private void pickerPressed_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ViewModel.ColorChanged(nameof(ViewModel.ControlPressedText), e.NewValue);
        }

        #endregion

        private void ColorPicker_Closed(object sender, RoutedEventArgs e)
        {
            SetSelectedThemeToCustom();
        }

        private void btnBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectBackgroundImage();
        }
    }
}
