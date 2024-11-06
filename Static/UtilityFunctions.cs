// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="UtilityFunctions.cs" company="Terry D. Eppler">
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
//   UtilityFunctions.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using MdXaml;
    using Model;
    using ModernWpf;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Properties;
    using MessageBox = ModernWpf.MessageBox;

    /// <summary>
    /// 
    /// </summary>
    [ SuppressMessage( "ReSharper", "ClassNeverInstantiated.Global" ) ]
    public class UtilityFunctions
    {
        /// <summary>
        /// Setups the instruction ComboBox.
        /// </summary>
        /// <returns></returns>
        public static string[ ] SetupInstructionComboBox( )
        {
            var _instructionList = AppSettings.InstructionListSetting?.Cast<string>( )
                ?.Where( ( s, i ) => i % 2 == 0 )?.ToArray( );

            if( _instructionList != null )
            {
                Array.Resize( ref _instructionList, _instructionList.Length + 1 );
                _instructionList[ _instructionList.Length - 1 ] = "";
                return _instructionList;
            }

            return null;
        }

        /// <summary>
        /// Initializes the configuration data table.
        /// </summary>
        public static void InitializeConfigDataTable( )
        {
            if( AppSettings.ConfigDataTable == null )
            {
                var _ds = new DataSet( );
                AppSettings.ConfigDataTable = new DataTable( );
                AppSettings.ConfigDataTable.Columns.Add( "ConfigurationName", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "Provider", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "Model", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "APIKey", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "DeploymentId", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "BaseDomain", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "ApiVersion", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "Temperature", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "MaxTokens", typeof( string ) );
                AppSettings.ConfigDataTable.Columns.Add( "Vision", typeof( bool ) );
                _ds.Tables.Add( AppSettings.ConfigDataTable );
            }
        }

        /// <summary>
        /// Initials the color set.
        /// </summary>
        public static void InitialColorSet( )
        {
            var _theme = Settings.Default.Theme;
            if( _theme == "Dark" )
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
            else if( _theme == "Light" )
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = null;
            }

            var _accentColor = Settings.Default.AccentColor;
            if( _accentColor == "Default"
                || _accentColor == "" )
            {
                ThemeManager.Current.AccentColor = null;
            }
            else
            {
                var _color = ( Color )ColorConverter.ConvertFromString( _accentColor );
                ThemeManager.Current.AccentColor = _color;
            }
        }

        /// <summary>
        /// Ensures the type of the columns for.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="type">The type.</param>
        public static void EnsureColumnsForType( DataTable dataTable, Type type )
        {
            foreach( var _propertyInfo in type.GetProperties( ) )
            {
                if( !dataTable.Columns.Contains( _propertyInfo.Name ) )
                {
                    var _columnType = Nullable.GetUnderlyingType( _propertyInfo.PropertyType )
                        ?? _propertyInfo.PropertyType;

                    var _column = new DataColumn( _propertyInfo.Name, _columnType );
                    if( _columnType == typeof( string ) )
                    {
                        _column.DefaultValue = "";
                    }
                    else if( _columnType == typeof( bool ) )
                    {
                        _column.DefaultValue = false;
                    }

                    dataTable.Columns.Add( _column );
                }
            }
        }

        /// <summary>
        /// Shows the messagebox.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The content.</param>
        public static void ShowMessagebox( string title, string content )
        {
            var _window = new Messagebox( title, content )
            {
                Owner = Application.Current.Windows.OfType<MainWindow>( ).FirstOrDefault( )
            };

            _window.ShowDialog( );
        }

        /// <summary>
        /// Creates the opacity animation.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static Storyboard CreateOpacityAnimation( DependencyObject target )
        {
            var _animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.5,
                Duration = TimeSpan.FromSeconds( 1 ),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget( _animation, target );
            Storyboard.SetTargetProperty( _animation, new PropertyPath( "Opacity" ) );
            var _storyboard = new Storyboard( );
            _storyboard.Children.Add( _animation );
            return _storyboard;
        }

        /// <summary>
        /// Creates the text color animation.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="initialColor">The initial color.</param>
        /// <returns></returns>
        public static Storyboard CreateTextColorAnimation( TextBox textBox, out Color initialColor )
        {
            initialColor = ( textBox.Foreground as SolidColorBrush ).Color;
            var _startColor = initialColor;
            _startColor.A = ( byte )( 255 * 0.5 );
            var _animation = new ColorAnimation
            {
                From = initialColor,
                To = _startColor,
                Duration = TimeSpan.FromSeconds( 1 ),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget( _animation, textBox );
            Storyboard.SetTargetProperty( _animation, new PropertyPath( "Foreground.Color" ) );
            var _storyboard = new Storyboard( );
            _storyboard.Children.Add( _animation );
            return _storyboard;
        }

        /// <summary>
        /// Animates the button opacity to original.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="originalOpacity">The original opacity.</param>
        /// <param name="duration">The duration.</param>
        public static void AnimateButtonOpacityToOriginal( Button button, double originalOpacity,
            TimeSpan duration )
        {
            button.Opacity = 1.0;
            var _opacityAnimation = new DoubleAnimation
            {
                To = originalOpacity,
                Duration = duration,
                FillBehavior = FillBehavior.Stop
            };

            _opacityAnimation.Completed += ( s, e ) =>
            {
                button.Opacity = originalOpacity;
            };

            button.BeginAnimation( UIElement.OpacityProperty, _opacityAnimation );
        }

        /// <summary>
        /// Serializes the array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public static string SerializeArray( string[ , ] array )
        {
            return JsonConvert.SerializeObject( array );
        }

        /// <summary>
        /// Deserializes the array.
        /// </summary>
        /// <param name="serializedArray">The serialized array.</param>
        /// <returns></returns>
        public static string[ , ] DeserializeArray( string serializedArray )
        {
            if( serializedArray == ""
                || serializedArray == null )
            {
                return new string[ 0, 0 ];
            }
            else
            {
                return JsonConvert.DeserializeObject<string[ , ]>( serializedArray );
            }
        }

        /// <summary>
        /// Serializes the data table.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        public static string SerializeDataTable( DataTable dataTable )
        {
            if( dataTable == null )
            {
                return "";
            }

            using var _stream = new MemoryStream( );
            var _formatter = new BinaryFormatter( );
            _formatter.Serialize( _stream, dataTable );
            return Convert.ToBase64String( _stream.ToArray( ) );
        }

        /// <summary>
        /// Deserializes the data table.
        /// </summary>
        /// <param name="serializedDataTable">The serialized data table.</param>
        /// <returns></returns>
        public static DataTable DeserializeDataTable( string serializedDataTable )
        {
            if( serializedDataTable == ""
                || serializedDataTable == null )
            {
                return null;
            }

            using var _stream = new MemoryStream( Convert.FromBase64String( serializedDataTable ) );
            var _formatter = new BinaryFormatter( );
            return ( DataTable )_formatter.Deserialize( _stream );
        }

        /// <summary>
        /// Copies the text from message grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        public static void CopyTextFromMessageGrid( Grid grid )
        {
            foreach( var _child in grid.Children )
            {
                if( _child is TextBox _textBox )
                {
                    Clipboard.SetText( _textBox.Text );
                    break;
                }
                else if( _child is MarkdownScrollViewer _markdownScrollViewer )
                {
                    Clipboard.SetText( _markdownScrollViewer.Markdown );
                    break;
                }
            }
        }

        /// <summary>
        /// Translates the text from message grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="selectedItem">The selected item.</param>
        public static void TranslateTextFromMessageGrid( Grid grid, object selectedItem )
        {
            foreach( var _child in grid.Children )
            {
                if( _child is TextBox _textBox )
                {
                    TranslateText( _textBox, selectedItem );
                }
                else if( _child is MarkdownScrollViewer _markdownScrollViewer )
                {
                    TranslateText( _markdownScrollViewer, selectedItem );
                }
            }
        }

        /// <summary>
        /// Translates the text.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="selectedItem">The selected item.</param>
        public static async void TranslateText( object target, object selectedItem )
        {
            var _mainWindow = ( MainWindow )Application.Current.MainWindow;
            Storyboard _animation = null;
            if( target is TextBox _textBox )
            {
                try
                {
                    _animation = CreateOpacityAnimation( _textBox );
                    _animation.Begin( );
                    var _beforeText = _textBox.Text;
                    var _translatedText = await _mainWindow.TranslateApiRequestAsync( _beforeText,
                        AppSettings.FromTranslationLanguage );

                    _translatedText = _translatedText.TrimEnd( '\r', '\n' );
                    _textBox.Text = _translatedText;
                    var _messageboxResult = MessageBox.Show(
                        "Would you like the translation results to be reflected in the existing conversation history?",
                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question );

                    if( _messageboxResult == MessageBoxResult.Yes )
                    {
                        if( selectedItem is ConversationHistory _targetConversation )
                        {
                            foreach( var _message in _targetConversation.Messages )
                            {
                                (string user, string image) _result =
                                    ExtractUserAndImageFromMessage( _message.Content );

                                if( _result.user == _beforeText )
                                {
                                    _message.Content = _translatedText;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch( Exception ex )
                {
                    MessageBox.Show( ex.Message );
                }
                finally
                {
                    _animation?.Stop( );
                    _textBox.Opacity = 1.0;
                }
            }
            else if( target is MarkdownScrollViewer _markdownScrollViewer )
            {
                try
                {
                    _animation = CreateOpacityAnimation( _markdownScrollViewer );
                    _animation.Begin( );
                    var _beforeText = _markdownScrollViewer.Markdown;
                    var _translatedText = await TranslateTextWithCodeBlocks( _beforeText );
                    _translatedText = _translatedText.TrimEnd( '\r', '\n' );
                    _translatedText = Regex.Replace( _translatedText, @"(\d+\.)\s*(\S)", "$1 $2" );
                    _markdownScrollViewer.Markdown = _translatedText;
                    var _messageboxResult = MessageBox.Show(
                        "Would you like the translation results to be reflected in the existing conversation history?",
                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question );

                    if( _messageboxResult == MessageBoxResult.Yes )
                    {
                        if( selectedItem is ConversationHistory _targetConversation )
                        {
                            foreach( var _message in _targetConversation.Messages )
                            {
                                (string user, string image) _result =
                                    ExtractUserAndImageFromMessage( _message.Content );

                                if( _result.user == _beforeText )
                                {
                                    _message.Content = _translatedText;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch( Exception ex )
                {
                    MessageBox.Show( ex.Message );
                }
                finally
                {
                    _animation?.Stop( );
                    _markdownScrollViewer.Opacity = ThemeManager.Current.ActualApplicationTheme
                        == ApplicationTheme.Dark
                            ? 0.9
                            : 1;
                }
            }
        }

        /// <summary>
        /// Translates the text with code blocks.
        /// </summary>
        /// <param name="markdownText">The markdown text.</param>
        /// <returns></returns>
        private static async Task<string> TranslateTextWithCodeBlocks( string markdownText )
        {
            var _regex = new Regex( @"(```\w*\s[\s\S]*?```)" );
            var _matches = _regex.Matches( markdownText );
            var _lastPos = 0;
            var _translatedText = new StringBuilder( );
            foreach( Match _match in _matches )
            {
                var _textToTranslate = markdownText.Substring( _lastPos, _match.Index - _lastPos );
                var _translatedSegment = await TranslateTextAsync( _textToTranslate );
                _translatedText.Append( _translatedSegment );
                _translatedText.Append( _match.Value );
                _lastPos = _match.Index + _match.Length;
            }

            if( _lastPos < markdownText.Length )
            {
                var _remainingText = markdownText.Substring( _lastPos );
                var _translatedRemaining = await TranslateTextAsync( _remainingText );
                _translatedText.Append( _translatedRemaining );
            }

            return _translatedText.ToString( );
        }

        /// <summary>
        /// Translates the text asynchronous.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private static async Task<string> TranslateTextAsync( string text )
        {
            var _mainWindow = ( MainWindow )Application.Current.MainWindow;
            if( string.IsNullOrWhiteSpace( text ) )
            {
                return text;
            }

            return await _mainWindow.TranslateApiRequestAsync( text,
                AppSettings.FromTranslationLanguage );
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetAllChildren( DependencyObject parent )
        {
            for( var _i = 0; _i < VisualTreeHelper.GetChildrenCount( parent ); _i++ )
            {
                var _child = VisualTreeHelper.GetChild( parent, _i );
                yield return _child;
                foreach( var _grandChild in GetAllChildren( _child ) )
                {
                    yield return _grandChild;
                }
            }
        }

        /// <summary>
        /// Extracts the user and image from message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static (string userMessage, string image) ExtractUserAndImageFromMessage(
            string message )
        {
            JToken _token;
            try
            {
                _token = JToken.Parse( message );
                if( _token.Type != JTokenType.Array )
                {
                    _token = null;
                }
            }
            catch( Exception )
            {
                _token = null;
            }

            var _user = "";
            var _image = "";
            if( _token != null )
            {
                var _items = _token.ToObject<List<VisionUserContentItem>>( );
                foreach( var _item in _items )
                {
                    if( _item.type == "text" )
                    {
                        _user = _item.text;
                    }

                    if( ( _item.type == "image_url" || _item.type == "image" )
                        && _item.image_url?.url != null )
                    {
                        _image = _item.image_url.url;
                    }
                }
            }
            else
            {
                _user = message;
            }

            return ( _user, _image );
        }
    }
}