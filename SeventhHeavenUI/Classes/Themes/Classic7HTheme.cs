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
    public class Classic7HTheme : ITheme
    {
        public string Name { get => "Classic 7H"; }

        public string PrimaryAppBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("ClassicBackgroundColor") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string SecondaryAppBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("ClassicSecondaryBackgroundColor") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("ClassicControlBackground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlForeground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("ClassicControlForeground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlPressed
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("ClassicControlPressed") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlMouseOver
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("ClassicControlMouseOver") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlDisabledBackground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("ClassicControlDisabledBackground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string PrimaryControlDisabledForeground
        {
            get
            {
                Color? colorResource = App.Current.TryFindResource("ClassicControlDisabledForeground") as Color?;
                return ThemeSettings.ColorToHexString(colorResource.Value);
            }
        }

        public string BackgroundImageName { get => null; }
        public string BackgroundImageBase64 { get => null; }
    }
}
