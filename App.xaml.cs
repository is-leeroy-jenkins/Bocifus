// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 10-31-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        10-31-2024
// ******************************************************************************************
// <copyright file="App.xaml.cs" company="Terry D. Eppler">
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
//   App.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System.Diagnostics.CodeAnalysis;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using MessageBox = ModernWpf.MessageBox;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Application" />
    [ SuppressMessage( "ReSharper", "RedundantExtendsListEntry" ) ]
    public partial class App : Application
    {
        /// <summary>
        /// The mutex
        /// </summary>
        private Mutex _mutex;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup( StartupEventArgs e )
        {
            _mutex = new Mutex( true, "BocifusMutex", out var _isNewInstance );
            if( !_isNewInstance )
            {
                MessageBox.Show( "The application is already up and running." );
                _mutex = null;
                Current.Shutdown( );
            }

            DispatcherUnhandledException += ( s, args ) => HandleException( args.Exception );
            TaskScheduler.UnobservedTaskException += ( s, args ) =>
                HandleException( args.Exception?.InnerException );

            AppDomain.CurrentDomain.UnhandledException += ( s, args ) =>
                HandleException( args.ExceptionObject as Exception );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Exit" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
        protected override void OnExit( ExitEventArgs e )
        {
            _mutex?.ReleaseMutex( );
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="e">The e.</param>
        private void HandleException( Exception e )
        {
            if( e == null )
            {
                return;
            }

            Fail( e );
            Environment.Exit( 1 );
        }

        /// <summary>
        /// Shows the custom error dialog.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowCustomErrorDialog( string message )
        {
            var _errorWindow = new Window
            {
                Title = "Abnormal termination",
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var _textBox = new TextBox
            {
                Text = message,
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            _errorWindow.Content = _textBox;
            _errorWindow.ShowDialog( );
            Environment.Exit( 1 );
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