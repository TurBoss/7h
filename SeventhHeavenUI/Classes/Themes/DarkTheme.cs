using SeventhHeaven.ViewModels;
using SeventhHeavenUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SeventhHeaven.Classes.Themes
{
    public class DarkTheme : ITheme
    {
        public string Name { get => "Dark Mode"; }

        public string PrimaryAppBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkBackgroundColor") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string SecondaryAppBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("MedDarkBackgroundColor") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlBackground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlForeground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlForeground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlPressed
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlPressed") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlMouseOver
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlMouseOver") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlDisabledBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlDisabledBackground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlDisabledForeground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("DarkControlDisabledForeground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }
    }
}
