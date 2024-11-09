// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-07-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-07-2024
// ******************************************************************************************
// <copyright file="LargeUserTextInput.xaml.cs" company="Terry D. Eppler">
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
//    You can contact me at: terryeppler@gmail.com or eppler.terry@epa.gov
// </copyright>
// <summary>
//   LargeUserTextInput.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Syncfusion.SfSkinManager;
    using ToastNotifications;
    using ToastNotifications.Lifetime;
    using ToastNotifications.Messages;
    using ToastNotifications.Position;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    [ SuppressMessage( "ReSharper", "ClassCanBeSealed.Global" ) ]
    [ SuppressMessage( "ReSharper", "PossibleNullReferenceException" ) ]
    public partial class TextInputWindow : IDisposable
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
        /// The stop watch
        /// </summary>
        private protected readonly Stopwatch _stopWatch = new Stopwatch();

        /// <summary>
        /// Creates new title.
        /// </summary>
        /// <value>
        /// The new title.
        /// </value>
        public string NewTitle { get; private set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TextInputWindow"/> class.
        /// </summary>
        public TextInputWindow( )
        {
            InitializeComponent( );
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TextInputWindow"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public TextInputWindow( string text )
        {
            InitializeComponent( );
            UserLargeTextBox.Text = text;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is busy.
        /// </summary>
        /// <value>
        /// <c> true </c>
        /// if this instance is busy; otherwise,
        /// <c> false </c>
        /// </value>
        public bool IsBusy
        {
            get
            {
                lock(_entry)
                {
                    return _busy;
                }
            }
        }

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

                return new Notifier( _cfg =>
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
        /// Called when [window loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowLoaded( object sender, RoutedEventArgs e )
        {
            UserLargeTextBox.Focus( );
            UserLargeTextBox.CaretIndex = UserLargeTextBox.Text.Length;
        }

        /// <summary>
        /// Called when [ok button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnOkButtonClick( object sender, RoutedEventArgs e )
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            // ReSharper disable once PossibleNullReferenceException
            mainWindow.UserTextBox.Text = UserLargeTextBox.Text;
            DialogResult = true;
        }

        /// <summary>
        /// Called when [cancel button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnCancelButtonClick( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
        }

        /// <summary>
        /// Called when [window key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.Escape )
            {
                DialogResult = false;
            }

            if( e.Key == Key.Enter
                && ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
            {
                OnOkButtonClick( sender, e );
            }
        }

        /// <summary>
        /// Called when [window closing].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowClosing( object sender, CancelEventArgs e )
        {
            DialogResult = DialogResult == true;
        }

        /// <summary>
        /// Called when [user large text box preview mouse wheel].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/>
        /// instance containing the event data.</param>
        private void OnUserLargeTextBoxPreviewMouseWheel( object sender, MouseWheelEventArgs e )
        {
            if( Keyboard.Modifiers == ModifierKeys.Control )
            {
                var textBox = sender as TextBox;
                if( e.Delta > 0
                    && textBox.FontSize < 40 )
                {
                    textBox.FontSize += 2;
                }
                else if( e.Delta < 0
                    && textBox.FontSize > 10 )
                {
                    textBox.FontSize -= 2;
                }

                e.Handled = true;
            }
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