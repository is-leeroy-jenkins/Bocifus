// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-09-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-09-2024
// ******************************************************************************************
// <copyright file="PromptInputWindow.xaml.cs" company="Terry D. Eppler">
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
//   PromptInputWindow.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.ComponentModel;
    using Model;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Syncfusion.SfSkinManager;
    using MessageBox = ModernWpf.MessageBox;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public partial class PromptInputWindow : IDisposable
    {
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
        /// Initializes a new instance of the <see cref="PromptInputWindow" /> class.
        /// </summary>
        public PromptInputWindow( )
        {
            InitializeComponent( );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptInputWindow" /> class.
        /// </summary>
        /// <param name="promptTemplate">The prompt template.</param>
        public PromptInputWindow( PromptTemplate promptTemplate )
        {
            InitializeComponent( );
            TargetTemplate = promptTemplate;
            Title = promptTemplate.Title;
            if( promptTemplate != null )
            {
                TitleTextBox.Text = promptTemplate.Title;
                DescriptionTextBox.Text = promptTemplate.Description;
                PromptTextBox.Text = promptTemplate.Prompt;
            }
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
                lock( _entry )
                {
                    return _busy;
                }
            }
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public PromptTemplate Result { get; set; }

        /// <summary>
        /// Gets or sets the target template.
        /// </summary>
        /// <value>
        /// The target template.
        /// </value>
        public PromptTemplate TargetTemplate { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <returns></returns>
        private bool ValidateInput()
        {
            if(string.IsNullOrWhiteSpace(TitleTextBox.Text)
                || string.IsNullOrWhiteSpace(PromptTextBox.Text))
            {
                MessageBox.Show("All fields are required.", "Input Error", MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when [window loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" />
        /// instance containing the event data.</param>
        private void OnWindowLoaded( object sender, RoutedEventArgs e )
        {
            if( Title != null )
            {
                PromptTextBox.Focus( );
                PromptTextBox.CaretIndex = PromptTextBox.Text.Length;
            }
            else
            {
                TitleTextBox.Focus( );
            }
        }

        /// <summary>
        /// Called when [ok button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" />
        /// instance containing the event data.</param>
        private void OnOkButtonClick( object sender, RoutedEventArgs e )
        {
            if( !ValidateInput( ) )
            {
                return;
            }

            Result = new PromptTemplate( )
            {
                Title = TitleTextBox.Text,
                Description = DescriptionTextBox.Text,
                Prompt = PromptTextBox.Text
            };

            DialogResult = true;
        }

        /// <summary>
        /// Called when [cancel button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" />
        /// instance containing the event data.</param>
        private void OnCancelButtonClick( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
        }

        /// <summary>
        /// Called when [window key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs" />
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
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs" />
        /// instance containing the event data.</param>
        private void OnWindowClosing( object sender, CancelEventArgs e )
        {
            DialogResult = DialogResult == true;
        }

        /// <summary>
        /// Called when [prompt text box preview mouse wheel].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs" />
        /// instance containing the event data.</param>
        private void OnPromptTextBoxPreviewMouseWheel( object sender, MouseWheelEventArgs e )
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

        /// <summary>
        /// Called when [delete button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" />
        /// instance containing the event data.</param>
        private void OnDeleteButtonClick( object sender, RoutedEventArgs e )
        {
            var result = MessageBox.Show( "Are you sure you want to delete this item?",
                "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning );

            if( result == MessageBoxResult.Yes )
            {
                if( TargetTemplate != null )
                {
                    var sortOrderToDelete = TargetTemplate.SortOrder;
                    AppSettings.PromptTemplateManager.Templates.Remove( TargetTemplate );
                    foreach( var item in AppSettings.PromptTemplateManager.Templates.Where( t =>
                        t.SortOrder > sortOrderToDelete ) )
                    {
                        item.SortOrder--;
                    }

                    DialogResult = false;
                }
            }
        }

        /// <summary>
        /// Called when [apply button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" />
        /// instance containing the event data.</param>
        private void OnApplyButtonClick( object sender, RoutedEventArgs e )
        {
            if( !ValidateInput( ) )
            {
                return;
            }

            Result = new PromptTemplate( )
            {
                Title = TitleTextBox.Text,
                Description = DescriptionTextBox.Text,
                Prompt = PromptTextBox.Text
            };

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if( !string.IsNullOrEmpty( mainWindow.UserTextBox.Text ) )
            {
                var _msg = "The text box already contains text. Do you want to replace it?";
                var result = MessageBox.Show( _msg, "Confirm Replace", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning );

                if( result == MessageBoxResult.Yes )
                {
                    mainWindow.UserTextBox.Text = PromptTextBox.Text;
                }
                else
                {
                    return;
                }
            }
            else
            {
                mainWindow.UserTextBox.Text = PromptTextBox.Text;
            }

            DialogResult = true;
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