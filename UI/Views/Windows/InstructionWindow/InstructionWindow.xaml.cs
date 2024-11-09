// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="InstructionWindow.xaml.cs" company="Terry D. Eppler">
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
//   InstructionWindow.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using ModernWpf;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using static MainWindow;
    using HorizontalAlignment = System.Windows.HorizontalAlignment;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using MessageBox = ModernWpf.MessageBox;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    public partial class InstructionWindow
    {
        /// <summary>
        /// Gets the input result.
        /// </summary>
        /// <value>
        /// The input result.
        /// </value>
        public string[ , ] InputResult
        {
            get
            {
                return Items;
            }
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        private string[ , ] Items { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstructionWindow"/> class.
        /// </summary>
        /// <param name="param">The parameter.</param>
        public InstructionWindow( string[ , ] param )
        {
            InitializeComponent( );
            Items = param;
            var _accentColor = ThemeManager.Current.AccentColor;
            if( _accentColor == null )
            {
                _accentColor = SystemParameters.WindowGlassColor;
            }

            var _accentColorBrush = new SolidColorBrush( ( Color )_accentColor );
            SaveButton.Background = _accentColorBrush;
            InstructionListBox.ContextMenu = new ContextMenu( );
            var _upSwap = new MenuItem
            {
                Header = "⬆"
            };

            _upSwap.Click += OnUpSwapClick;
            _upSwap.HorizontalAlignment = HorizontalAlignment.Center;
            var _downSwap = new MenuItem
            {
                Header = "⬇"
            };

            _downSwap.Click += OnDownSwapClick;
            _downSwap.HorizontalAlignment = HorizontalAlignment.Center;
            InstructionListBox.ContextMenu.Items.Add( _upSwap );
            InstructionListBox.ContextMenu.Items.Add( _downSwap );
            if( Items == null )
            {
                Items = new string[ 1, 2 ];
                Items[ 0, 0 ] = "";
                Items[ 0, 1 ] = "";
            }

            for( var _i = 0; _i < Items.GetLength( 0 ); _i++ )
            {
                InstructionListBox.Items.Add( Items[ _i, 0 ] );
            }

            for( var _i = 0; _i < Items.GetLength( 0 ); _i++ )
            {
                if( AppSettings.InstructionSetting == ""
                    || AppSettings.InstructionSetting == null )
                {
                    InstructionListBox.SelectedIndex = 0;
                    break;
                }

                if( Items[ _i, 0 ] == AppSettings.InstructionSetting )
                {
                    InstructionListBox.SelectedIndex = _i;
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the instruction ListBox.
        /// </summary>
        private void UpdateInstructionListBox( )
        {
            InstructionListBox.Items.Clear( );
            for( var _i = 0; _i < Items.GetLength( 0 ); _i++ )
            {
                InstructionListBox.Items.Add( Items[ _i, 0 ] );
            }
        }

        /// <summary>
        /// Duplicates the control.
        /// </summary>
        private void DuplicateControl( )
        {
            for( var _i = 0; _i < Items.GetLength( 0 ); _i++ )
            {
                var _currentName = Items[ _i, 0 ];
                for( var _j = 0; _j < Items.GetLength( 0 ); _j++ )
                {
                    if( _i == _j )
                    {
                        continue;
                    }

                    if( Items[ _i, 0 ] == Items[ _j, 0 ] )
                    {
                        _currentName += "*";
                        Items[ _j, 0 ] = _currentName;
                    }
                }
            }
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        private void Save( )
        {
            if( InstructionListBox.SelectedIndex == -1 )
            {
                return;
            }

            if( InstructionTextBox.Text == "" )
            {
                var _msg = "The instruction name has not been entered.";
                MessageBox.Show( _msg, "Error", MessageBoxButton.OK );
                return;
            }

            var _index = InstructionListBox.SelectedIndex;
            Items[ _index, 0 ] = InstructionTextBox.Text;
            Items[ _index, 1 ] = ContentsTextBox.Text;
            DuplicateControl( );
            UpdateInstructionListBox( );
            InstructionListBox.SelectedIndex = _index;
        }

        /// <summary>
        /// Called when [ListView selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnListViewSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if( InstructionListBox.SelectedItem == null )
            {
                return;
            }

            if( InstructionListBox.SelectedIndex == -1 )
            {
                return;
            }

            InstructionTextBox.Text = Items[ InstructionListBox.SelectedIndex, 0 ];
            ContentsTextBox.Text = Items[ InstructionListBox.SelectedIndex, 1 ];
        }

        /// <summary>
        /// Called when [save button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnSaveButtonClick( object sender, RoutedEventArgs e )
        {
            Save( );
        }

        /// <summary>
        /// Called when [close button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnCloseButtonClick( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
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
                DialogResult = false;
            }
        }

        /// <summary>
        /// Called when [text box key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnTextBoxKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.Enter
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                Save( );
            }

            if( e.Key == Key.S
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                Save( );
            }
        }

        /// <summary>
        /// Called when [add button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnAddButtonClick( object sender, RoutedEventArgs e )
        {
            var _newItems = new string[ Items.GetLength( 0 ) + 1, 2 ];
            for( var _i = 0; _i < Items.GetLength( 0 ); _i++ )
            {
                _newItems[ _i, 0 ] = Items[ _i, 0 ];
                _newItems[ _i, 1 ] = Items[ _i, 1 ];
            }

            _newItems[ Items.GetLength( 0 ), 0 ] = "";
            _newItems[ Items.GetLength( 0 ), 1 ] = "";
            Items = _newItems;
            UpdateInstructionListBox( );
            InstructionListBox.SelectedIndex = Items.GetLength( 0 ) - 1;
        }

        /// <summary>
        /// Called when [remove button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnRemoveButtonClick( object sender, RoutedEventArgs e )
        {
            var _index = InstructionListBox.SelectedIndex;
            if( _index == -1 )
            {
                return;
            }

            var _newItems = new string[ Items.GetLength( 0 ) - 1, 2 ];
            for( var _i = 0; _i < _index; _i++ )
            {
                _newItems[ _i, 0 ] = Items[ _i, 0 ];
                _newItems[ _i, 1 ] = Items[ _i, 1 ];
            }

            for( var _i = _index + 1; _i < Items.GetLength( 0 ); _i++ )
            {
                _newItems[ _i - 1, 0 ] = Items[ _i, 0 ];
                _newItems[ _i - 1, 1 ] = Items[ _i, 1 ];
            }

            Items = _newItems;
            UpdateInstructionListBox( );
            InstructionListBox.SelectedIndex = Items.GetLength( 0 ) - 1;
        }

        /// <summary>
        /// Swaps the items.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="isUp">if set to <c>true</c> [is up].</param>
        private void SwapItems( int index, bool isUp )
        {
            if( index == -1
                || ( isUp && index == 0 )
                || ( !isUp && index == Items.GetLength( 0 ) - 1 ) )
            {
                return;
            }

            var _newIndex = isUp
                ? index - 1
                : index + 1;

            var _newItems = new string[ Items.GetLength( 0 ), 2 ];
            for( var _i = 0; _i < Items.GetLength( 0 ); _i++ )
            {
                if( _i == index )
                {
                    _newItems[ _newIndex, 0 ] = Items[ _i, 0 ];
                    _newItems[ _newIndex, 1 ] = Items[ _i, 1 ];
                }
                else if( _i == _newIndex )
                {
                    _newItems[ index, 0 ] = Items[ _i, 0 ];
                    _newItems[ index, 1 ] = Items[ _i, 1 ];
                }
                else
                {
                    _newItems[ _i, 0 ] = Items[ _i, 0 ];
                    _newItems[ _i, 1 ] = Items[ _i, 1 ];
                }
            }

            Items = _newItems;
            UpdateInstructionListBox( );
            InstructionListBox.SelectedIndex = _newIndex;
        }

        /// <summary>
        /// Ups the swap.
        /// </summary>
        private void UpSwap( )
        {
            var _index = InstructionListBox.SelectedIndex;
            SwapItems( _index, true );
        }

        /// <summary>
        /// Downs the swap.
        /// </summary>
        private void DownSwap( )
        {
            var _index = InstructionListBox.SelectedIndex;
            SwapItems( _index, false );
        }

        /// <summary>
        /// Called when [up swap click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnUpSwapClick( object sender, RoutedEventArgs e )
        {
            UpSwap( );
        }

        /// <summary>
        /// Called when [down swap click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnDownSwapClick( object sender, RoutedEventArgs e )
        {
            DownSwap( );
        }

        /// <summary>
        /// Called when [instruction ListBox key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnInstructionListBoxKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.K )
            {
                UpSwap( );
            }

            if( e.Key == Key.J )
            {
                DownSwap( );
            }
        }

        /// <summary>
        /// Called when [export button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnExportButtonClick( object sender, RoutedEventArgs e )
        {
            try
            {
                var _json = JsonConvert.SerializeObject( Items );
                _json = JToken.Parse( _json ).ToString( Formatting.Indented );
                var _dialog = new SaveFileDialog( );
                _dialog.Title = "Please select an export file.";
                _dialog.FileName = DateTime.Now.ToString( "yyyyMMdd" ) + "_SystemPrompt.json";
                _dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                _dialog.DefaultExt = "json";
                var _result = _dialog.ShowDialog( );
                if( _result == System.Windows.Forms.DialogResult.OK )
                {
                    File.WriteAllText( _dialog.FileName, _json );
                    MessageBox.Show( "Exported successfully." );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        /// <summary>
        /// Called when [import button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnImportButtonClick( object sender, RoutedEventArgs e )
        {
            try
            {
                var _msg = "Overwrite with the contents of the selected json file. Are you sure?";
                var _okFlg = MessageBox.Show( _msg, "Question", MessageBoxButton.YesNo );
                if( _okFlg == MessageBoxResult.Yes )
                {
                    var _dialog = new OpenFileDialog( );
                    _dialog.Title = "Please select a json file.";
                    _dialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                    _dialog.FilterIndex = 1;
                    _dialog.RestoreDirectory = true;
                    var _result = _dialog.ShowDialog( );
                    if( _result == System.Windows.Forms.DialogResult.OK )
                    {
                        var _path = _dialog.FileName;
                        var _json = File.ReadAllText( _path );
                        Items = JsonConvert.DeserializeObject<string[ , ]>( _json );
                        UpdateInstructionListBox( );
                        InstructionListBox.SelectedIndex = Items.GetLength( 0 ) - 1;
                        MessageBox.Show( "Imported successfully." );
                    }
                }

                DuplicateControl( );
                UpdateInstructionListBox( );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }
    }
}