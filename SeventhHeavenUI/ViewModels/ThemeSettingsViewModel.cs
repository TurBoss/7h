using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeavenUI;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SeventhHeaven.ViewModels
{
    public class ThemeSettingsViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _statusText;
        private string _selectedThemeText;
        private List<string> _themeDropdownItems;
        private string _appBackgroundText;
        private string _secondaryBackgroundText;
        private string _controlDisabledBgText;
        private string _controlDisabledFgText;
        private string _controlBackgroundText;
        private string _controlForegroundText;
        private string _controlMouseOverText;
        private string _controlPressedText;

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                NotifyPropertyChanged();
            }
        }


        public bool IsCustomThemeEnabled
        {
            get
            {
                return SelectedThemeText == "Custom";
            }
        }

        public string SelectedThemeText
        {
            get
            {
                return _selectedThemeText;
            }
            set
            {
                _selectedThemeText = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsCustomThemeEnabled));
            }
        }

        public List<string> ThemeDropdownItems
        {
            get
            {
                if (_themeDropdownItems == null)
                    _themeDropdownItems = new List<string>() { "Dark Mode", "Light Mode", "Custom" };

                return _themeDropdownItems;
            }
            set
            {
                _themeDropdownItems = value;
                NotifyPropertyChanged();
            }
        }

        public string AppBackgroundText
        {
            get
            {
                return _appBackgroundText;
            }
            set
            {
                _appBackgroundText = value;
                NotifyPropertyChanged();
            }
        }

        public string SecondaryBackgroundText
        {
            get
            {
                return _secondaryBackgroundText;
            }
            set
            {
                _secondaryBackgroundText = value;
                NotifyPropertyChanged();
            }
        }

        public string ControlDisabledBgText
        {
            get
            {
                return _controlDisabledBgText;
            }
            set
            {
                _controlDisabledBgText = value;
                NotifyPropertyChanged();
            }
        }

        public string ControlDisabledFgText
        {
            get
            {
                return _controlDisabledFgText;
            }
            set
            {
                _controlDisabledFgText = value;
                NotifyPropertyChanged();
            }
        }

        public string ControlBackgroundText
        {
            get
            {
                return _controlBackgroundText;
            }
            set
            {
                _controlBackgroundText = value;
                NotifyPropertyChanged();
            }
        }

        public string ControlForegroundText
        {
            get
            {
                return _controlForegroundText;
            }
            set
            {
                _controlForegroundText = value;
                NotifyPropertyChanged();
            }
        }

        public string ControlMouseOverText
        {
            get
            {
                return _controlMouseOverText;
            }
            set
            {
                _controlMouseOverText = value;
                NotifyPropertyChanged();
            }
        }

        public string ControlPressedText
        {
            get
            {
                return _controlPressedText;
            }
            set
            {
                _controlPressedText = value;
                NotifyPropertyChanged();
            }
        }

        public ThemeSettingsViewModel()
        {
            StatusText = "";
            SelectedThemeText = GetSavedThemeName();
            InitColorTextInput();
        }

        public ThemeSettingsViewModel(bool loadThemeXml)
        {
            StatusText = "";
            SelectedThemeText = "Custom";

            if (loadThemeXml)
            {
                SelectedThemeText = GetSavedThemeName();
                InitColorTextInput();
            }
        }

        /// <summary>
        /// imports the theme.xml file and sets the app color brushes.
        /// </summary>
        internal static void LoadThemeFromFile()
        {
            string pathToThemeFile = Path.Combine(Sys.SysFolder, "theme.xml");

            // dark theme will be applied as the default when theme.xml file does not exist
            if (!File.Exists(pathToThemeFile))
            {
                new ThemeSettingsViewModel(loadThemeXml: false).ApplyDarkTheme();
                return;
            }

            ImportTheme(pathToThemeFile);
        }

        /// <summary>
        /// Reads theme .xml and sets App Brush resources
        /// </summary>
        /// <param name="themeFile"></param>
        internal static void ImportTheme(string themeFile)
        {
            try
            {
                ThemeSettings theme = Util.Deserialize<ThemeSettings>(themeFile);
                ThemeSettingsViewModel settingsViewModel = new ThemeSettingsViewModel(loadThemeXml: false);
                
                if (theme.Name == "Dark Mode")
                {
                    settingsViewModel.ApplyDarkTheme();
                    return;
                }
                else if (theme.Name == "Light Mode")
                {
                    settingsViewModel.ApplyLightTheme();
                    return;
                }

                settingsViewModel.ApplyThemeFromFile(themeFile);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// Reads theme.xml file and returns Name of theme ("Dark Mode", "Light Mode", or "Custom")
        /// </summary>
        /// <returns></returns>
        internal static string GetSavedThemeName()
        {
            try
            {
                string pathToThemeFile = Path.Combine(Sys.SysFolder, "theme.xml");
                ThemeSettings savedTheme = Util.Deserialize<ThemeSettings>(pathToThemeFile);
                return savedTheme.Name;
            }
            catch (Exception e)
            {
                Logger.Warn(e);
            }

            return "Dark Mode";
        }

        /// <summary>
        /// saves current colors to theme.xml
        /// </summary>
        internal void SaveTheme()
        {
            string pathToThemeFile = Path.Combine(Sys.SysFolder, "theme.xml");
            SaveTheme(pathToThemeFile);
            StatusText = "Theme saved!";
        }

        internal void SaveTheme(string pathToTheme)
        {
            ThemeSettings settings = new ThemeSettings()
            {
                Name = SelectedThemeText
            };

            if (settings.Name == "Custom")
            {
                settings.PrimaryAppBackground = AppBackgroundText;
                settings.SecondaryAppBackground = SecondaryBackgroundText;
                settings.PrimaryControlDisabledBackground = ControlDisabledBgText;
                settings.PrimaryControlDisabledForeground = ControlDisabledFgText;
                settings.PrimaryControlBackground = ControlBackgroundText;
                settings.PrimaryControlForeground = ControlForegroundText;
                settings.PrimaryControlMouseOver = ControlMouseOverText;
                settings.PrimaryControlPressed = ControlPressedText;
            }


            try
            {
                using (FileStream file = new FileStream(pathToTheme, FileMode.Create, FileAccess.ReadWrite))
                {
                    Util.Serialize(settings, file);
                }

                StatusText = $"Theme saved as {Path.GetFileName(pathToTheme)}";
            }
            catch (Exception e)
            {
                Logger.Error(e);
                StatusText = "Failed to save theme...";
            }

        }

        internal void ChangeTheme()
        {
            if (SelectedThemeText == "Dark Mode")
            {
                ApplyDarkTheme();

            }
            else if (SelectedThemeText == "Light Mode")
            {
                ApplyLightTheme();
            }
            else
            {
                InitColorTextInput();
                ApplyCustomTheme();
                return;
            }
        }

        private void ApplyLightTheme()
        {
            Color? darkBg = App.Current.TryFindResource("LightBackgroundColor") as Color?;
            Color? secondBg = App.Current.TryFindResource("MedLightBackgroundColor") as Color?;
            Color? controlBg = App.Current.TryFindResource("LightControlBackground") as Color?;
            Color? controlFg = App.Current.TryFindResource("LightControlForeground") as Color?;
            Color? controlPressed = App.Current.TryFindResource("LightControlPressed") as Color?;
            Color? controlMouseOver = App.Current.TryFindResource("LightControlMouseOver") as Color?;
            Color? controlDisabledBg = App.Current.TryFindResource("LightControlDisabledBackground") as Color?;
            Color? controlDisabledFg = App.Current.TryFindResource("LightControlDisabledForeground") as Color?;

            AppBackgroundText = ColorToHexString(darkBg.Value);
            SecondaryBackgroundText = ColorToHexString(secondBg.Value);
            ControlBackgroundText = ColorToHexString(controlBg.Value);
            ControlForegroundText = ColorToHexString(controlFg.Value);
            ControlPressedText = ColorToHexString(controlPressed.Value);
            ControlMouseOverText = ColorToHexString(controlMouseOver.Value);
            ControlDisabledBgText = ColorToHexString(controlDisabledBg.Value);
            ControlDisabledFgText = ColorToHexString(controlDisabledFg.Value);

            ApplyCustomTheme();
        }

        private void ApplyDarkTheme()
        {
            Color? darkBg = App.Current.TryFindResource("DarkBackgroundColor") as Color?;
            Color? secondBg = App.Current.TryFindResource("MedDarkBackgroundColor") as Color?;
            Color? controlBg = App.Current.TryFindResource("DarkControlBackground") as Color?;
            Color? controlFg = App.Current.TryFindResource("DarkControlForeground") as Color?;
            Color? controlPressed = App.Current.TryFindResource("DarkControlPressed") as Color?;
            Color? controlMouseOver = App.Current.TryFindResource("DarkControlMouseOver") as Color?;
            Color? controlDisabledBg = App.Current.TryFindResource("DarkControlDisabledBackground") as Color?;
            Color? controlDisabledFg = App.Current.TryFindResource("DarkControlDisabledForeground") as Color?;

            AppBackgroundText = ColorToHexString(darkBg.Value);
            SecondaryBackgroundText = ColorToHexString(secondBg.Value);
            ControlBackgroundText = ColorToHexString(controlBg.Value);
            ControlForegroundText = ColorToHexString(controlFg.Value);
            ControlPressedText = ColorToHexString(controlPressed.Value);
            ControlMouseOverText = ColorToHexString(controlMouseOver.Value);
            ControlDisabledBgText = ColorToHexString(controlDisabledBg.Value);
            ControlDisabledFgText = ColorToHexString(controlDisabledFg.Value);

            ApplyCustomTheme();
        }

        /// <summary>
        /// Updates App brush resources based on properties in view model (e.g. <see cref="AppBackgroundText"/>, <see cref="ControlBackgroundText"/>, etc...)
        /// </summary>
        internal void ApplyCustomTheme()
        {
            try
            {
                App.Current.Resources["PrimaryAppBackground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(AppBackgroundText));
                App.Current.Resources["SecondaryAppBackground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(SecondaryBackgroundText));

                App.Current.Resources["PrimaryControlBackground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(ControlBackgroundText));
                App.Current.Resources["PrimaryControlForeground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(ControlForegroundText));
                App.Current.Resources["PrimaryControlPressed"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(ControlPressedText));
                App.Current.Resources["PrimaryControlMouseOver"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(ControlMouseOverText));

                App.Current.Resources["PrimaryControlDisabledBackground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(ControlDisabledBgText));
                App.Current.Resources["PrimaryControlDisabledForeground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(ControlDisabledFgText));

                App.Current.Resources["iconColorBrush"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(ControlForegroundText));
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                StatusText = $"Failed to apply theme: {e.Message}";
            }
        }

        /// <summary>
        /// Updates app brush resources from valid theme .xml file
        /// </summary>
        /// <param name="themeFile"></param>
        internal void ApplyThemeFromFile(string themeFile)
        {
            try
            {
                ThemeSettings theme = Util.Deserialize<ThemeSettings>(themeFile);

                AppBackgroundText = theme.PrimaryAppBackground;
                SecondaryBackgroundText = theme.SecondaryAppBackground;
                ControlBackgroundText = theme.PrimaryControlBackground;
                ControlForegroundText = theme.PrimaryControlForeground;
                ControlPressedText = theme.PrimaryControlPressed;
                ControlMouseOverText = theme.PrimaryControlMouseOver;
                ControlDisabledBgText = theme.PrimaryControlDisabledBackground;
                ControlDisabledFgText = theme.PrimaryControlDisabledForeground;

                ApplyCustomTheme();
                SelectedThemeText = "Custom";

                StatusText = "Theme loaded! Click 'Save' to save this theme as the default ...";
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                StatusText = $"Failed to load theme: {e.Message}";
            }
        }

        /// <summary>
        /// Reads theme.xml file and sets properties in view model
        /// </summary>
        private void InitColorTextInput()
        {
            ThemeSettings savedTheme = null;

            try
            {
                string pathToThemeFile = Path.Combine(Sys.SysFolder, "theme.xml");
                savedTheme = Util.Deserialize<ThemeSettings>(pathToThemeFile);
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return;
            }

            if (savedTheme == null || savedTheme?.Name != "Custom")
            {
                return;
            }

            AppBackgroundText = savedTheme.PrimaryAppBackground;
            SecondaryBackgroundText = savedTheme.SecondaryAppBackground;
            ControlBackgroundText = savedTheme.PrimaryControlBackground;
            ControlForegroundText = savedTheme.PrimaryControlForeground;
            ControlPressedText = savedTheme.PrimaryControlPressed;
            ControlMouseOverText = savedTheme.PrimaryControlMouseOver;
            ControlDisabledBgText = savedTheme.PrimaryControlDisabledBackground;
            ControlDisabledFgText = savedTheme.PrimaryControlDisabledForeground;
        }

        public static string ColorToHexString(Color color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }
    }
}
