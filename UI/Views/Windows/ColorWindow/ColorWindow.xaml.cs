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
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using Properties;
    using Syncfusion.SfSkinManager;
    using ToastNotifications;
    using ToastNotifications.Lifetime;
    using ToastNotifications.Messages;
    using ToastNotifications.Position;
    using ElementTheme = SourceChord.FluentWPF.ElementTheme;
    using ResourceDictionaryEx = SourceChord.FluentWPF.ResourceDictionaryEx;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="T:System.Windows.Markup.IStyleConnector" />
    [ SuppressMessage( "ReSharper", "PossibleNullReferenceException" ) ]
    public partial class ColorWindow
    {
        /// <summary>
        /// The busy
        /// </summary>
        private protected bool _busy;

        /// <summary>
        /// The path
        /// </summary>
        private protected object _entry = new object();

        /// <summary>
        /// The seconds
        /// </summary>
        private protected int _seconds;

        /// <summary>
        /// The time
        /// </summary>
        private protected int _time;

        /// <summary>
        /// The timer
        /// </summary>
        private protected Timer _timer;

        /// <summary>
        /// The timer
        /// </summary>
        private protected TimerCallback _timerCallback;

        /// <summary>
        /// The status update
        /// </summary>
        private protected Action _statusUpdate;

        /// <summary>
        /// The theme
        /// </summary>
        private protected readonly DarkMode _theme = new DarkMode();

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
        /// Initializes a new instance of the <see cref="ColorWindow"/> class.
        /// </summary>
        public ColorWindow( )
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
                DarkThemeRadioButton.IsChecked = true;
            }
            else if( ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light )
            {
                LightThemeRadioButton.IsChecked = true;
            }
            else
            {
                ThemeSystemRadioButton.IsChecked = true;
            }

            if( ThemeManager.Current.AccentColor == null )
            {
                SystemAccentRadioButton.IsChecked = true;
            }
            else
            {
                SetColorAccentRadioButton.IsChecked = true;
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
        /// Invokes if needed.
        /// </summary>
        /// <param name="action">The action.</param>
        private void InvokeIf(Action action)
        {
            try
            {
                ThrowIf.Null(action, nameof(action));
                if(Dispatcher.CheckAccess())
                {
                    action?.Invoke();
                }
                else
                {
                    Dispatcher.BeginInvoke(action);
                }
            }
            catch(Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Invokes if.
        /// </summary>
        /// <param name="action">The action.</param>
        private void InvokeIf(Action<object> action)
        {
            try
            {
                ThrowIf.Null(action, nameof(action));
                if(Dispatcher.CheckAccess())
                {
                    action?.Invoke(null);
                }
                else
                {
                    Dispatcher.BeginInvoke(action);
                }
            }
            catch(Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Begins the initialize.
        /// </summary>
        private void Busy()
        {
            try
            {
                lock(_entry)
                {
                    _busy = true;
                }
            }
            catch(Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Ends the initialize.
        /// </summary>
        private void Chill()
        {
            try
            {
                lock(_entry)
                {
                    _busy = false;
                }
            }
            catch(Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Creates a notifier.
        /// </summary>
        /// <returns>
        /// Notifier
        /// </returns>
        private Notifier CreateNotifier()
        {
            try
            {
                var _position = new PrimaryScreenPositionProvider(Corner.BottomRight, 10, 10);
                var _lifeTime = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(5),
                    MaximumNotificationCount.UnlimitedNotifications());

                return new Notifier(_cfg =>
                {
                    _cfg.LifetimeSupervisor = _lifeTime;
                    _cfg.PositionProvider = _position;
                    _cfg.Dispatcher = Application.Current.Dispatcher;
                });
            }
            catch(Exception ex)
            {
                Fail(ex);
                return default(Notifier);
            }
        }

        /// <summary>
        /// Sends the notification.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SendNotification(string message)
        {
            try
            {
                ThrowIf.Null(message, nameof(message));
                var _notification = CreateNotifier();
                _notification.ShowInformation(message);
            }
            catch(Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Shows the splash message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SendMessage(string message)
        {
            try
            {
                ThrowIf.Null(message, nameof(message));
                var _splash = new SplashMessage(message)
                {
                    Owner = this
                };

                _splash.Show();
            }
            catch(Exception ex)
            {
                Fail(ex);
            }
        }

        /// <summary>
        /// Themes the load.
        /// </summary>
        private void LoadTheme( )
        {
            var _item = new ResourceDictionary( );
            var _themeSource = "Dark";
            if( ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light )
            {
                _themeSource = "Light";
            }

            var _url =
                $"pack://application:,,,/ModernWpf;component/ThemeResources/{_themeSource}.xaml";

            _item.Source = new Uri( _url );
            Application.Current.Resources.MergedDictionaries.Add( _item );
        }

        /// <summary>
        /// Called when [window closing].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/>
        /// instance containing the event data.</param>
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
            if( _ctrl == LightThemeRadioButton )
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                ResourceDictionaryEx.GlobalTheme = ElementTheme.Light;
            }
            else if( _ctrl == DarkThemeRadioButton )
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                ResourceDictionaryEx.GlobalTheme = ElementTheme.Dark;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = null;
                ResourceDictionaryEx.GlobalTheme = null;
            }

            LoadTheme( );
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
            LoadTheme( );
        }

        /// <summary>
        /// Called when [accent color set checked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnAccentColorSetChecked( object sender, RoutedEventArgs e )
        {
            AccentColorList.IsEnabled = true;
            LoadTheme( );
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
            LoadTheme( );
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks
        /// associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c>
        /// to release both managed
        /// and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                SfSkinManager.Dispose(this);
                _timer?.Dispose();
            }
        }

        /// <summary>
        /// Fails the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private protected void Fail(Exception ex)
        {
            var _error = new ErrorWindow(ex);
            _error?.SetText();
            _error?.ShowDialog();
        }
    }
}