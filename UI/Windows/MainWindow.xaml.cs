// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-02-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-02-2024
// ******************************************************************************************
// <copyright file="MainWindow.xaml.cs" company="Terry D. Eppler">
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
//   MainWindow.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using Model;
    using MdXaml;
    using Microsoft.Toolkit.Uwp.Notifications;
    using Microsoft.Win32;
    using ModernWpf;
    using ModernWpf.Controls;
    using OpenAI.Tokenizer.GPT3;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using ICSharpCode.AvalonEdit.Rendering;
    using Properties;
    using Syncfusion.SfSkinManager;
    using ToastNotifications;
    using ToastNotifications.Lifetime;
    using ToastNotifications.Messages;
    using ToastNotifications.Position;
    using ApplicationTheme = ModernWpf.ApplicationTheme;
    using MessageBox = ModernWpf.MessageBox;
    using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="T:System.Windows.Markup.IStyleConnector" />
    [ SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" ) ]
    [ SuppressMessage( "ReSharper", "FieldCanBeMadeReadOnly.Global" ) ]
    public partial class MainWindow : IDisposable
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
        /// The select instruction content
        /// </summary>
        private protected string _selectInstructionContent = "";

        /// <summary>
        /// The stop watch
        /// </summary>
        private protected readonly Stopwatch _stopWatch = new Stopwatch( );

        /// <summary>
        /// The gkey pressed
        /// </summary>
        private protected bool _gkeyPressed;

        /// <summary>
        /// The is filtering
        /// </summary>
        private protected bool _isFiltering;

        /// <summary>
        /// The image file path
        /// </summary>
        private protected string _imageFilePath;

        /// <summary>
        /// The vision enabled
        /// </summary>
        private protected bool _visionEnabled;

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
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow( )
        {
            InitializeComponent( );
            SettingsManager.InitializeSettings( );
            InitializeInterface( );
            RecoverWindowBounds( );
        }

        /// <summary>
        /// Invokes if needed.
        /// </summary>
        /// <param name="action">The action.</param>
        private void InvokeIf( Action action )
        {
            try
            {
                ThrowIf.Null( action, nameof( action ) );
                if( Dispatcher.CheckAccess( ) )
                {
                    action?.Invoke( );
                }
                else
                {
                    Dispatcher.BeginInvoke( action );
                }
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Invokes if.
        /// </summary>
        /// <param name="action">The action.</param>
        private void InvokeIf( Action<object> action )
        {
            try
            {
                ThrowIf.Null( action, nameof( action ) );
                if( Dispatcher.CheckAccess( ) )
                {
                    action?.Invoke( null );
                }
                else
                {
                    Dispatcher.BeginInvoke( action );
                }
            }
            catch( Exception ex )
            {
                Fail( ex );
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
                var _position = new PrimaryScreenPositionProvider( Corner.BottomRight, 10, 10 );
                var _lifeTime = new TimeAndCountBasedLifetimeSupervisor( TimeSpan.FromSeconds( 5 ),
                    MaximumNotificationCount.UnlimitedNotifications( ) );

                return new Notifier( _cfg =>
                {
                    _cfg.LifetimeSupervisor = _lifeTime;
                    _cfg.PositionProvider = _position;
                    _cfg.Dispatcher = Application.Current.Dispatcher;
                } );
            }
            catch( Exception ex )
            {
                Fail( ex );
                return default( Notifier );
            }
        }

        /// <summary>
        /// Sends the notification.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SendNotification( string message )
        {
            try
            {
                ThrowIf.Null( message, nameof( message ) );
                var _notification = CreateNotifier( );
                _notification.ShowInformation( message );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Shows the splash message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SendMessage( string message )
        {
            try
            {
                ThrowIf.Null( message, nameof( message ) );
                var _splash = new SplashMessage( message )
                {
                    Owner = this
                };

                _splash.Show( );
            }
            catch( Exception ex )
            {
                Fail( ex );
            }
        }

        /// <summary>
        /// Refreshes the conversation list.
        /// </summary>
        public void RefreshConversationList()
        {
            var _collectionViewSource =
                FindResource("SortedConversations") as CollectionViewSource;

            if(_collectionViewSource != null)
            {
                _collectionViewSource.Source = AppSettings.ConversationManager.Histories;
                _collectionViewSource.View.Refresh();
            }
        }

        /// <summary>
        /// Saves the window bounds.
        /// </summary>
        private void SaveWindowBounds( )
        {
            var _settings = Settings.Default;
            _settings.WindowMaximized = WindowState == WindowState.Maximized;
            WindowState = WindowState.Normal;
            _settings.WindowLeft = Left;
            _settings.WindowTop = Top;
            _settings.WindowWidth = Width;
            _settings.WindowHeight = Height;
            if( SystemPromptGridColumn.Width.Value > 0 )
            {
                Settings.Default.SystemPromptColumnWidth = SystemPromptGridColumn.Width.Value;
            }

            if( ConversationHistorytGridColumn.Width.Value > 0 )
            {
                Settings.Default.ConversationColumnWidth =
                    ConversationHistorytGridColumn.Width.Value;
            }

            _settings.Save( );
        }

        /// <summary>
        /// Recovers the window bounds.
        /// </summary>
        private void RecoverWindowBounds( )
        {
            var _settings = Settings.Default;
            if( _settings.WindowLeft >= 0
                && _settings.WindowLeft + _settings.WindowWidth
                < SystemParameters.VirtualScreenWidth )
            {
                Left = _settings.WindowLeft;
            }

            if( _settings.WindowTop >= 0
                && _settings.WindowTop + _settings.WindowHeight
                < SystemParameters.VirtualScreenHeight )
            {
                Top = _settings.WindowTop;
            }

            if( _settings.WindowWidth > 0
                && _settings.WindowWidth <= SystemParameters.WorkArea.Width )
            {
                Width = _settings.WindowWidth;
            }

            if( _settings.WindowHeight > 0
                && _settings.WindowHeight <= SystemParameters.WorkArea.Height )
            {
                Height = _settings.WindowHeight;
            }

            if( _settings.WindowMaximized )
            {
                Loaded += ( o, e ) => WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Initializes the interface.
        /// </summary>
        private void InitializeInterface( )
        {
            UtilityFunctions.InitialColorSet( );
            ToastNotificationManagerCompat.OnActivated += OnToastNotificationManagerCompatActivated;
            UserTextBox.Focus( );
            NoticeToggleSwitch.IsOn = AppSettings.NoticeFlgSetting;
            if( AppSettings.ConversationManager.Histories == null )
            {
                AppSettings.ConversationManager.Histories =
                    new ObservableCollection<ConversationHistory>( );
            }
            else
            {
                var _selectedConversation =
                    AppSettings.ConversationManager.Histories.FirstOrDefault( ch => ch.IsSelected );

                if( _selectedConversation != null )
                {
                    ConversationListBox.SelectedItem = _selectedConversation;
                    ConversationListBox.ScrollIntoView( _selectedConversation );
                }
            }

            if( AppSettings.PromptTemplateManager.Templates == null )
            {
                AppSettings.PromptTemplateManager.Templates =
                    new ObservableCollection<PromptTemplate>( );
            }

            SystemPromptComboBox.ItemsSource = UtilityFunctions.SetupInstructionComboBox( );
            SystemPromptComboBox.Text = String.IsNullOrEmpty( AppSettings.InstructionSetting )
                ? ""
                : AppSettings.InstructionSetting;

            SystemPromptComboBox2.ItemsSource = UtilityFunctions.SetupInstructionComboBox( );
            SystemPromptComboBox2.Text = String.IsNullOrEmpty( AppSettings.InstructionSetting )
                ? ""
                : AppSettings.InstructionSetting;

            var _appSettings =
                ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel
                    .PerUserRoamingAndLocal );

            Debug.Print( "Path to save the configuration file:" + _appSettings.FilePath );
            UtilityFunctions.InitializeConfigDataTable( );
            UtilityFunctions.EnsureColumnsForType( AppSettings.ConfigDataTable,
                typeof( ModelList ) );

            ConfigurationComboBox.ItemsSource = AppSettings.ConfigDataTable.AsEnumerable( )
                .Select( x => x.Field<string>( "ConfigurationName" ) ).ToList( );

            ConfigurationComboBox.Text = AppSettings.SelectConfigSetting;
            UseConversationHistoryToggleSwitch.IsOn = AppSettings.UseConversationHistoryFlg;
            MessageScrollViewer.ScrollToBottom( );
            InitializeSystemPromptColumn( );
            var _isCollapsed = !AppSettings.IsPromptTemplateListVisible;
            PromptTemplateListBox.Visibility = _isCollapsed
                ? Visibility.Collapsed
                : Visibility.Visible;

            NewTemplateButton.Visibility = _isCollapsed
                ? Visibility.Collapsed
                : Visibility.Visible;

            ToggleVisibilityPromptTemplateButton.Content = _isCollapsed
                ? "▲"
                : "▼";

            var _currentPadding = UserTextBox.Padding;
            if( AppSettings.TranslationApiUseFlg )
            {
                TranslateButton.Visibility = Visibility.Visible;
                UserTextBox.Padding = new Thickness( _currentPadding.Left, _currentPadding.Top, 30,
                    _currentPadding.Bottom );
            }
            else
            {
                TranslateButton.Visibility = Visibility.Collapsed;
                UserTextBox.Padding = new Thickness( _currentPadding.Left, _currentPadding.Top, 10,
                    _currentPadding.Bottom );
            }

            if( ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark )
            {
                ConversationListBox.Opacity = 0.9;
                PromptTemplateListBox.Opacity = 0.9;
            }

            ImageFilePathLabel.Content = string.Empty;
        }

        /// <summary>
        /// Initializes the system prompt column.
        /// </summary>
        private void InitializeSystemPromptColumn( )
        {
            if( AppSettings.IsSystemPromptColumnVisible )
            {
                SystemPromptGridColumn.Width =
                    new GridLength( Settings.Default.SystemPromptColumnWidth );

                GridSplitterGridColumn.Width = new GridLength( 1, GridUnitType.Auto );
                SystemPromptSplitter.Visibility = Visibility.Visible;
                OpenSytemPromptWindowButtonIcon.Symbol = Symbol.ClosePane;
                SystemPromptComboBox2.SelectedIndex = SystemPromptComboBox.SelectedIndex;
            }
            else
            {
                SystemPromptGridColumn.Width = new GridLength( 0 );
                GridSplitterGridColumn.Width = new GridLength( 0 );
                SystemPromptSplitter.Visibility = Visibility.Hidden;
                OpenSytemPromptWindowButtonIcon.Symbol = Symbol.OpenPane;
            }

            if( AppSettings.IsConversationColumnVisible )
            {
                ConversationHistorytGridColumn.Width =
                    new GridLength( Settings.Default.ConversationColumnWidth );

                GridSplitterGridColumn2.Width = new GridLength( 1, GridUnitType.Auto );
            }
            else
            {
                ConversationHistorytGridColumn.Width = new GridLength( 0 );
                GridSplitterGridColumn2.Width = new GridLength( 0 );
            }
        }

        /// <summary>
        /// Applies the filter.
        /// </summary>
        /// <param name="filterText">The filter text.</param>
        /// <param name="isFilteringByFavorite">The is filtering by favorite.</param>
        private void ApplyFilter(string filterText, bool? isFilteringByFavorite = null)
        {
            var _collectionViewSource =
                FindResource("SortedConversations") as CollectionViewSource;

            if(_collectionViewSource != null)
            {
                _collectionViewSource.View.Filter = item =>
                {
                    var _conversationHistory = item as ConversationHistory;
                    if(_conversationHistory != null)
                    {
                        var _matchesTextFilter = string.IsNullOrEmpty(filterText)
                            || _conversationHistory.Messages.Any(message =>
                                message.Content.Contains(filterText,
                                    StringComparison.OrdinalIgnoreCase));

                        var _matchesFavoriteFilter = isFilteringByFavorite == null
                            || isFilteringByFavorite.Value == false || _conversationHistory.Favorite
                            == isFilteringByFavorite.Value;

                        return _matchesTextFilter && _matchesFavoriteFilter;
                    }

                    return false;
                };

                _collectionViewSource.View.Refresh();
            }
        }

        /// <summary>
        /// Copies the markdown table to clipboard.
        /// </summary>
        /// <param name="markdownText">The markdown text.</param>
        public static void CopyMarkdownTableToClipboard(string markdownText)
        {
            var _lines = markdownText.Split(new[]
            {
                '\n'
            }, StringSplitOptions.RemoveEmptyEntries);

            var _tableData = new List<List<string>>();
            foreach(var _line in _lines)
            {
                if(!_line.StartsWith("|"))
                {
                    continue;
                }

                if(_line.Contains("---"))
                {
                    continue;
                }

                var _row = _line.Trim('|').Split('|').Select(s => s.Trim()).ToList();
                _tableData.Add(_row);
            }

            if(_tableData.Count > 0)
            {
                var _stringBuilder = new StringBuilder();
                foreach(var _row in _tableData)
                {
                    _stringBuilder.AppendLine(string.Join("\t", _row));
                }

                Clipboard.SetText(_stringBuilder.ToString());
                Console.WriteLine(
                    "Table data has been copied to the clipboard. You can now paste it into Excel.");
            }
            else
            {
                Console.WriteLine("No table data found in the markdown text.");
            }
        }

        /// <summary>
        /// Determines whether [is markdown table] [the specified text].
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///   <c>true</c> if [is markdown table] [the specified text]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsMarkdownTable(string text)
        {
            var _pattern = @"^\|.*\|\s*\n\|\s*[-:]+\s*\|";
            return Regex.IsMatch(text, _pattern, RegexOptions.Multiline);
        }

        /// <summary>
        /// Determines whether the specified text contains japanese.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///   <c>true</c> if the specified text contains japanese; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsJapanese(string text)
        {
            return text.Any(c =>
                (c >= 0x3040 && c <= 0x30FF) || (c >= 0x4E00 && c <= 0x9FAF)
                || (c >= 0xFF66 && c <= 0xFF9D));
        }

        /// <summary>
        /// Shows the mermaid preview.
        /// </summary>
        /// <param name="mermaidCode">The mermaid code.</param>
        private void ShowMermaidPreview(string mermaidCode)
        {
            string _theme;
            string _backgroundColor;
            if(ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                _theme = "dark";
                _backgroundColor = "#333";
            }
            else
            {
                _theme = "default";
                _backgroundColor = "#FFFFFF";
            }

            var _htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ background-color: {_backgroundColor}; }}
    </style>
    <script src='https://cdn.jsdelivr.net/npm/mermaid@10.9.0/dist/mermaid.min.js'></script>
    <script>mermaid.initialize({{startOnLoad:true, theme: '{_theme}'}});</script>
</head>
<body>
    <div class='mermaid'>
{mermaidCode}
    </div>
</body>
</html>";

            var _previewWindow = new WebBrowserPreview(_htmlContent);
            var _parentCenterX = Left + Width / 2;
            var _parentCenterY = Top + Height / 2;
            _previewWindow.Left = _parentCenterX - _previewWindow.Width / 2;
            _previewWindow.Top = _parentCenterY - _previewWindow.Height / 2;
            _previewWindow.Show();
        }

        /// <summary>
        /// Determines whether [is mermaid code] [the specified text].
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///   <c>true</c> if [is mermaid code] [the specified text]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMermaidCode(string text)
        {
            if(string.IsNullOrEmpty(text))
            {
                return false;
            }

            var _patterns = new[]
            {
                @"^\s*graph\s+(?:TB|BT|RL|LR|TD|DT)\s*",
                @"^\s*sequenceDiagram\s*",
                @"^\s*classDiagram\s*",
                @"^\s*stateDiagram\s*",
                @"^\s*entityRelationship\s*",
                @"^\s*erDiagram\s*",
                @"^\s*gantt\s*",
                @"^\s*pie\s*",
                @"^\s*gitGraph\s*",
                @"^\s*Journey\s*",
                @"^\s*flowchart\s+(?:TB|BT|RL|LR)\s*"
            };

            var _firstLine = text.Split(new[]
            {
                '\n'
            }, StringSplitOptions.RemoveEmptyEntries)[0];

            foreach(var _pattern in _patterns)
            {
                if(Regex.IsMatch(_firstLine, _pattern,
                    RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates the context menu.
        /// </summary>
        /// <param name="paragraphText">The paragraph text.</param>
        /// <returns></returns>
        private ContextMenu CreateContextMenu( string paragraphText = null )
        {
            var _contextMenu = new ContextMenu( );
            var _copyTextMenuItem = new MenuItem( );
            _copyTextMenuItem.Icon = new SymbolIcon( Symbol.Copy );
            _contextMenu.Opened += ( s, e ) =>
                UpdateMenuItemButtonContent( _contextMenu.PlacementTarget, _copyTextMenuItem );

            var _copyTextAndCloseMenu = ( ) =>
            {
                _contextMenu.IsOpen = false;
            };

            _copyTextMenuItem.Click += ( s, e ) => _copyTextAndCloseMenu( );
            _copyTextMenuItem.Click += CopyTextToClipboard;
            _copyTextMenuItem.Header = "Copy Text";

            void CopyTextToClipboard( object sender, RoutedEventArgs e )
            {
                var _target = _contextMenu.PlacementTarget;
                if( _target is TextBox _textBox )
                {
                    var _textToCopy = _textBox.SelectedText.Length > 0
                        ? _textBox.SelectedText
                        : _textBox.Text;

                    Clipboard.SetText( _textToCopy );
                }
                else if( _target is MarkdownScrollViewer _markdownScrollViewer )
                {
                    var _selectedTextRange = new TextRange( _markdownScrollViewer.Selection.Start,
                        _markdownScrollViewer.Selection.End );

                    if( !string.IsNullOrEmpty( _selectedTextRange.Text ) )
                    {
                        Clipboard.SetText( _selectedTextRange.Text );
                    }
                    else
                    {
                        var _mousePos = Mouse.GetPosition( _markdownScrollViewer );
                        var _hitVisual = _markdownScrollViewer.InputHitTest( _mousePos ) as Visual;
                        if( _hitVisual is TextView _editor )
                        {
                            Clipboard.SetText( _editor.Document.Text );
                        }
                        else
                        {
                            Clipboard.SetText( _markdownScrollViewer.Markdown );
                        }
                    }
                }
                else if( _target is TextView _textView )
                {
                    var _mousePos = Mouse.GetPosition( _textView );
                    var _hitVisual = _textView.InputHitTest( _mousePos ) as Visual;
                    if( _hitVisual is TextView _editor )
                    {
                        Clipboard.SetText( _editor.Document.Text );
                    }
                    else
                    {
                        Clipboard.SetText( _textView.Document.Text );
                    }
                }
            }

            _contextMenu.Items.Add( _copyTextMenuItem );
            _contextMenu.Items.Add( new Separator( ) );
            var _currentFontSizeMenuItem = new MenuItem( );
            _currentFontSizeMenuItem.Icon = new SymbolIcon( Symbol.FontSize );
            _currentFontSizeMenuItem.Header = $"Font Size: {Settings.Default.FontSize}pt";
            _contextMenu.Items.Add( _currentFontSizeMenuItem );
            var _increaseFontSizeMenuItem = new MenuItem( );
            _increaseFontSizeMenuItem.Icon = new SymbolIcon( Symbol.FontIncrease );
            var _increaseFontSizeButton = new Button
            {
                Content = "Increase Font Size",
                Background = Brushes.Transparent
            };

            _increaseFontSizeMenuItem.Header = _increaseFontSizeButton;
            _increaseFontSizeButton.Click += ( s, e ) =>
                SetFontSize( Settings.Default.FontSize + 1, _currentFontSizeMenuItem );

            _increaseFontSizeMenuItem.Click += ( s, e ) =>
                SetFontSize( Settings.Default.FontSize + 1, _currentFontSizeMenuItem );

            var _decreaseFontSizeMenuItem = new MenuItem( );
            _decreaseFontSizeMenuItem.Icon = new SymbolIcon( Symbol.FontDecrease );
            var _decreaseFontSizeButton = new Button
            {
                Content = "Decrease Font Size",
                Background = Brushes.Transparent
            };

            _decreaseFontSizeMenuItem.Header = _decreaseFontSizeButton;
            _decreaseFontSizeButton.Click += ( s, e ) =>
                SetFontSize( Settings.Default.FontSize - 1, _currentFontSizeMenuItem );

            _decreaseFontSizeMenuItem.Click += ( s, e ) =>
                SetFontSize( Settings.Default.FontSize - 1, _currentFontSizeMenuItem );

            var _defaultFontSizeMenuItem = new MenuItem
            {
                Header = "Default Font Size"
            };

            _defaultFontSizeMenuItem.Icon = new SymbolIcon( Symbol.Refresh );
            var _defaultFontSizeButton = new Button
            {
                Content = "Default Font Size",
                Background = Brushes.Transparent
            };

            _defaultFontSizeMenuItem.Header = _defaultFontSizeButton;
            _defaultFontSizeButton.Click += ( s, e ) => SetFontSize( 16, _currentFontSizeMenuItem );
            _defaultFontSizeMenuItem.Click +=
                ( s, e ) => SetFontSize( 16, _currentFontSizeMenuItem );

            _currentFontSizeMenuItem.Items.Add( _increaseFontSizeMenuItem );
            _currentFontSizeMenuItem.Items.Add( _decreaseFontSizeMenuItem );
            _currentFontSizeMenuItem.Items.Add( _defaultFontSizeMenuItem );
            var _currentFontWeightMenuItem = new MenuItem( );
            _currentFontWeightMenuItem.Icon = new SymbolIcon( Symbol.Font );
            _currentFontWeightMenuItem.Header = $"Font Weight: {Settings.Default.FontWeight}";
            _contextMenu.Items.Add( _currentFontWeightMenuItem );
            var _increaseFontWeightMenuItem = new MenuItem( );
            _increaseFontWeightMenuItem.Icon = new SymbolIcon( Symbol.FontIncrease );
            var _increaseFontWeightButton = new Button
            {
                Content = "Increase Font Weight",
                Background = Brushes.Transparent
            };

            _increaseFontWeightMenuItem.Header = _increaseFontWeightButton;
            _increaseFontWeightButton.Click += ( s, e ) =>
                SetFontWeight( Settings.Default.FontWeight + 50, _currentFontWeightMenuItem );

            _increaseFontWeightMenuItem.Click += ( s, e ) =>
                SetFontWeight( Settings.Default.FontWeight + 50, _currentFontWeightMenuItem );

            var _decreaseFontWeightMenuItem = new MenuItem( );
            _decreaseFontWeightMenuItem.Icon = new SymbolIcon( Symbol.FontDecrease );
            var _decreaseFontWeightButton = new Button
            {
                Content = "Decrease Font Weight",
                Background = Brushes.Transparent
            };

            _decreaseFontWeightMenuItem.Header = _decreaseFontWeightButton;
            _decreaseFontWeightButton.Click += ( s, e ) =>
                SetFontWeight( Settings.Default.FontWeight - 50, _currentFontWeightMenuItem );

            _decreaseFontWeightMenuItem.Click += ( s, e ) =>
                SetFontWeight( Settings.Default.FontWeight - 50, _currentFontWeightMenuItem );

            var _defaultFontWeightMenuItem = new MenuItem
            {
                Header = "Default Font Weight"
            };

            _defaultFontWeightMenuItem.Icon = new SymbolIcon( Symbol.Refresh );
            var _defaultFontWeightButton = new Button
            {
                Content = "Default Font Weight",
                Background = Brushes.Transparent
            };

            _defaultFontWeightMenuItem.Header = _defaultFontWeightButton;
            _defaultFontWeightButton.Click +=
                ( s, e ) => SetFontWeight( 400, _currentFontWeightMenuItem );

            _defaultFontWeightMenuItem.Click +=
                ( s, e ) => SetFontWeight( 400, _currentFontWeightMenuItem );

            _currentFontWeightMenuItem.Items.Add( _increaseFontWeightMenuItem );
            _currentFontWeightMenuItem.Items.Add( _decreaseFontWeightMenuItem );
            _currentFontWeightMenuItem.Items.Add( _defaultFontWeightMenuItem );

            void SetFontSize( int newSize, MenuItem menuItem )
            {
                var _minSize = 8;
                var _maxSize = 32;
                newSize = Math.Max( _minSize, Math.Min( _maxSize, newSize ) );
                Settings.Default.FontSize = newSize;
                Settings.Default.Save( );
                foreach( var _item in MessagesPanel.Children )
                {
                    if( _item is Grid _grid )
                    {
                        foreach( var _child in _grid.Children )
                        {
                            if( _child is TextBox _textBox )
                            {
                                _textBox.FontSize = newSize;
                            }
                            else if( _child is MarkdownScrollViewer _markdownScrollViewer )
                            {
                                _markdownScrollViewer.Document.FontSize = newSize;
                            }
                        }
                    }
                }

                menuItem.Header = $"Font Size: {Settings.Default.FontSize}pt";
            }

            void SetFontWeight( int newWeight, MenuItem menuItem )
            {
                var _minSize = 300;
                var _maxSize = 600;
                newWeight = Math.Max( _minSize, Math.Min( _maxSize, newWeight ) );
                Settings.Default.FontWeight = newWeight;
                Settings.Default.Save( );
                foreach( var _item in MessagesPanel.Children )
                {
                    if( _item is Grid _grid )
                    {
                        foreach( var _child in _grid.Children )
                        {
                            if( _child is TextBox _textBox )
                            {
                                _textBox.FontWeight = FontWeight.FromOpenTypeWeight( newWeight );
                            }
                            else if( _child is MarkdownScrollViewer _markdownScrollViewer )
                            {
                                _markdownScrollViewer.Document.FontWeight =
                                    FontWeight.FromOpenTypeWeight( newWeight );
                            }
                        }
                    }
                }

                menuItem.Header = $"Font Weight: {Settings.Default.FontWeight}";
            }

            if( paragraphText is not null
                && IsMermaidCode( paragraphText ) )
            {
                _contextMenu.Items.Add( new Separator( ) );
                var _mermaidMenuItem = new MenuItem( );
                _mermaidMenuItem.Icon = new SymbolIcon( Symbol.AllApps );
                var _mermaidTextAndCloseMenu = ( ) =>
                {
                    OnMermaidPreviewContextMenuClick( paragraphText );
                    _contextMenu.IsOpen = false;
                };

                _mermaidMenuItem.Click += ( s, e ) => _mermaidTextAndCloseMenu( );
                _mermaidMenuItem.Header = "Mermaid Preview";
                _contextMenu.Items.Add( _mermaidMenuItem );
            }

            if( paragraphText is not null
                && IsMarkdownTable( paragraphText ) )
            {
                _contextMenu.Items.Add( new Separator( ) );
                var _copyTableMenuItem = new MenuItem( );
                _copyTableMenuItem.Icon = new SymbolIcon( Symbol.Copy );
                var _copyTableAndCloseMenu = ( ) =>
                {
                    CopyMarkdownTableToClipboard( paragraphText );
                    _contextMenu.IsOpen = false;
                };

                _copyTableMenuItem.Click += ( s, e ) => _copyTableAndCloseMenu( );
                _copyTableMenuItem.Header = "Copy Table to Clipboard";
                _contextMenu.Items.Add( _copyTableMenuItem );
                var _exportCsvMenuItem = new MenuItem( );
                _exportCsvMenuItem.Icon = new SymbolIcon( Symbol.Download );
                var _exportCsvAndCloseMenu = ( ) =>
                {
                    OnExportCsvContextMenuClick( paragraphText );
                    _contextMenu.IsOpen = false;
                };

                _exportCsvMenuItem.Click += ( s, e ) => _exportCsvAndCloseMenu( );
                _exportCsvMenuItem.Header = "Export CSV";
                _contextMenu.Items.Add( _exportCsvMenuItem );
            }

            return _contextMenu;
        }

        /// <summary>
        /// Updates the content of the menu item button.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="menuItem">The menu item.</param>
        private void UpdateMenuItemButtonContent(object target, MenuItem menuItem)
        {
            var _headerText = "Copy All Text";
            if(target is TextBox _textBox
                && !string.IsNullOrEmpty(_textBox.SelectedText))
            {
                _headerText = "Copy Selected Text";
            }
            else if(target is MarkdownScrollViewer _markdownScrollViewer)
            {
                var _selectedTextRange = new TextRange(_markdownScrollViewer.Selection.Start,
                    _markdownScrollViewer.Selection.End);

                if(!string.IsNullOrEmpty(_selectedTextRange.Text))
                {
                    _headerText = "Copy Selected Text";
                }
                else
                {
                    var _mousePos = Mouse.GetPosition(_markdownScrollViewer);
                    var _hitVisual = _markdownScrollViewer.InputHitTest(_mousePos) as Visual;
                    if(_hitVisual is TextView _editor)
                    {
                        _headerText = "Copy Code Block Text";
                    }
                    else
                    {
                        _headerText = "Copy All Text";
                    }
                }
            }
            else if(target is TextView _textView)
            {
                var _mousePos = Mouse.GetPosition(_textView);
                var _hitVisual = _textView.InputHitTest(_mousePos) as Visual;
                if(_hitVisual is TextView _editor)
                {
                    _headerText = "Copy Code Block Text";
                }
                else
                {
                    _headerText = "Copy All Text";
                }
            }

            menuItem.Header = _headerText;
        }

        /// <summary>
        /// Updates the UI based on vision.
        /// </summary>
        private void UpdateUIBasedOnVision()
        {
            if(ConfigurationComboBox.SelectedItem == null)
            {
                return;
            }

            var _selectedConfigName = ConfigurationComboBox.SelectedItem.ToString();
            var _row = AppSettings.ConfigDataTable.AsEnumerable().FirstOrDefault(x =>
                x.Field<string>("ConfigurationName") == _selectedConfigName);

            if(_row != null)
            {
                _visionEnabled = _row.Field<bool>("Vision");
            }

            AttachFileButton.Visibility = _visionEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;

            var _currentPadding = UserTextBox.Padding;
            var _leftPadding = _visionEnabled
                ? 35
                : 10;

            UserTextBox.Padding = new Thickness(_leftPadding, _currentPadding.Top,
                _currentPadding.Right, _currentPadding.Bottom);
        }

        /// <summary>
        /// Called when [main window loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnMainWindowLoaded( object sender, RoutedEventArgs e )
        {
            var _collectionViewSource =
                FindResource( "SortedConversations" ) as CollectionViewSource;

            if( _collectionViewSource != null )
            {
                _collectionViewSource.Source = AppSettings.ConversationManager.Histories;
                ConversationListBox.ItemsSource = _collectionViewSource.View;
            }

            var _promptTemplateSource =
                FindResource( "SortedPromptTemplates" ) as CollectionViewSource;

            if( _promptTemplateSource != null )
            {
                _promptTemplateSource.Source = AppSettings.PromptTemplateManager.Templates;
                PromptTemplateListBox.ItemsSource = _promptTemplateSource.View;
            }

            PromptTemplateListBox.SelectedItem = null;
            if( AppSettings.PromptTemplateGridRowHeighSetting > 0 )
            {
                ChatListGridRow.Height = new GridLength( AppSettings.ChatListGridRowHeightSetting,
                    GridUnitType.Star );

                PromptTemplateGridRow.Height =
                    new GridLength( AppSettings.PromptTemplateGridRowHeighSetting,
                        GridUnitType.Star );
            }
            else
            {
                PromptTemplateGridRow.Height = new GridLength( 0 );
            }
        }

        /// <summary>
        /// Called when [window closing].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void OnWindowClosing( object sender, CancelEventArgs e )
        {
            AppSettings.PromptTemplateGridRowHeighSetting = PromptTemplateGridRow.ActualHeight;
            AppSettings.ChatListGridRowHeightSetting = ChatListGridRow.ActualHeight;
            SettingsManager.SaveSettings( );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing( CancelEventArgs e )
        {
            SaveWindowBounds( );
            base.OnClosing( e );
        }

        /// <summary>
        /// Called when [window key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnWindowKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.F2 )
            {
                var _currentText = UserTextBox.Text;
                var _window = new LargeUserTextInput( _currentText );
                _window.Owner = this;
                _window.ShowDialog( );
                UserTextBox.Focus( );
            }

            if( e.Key == Key.F3 )
            {
                ShowTable( );
            }
        }

        /// <summary>
        /// Called when [acrylic window preview key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnAcrylicWindowPreviewKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.N
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                OnNewChatButtonClick( sender, e );
            }
            else if( e.Key == Key.S
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                try
                {
                    DataManager.SaveConversationsAsJson( AppSettings.ConversationManager );
                    DataManager.SavePromptTemplateAsJson( AppSettings.PromptTemplateManager );
                    var _documentsPath =
                        Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );

                    MessageBox.Show(
                        "Saved to " + _documentsPath + @"\Bocifus\ConversationHistory" + "\r\n"
                        + _documentsPath + @"\Bocifus\PromptTemplate", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information );

                    e.Handled = true;
                }
                catch( Exception ex )
                {
                    MessageBox.Show( ex.Message, "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error );
                }
            }
            else if( e.Key == Key.Tab
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                if( ConversationListBox.SelectedIndex < ConversationListBox.Items.Count - 1 )
                {
                    ConversationListBox.SelectedIndex++;
                }
                else
                {
                    ConversationListBox.SelectedIndex = 0;
                }

                ConversationListBox.ScrollIntoView( ConversationListBox.SelectedItem );
                e.Handled = true;
            }
            else if( e.Key == Key.Tab
                && Keyboard.Modifiers == ( ModifierKeys.Control | ModifierKeys.Shift ) )
            {
                if( ConversationListBox.SelectedIndex > 0 )
                {
                    ConversationListBox.SelectedIndex--;
                }
                else
                {
                    ConversationListBox.SelectedIndex = ConversationListBox.Items.Count - 1;
                }

                ConversationListBox.ScrollIntoView( ConversationListBox.SelectedItem );
                e.Handled = true;
            }
            else if( e.Key == Key.F
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                OnToggleFilterButtonClick( sender, null );
                FilterTextBox.Focus( );
            }
        }

        /// <summary>
        /// Called when [user text box key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnUserTextBoxKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.Enter
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                _ = ProcessOpenAiAsync( UserTextBox.Text );
            }
            else if( e.Key == Key.Enter
                && Keyboard.Modifiers == ( ModifierKeys.Control | ModifierKeys.Alt ) )
            {
                if( AppSettings.TranslationApiUseFlg )
                {
                    OnTranslateButtonClick( sender, e );
                }
            }
            else if( e.Key == Key.K
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset( _newVerticalOffset );
            }
            else if( e.Key == Key.J
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset( _newVerticalOffset );
            }
        }

        /// <summary>
        /// Called when [execute button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnExecButtonClick( object sender, RoutedEventArgs e )
        {
            if( !string.IsNullOrWhiteSpace( UserTextBox.Text ) )
            {
                _ = ProcessOpenAiAsync( UserTextBox.Text );
            }
        }

        /// <summary>
        /// The cancellation token source
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Called when [cancel button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCancelButtonClick( object sender, RoutedEventArgs e )
        {
            _cancellationTokenSource?.Cancel( );
        }

        /// <summary>
        /// Called when [assistant message grid mouse enter].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void OnAssistantMessageGridMouseEnter( object sender, MouseEventArgs e )
        {
            if( _isProcessing )
            {
                CancelButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Called when [assistant message grid mouse leave].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void OnAssistantMessageGridMouseLeave( object sender, MouseEventArgs e )
        {
            CancelButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Called when [user text box text changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void OnUserTextBoxTextChanged( object sender, TextChangedEventArgs e )
        {
            var _tokens = TokenizerGpt3.Encode( UserTextBox.Text );
            var _tooltip = $"Tokens : {_tokens.Count( )}";
            UserTextBox.ToolTip = _tooltip;
        }

        /// <summary>
        /// Called when [user text box size changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void OnUserTextBoxSizeChanged( object sender, SizeChangedEventArgs e )
        {
            if( UserTextBox.ActualHeight >= UserTextBox.MaxHeight )
            {
                ShowLargeTextInputWindowButton.Visibility = Visibility.Visible;
            }
            else
            {
                ShowLargeTextInputWindowButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Called when [notice toggle switch toggled].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnNoticeToggleSwitchToggled( object sender, RoutedEventArgs e )
        {
            AppSettings.NoticeFlgSetting = NoticeToggleSwitch.IsOn;
        }

        /// <summary>
        /// Called when [tokens label mouse left button down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnTokensLabelMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            UtilityFunctions.ShowMessagebox( "Tokens", TokensLabel.ToolTip.ToString( ) );
        }

        /// <summary>
        /// Called when [configuration ComboBox selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnConfigurationComboBoxSelectionChanged( object sender,
            SelectionChangedEventArgs e )
        {
            if( ConfigurationComboBox.SelectedItem == null )
            {
                return;
            }

            AppSettings.SelectConfigSetting = ConfigurationComboBox.SelectedItem.ToString( );
            UpdateUIBasedOnVision( );
        }

        /// <summary>
        /// Called when [system prompt ComboBox selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnSystemPromptComboBoxSelectionChanged( object sender,
            SelectionChangedEventArgs e )
        {
            SystemPromptComboBox2.SelectedIndex = SystemPromptComboBox.SelectedIndex;
            if( SystemPromptComboBox.SelectedItem == "" )
            {
                AppSettings.InstructionSetting = "";
                return;
            }

            AppSettings.InstructionSetting = SystemPromptComboBox.SelectedItem.ToString( );
            var _selectInstructionContent = "";
            if( !String.IsNullOrEmpty( AppSettings.InstructionSetting ) )
            {
                var _instructionList = AppSettings.InstructionListSetting?.Cast<string>( )
                    .Where( ( s, i ) => i % 2 == 0 ).ToArray( );

                var _index = Array.IndexOf( _instructionList, AppSettings.InstructionSetting );
                _selectInstructionContent = AppSettings.InstructionListSetting[ _index, 1 ];
            }

            SystemPromptComboBox.ToolTip = "# " + AppSettings.InstructionSetting + "\r\n"
                + _selectInstructionContent;
        }

        /// <summary>
        /// Called when [system prompt combo box2 selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnSystemPromptComboBox2SelectionChanged( object sender,
            SelectionChangedEventArgs e )
        {
            SystemPromptComboBox.SelectedIndex = SystemPromptComboBox2.SelectedIndex;
            var _selectInstructionContent = "";
            if( !String.IsNullOrEmpty( SystemPromptComboBox2.SelectedItem.ToString( ) ) )
            {
                var _instructionList = AppSettings.InstructionListSetting?.Cast<string>( )
                    .Where( ( s, i ) => i % 2 == 0 ).ToArray( );

                var _index = Array.IndexOf( _instructionList,
                    SystemPromptComboBox2.SelectedItem.ToString( ) );

                _selectInstructionContent = AppSettings.InstructionListSetting[ _index, 1 ];
            }

            SystemPromptContentsTextBox.Text = _selectInstructionContent;
            UnsavedLabel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Called when [user text box mouse wheel].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/> instance containing the event data.</param>
        private void OnUserTextBoxMouseWheel( object sender, MouseWheelEventArgs e )
        {
            if( Keyboard.Modifiers == ModifierKeys.Control )
            {
                if( e.Delta > 0
                    && UserTextBox.FontSize < 40 )
                {
                    UserTextBox.FontSize += 2;
                }
                else if( e.Delta < 0
                    && UserTextBox.FontSize > 10 )
                {
                    UserTextBox.FontSize -= 2;
                }
            }
        }

        /// <summary>
        /// Called when [token usage mouse left button down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnTokenUsageMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            var _window = new TokenUsageWindow( );
            _window.Owner = this;
            _window.ShowDialog( );
        }

        /// <summary>
        /// Called when [configuration setting button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnConfigurationSettingButtonClick( object sender, RoutedEventArgs e )
        {
            var _window = new ConfigSettingWindow( );
            _window.Owner = this;
            _window.ShowDialog( );
            ConfigurationComboBox.ItemsSource = AppSettings.ConfigDataTable.AsEnumerable( )
                .Select( x => x.Field<string>( "ConfigurationName" ) ).ToList( );

            UpdateUIBasedOnVision( );
        }

        /// <summary>
        /// Called when [instruction setting button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnInstructionSettingButtonClick( object sender, RoutedEventArgs e )
        {
            var _window = new InstructionSettingWindow( AppSettings.InstructionListSetting );
            _window.Owner = this;
            var _result = ( bool )_window.ShowDialog( );
            if( _result )
            {
                AppSettings.InstructionListSetting = _result
                    ? _window.inputResult
                    : null;

                var _instructionList = AppSettings.InstructionListSetting?.Cast<string>( )
                    .Where( ( s, i ) => i % 2 == 0 ).ToArray( );

                Array.Resize( ref _instructionList, _instructionList.Length + 1 );
                _instructionList[ _instructionList.Length - 1 ] = "";
                SystemPromptComboBox.ItemsSource = _instructionList;
                SystemPromptComboBox2.ItemsSource = _instructionList;
            }
        }

        /// <summary>
        /// Called when [color menu item click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnColorMenuItemClick( object sender, RoutedEventArgs e )
        {
            var _window = new ColorSettings( );
            _window.Owner = this;
            _window.ShowDialog( );
        }

        /// <summary>
        /// Called when [translation API menu item click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnTranslationApiMenuItemClick( object sender, RoutedEventArgs e )
        {
            var _window = new TranslationSettingWindow( );
            _window.Owner = this;
            _window.ShowDialog( );
        }

        /// <summary>
        /// Called when [title generation menu item click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnTitleGenerationMenuItemClick( object sender, RoutedEventArgs e )
        {
            var _window = new TitleGenerationSettings( );
            _window.Owner = this;
            _window.ShowDialog( );
        }

        /// <summary>
        /// Called when [version information menu item click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnVersionInformationMenuItemClick( object sender, RoutedEventArgs e )
        {
            var _window = new VersionWindow( );
            _window.Owner = this;
            _window.ShowDialog( );
        }

        /// <summary>
        /// Called when [resize thumb drag delta].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragDeltaEventArgs"/> instance containing the event data.</param>
        private void OnResizeThumbDragDelta( object sender, DragDeltaEventArgs e )
        {
            UserTextBox.Height = Math.Max( UserTextBox.ActualHeight + e.VerticalChange,
                UserTextBox.MinHeight );
        }

        /// <summary>
        /// Called when [user text box mouse down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnUserTextBoxMouseDown( object sender, MouseButtonEventArgs e )
        {
            Keyboard.ClearFocus( );
            ConversationListBox.Focus( );
        }

        /// <summary>
        /// Called when [message grid size changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void OnMessageGridSizeChanged( object sender, SizeChangedEventArgs e )
        {
            if( sender is Grid _messageGrid )
            {
                if( _messageGrid.ActualWidth * 0.8 > 1200 )
                {
                    _messageGrid.ColumnDefinitions[ 1 ].Width = new GridLength( 1200 );
                }
                else
                {
                    _messageGrid.ColumnDefinitions[ 1 ].Width =
                        new GridLength( _messageGrid.ActualWidth * 0.8 );
                }
            }
        }

        /// <summary>
        /// Called when [mermaid preview context menu click].
        /// </summary>
        /// <param name="text">The text.</param>
        private void OnMermaidPreviewContextMenuClick( string text )
        {
            ShowMermaidPreview( text );
        }

        /// <summary>
        /// Called when [export CSV context menu click].
        /// </summary>
        /// <param name="text">The text.</param>
        private void OnExportCsvContextMenuClick( string text )
        {
            var _lines = text.Split( new[ ]
            {
                '\n'
            }, StringSplitOptions.RemoveEmptyEntries );

            var _csvLines = new StringBuilder( );
            foreach( var _line in _lines )
            {
                if( !_line.StartsWith( "|" ) )
                {
                    continue;
                }

                if( _line.Contains( "---" ) )
                {
                    continue;
                }

                var _cleanedLine = _line.Trim( '|', ' ' );
                var _values = _cleanedLine.Split( new[ ]
                {
                    '|'
                }, StringSplitOptions.RemoveEmptyEntries );

                var _csvLine = string.Join( ",", _values.Select( v => v.Trim( ) ) );
                _csvLines.AppendLine( _csvLine );
            }

            var _dialog = new SaveFileDialog( );
            _dialog.Title = "Please select an export file.";
            _dialog.FileName = DateTime.Now.ToString( "yyyyMMdd" ) + "_output.csv";
            _dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            _dialog.DefaultExt = "csv";
            var _result = _dialog.ShowDialog( );
            if( _result == System.Windows.Forms.DialogResult.OK )
            {
                if( ContainsJapanese( _csvLines.ToString( ) ) )
                {
                    Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );
                    var _sjisEncoding = Encoding.GetEncoding( "shift_jis" );
                    File.WriteAllText( _dialog.FileName, _csvLines.ToString( ), _sjisEncoding );
                }
                else
                {
                    File.WriteAllText( _dialog.FileName, _csvLines.ToString( ) );
                }

                MessageBox.Show( "Exported successfully." );
            }
        }

        /// <summary>
        /// Called when [mouse wheel preview].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/> instance containing the event data.</param>
        private void OnMouseWheelPreview( object sender, MouseWheelEventArgs e )
        {
            var _element = sender as UIElement;
            while( _element != null )
            {
                _element = VisualTreeHelper.GetParent( _element ) as UIElement;
                if( _element is ScrollViewer _scrollViewer )
                {
                    var _delta = _scrollViewer.VerticalOffset - ( e.Delta / 3 );
                    _scrollViewer.ScrollToVerticalOffset( _delta );
                    e.Handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Called when [message scroll viewer scroll changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ScrollChangedEventArgs"/> instance containing the event data.</param>
        private void OnMessageScrollViewerScrollChanged( object sender, ScrollChangedEventArgs e )
        {
            var _isAtBottom = MessageScrollViewer.VerticalOffset
                >= MessageScrollViewer.ScrollableHeight;

            BottomScrollButton.Visibility = _isAtBottom
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        /// <summary>
        /// Called when [bottom scroll button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnBottomScrollButtonClick( object sender, RoutedEventArgs e )
        {
            MessageScrollViewer.ScrollToBottom( );
        }

        /// <summary>
        /// Called when [message scroll viewer preview key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnMessageScrollViewerPreviewKeyDown( object sender, KeyEventArgs e )
        {
            if( ( e.Key == Key.G && Keyboard.IsKeyDown( Key.LeftShift ) )
                || Keyboard.IsKeyDown( Key.RightShift ) )
            {
                MessageScrollViewer.ScrollToBottom( );
                _gkeyPressed = false;
            }
            else if( e.Key == Key.G )
            {
                if( _gkeyPressed )
                {
                    MessageScrollViewer.ScrollToTop( );
                    _gkeyPressed = false;
                }
                else
                {
                    _gkeyPressed = true;
                }
            }
            else if( e.Key == Key.Home )
            {
                MessageScrollViewer.ScrollToTop( );
                _gkeyPressed = false;
            }
            else if( e.Key == Key.End )
            {
                MessageScrollViewer.ScrollToBottom( );
                _gkeyPressed = false;
            }
            else if( e.Key == Key.U
                && ( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) ) )
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset
                    - MessageScrollViewer.ViewportHeight / 2;

                MessageScrollViewer.ScrollToVerticalOffset( _newVerticalOffset );
                _gkeyPressed = false;
            }
            else if( e.Key == Key.D
                && ( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) ) )
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset
                    + MessageScrollViewer.ViewportHeight / 2;

                MessageScrollViewer.ScrollToVerticalOffset( _newVerticalOffset );
                _gkeyPressed = false;
            }
            else if( e.Key == Key.E
                && ( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) ) )
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset( _newVerticalOffset );
                _gkeyPressed = false;
            }
            else if( e.Key == Key.Y
                && ( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) ) )
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset( _newVerticalOffset );
                _gkeyPressed = false;
            }
            else if( e.Key == Key.J )
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset( _newVerticalOffset );
                _gkeyPressed = false;
            }
            else if( e.Key == Key.K )
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset( _newVerticalOffset );
                _gkeyPressed = false;
            }
            else
            {
                _gkeyPressed = false;
            }
        }

        /// <summary>
        /// Called when [open sytem prompt window button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnOpenSytemPromptWindowButtonClick( object sender, RoutedEventArgs e )
        {
            if( SystemPromptGridColumn.Width.Value > 0 )
            {
                Settings.Default.SystemPromptColumnWidth = SystemPromptGridColumn.Width.Value;
                Settings.Default.Save( );
                SystemPromptGridColumn.Width = new GridLength( 0 );
                GridSplitterGridColumn.Width = new GridLength( 0 );
                SystemPromptSplitter.Visibility = Visibility.Hidden;
                OpenSytemPromptWindowButtonIcon.Symbol = Symbol.OpenPane;
                AppSettings.IsSystemPromptColumnVisible = false;
            }
            else
            {
                SystemPromptGridColumn.Width =
                    new GridLength( Settings.Default.SystemPromptColumnWidth );

                GridSplitterGridColumn.Width = new GridLength( 1, GridUnitType.Auto );
                SystemPromptSplitter.Visibility = Visibility.Visible;
                OpenSytemPromptWindowButtonIcon.Symbol = Symbol.ClosePane;
                AppSettings.IsSystemPromptColumnVisible = true;
                SystemPromptComboBox2.SelectedIndex = SystemPromptComboBox.SelectedIndex;
            }

            if( AppSettings.IsConversationColumnVisible )
            {
                ConversationHistorytGridColumn.Width =
                    new GridLength( Settings.Default.ConversationColumnWidth );

                GridSplitterGridColumn2.Width = new GridLength( 1, GridUnitType.Auto );
            }
            else
            {
                ConversationHistorytGridColumn.Width = new GridLength( 0 );
                GridSplitterGridColumn2.Width = new GridLength( 0 );
            }
        }

        /// <summary>
        /// Called when [system prompt contents text box text changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void OnSystemPromptContentsTextBoxTextChanged( object sender,
            TextChangedEventArgs e )
        {
            UnsavedLabel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Called when [new chat button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnNewChatButtonClick( object sender, RoutedEventArgs e )
        {
            MessagesPanel.Children.Clear( );
            if( ConversationListBox.SelectedItem is ConversationHistory _selectedItem )
            {
                _selectedItem.IsSelected = false;
            }

            ConversationListBox.SelectedItem = null;
            UserTextBox.Focus( );
            UserTextBox.CaretIndex = UserTextBox.Text.Length;
        }

        /// <summary>
        /// Called when [conversation delete button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnConversationDeleteButtonClick( object sender, RoutedEventArgs e )
        {
            ConversationHistory _itemToDelete = null;
            if( sender is MenuItem )
            {
                _itemToDelete = ( ConversationHistory )( ( MenuItem )sender ).DataContext;
            }

            if( sender is ContextMenu )
            {
                _itemToDelete = ( ConversationHistory )( ( ContextMenu )sender ).DataContext;
            }

            var _result = MessageBox.Show( "Are you sure you want to delete this conversation?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question );

            if( _result == MessageBoxResult.Yes )
            {
                AppSettings.ConversationManager.Histories.Remove( _itemToDelete );
                ConversationListBox.Items.Refresh( );
            }
        }

        /// <summary>
        /// Called when [conversation title edit button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnConversationTitleEditButtonClick( object sender, RoutedEventArgs e )
        {
            ConversationHistory _itemToDelete = null;
            if( sender is MenuItem )
            {
                _itemToDelete = ( ConversationHistory )( ( MenuItem )sender ).DataContext;
            }

            if( sender is ContextMenu )
            {
                _itemToDelete = ( ConversationHistory )( ( ContextMenu )sender ).DataContext;
            }

            var _currentTitle = _itemToDelete.Title;
            var _editWindow = new TitleEditWindow( _currentTitle );
            _editWindow.Owner = this;
            if( _editWindow.ShowDialog( ) == true )
            {
                var _newTitle = _editWindow.NewTitle;
                _itemToDelete.Title = _newTitle;
            }
        }

        /// <summary>
        /// Called when [conversation ListBox context menu preview key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnConversationListBoxContextMenuPreviewKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.F )
            {
                OnConversationFavoriteButtonClick( sender, e );
            }

            if( e.Key == Key.T )
            {
                OnConversationTitleEditButtonClick( sender, e );
            }

            if( e.Key == Key.D )
            {
                OnConversationDeleteButtonClick( sender, e );
            }
        }

        /// <summary>
        /// Called when [conversation favorite button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnConversationFavoriteButtonClick( object sender, RoutedEventArgs e )
        {
            ConversationHistory _item = null;
            if( sender is MenuItem )
            {
                _item = ( ConversationHistory )( ( MenuItem )sender ).DataContext;
            }

            if( sender is ContextMenu )
            {
                _item = ( ConversationHistory )( ( ContextMenu )sender ).DataContext;
            }

            _item.Favorite = !_item.Favorite;
        }

        /// <summary>
        /// Called when [conversation ListBox more button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnConversationListBoxMoreButtonClick( object sender, RoutedEventArgs e )
        {
            var _button = sender as Button;
            if( _button.ContextMenu != null )
            {
                _button.ContextMenu.IsOpen = false;
                _button.ContextMenu.PlacementTarget = _button;
                _button.ContextMenu.Placement = PlacementMode.Right;
                _button.ContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Called when [toast notification manager compat activated].
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnToastNotificationManagerCompatActivated(
            ToastNotificationActivatedEventArgsCompat e )
        {
            Dispatcher.Invoke( ( ) =>
            {
                Activate( );
                Topmost = true;
                Topmost = false;
            } );
        }

        /// <summary>
        /// Called when [translate button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void OnTranslateButtonClick( object sender, RoutedEventArgs e )
        {
            Storyboard _animation = null;
            Color _initialTextColor;
            try
            {
                TranslateButton.IsEnabled = false;
                _animation =
                    UtilityFunctions.CreateTextColorAnimation( UserTextBox, out _initialTextColor );

                _animation.Begin( );
                var _resultText = await TranslateApiRequestAsync( UserTextBox.Text,
                    AppSettings.ToTranslationLanguage );

                UserTextBox.Text = _resultText;
                UserTextBox.CaretIndex = UserTextBox.Text.Length;
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
            }
            finally
            {
                TranslateButton.IsEnabled = true;
                _animation?.Stop( );
                UserTextBox.Foreground = new SolidColorBrush( _initialTextColor );
            }
        }

        /// <summary>
        /// Called when [filter text box text changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void OnFilterTextBoxTextChanged( object sender, TextChangedEventArgs e )
        {
            _isFiltering = true;
            var _isFilteringByFavorite = FavoriteFilterToggleButton.IsChecked;
            ApplyFilter( FilterTextBox.Text, _isFilteringByFavorite );
            _isFiltering = false;
        }

        /// <summary>
        /// Called when [toggle filter button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnToggleFilterButtonClick( object sender, RoutedEventArgs e )
        {
            FilterTextBox.Visibility = FilterTextBox.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            FilterTextBoxClearButton.Visibility =
                FilterTextBoxClearButton.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            FavoriteFilterToggleButton.Visibility =
                FavoriteFilterToggleButton.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            FilterTextBox.Text = string.Empty;
            FavoriteFilterToggleButton.IsChecked = false;
            ApplyFilter( "", false );
        }

        /// <summary>
        /// Called when [clear text button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClearTextButtonClick( object sender, RoutedEventArgs e )
        {
            FilterTextBox.Text = string.Empty;
        }

        /// <summary>
        /// Called when [favorite filter toggle button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnFavoriteFilterToggleButtonClick( object sender, RoutedEventArgs e )
        {
            var _toggleButton = sender as ToggleButton;
            var _isFilteringByFavorite = _toggleButton.IsChecked;
            ApplyFilter( FilterTextBox.Text, _isFilteringByFavorite );
            FavoriteFilterToggleButton.Content = FavoriteFilterToggleButton.IsChecked == true
                ? "★"
                : "☆";
        }

        /// <summary>
        /// Called when [attach file button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnAttachFileButtonClick( object sender, RoutedEventArgs e )
        {
            var _button = sender as Button;
            if( _button.ContextMenu != null )
            {
                _button.ContextMenu.IsOpen = false;
                _button.ContextMenu.PlacementTarget = _button;
                _button.ContextMenu.Placement = PlacementMode.Top;
                _button.ContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Called when [select file click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnSelectFileClick( object sender, RoutedEventArgs e )
        {
            var _openFileDialog = new OpenFileDialog( );
            _openFileDialog.Filter =
                "Image files (*.png;*.jpeg;*.jpg;*.webp;*.gif)|*.png;*.jpeg;*.jpg;*.webp;*.gif";

            _openFileDialog.Multiselect = false;
            if( _openFileDialog.ShowDialog( ) == true )
            {
                _imageFilePath = _openFileDialog.FileName;
                ImageFilePathLabel.Content = _imageFilePath;
                _clipboardImage = null;
            }
        }

        /// <summary>
        /// Called when [paste from clipboard click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnPasteFromClipboardClick( object sender, RoutedEventArgs e )
        {
            if( Clipboard.ContainsImage( ) )
            {
                var _image = Clipboard.GetImage( );
                using var _memoryStream = new MemoryStream( );
                var _encoder = new PngBitmapEncoder( );
                _encoder.Frames.Add( BitmapFrame.Create( _image ) );
                _encoder.Save( _memoryStream );
                _clipboardImage = _memoryStream.ToArray( );
                _imageFilePath = null;
                ImageFilePathLabel.Content = "clipboard";
            }
            else
            {
                MessageBox.Show( "The clipboard does not contain any images.", "error",
                    MessageBoxButton.OK, MessageBoxImage.Warning );
            }
        }

        /// <summary>
        /// Called when [context menu opened].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnContextMenuOpened( object sender, RoutedEventArgs e )
        {
            var _imageAvailable = Clipboard.ContainsImage( );
            PasteFromClipboardMenuItem.IsEnabled = _imageAvailable;
        }

        /// <summary>
        /// Called when [clear image file path label button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClearImageFilePathLabelButtonClick( object sender, RoutedEventArgs e )
        {
            _imageFilePath = null;
            ImageFilePathLabel.Content = string.Empty;
        }

        /// <summary>
        /// Called when [image file path label mouse up].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnImageFilePathLabelMouseUp( object sender, MouseButtonEventArgs e )
        {
            var _argument = $"/select, \"{_imageFilePath}\"";
            Process.Start( "explorer.exe", _argument );
        }

        /// <summary>
        /// Called when [show large text input window button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnShowLargeTextInputWindowButtonClick( object sender, RoutedEventArgs e )
        {
            var _currentText = UserTextBox.Text;
            var _window = new LargeUserTextInput( _currentText );
            _window.Owner = this;
            _window.ShowDialog( );
            UserTextBox.Focus( );
        }

        /// <summary>
        /// Called when [toggle visibility prompt template button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnToggleVisibilityPromptTemplateButtonClick( object sender, RoutedEventArgs e )
        {
            var _isCollapsed = PromptTemplateListBox.Visibility == Visibility.Collapsed;
            PromptTemplateListBox.Visibility = _isCollapsed
                ? Visibility.Visible
                : Visibility.Collapsed;

            NewTemplateButton.Visibility = _isCollapsed
                ? Visibility.Visible
                : Visibility.Collapsed;

            ToggleVisibilityPromptTemplateButton.Content = _isCollapsed
                ? "▼"
                : "▲";

            if( _isCollapsed )
            {
                ChatListGridRow.Height = new GridLength( AppSettings.ChatListGridRowHeightSetting,
                    GridUnitType.Star );

                PromptTemplateGridRow.Height = new GridLength(
                    AppSettings.PromptTemplateGridRowHeightSaveSetting, GridUnitType.Star );
            }
            else
            {
                AppSettings.ChatListGridRowHeightSetting = ChatListGridRow.ActualHeight;
                AppSettings.PromptTemplateGridRowHeightSaveSetting =
                    PromptTemplateGridRow.ActualHeight;

                PromptTemplateGridRow.Height = new GridLength( 0 );
            }

            AppSettings.IsPromptTemplateListVisible = _isCollapsed;
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
        private protected void Fail( Exception ex )
        {
            var _error = new ErrorWindow( ex );
            _error?.SetText( );
            _error?.ShowDialog( );
        }
    }
}