// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="ColorSettings.xaml.cs" company="Terry D. Eppler">
//   Bocifus is an open source windows (wpf) application that interacts with OpenAI GPT-3.5 Turbo API
//   based on NET6 and written in C-Sharp.
// 
//    Copyright ©  2020-2024 Terry D. Eppler
// 
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the “Software”),
//    to deal in the Software without restriction,
//    including without limitation the rights to use,
//    copy, modify, merge, publish, distribute, sublicense,
//    and/or sell copies of the Software,
//    and to permit persons to whom the Software is furnished to do so,
//    subject to the following conditions:
// 
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
// 
//    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT.
//    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
//    DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//    ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//    DEALINGS IN THE SOFTWARE.
// 
//    You can contact me at:  terryeppler@gmail.com or eppler.terry@epa.gov
// </copyright>
// <summary>
//   ColorSettings.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using ModernWpf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using Properties;
    using ElementTheme = SourceChord.FluentWPF.ElementTheme;
    using ResourceDictionaryEx = SourceChord.FluentWPF.ResourceDictionaryEx;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="T:System.Windows.Markup.IStyleConnector" />
    [ SuppressMessage( "ReSharper", "PossibleNullReferenceException" ) ]
    public partial class ColorSettings
    {
        /// <summary>
        /// Gets the known color names.
        /// </summary>
        /// <value>
        /// The known color names.
        /// </value>
        public static IReadOnlyList<string> KnownColorNames { get; } = typeof( Colors )
            .GetProperties( BindingFlags.Public | BindingFlags.Static )
            ?.Select( info => info.Name )
            .ToArray( );

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSettings"/> class.
        /// </summary>
        public ColorSettings( )
        {
            InitializeComponent( );
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if( ThemeManager.Current.AccentColor != null )
            {
                var _color = ThemeManager.Current.AccentColor;
            }
            else
            {
                var _color = ThemeManager.Current.ActualAccentColor;
            }

            OkFlg = false;
            AccentColorList.ItemsSource = KnownColorNames;
            BefTheme = ThemeManager.Current.ApplicationTheme;
            BefAccent = ThemeManager.Current.AccentColor;
            if( ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark )
            {
                ThemeDark.IsChecked = true;
            }
            else if( ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light )
            {
                ThemeLight.IsChecked = true;
            }
            else
            {
                ThemeSystem.IsChecked = true;
            }

            if( ThemeManager.Current.AccentColor == null )
            {
                AccentColorSystem.IsChecked = true;
            }
            else
            {
                AccentColorSet.IsChecked = true;
                AccentColorList.SelectedItem = Settings.Default.AccentColorName;
                AccentColorList.ScrollIntoView( AccentColorList.SelectedItem );
            }
        }

        /// <summary>
        /// Gets or sets the bef theme.
        /// </summary>
        /// <value>
        /// The bef theme.
        /// </value>
        public static ApplicationTheme? BefTheme { get; set; }

        /// <summary>
        /// Gets or sets the bef accent.
        /// </summary>
        /// <value>
        /// The bef accent.
        /// </value>
        public static Color? BefAccent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [ok FLG].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [ok FLG]; otherwise, <c>false</c>.
        /// </value>
        public static bool OkFlg { get; set; }

        /// <summary>
        /// Themes the load.
        /// </summary>
        private void ThemeLoad( )
        {
            var _theme = new ResourceDictionary( );
            var _themeSource = "Dark";
            if( ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light )
            {
                _themeSource = "Light";
            }

            var _url =
                $"pack://application:,,,/ModernWpf;component/ThemeResources/{_themeSource}.xaml";

            _theme.Source = new Uri( _url );
            Application.Current.Resources.MergedDictionaries.Add( _theme );
        }

        /// <summary>
        /// Called when [window closing].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        protected virtual void OnWindowClosing( object sender, CancelEventArgs e )
        {
            if( OkFlg == false )
            {
                ThemeManager.Current.ApplicationTheme = BefTheme;
                ThemeManager.Current.AccentColor = BefAccent;
            }
        }

        /// <summary>
        /// Called when [theme RadioButton click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnThemeRadioButtonClick( object sender, RoutedEventArgs e )
        {
            var _ctrl = sender as Control;
            if( _ctrl == ThemeLight )
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ResourceDictionaryEx.GlobalTheme = ElementTheme.Light;
            }
            else if( _ctrl == ThemeDark )
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = null;
                ResourceDictionaryEx.GlobalTheme = null;
            }

            ThemeLoad( );
        }

        /// <summary>
        /// Called when [ok button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnOkButtonClick( object sender, RoutedEventArgs e )
        {
            OkFlg = true;
            if( ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark )
            {
                Settings.Default.Theme = "Dark";
            }
            else if( ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light )
            {
                Settings.Default.Theme = "Light";
            }
            else
            {
                Settings.Default.Theme = "Default";
            }

            if( ThemeManager.Current.AccentColor == null )
            {
                Settings.Default.AccentColor = "Default";
            }
            else
            {
                Settings.Default.AccentColor = ThemeManager.Current.AccentColor.ToString( );
                Settings.Default.AccentColorName = AccentColorList.SelectedValue.ToString( );
            }

            DialogResult = true;
        }

        /// <summary>
        /// Called when [window key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnWindowKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.Escape )
            {
                DialogResult = false;
            }
        }

        /// <summary>
        /// Called when [accent color system checked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnAccentColorSystemChecked( object sender, RoutedEventArgs e )
        {
            ThemeManager.Current.AccentColor = null;
            AccentColorList.IsEnabled = false;
            ThemeLoad( );
        }

        /// <summary>
        /// Called when [accent color set checked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnAccentColorSetChecked( object sender, RoutedEventArgs e )
        {
            AccentColorList.IsEnabled = true;
            ThemeLoad( );
        }

        /// <summary>
        /// Called when [accent color list selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnAccentColorListSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            var _selection = AccentColorList?.SelectedValue?.ToString( );
            var _color = ( Color )ColorConverter.ConvertFromString( _selection );
            ThemeManager.Current.AccentColor = _color;
            ThemeLoad( );
        }
    }
}