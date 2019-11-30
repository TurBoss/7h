using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SeventhHeaven.Classes
{
    /// <summary>
    /// Button that has a UriSource property to include an icon
    /// </summary>
    public class ButtonWithImage : Button
    {
        static ButtonWithImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonWithImage), new FrameworkPropertyMetadata(typeof(ButtonWithImage)));
        }

        #region Public properties

        public Uri UriSource
        {
            get { return (Uri)GetValue(UriSourceProperty); }
            set { SetValue(UriSourceProperty, value); }
        }

        public ControlTemplate TemplateSource
        {
            get { return (ControlTemplate)GetValue(TemplateSourceProperty); }
            set { SetValue(TemplateSourceProperty, value); }
        }

        #endregion


        #region Dependency Properties


        public static readonly DependencyProperty UriSourceProperty =
            DependencyProperty.Register(
            "UriSource",
            typeof(Uri),
            typeof(ButtonWithImage),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty TemplateSourceProperty =
            DependencyProperty.Register(
            "TemplateSource",
            typeof(ControlTemplate),
            typeof(ButtonWithImage),
            new FrameworkPropertyMetadata(null));

        #endregion
    }
}
