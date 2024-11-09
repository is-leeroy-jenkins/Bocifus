// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-09-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-09-2024
// ******************************************************************************************
// <copyright file="WebBrowserWindow.xaml.cs" company="Terry D. Eppler">
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
//   WebBrowserWindow.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using Microsoft.Web.WebView2.Core;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Syncfusion.SfSkinManager;
    using ToastNotifications;
    using ToastNotifications.Lifetime;
    using ToastNotifications.Messages;
    using ToastNotifications.Position;
    using Application = System.Windows.Application;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using MessageBox = ModernWpf.MessageBox;
    using Timer = System.Threading.Timer;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class WebBrowserWindow : IDisposable

    {
        /// <summary>
        /// The HTML
        /// </summary>
        private string html;

        /// <summary>
        /// The busy
        /// </summary>
        private protected bool _busy;

        /// <summary>
        /// The path
        /// </summary>
        private protected object _entry = new object( );

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
        private protected readonly DarkMode _theme = new DarkMode( );

        /// <summary>
        /// The stop watch
        /// </summary>
        private protected readonly Stopwatch _stopWatch = new Stopwatch( );

        /// <summary>
        /// Initializes a new instance of the <see cref="WebBrowserWindow"/> class.
        /// </summary>
        public WebBrowserWindow( )
        {
            InitializeComponent( );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebBrowserWindow"/> class.
        /// </summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        public WebBrowserWindow( string htmlContent )
        {
            InitializeComponent( );
            html = htmlContent;
            Loaded += async ( sender, e ) =>
            {
                await WebView.EnsureCoreWebView2Async( null );
                WebView.CoreWebView2.NavigateToString( htmlContent );
            };
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
        private Notifier CreateNotifier( )
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
        /// Called when [window key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.Escape )
            {
                Close( );
            }

            if( e.Key == Key.Enter
                && ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
            {
                OnToggleEditModeClick( sender, e );
            }
        }

        /// <summary>
        /// Called when [core web view initialization completed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CoreWebView2InitializationCompletedEventArgs"/> instance containing the event data.</param>
        private void OnCoreWebViewInitializationCompleted( object sender,
            CoreWebView2InitializationCompletedEventArgs e )
        {
            WebView.CoreWebView2.Settings.IsZoomControlEnabled = true;
        }

        /// <summary>
        /// Called when [save HTML button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnSaveHtmlButtonClick( object sender, RoutedEventArgs e )
        {
            SaveHtmlToFile( );
        }

        /// <summary>
        /// Saves the HTML to file.
        /// </summary>
        private void SaveHtmlToFile( )
        {
            var htmlContent = html;
            var dateTimeSuffix = DateTime.Now.ToString( "yyyyMMddHHmmss" );
            var saveFileDialog = new SaveFileDialog( );
            saveFileDialog.Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "html";
            saveFileDialog.FileName = $"MermaidDiagram_{dateTimeSuffix}.html";
            var result = saveFileDialog.ShowDialog( );
            if( result == System.Windows.Forms.DialogResult.OK )
            {
                File.WriteAllText( saveFileDialog.FileName, htmlContent );
                MessageBox.Show( "Exported successfully." );
            }
        }

        /// <summary>
        /// The is in edit mode
        /// </summary>
        private bool isInEditMode;

        /// <summary>
        /// Called when [toggle edit mode click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnToggleEditModeClick( object sender, RoutedEventArgs e )
        {
            if( isInEditMode )
            {
                html = EditTextBox.Text;
                WebView.Visibility = Visibility.Visible;
                EditTextBox.Visibility = Visibility.Collapsed;
                WebView.CoreWebView2.NavigateToString( EditTextBox.Text );
                isInEditMode = false;
            }
            else
            {
                WebView.Visibility = Visibility.Collapsed;
                EditTextBox.Visibility = Visibility.Visible;
                EditTextBox.Text = html;
                isInEditMode = true;
            }
        }

        /// <summary>
        /// Called when [zoom in button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnZoomInButtonClick( object sender, RoutedEventArgs e )
        {
            WebView.ZoomFactor *= 1.1;
        }

        /// <summary>
        /// Called when [zoom out button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnZoomOutButtonClick( object sender, RoutedEventArgs e )
        {
            WebView.ZoomFactor /= 1.1;
        }

        /// <summary>
        /// Called when [window closing].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowClosing( object sender, CancelEventArgs e )
        {
        }

        /// <summary>
        /// Called when [window loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowLoaded( object sender, RoutedEventArgs e )
        {
        }
        
        /// <summary>
        /// Called when [calculator menu option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// instance containing the event data.</param>
        private void OnCalculatorMenuOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                var _calculator = new CalculatorWindow( );
                _calculator.ShowDialog( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [file menu option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// instance containing the event data.</param>
        private void OnFileMenuOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                var _fileBrowser = new FileBrowser
                {
                    Owner = this,
                    Topmost = true
                };

                _fileBrowser.Show( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [folder menu option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// instance containing the event data.</param>
        private void OnFolderMenuOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                var _fileBrowser = new FileBrowser
                {
                    Owner = this,
                    Topmost = true
                };

                _fileBrowser.Show( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [control panel option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// instance containing the event data.</param>
        private void OnControlPanelOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                WinMinion.LaunchControlPanel( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [task manager option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// instance containing the event data.</param>
        private void OnTaskManagerOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                WinMinion.LaunchTaskManager( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [close option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// instance containing the event data.</param>
        private void OnCloseOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                Application.Current.Shutdown( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [chrome option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// containing the event data.</param>
        private void OnChromeOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                WebMinion.RunChrome( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [edge option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// instance containing the event data.</param>
        private void OnEdgeOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                WebMinion.RunEdge( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Called when [firefox option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" />
        /// containing the event data.</param>
        private void OnFirefoxOptionClick( object sender, RoutedEventArgs e )
        {
            try
            {
                WebMinion.RunFirefox( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks
        /// associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose( )
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c>
        /// to release both managed
        /// and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose( bool disposing )
        {
            if( disposing )
            {
                SfSkinManager.Dispose( this );
                _timer?.Dispose( );
            }
        }

        /// <summary>
        /// Fails the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private protected void Fail( Exception ex )
        {
            var _error = new ErrorWindow( ex );
            _error?.SetText( );
            _error?.ShowDialog( );
        }
    }
}