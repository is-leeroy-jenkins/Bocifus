// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="ConversationManager.cs" company="Terry D. Eppler">
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
//   ConversationManager.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using Model;
    using MdXaml;
    using ModernWpf;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using ICSharpCode.AvalonEdit.Rendering;
    using ModernWpf.Controls;
    using Properties;
    using MessageBox = ModernWpf.MessageBox;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.IDisposable" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="T:System.Windows.Markup.IStyleConnector" />
    [ SuppressMessage( "ReSharper", "UseNegatedPatternMatching" ) ]
    public partial class MainWindow
    {
        /// <summary>
        /// Called when [conversation ListBox selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnConversationListBoxSelectionChanged( object sender,
            SelectionChangedEventArgs e )
        {
            if( ConversationListBox.SelectedItem == null )
            {
                MessagesPanel.Children.Clear( );
                return;
            }

            var _selectedConversation = ( ConversationHistory )ConversationListBox.SelectedItem;
            var _messages = _selectedConversation.Messages.ToList( );
            MessagesPanel.Children.Clear( );
            var _targetMessages = _selectedConversation.Messages;
            for( var _i = 0; _i < _targetMessages.Count; _i++ )
            {
                var _message = _targetMessages[ _i ];
                if( _message.Role == null ) { break; }
                var _isUser = _message.Role == "user";
                var _isLastMessage = _i == _targetMessages.Count - 1;
                var _messageContent = _message.Content 
                    ?? JsonSerializer.Serialize( _message.Contents, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    } );

                var _result = UtilityFunctions.ExtractUserAndImageFromMessage( _messageContent );
                var _messageElement =
                    CreateMessageElement( _result.userMessage, _isUser, _isLastMessage );

                MessagesPanel.Children.Add( _messageElement );
                if( _result.image != "" )
                {
                    var _messageElementImage =
                        CreateMessageElement( "", false, _isLastMessage, _result.image );

                    MessagesPanel.Children.Add( _messageElementImage );
                }
            }

            MessagesPanel.PreviewMouseWheel += OnMouseWheelPreview;
            foreach( var _item in ConversationListBox.Items.OfType<ConversationHistory>( ) )
            {
                _item.IsSelected = false;
            }

            if( _selectedConversation != null )
            {
                _selectedConversation.IsSelected = true;
            }

            foreach( ConversationHistory _item in e.RemovedItems )
            {
                _item.IsSelected = false;
            }

            if( !_isFiltering )
            {
                UserTextBox.Focus( );
            }

            UserTextBox.CaretIndex = UserTextBox.Text.Length;
        }

        /// <summary>
        /// Called when [conversation history button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> i
        /// nstance containing the event data.</param>
        private void OnConversationHistoryButtonClick( object sender, RoutedEventArgs e )
        {
            ShowTable( );
        }

        /// <summary>
        /// Shows the table.
        /// </summary>
        private void ShowTable( )
        {
            var _targetConversation = ConversationListBox.SelectedItem as ConversationHistory;
            if( _targetConversation == null )
            {
                return;
            }

            var _window = new Table( _targetConversation )
            {
                Owner = Application.Current.Windows.OfType<MainWindow>( ).FirstOrDefault( )
            };

            var _result = ( bool )_window.ShowDialog( );
            if( _result )
            {
                _targetConversation.Messages = _window.UpdatedConversationHistory.Messages;
                MessagesPanel.Children.Clear( );
                var _selectedConversation = ConversationListBox.SelectedItem as ConversationHistory;
                if( _selectedConversation == null )
                {
                    return;
                }

                var _targetMessages = _selectedConversation.Messages;
                for( var _i = 0; _i < _targetMessages.Count; _i++ )
                {
                    var _message = _targetMessages[ _i ];
                    if( _message.Role == null ) { break; }
                    var _isUser = _message.Role == "user";
                    var _isLastMessage = _i == _targetMessages.Count - 1;
                    var _messageContent = _message.Content 
                        ?? JsonSerializer.Serialize( _message.Contents, 
                            new JsonSerializerOptions
                            {
                                WriteIndented = true
                            } );

                    var _content =
                        UtilityFunctions.ExtractUserAndImageFromMessage( _messageContent );

                    var _messageElement =
                        CreateMessageElement( _content.userMessage, _isUser, _isLastMessage );

                    MessagesPanel.Children.Add( _messageElement );
                    if( _content.image != "" )
                    {
                        var _messageElementImage =
                            CreateMessageElement( "", false, _isLastMessage, _content.image );

                        MessagesPanel.Children.Add( _messageElementImage );
                    }
                }

                MessagesPanel.PreviewMouseWheel += OnMouseWheelPreview;
            }
        }

        /// <summary>
        /// Creates the message element.
        /// </summary>
        /// <param name="messageContent">Content of the message.</param>
        /// <param name="isUser">if set to <c>true</c> [is user].</param>
        /// <param name="isLastMessage">if set to <c>true</c> [is last message].</param>
        /// <param name="visionImage">The vision image.</param>
        /// <returns></returns>
        private FrameworkElement CreateMessageElement( string messageContent, bool isUser,
            bool isLastMessage, string visionImage = null )
        {
            var _accentColor = ThemeManager.Current.AccentColor;
            if( _accentColor == null )
            {
                _accentColor = SystemParameters.WindowGlassColor;
            }

            var _accentColorBrush = new SolidColorBrush( ( Color )_accentColor )
            {
                Opacity = 0.3
            };

            var _messageGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition
                    {
                        Width = new GridLength( 1, GridUnitType.Star )
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength( 8, GridUnitType.Star )
                    },
                    new ColumnDefinition
                    {
                        Width = new GridLength( 1, GridUnitType.Star )
                    }
                }
            };

            _messageGrid.SizeChanged += OnMessageGridSizeChanged;
            var _copyIcon = new SymbolIcon( Symbol.Copy )
            {
                Foreground =
                    ( Brush )Application.Current.Resources[ "SystemBaseMediumHighColorBrush" ]
            };

            var _copyViewBox = new Viewbox
            {
                Width = 16,
                Child = _copyIcon
            };

            var _copyTextButton = new Button
            {
                Width = 30,
                Opacity = 0.5,
                Height = 30,
                Content = _copyViewBox,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Padding = new Thickness( 0 ),
                Margin = new Thickness( 0, 5, -30, 0 ),
                Background = Brushes.Transparent,
                Visibility = Visibility.Collapsed
            };

            _copyTextButton.Click += ( s, e ) =>
            {
                UtilityFunctions.CopyTextFromMessageGrid( _messageGrid );
                UtilityFunctions.AnimateButtonOpacityToOriginal( _copyTextButton, 0.5,
                    TimeSpan.FromMilliseconds( 500 ) );
            };

            var _translateIcon = new SymbolIcon( Symbol.Globe )
            {
                Foreground =
                    ( Brush )Application.Current.Resources[ "SystemBaseMediumHighColorBrush" ]
            };

            var _viewbox = new Viewbox
            {
                Width = 16,
                Child = _translateIcon
            };

            var _translateButton = new Button
            {
                Width = 30,
                Opacity = 0.5,
                Height = 30,
                Content = _viewbox,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Padding = new Thickness( 0 ),
                Margin = new Thickness( 0, 5, -60, 0 ),
                Background = Brushes.Transparent,
                Visibility = Visibility.Collapsed
            };

            _translateButton.Click += ( s, e ) =>
            {
                UtilityFunctions.TranslateTextFromMessageGrid( _messageGrid,
                    ConversationListBox.SelectedItem );

                UtilityFunctions.AnimateButtonOpacityToOriginal( _translateButton, 0.5,
                    TimeSpan.FromMilliseconds( 500 ) );
            };

            double _opacity = 1;
            if( ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark )
            {
                _opacity = 0.9;
            }

            if( isUser && visionImage == null )
            {
                var _userTextBox = new TextBox
                {
                    Padding = new Thickness( 10 ),
                    FontSize = Settings.Default.FontSize,
                    TextAlignment = TextAlignment.Left,
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = _opacity,
                    IsReadOnly = true,
                    Style = ( Style )Application.Current.FindResource( "NoBorderTextBoxStyle" ),
                    FontWeight = FontWeight.FromOpenTypeWeight( Settings.Default.FontWeight ),
                    Text = messageContent
                };

                _userTextBox.MouseDown += OnUserTextBoxMouseDown;
                var _contextMenu = CreateContextMenu( );
                _userTextBox.ContextMenu = _contextMenu;
                Grid.SetColumn( _userTextBox, 1 );
                _messageGrid.Children.Add( _userTextBox );
                var _backgroundRect = new Rectangle
                {
                    Fill = _accentColorBrush
                };

                Grid.SetColumnSpan( _backgroundRect, 3 );
                _messageGrid.Children.Add( _backgroundRect );
                Panel.SetZIndex( _backgroundRect, -1 );
                Grid.SetColumn( _copyTextButton, 1 );
                _messageGrid.Children.Add( _copyTextButton );
                Grid.SetColumn( _translateButton, 1 );
                _messageGrid.Children.Add( _translateButton );
                _userTextBox.MouseEnter += ShowButtonOnMouseEnter;
                _userTextBox.MouseLeave += HideButtonOnMouseLeave;
                _backgroundRect.MouseEnter += ShowButtonOnMouseEnter;
                _backgroundRect.MouseLeave += HideButtonOnMouseLeave;

                void ShowButtonOnMouseEnter( object s, MouseEventArgs e )
                {
                    _copyTextButton.Visibility = Visibility.Visible;
                    _translateButton.Visibility = Visibility.Visible;
                }

                void HideButtonOnMouseLeave( object s, MouseEventArgs e )
                {
                    if( _copyTextButton.IsMouseOver )
                    {
                        return;
                    }

                    if( _translateButton.IsMouseOver )
                    {
                        return;
                    }

                    var _mousePosToWindow = Mouse.GetPosition( Application.Current.MainWindow );
                    if( PresentationSource.FromVisual( _userTextBox ) != null )
                    {
                        var _topBoundary = _userTextBox.PointToScreen( new Point( 0, 0 ) ).Y;
                        var _bottomBoundary = _userTextBox
                            .PointToScreen( new Point( 0, _userTextBox.ActualHeight ) ).Y;

                        if( _mousePosToWindow.Y >= _topBoundary
                            && _mousePosToWindow.Y <= _bottomBoundary )
                        {
                            _copyTextButton.Visibility = Visibility.Visible;
                            _translateButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            _copyTextButton.Visibility = Visibility.Collapsed;
                            _translateButton.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            else if( !isUser
                && visionImage == null )
            {
                var _markDownScrollViewer = new MarkdownScrollViewer
                {
                    MarkdownStyle = ( Style )Application.Current.FindResource( "MdXamlStyle" ),
                    Engine =
                    {
                        DisabledContextMenu = true
                    },
                    UseSoftlineBreakAsHardlineBreak = true,
                    UseDarkThemeSyntaxHighlighting = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark,
                    Markdown = messageContent,
                    Opacity = _opacity,
                    SelectionBrush = new SolidColorBrush( ThemeManager.Current.ActualAccentColor ),
                    Padding = new Thickness( 12, 10, 12, 10 ),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Document =
                    {
                        FontSize = Settings.Default.FontSize,
                        FontFamily = new FontFamily( "Yu Gothic UI" ),
                        FontWeight = FontWeight.FromOpenTypeWeight( Settings.Default.FontWeight )
                    }
                };

                _markDownScrollViewer.ContextMenuOpening += OnMarkdownScrollContextMenuOpening;

                void OnMarkdownScrollContextMenuOpening( object sender, ContextMenuEventArgs e )
                {
                    var _paragraphText = "";
                    var _msv = sender as MarkdownScrollViewer;
                    if( _msv != null )
                    {
                        var _mousePos = Mouse.GetPosition( _msv );
                        var _hitVisual = _msv.InputHitTest( _mousePos ) as Visual;
                        if( _hitVisual != null
                            && _hitVisual is TextView )
                        {
                            var _editor = _hitVisual as TextView;
                            _paragraphText = _editor.Document.Text;
                        }
                        else if( _hitVisual != null
                            && _hitVisual is Rectangle )
                        {
                            _paragraphText = "";
                        }
                        else if( _msv != null )
                        {
                            _paragraphText = _msv.Markdown;
                        }

                        var _contextMenu = CreateContextMenu( _paragraphText );
                        _markDownScrollViewer.ContextMenu = _contextMenu;
                    }
                }

                Grid.SetColumn( _markDownScrollViewer, 1 );
                _messageGrid.Children.Add( _markDownScrollViewer );
                var _backgroundRect = new Rectangle
                {
                    Fill = Brushes.Transparent
                };

                Grid.SetColumnSpan( _backgroundRect, 3 );
                _messageGrid.Children.Add( _backgroundRect );
                Panel.SetZIndex( _backgroundRect, -1 );
                Grid.SetColumn( _copyTextButton, 1 );
                _messageGrid.Children.Add( _copyTextButton );
                Grid.SetColumn( _translateButton, 1 );
                _messageGrid.Children.Add( _translateButton );
                Button _regenerateButton = null;
                if( isLastMessage )
                {
                    var _icon = new SymbolIcon( Symbol.Sync )
                    {
                        Foreground =
                            ( Brush )Application.Current.Resources[
                                "SystemBaseMediumHighColorBrush" ]
                    };

                    var _viewBox = new Viewbox
                    {
                        Width = 16,
                        Child = _icon
                    };

                    _regenerateButton = new Button
                    {
                        Tag = "RegenerateButton",
                        Width = 30,
                        Opacity = 0.5,
                        Height = 30,
                        Content = _viewBox,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Padding = new Thickness( 0 ),
                        Margin = new Thickness( 0, 5, -90, 0 ),
                        Background = Brushes.Transparent,
                        Visibility = Visibility.Collapsed
                    };

                    _regenerateButton.Click += ( s, e ) =>
                    {
                        UtilityFunctions.AnimateButtonOpacityToOriginal( _regenerateButton, 0.5,
                            TimeSpan.FromMilliseconds( 500 ) );

                        RegenerateLatestResponse( );
                    };

                    Grid.SetColumn( _regenerateButton, 1 );
                    _messageGrid.Children.Add( _regenerateButton );
                }

                _markDownScrollViewer.MouseEnter += OnShowButtonMouseEnter;
                _markDownScrollViewer.MouseLeave += OnHideButtonMouseLeave;
                _backgroundRect.MouseEnter += OnShowButtonMouseEnter;
                _backgroundRect.MouseLeave += OnHideButtonMouseLeave;

                void OnShowButtonMouseEnter( object s, MouseEventArgs e )
                {
                    _copyTextButton.Visibility = Visibility.Visible;
                    _translateButton.Visibility = Visibility.Visible;
                    if( _regenerateButton != null )
                    {
                        _regenerateButton.Visibility = Visibility.Visible;
                    }
                }

                void OnHideButtonMouseLeave( object s, MouseEventArgs e )
                {
                    if( _copyTextButton.IsMouseOver )
                    {
                        return;
                    }

                    if( _translateButton.IsMouseOver )
                    {
                        return;
                    }

                    if( _regenerateButton != null
                        && _regenerateButton.IsMouseOver )
                    {
                        return;
                    }

                    var _mousePosToWindow = Mouse.GetPosition( Application.Current.MainWindow );
                    if( PresentationSource.FromVisual( _markDownScrollViewer ) != null )
                    {
                        var _topBoundary =
                            _markDownScrollViewer.PointToScreen( new Point( 0, 0 ) ).Y;

                        var _bottomBoundary = _markDownScrollViewer
                            .PointToScreen( new Point( 0, _markDownScrollViewer.ActualHeight ) ).Y;

                        if( _mousePosToWindow.Y >= _topBoundary
                            && _mousePosToWindow.Y <= _bottomBoundary )
                        {
                            _copyTextButton.Visibility = Visibility.Visible;
                            _translateButton.Visibility = Visibility.Visible;
                            if( _regenerateButton != null )
                            {
                                _regenerateButton.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            _copyTextButton.Visibility = Visibility.Collapsed;
                            _translateButton.Visibility = Visibility.Collapsed;
                            if( _regenerateButton != null )
                            {
                                _regenerateButton.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }

            if( visionImage != null )
            {
                var _base64Data = visionImage.Substring( visionImage.IndexOf( "," ) + 1 );
                var _imageBytes = Convert.FromBase64String( _base64Data );
                var _bitmapImage = new BitmapImage( );
                using( var _ms = new MemoryStream( _imageBytes ) )
                {
                    _bitmapImage.BeginInit( );
                    _bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    _bitmapImage.StreamSource = _ms;
                    _bitmapImage.EndInit( );
                }

                _bitmapImage.Freeze( );
                var _imageControl = new Image
                {
                    Source = _bitmapImage,
                    Stretch = Stretch.Uniform,
                    MaxWidth = 400,
                    MaxHeight = 400
                };

                _messageGrid.Children.Add( _imageControl );
                Grid.SetColumn( _imageControl, 1 );
                var _backgroundRect = new Rectangle
                {
                    Fill = _accentColorBrush
                };

                Grid.SetColumnSpan( _backgroundRect, 3 );
                _messageGrid.Children.Add( _backgroundRect );
                Panel.SetZIndex( _backgroundRect, -1 );
            }

            return _messageGrid;
        }

        /// <summary>
        /// Regenerates the latest response.
        /// </summary>
        private void RegenerateLatestResponse( )
        {
            var _messages = ConversationListBox.SelectedItems.OfType<ConversationHistory>( )
                .SelectMany( item => item.Messages ).ToList( );

            if( _messages.Count > 1 )
            {
                var _userMessage = _messages[ _messages.Count - 2 ].Content;
                (string user, string image) _result =
                    UtilityFunctions.ExtractUserAndImageFromMessage( _userMessage );

                foreach( var _item in ConversationListBox.SelectedItems
                    .OfType<ConversationHistory>( ) )
                {
                    if( _item.Messages.Count > 1 )
                    {
                        _item.Messages.RemoveAt( _item.Messages.Count - 1 );
                        _item.Messages.RemoveAt( _item.Messages.Count - 1 );
                    }
                    else if( _item.Messages.Count == 1 )
                    {
                        _item.Messages.RemoveAt( 0 );
                    }
                }

                MessagesPanel.Children.RemoveRange( MessagesPanel.Children.Count - 2, 2 );
                _ = ProcessOpenAiAsync( _result.user );
            }
        }

        /// <summary>
        /// Called when [use conversation history toggle switch toggled].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnUseConversationHistoryToggleSwitchToggled( object sender, RoutedEventArgs e )
        {
            if( UseConversationHistoryToggleSwitch.IsOn == false )
            {
                AppSettings.UseConversationHistoryFlg = false;
            }
            else
            {
                AppSettings.UseConversationHistoryFlg = true;
            }
        }

        /// <summary>
        /// Called when [conversation history clear button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnConversationHistoryClearButtonClick( object sender, RoutedEventArgs e )
        {
            var _yesno = MessageBox.Show( "Do you want to delete the entire conversation history?",
                "Delete Conversation History", MessageBoxButton.YesNo, MessageBoxImage.Question );

            if( _yesno == MessageBoxResult.No )
            {
                return;
            }

            var _targetConversation = ConversationListBox.SelectedItem as ConversationHistory;
            if( _targetConversation == null )
            {
                return;
            }

            _targetConversation.Messages.Clear( );
            MessagesPanel.Children.Clear( );
        }
    }
}