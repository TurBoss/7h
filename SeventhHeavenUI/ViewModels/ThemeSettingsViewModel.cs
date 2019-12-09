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
    class ThemeSettingsViewModel : ViewModelBase
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
                    _themeDropdownItems = new List<string>();

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

        internal void SaveTheme()
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

            string pathToThemeFile = Path.Combine(Sys.SysFolder, "theme.xml");

            try
            {
                using (FileStream file = new FileStream(pathToThemeFile, FileMode.Create, FileAccess.ReadWrite))
                {
                    Util.Serialize(settings, file);
                }

                StatusText = "Theme saved!";
            }
            catch (Exception e)
            {
                Logger.Error(e);
                StatusText = "Failed to save theme...";
            }

        }

        internal static void LoadTheme()
        {
            try
            {
                string pathToThemeFile = Path.Combine(Sys.SysFolder, "theme.xml");
                ThemeSettings savedTheme = Util.Deserialize<ThemeSettings>(pathToThemeFile);

                App.Current.Resources["PrimaryAppBackground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.PrimaryAppBackground));
                App.Current.Resources["SecondaryAppBackground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.SecondaryAppBackground));

                App.Current.Resources["PrimaryControlBackground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.PrimaryControlBackground));
                App.Current.Resources["PrimaryControlForeground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.PrimaryControlForeground));
                App.Current.Resources["PrimaryControlPressed"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.PrimaryControlPressed));
                App.Current.Resources["PrimaryControlMouseOver"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.PrimaryControlMouseOver));

                App.Current.Resources["PrimaryControlDisabledBackground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.PrimaryControlDisabledBackground));
                App.Current.Resources["PrimaryControlDisabledForeground"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.PrimaryControlDisabledForeground));

                App.Current.Resources["iconColorBrush"] = (SolidColorBrush)(new BrushConverter().ConvertFrom(savedTheme.PrimaryControlForeground));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        internal void ChangeTheme()
        {
            Color? darkBg = null;
            Color? secondBg = null;
            Color? controlBg = null;
            Color? controlFg = null;
            Color? controlPressed = null;
            Color? controlMouseOver = null;
            Color? controlDisabledBg = null;
            Color? controlDisabledFg = null;


            if (SelectedThemeText == "Dark Mode")
            {
                darkBg = App.Current.TryFindResource("DarkBackgroundColor") as Color?;
                secondBg = App.Current.TryFindResource("MedDarkBackgroundColor") as Color?;
                controlBg = App.Current.TryFindResource("DarkControlBackground") as Color?;
                controlFg = App.Current.TryFindResource("DarkControlForeground") as Color?;
                controlPressed = App.Current.TryFindResource("DarkControlPressed") as Color?;
                controlMouseOver = App.Current.TryFindResource("DarkControlMouseOver") as Color?;
                controlDisabledBg = App.Current.TryFindResource("DarkControlDisabledBackground") as Color?;
                controlDisabledFg = App.Current.TryFindResource("DarkControlDisabledForeground") as Color?;

            }
            else if (SelectedThemeText == "Light Mode")
            {
                darkBg = App.Current.TryFindResource("LightBackgroundColor") as Color?;
                secondBg = App.Current.TryFindResource("MedLightBackgroundColor") as Color?;
                controlBg = App.Current.TryFindResource("LightControlBackground") as Color?;
                controlFg = App.Current.TryFindResource("LightControlForeground") as Color?;
                controlPressed = App.Current.TryFindResource("LightControlPressed") as Color?;
                controlMouseOver = App.Current.TryFindResource("LightControlMouseOver") as Color?;
                controlDisabledBg = App.Current.TryFindResource("LightControlDisabledBackground") as Color?;
                controlDisabledFg = App.Current.TryFindResource("LightControlDisabledForeground") as Color?;
            }
            else
            {
                InitColorTextInput();
                ApplyCustomTheme();
                return;
            }

            AppBackgroundText = ColorToHexString(darkBg.Value);
            SecondaryBackgroundText = ColorToHexString(secondBg.Value);
            ControlBackgroundText = ColorToHexString(controlBg.Value);
            ControlForegroundText = ColorToHexString(controlFg.Value);
            ControlPressedText = ColorToHexString(controlPressed.Value);
            ControlMouseOverText = ColorToHexString(controlMouseOver.Value);
            ControlDisabledBgText = ColorToHexString(controlDisabledBg.Value);
            ControlDisabledFgText = ColorToHexString(controlDisabledFg.Value);

            App.Current.Resources["PrimaryAppBackground"] = new SolidColorBrush(darkBg.Value);
            App.Current.Resources["SecondaryAppBackground"] = new SolidColorBrush(secondBg.Value);

            App.Current.Resources["PrimaryControlBackground"] = new SolidColorBrush(controlBg.Value);
            App.Current.Resources["PrimaryControlForeground"] = new SolidColorBrush(controlFg.Value);
            App.Current.Resources["PrimaryControlPressed"] = new SolidColorBrush(controlPressed.Value);
            App.Current.Resources["PrimaryControlMouseOver"] = new SolidColorBrush(controlMouseOver.Value);

            App.Current.Resources["PrimaryControlDisabledBackground"] = new SolidColorBrush(controlDisabledBg.Value);
            App.Current.Resources["PrimaryControlDisabledForeground"] = new SolidColorBrush(controlDisabledFg.Value);

            App.Current.Resources["iconColorBrush"] = new SolidColorBrush(controlFg.Value);
        }

        private void ApplyCustomTheme()
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

        private string ColorToHexString(Color color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }

        private string GetSavedThemeName()
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

            if (savedTheme == null)
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

        public ThemeSettingsViewModel()
        {
            StatusText = "";
            SelectedThemeText = GetSavedThemeName();
            InitColorTextInput();
            ThemeDropdownItems = new List<string>() { "Dark Mode", "Light Mode", "Custom" };
        }
    }
}
