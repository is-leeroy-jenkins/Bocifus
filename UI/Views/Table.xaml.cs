// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="Table.xaml.cs" company="Terry D. Eppler">
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
//   Table.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System.Windows.Controls.Primitives;
    using Model;
    using ModernWpf;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OpenAI.ObjectModels.RequestModels;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Unicode;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using ModernWpf.Controls;
    using Application = System.Windows.Application;
    using Binding = System.Windows.Data.Binding;
    using ComboBox = System.Windows.Controls.ComboBox;
    using JsonSerializer = System.Text.Json.JsonSerializer;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using MessageBox = ModernWpf.MessageBox;
    using TextBox = System.Windows.Controls.TextBox;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class Table
    {
        /// <summary>
        /// Gets the updated conversation history.
        /// </summary>
        /// <value>
        /// The updated conversation history.
        /// </value>
        public ConversationHistory UpdatedConversationHistory { get; private set; }

        /// <summary>
        /// The view model
        /// </summary>
        private ViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="conversationHistory">The conversation history.</param>
        public Table( ConversationHistory conversationHistory )
        {
            InitializeComponent( );
            _viewModel = new ViewModel( );
            DataContext = _viewModel;
            _viewModel.ComboBoxItems.Add( "user" );
            _viewModel.ComboBoxItems.Add( "assistant" );
            var _list = new ObservableCollection<DataTableItem>( );
            foreach( var _message in conversationHistory.Messages )
            {
                var _result = UtilityFunctions.ExtractUserAndImageFromMessage( _message.Content );
                _list.Add( new DataTableItem( )
                {
                    Role = _message.Role,
                    Content = _result.userMessage,
                    ImageUrl = _result.image
                } );
            }

            DataTable.ItemsSource = _list;
            if( ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light )
            {
                Brush _brush =
                    new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( "#19000000" ) );

                DataTable.AlternatingRowBackground = _brush;
            }
            else
            {
                Brush _brush =
                    new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( "#19FFFFFF" ) );

                DataTable.AlternatingRowBackground = _brush;
            }

            SetHistoryCountButton.Content =
                $"Set Number of Past Conversations: {AppSettings.ConversationHistoryCountSetting}";

            var _accentColor = ThemeManager.Current.AccentColor;
            if( _accentColor == null )
            {
                _accentColor = SystemParameters.WindowGlassColor;
            }

            var _accentColorBrush = new SolidColorBrush( ( Color )_accentColor );
            SaveButton.Background = _accentColorBrush;
        }

        /// <summary>
        /// Called when [save button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnSaveButtonClick( object sender, RoutedEventArgs e )
        {
            var _list = ( ObservableCollection<DataTableItem> )DataTable.ItemsSource;
            UpdatedConversationHistory = new ConversationHistory( );
            foreach( var _item in _list )
            {
                if( _item.Role == "user" )
                {
                    var _options = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.Create( UnicodeRanges.All )
                    };

                    var _contentJson = JsonSerializer.Serialize( new List<VisionUserContentItem>
                    {
                        new VisionUserContentItem
                        {
                            type = "text",
                            text = _item.Content
                        },
                        new VisionUserContentItem
                        {
                            type = "image_url",
                            image_url = new Image_Url
                            {
                                url = _item.ImageUrl,
                                detail = "auto"
                            }
                        }
                    }, _options );

                    UpdatedConversationHistory.Messages.Add( new ChatMessage( _item.Role,
                        _contentJson ) );
                }
                else
                {
                    UpdatedConversationHistory.Messages.Add( new ChatMessage( _item.Role,
                        _item.Content ) );
                }
            }

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
        /// Called when [window closing].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowClosing( object sender, CancelEventArgs e )
        {
            DialogResult = DialogResult == true;
        }

        /// <summary>
        /// Called when [data grid automatic generating column].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataGridAutoGeneratingColumnEventArgs"/>
        /// instance containing the event data.</param>
        private void OnDataGridAutoGeneratingColumn( object sender,
            DataGridAutoGeneratingColumnEventArgs e )
        {
            ( ( DataGridTextColumn )e.Column ).EditingElementStyle =
                ( Style )Resources[ "editingTextBoxStyle" ];
        }

        /// <summary>
        /// Called when [editing text box key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnEditingTextBoxKeyDown( object sender, KeyEventArgs e )
        {
            if( Key.Return == e.Key
                && 0 < ( ModifierKeys.Shift & e.KeyboardDevice.Modifiers ) )
            {
                var _tb = ( TextBox )sender;
                var _caret = _tb.CaretIndex;
                _tb.Text = _tb.Text.Insert( _caret, "\r\n" );
                _tb.CaretIndex = _caret + 1;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Called when [data table loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnDataTableLoaded( object sender, RoutedEventArgs e )
        {
            if( DataTable.Columns.Count > 0 )
            {
                DataTable.Columns[ 1 ].SetValue( DataGridBoundColumn.ElementStyleProperty,
                    new Style( typeof( TextBlock ) )
                    {
                        Setters =
                        {
                            new Setter( TextBlock.TextWrappingProperty, TextWrapping.Wrap ),
                            new Setter( TextBlock.PaddingProperty, new Thickness( 5, 5, 5, 5 ) )
                        }
                    } );

                DataTable.Columns[ 1 ].Width =
                    new DataGridLength( 1.0, DataGridLengthUnitType.Star );

                DataTable.Columns[ 2 ].Width =
                    new DataGridLength( 1.0, DataGridLengthUnitType.SizeToHeader );

                var _comboBoxColumn = new DataGridTemplateColumn
                {
                    Header = DataTable.Columns[ 0 ].Header
                };

                var _comboBoxFactory = new FrameworkElementFactory( typeof( ComboBox ) );
                var _itemsSourceBinding = new Binding
                {
                    Path = new PropertyPath( "ComboBoxItems" ),
                    Mode = BindingMode.OneWay,
                    Source = _viewModel
                };

                _comboBoxFactory.SetBinding( ItemsControl.ItemsSourceProperty,
                    _itemsSourceBinding );

                var _selectedItemBinding = new Binding
                {
                    Path = new PropertyPath( "Role" ),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                _comboBoxFactory.SetValue( Selector.SelectedItemProperty, _selectedItemBinding );
                _comboBoxFactory.SetValue( WidthProperty, 100.0 );
                var _cellTemplate = new DataTemplate
                {
                    VisualTree = _comboBoxFactory
                };

                _comboBoxColumn.CellTemplate = _cellTemplate;
                DataTable.Columns.RemoveAt( 0 );
                DataTable.Columns.Insert( 0, _comboBoxColumn );
            }

            var _contextMenu = new ContextMenu( );
            var _menuItem = new MenuItem
            {
                Header = "Add new row after selected row",
                Icon = new SymbolIcon( Symbol.Add )
            };

            _menuItem.Click += OnAddNewRowBeforeSelectedClick;
            _contextMenu.Items.Add( _menuItem );
            var _deleteMenuItem = new MenuItem
            {
                Header = "Delete selected row",
                Icon = new SymbolIcon( Symbol.Delete )
            };

            _deleteMenuItem.Click += OnDeleteSelectedRowClick;
            _contextMenu.Items.Add( _deleteMenuItem );
            DataTable.ContextMenu = _contextMenu;
        }

        /// <summary>
        /// Called when [export button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnExportButtonClick( object sender, RoutedEventArgs e )
        {
            if( DataTable.Items.Count <= 1 )
            {
                MessageBox.Show( "No conversation history." );
                return;
            }

            var _dialog = new SaveFileDialog( );
            _dialog.Title = "Please select an export file.";
            var _fileName = DateTime.Now.ToString( "yyyyMMdd" ) + "_";
            if( ( ( DataTableItem )DataTable.Items[ 0 ] ).Content.Length < 20 )
            {
                var _item = ( ( DataTableItem )DataTable.Items[ 0 ] ).Content.Substring( 0,
                    ( ( DataTableItem )DataTable.Items[ 0 ] ).Content.Length );

                _fileName += _item.Replace( "/", "" ).Replace( ":", "" );
            }
            else
            {
                var _name = ( ( DataTableItem )DataTable.Items[ 0 ] ).Content.Substring( 0, 20 );
                _fileName += _name.Replace( "/", "" ).Replace( ":", "" ) + "~";
            }

            _dialog.FileName = _fileName;
            _dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            _dialog.DefaultExt = "json";
            var _result = _dialog.ShowDialog( );
            if( _result == System.Windows.Forms.DialogResult.OK )
            {
                var _json = JsonConvert.SerializeObject( DataTable.ItemsSource );
                _json = JToken.Parse( _json ).ToString( Formatting.Indented );
                var _path = _dialog.FileName;
                File.WriteAllText( _path, _json );
                MessageBox.Show( "Exported successfully." );
            }
        }

        /// <summary>
        /// Called when [import button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnImportButtonClick( object sender, RoutedEventArgs e )
        {
            var _dialog = new OpenFileDialog( );
            _dialog.Title = "Please select an import file.";
            _dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            var _result = _dialog.ShowDialog( );
            if( _result == System.Windows.Forms.DialogResult.OK )
            {
                var _path = _dialog.FileName;
                var _json = File.ReadAllText( _path );
                var _list =
                    JsonConvert.DeserializeObject<ObservableCollection<DataTableItem>>( _json );

                DataTable.ItemsSource = _list;
                DataTable.Columns.RemoveAt( 1 );
                OnDataTableLoaded( null, null );
                MessageBox.Show( "Imported successfully." );
            }
        }

        /// <summary>
        /// Called when [add new row before selected click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnAddNewRowBeforeSelectedClick( object sender, RoutedEventArgs e )
        {
            var _selectedIndex = DataTable.SelectedIndex;
            if( _selectedIndex >= 0 )
            {
                var _item = new DataTableItem
                {
                    Role = "User",
                    Content = ""
                };

                ( DataTable.ItemsSource as ObservableCollection<DataTableItem> ).Insert(
                    _selectedIndex + 1, _item );
            }
            else
            {
                var _item = new DataTableItem
                {
                    Role = "User",
                    Content = ""
                };

                ( DataTable.ItemsSource as ObservableCollection<DataTableItem> ).Insert( 0, _item );
            }
        }

        /// <summary>
        /// Called when [delete selected row click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnDeleteSelectedRowClick( object sender, RoutedEventArgs e )
        {
            var _selectedIndex = DataTable.SelectedIndex;
            if( _selectedIndex >= 0 )
            {
                ( DataTable.ItemsSource as ObservableCollection<DataTableItem> ).RemoveAt(
                    _selectedIndex );
            }
        }

        /// <summary>
        /// Called when [acrylic window loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnAcrylicWindowLoaded( object sender, RoutedEventArgs e )
        {
            Height = Owner.Height;
            Top = Owner.Top;
        }

        /// <summary>
        /// Called when [preview mouse wheel].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/> instance containing the event data.</param>
        private void OnPreviewMouseWheel( object sender, MouseWheelEventArgs e )
        {
            var _element = sender as UIElement;
            while( _element != null )
            {
                _element = VisualTreeHelper.GetParent( _element ) as UIElement;
                if( _element is ScrollViewer _scrollViewer )
                {
                    _scrollViewer.ScrollToVerticalOffset(
                        _scrollViewer.VerticalOffset - e.Delta / 3 );

                    e.Handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Called when [set history count button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnSetHistoryCountButtonClick( object sender, RoutedEventArgs e )
        {
            var _msg =
                "Adjust the number of past conversation histories to include in the conversation.";

            var _conversationHistoryCount = AppSettings.ConversationHistoryCountSetting;
            var _window = new Messagebox( "Conversation History Setting", _msg,
                _conversationHistoryCount )
            {
                Owner = Application.Current.Windows.OfType<MainWindow>( ).FirstOrDefault( )
            };

            if( _window.ShowDialog( ) == true )
            {
                AppSettings.ConversationHistoryCountSetting = _window.resultInt;
                var _convos =
                    $"Set Number of Past Conversations: {AppSettings.ConversationHistoryCountSetting}";

                SetHistoryCountButton.Content = _convos;
            }
        }
    }
}