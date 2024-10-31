

namespace Bocifus
{
    using Model;
    using MdXaml;
    using Microsoft.Toolkit.Uwp.Notifications;
    using Microsoft.Win32;
    using ModernWpf;
    using ModernWpf.Controls;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OpenAI.ObjectModels.RequestModels;
    using OpenAI.Tokenizer.GPT3;
    using Bocifus.Model;
    using SourceChord.FluentWPF;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Input.Manipulations;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using ToastNotifications;
    using ToastNotifications.Lifetime;
    using ToastNotifications.Messages;
    using ToastNotifications.Position;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class MainWindow
    {
        /// <summary>
        /// The busy
        /// </summary>
        private protected bool _busy;

        /// <summary>
        /// The path
        /// </summary>
        private protected object _path = new object( );

        /// <summary>
        /// The seconds
        /// </summary>
        private protected int _seconds;

        /// <summary>
        /// The update status
        /// </summary>
        private protected Action _statusUpdate;

        /// <summary>
        /// The theme
        /// </summary>
        private protected readonly DarkMode _theme = new DarkMode();

        string _selectInstructionContent = "";

        Stopwatch _stopWatch = new Stopwatch();

        private bool _gKeyPressed;

        private bool _isFiltering = false;

        public string? ImageFilePath = null;

        public bool VisionEnabled = false;

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
                lock( _path )
                {
                    return _busy;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataManagement.SettingsManager.InitializeSettings();
            InitializeInterface();
            RecoverWindowBounds();
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
        /// <param name="message">
        /// The message.
        /// </param>
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

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var _collectionViewSource = FindResource("SortedConversations") as CollectionViewSource;
            if (_collectionViewSource != null)
            {
                _collectionViewSource.Source = AppSettings.ConversationManager.Histories;
                ConversationListBox.ItemsSource = _collectionViewSource.View;
            }

            var _promptTemplateSource = FindResource("SortedPromptTemplates") as CollectionViewSource;
            if (_promptTemplateSource != null)
            {
                _promptTemplateSource.Source = AppSettings.PromptTemplateManager.Templates;
                PromptTemplateListBox.ItemsSource = _promptTemplateSource.View;
            }

            PromptTemplateListBox.SelectedItem = null;
            if (AppSettings.PromptTemplateGridRowHeighSetting > 0)
            {
                ChatListGridRow.Height = new GridLength(AppSettings.ChatListGridRowHeightSetting, GridUnitType.Star);
                PromptTemplateGridRow.Height = new GridLength(AppSettings.PromptTemplateGridRowHeighSetting, GridUnitType.Star);
            }
            else
            {
                PromptTemplateGridRow.Height = new GridLength(0);
            }
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettings.PromptTemplateGridRowHeighSetting = PromptTemplateGridRow.ActualHeight;
            AppSettings.ChatListGridRowHeightSetting = ChatListGridRow.ActualHeight;
            DataManagement.SettingsManager.SaveSettings();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveWindowBounds();
            base.OnClosing(e);
        }

        void SaveWindowBounds()
        {
            var _settings = Properties.Settings.Default;
            _settings.WindowMaximized = WindowState == WindowState.Maximized;
            WindowState = WindowState.Normal;  
            _settings.WindowLeft = Left;
            _settings.WindowTop = Top;
            _settings.WindowWidth = Width;
            _settings.WindowHeight = Height;
            if (SystemPromptGridColumn.Width.Value > 0)
            {
                Properties.Settings.Default.SystemPromptColumnWidth = SystemPromptGridColumn.Width.Value;
            }
            if (ConversationHistorytGridColumn.Width.Value > 0)
            {
                Properties.Settings.Default.ConversationColumnWidth = ConversationHistorytGridColumn.Width.Value;
            }
            _settings.Save();
        }

        void RecoverWindowBounds()
        {
            var _settings = Properties.Settings.Default;
            if (_settings.WindowLeft >= 0 &&
                (_settings.WindowLeft + _settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
            { Left = _settings.WindowLeft; }
            if (_settings.WindowTop >= 0 &&
                (_settings.WindowTop + _settings.WindowHeight) < SystemParameters.VirtualScreenHeight)
            { Top = _settings.WindowTop; }
            if (_settings.WindowWidth > 0 &&
                _settings.WindowWidth <= SystemParameters.WorkArea.Width)
            { Width = _settings.WindowWidth; }
            if (_settings.WindowHeight > 0 &&
                _settings.WindowHeight <= SystemParameters.WorkArea.Height)
            { Height = _settings.WindowHeight; }
            if (_settings.WindowMaximized)
            {
                Loaded += (o, e) => WindowState = WindowState.Maximized;
            }
        }

        private void InitializeInterface()
        {
            UtilityFunctions.InitialColorSet();
            ToastNotificationManagerCompat.OnActivated += this.OnToastNotificationManagerCompatActivated;
            UserTextBox.Focus();
            NoticeToggleSwitch.IsOn = AppSettings.NoticeFlgSetting;

            if (AppSettings.ConversationManager.Histories == null)
            {
                AppSettings.ConversationManager.Histories = new ObservableCollection<ConversationHistory>();
            }
            else
            {
                var _selectedConversation = AppSettings.ConversationManager.Histories.FirstOrDefault(ch => ch.IsSelected);
                if (_selectedConversation != null)
                {
                    ConversationListBox.SelectedItem = _selectedConversation;
                    ConversationListBox.ScrollIntoView(_selectedConversation);
                }
            }
            if (AppSettings.PromptTemplateManager.Templates == null)
            {
                AppSettings.PromptTemplateManager.Templates = new ObservableCollection<PromptTemplate>();
            }

            SystemPromptComboBox.ItemsSource = UtilityFunctions.SetupInstructionComboBox();
            SystemPromptComboBox.Text = String.IsNullOrEmpty(AppSettings.InstructionSetting) ? "" : AppSettings.InstructionSetting;
            SystemPromptComboBox2.ItemsSource = UtilityFunctions.SetupInstructionComboBox();
            SystemPromptComboBox2.Text = String.IsNullOrEmpty(AppSettings.InstructionSetting) ? "" : AppSettings.InstructionSetting;
            var _appSettings = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal);
            Debug.Print("Path to save the configuration file:" + _appSettings.FilePath);
            UtilityFunctions.InitializeConfigDataTable();
            UtilityFunctions.EnsureColumnsForType(AppSettings.ConfigDataTable, typeof(ConfigSettingWindow.ModelList));
            ConfigurationComboBox.ItemsSource = AppSettings.ConfigDataTable.AsEnumerable().Select(x => x.Field<string>("ConfigurationName")).ToList();
            ConfigurationComboBox.Text = AppSettings.SelectConfigSetting;
            UseConversationHistoryToggleSwitch.IsOn = AppSettings.UseConversationHistoryFlg;
            MessageScrollViewer.ScrollToBottom();
            InitializeSystemPromptColumn();
            var _isCollapsed = !(AppSettings.IsPromptTemplateListVisible);
            PromptTemplateListBox.Visibility = _isCollapsed ? Visibility.Collapsed : Visibility.Visible;
            NewTemplateButton.Visibility = _isCollapsed ? Visibility.Collapsed : Visibility.Visible;
            ToggleVisibilityPromptTemplateButton.Content = _isCollapsed ? "▲" : "▼";
            var _currentPadding = UserTextBox.Padding;
            if (AppSettings.TranslationAPIUseFlg == true)
            {
                TranslateButton.Visibility = Visibility.Visible;
                UserTextBox.Padding = new Thickness(_currentPadding.Left, _currentPadding.Top, 30, _currentPadding.Bottom);
            }
            else
            {
                TranslateButton.Visibility = Visibility.Collapsed;
                UserTextBox.Padding = new Thickness(_currentPadding.Left, _currentPadding.Top, 10, _currentPadding.Bottom);
            }

            if (ThemeManager.Current.ActualApplicationTheme == ModernWpf.ApplicationTheme.Dark)
            {
                ConversationListBox.Opacity = 0.9;
                PromptTemplateListBox.Opacity = 0.9;
            }

            ImageFilePathLabel.Content = string.Empty;
        }

        private void InitializeSystemPromptColumn()
        {
            if (AppSettings.IsSystemPromptColumnVisible == true)
            {
                SystemPromptGridColumn.Width = new GridLength(Properties.Settings.Default.SystemPromptColumnWidth);
                GridSplitterGridColumn.Width = new GridLength(1, GridUnitType.Auto);
                SystemPromptSplitter.Visibility = Visibility.Visible;
                OpenSytemPromptWindowButtonIcon.Symbol = ModernWpf.Controls.Symbol.ClosePane;
                SystemPromptComboBox2.SelectedIndex = SystemPromptComboBox.SelectedIndex;
            }
            else
            {
                SystemPromptGridColumn.Width = new GridLength(0);
                GridSplitterGridColumn.Width = new GridLength(0);
                SystemPromptSplitter.Visibility = Visibility.Hidden;
                OpenSytemPromptWindowButtonIcon.Symbol = ModernWpf.Controls.Symbol.OpenPane;
            }
            if (AppSettings.IsConversationColumnVisible == true)
            {
                ConversationHistorytGridColumn.Width = new GridLength(Properties.Settings.Default.ConversationColumnWidth);
                GridSplitterGridColumn2.Width = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                ConversationHistorytGridColumn.Width = new GridLength(0);
                GridSplitterGridColumn2.Width = new GridLength(0);
            }
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                var _currentText = UserTextBox.Text;
                var _window = new LargeUserTextInput(_currentText);
                _window.Owner = this;
                _window.ShowDialog();
                UserTextBox.Focus();
            }
            if (e.Key == Key.F3)
            {
                ShowTable();
            }
        }

        private void OnAcrylicWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OnNewChatButtonClick(sender, e);
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                try
                {
                    DataManagement.DataManager.SaveConversationsAsJson(AppSettings.ConversationManager);
                    DataManagement.DataManager.SavePromptTemplateAsJson(AppSettings.PromptTemplateManager);
                    var _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    ModernWpf.MessageBox.Show("Saved to " + _documentsPath + @"\Bocifus\ConversationHistory"
                        + "\r\n" + _documentsPath + @"\Bocifus\PromptTemplate"
                        ,"Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    e.Handled = true;
                }
                catch(Exception ex)
                {
                    ModernWpf.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (ConversationListBox.SelectedIndex < ConversationListBox.Items.Count - 1)
                {
                    ConversationListBox.SelectedIndex++;
                }
                else
                {
                    ConversationListBox.SelectedIndex = 0;
                }

                ConversationListBox.ScrollIntoView(ConversationListBox.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Tab && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (ConversationListBox.SelectedIndex > 0)
                {
                    ConversationListBox.SelectedIndex--;
                }
                else
                {
                    ConversationListBox.SelectedIndex = ConversationListBox.Items.Count - 1;
                }

                ConversationListBox.ScrollIntoView(ConversationListBox.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ToggleFilterButton_Click(sender, null);
                FilterTextBox.Focus();
            }
        }

        private void OnUserTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _ = ProcessOpenAIAsync(UserTextBox.Text);
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt))
            {
                if (AppSettings.TranslationAPIUseFlg == true)
                {
                    OnTranslateButtonClick(sender, e);
                }
            }
            else if (e.Key == Key.K && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset(_newVerticalOffset);
            }
            else if (e.Key == Key.J && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset(_newVerticalOffset);
            }
        }

        private void OnExecButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(string.IsNullOrWhiteSpace(UserTextBox.Text)))
            {
                _ = ProcessOpenAIAsync(UserTextBox.Text);
            }
        }

        private CancellationTokenSource _cancellationTokenSource;

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void OnAssistantMessageGridMouseEnter(object sender, MouseEventArgs e)
        {
            if (isProcessing)
            {
                CancelButton.Visibility = Visibility.Visible;
            }
        }

        private void OnAssistantMessageGridMouseLeave(object sender, MouseEventArgs e)
        {
            CancelButton.Visibility = Visibility.Collapsed;
        }

        private void OnUserTextBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var _tokens = TokenizerGpt3.Encode(UserTextBox.Text);
            var _tooltip = $"Tokens : {_tokens.Count()}";
            UserTextBox.ToolTip = _tooltip;
        }

        private void OnUserTextBoxSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (UserTextBox.ActualHeight >= UserTextBox.MaxHeight)
            {
                ShowLargeTextInputWindowButton.Visibility = Visibility.Visible;
            }
            else
            {
                ShowLargeTextInputWindowButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnNoticeToggleSwitchToggled(object sender, RoutedEventArgs e)
        {
            AppSettings.NoticeFlgSetting = (bool)NoticeToggleSwitch.IsOn;
        }

        private void OnTokensLabelMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UtilityFunctions.ShowMessagebox("Tokens", TokensLabel.ToolTip.ToString());
        }

        private void OnConfigurationComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ConfigurationComboBox.SelectedItem == null) return;
            AppSettings.SelectConfigSetting = ConfigurationComboBox.SelectedItem.ToString();
            UpdateUIBasedOnVision();
        }

        private void UpdateUIBasedOnVision()
        {
            if (ConfigurationComboBox.SelectedItem == null)
            {
                return;
            }

            var _selectedConfigName = ConfigurationComboBox.SelectedItem.ToString();
            var _row = AppSettings.ConfigDataTable.AsEnumerable()
                        .FirstOrDefault(x => x.Field<string>("ConfigurationName") == _selectedConfigName);

            if (_row != null)
            {
                VisionEnabled = _row.Field<bool>("Vision");
            }

            AttachFileButton.Visibility = VisionEnabled ? Visibility.Visible : Visibility.Collapsed;
            var _currentPadding = UserTextBox.Padding;
            var _leftPadding = VisionEnabled ? 35 : 10;
            UserTextBox.Padding = new Thickness(_leftPadding, _currentPadding.Top, _currentPadding.Right, _currentPadding.Bottom);
        }

        private void OnSystemPromptComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SystemPromptComboBox2.SelectedIndex = SystemPromptComboBox.SelectedIndex;
            if (SystemPromptComboBox.SelectedItem == "")
            {
                AppSettings.InstructionSetting = "";
                return;
            }

            AppSettings.InstructionSetting = SystemPromptComboBox.SelectedItem.ToString();
            var _selectInstructionContent = "";
            if (!String.IsNullOrEmpty(AppSettings.InstructionSetting))
            {
                var _instructionList = AppSettings.InstructionListSetting?.Cast<string>().Where((s, i) => i % 2 == 0).ToArray();
                var _index = Array.IndexOf(_instructionList, AppSettings.InstructionSetting);
                _selectInstructionContent = AppSettings.InstructionListSetting[_index, 1];
            }

            SystemPromptComboBox.ToolTip = "# " 
                + AppSettings.InstructionSetting 
                + "\r\n"
                + _selectInstructionContent;
        }

        private void OnSystemPromptComboBox2SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SystemPromptComboBox.SelectedIndex = SystemPromptComboBox2.SelectedIndex;
            var _selectInstructionContent = "";
            if (!String.IsNullOrEmpty(SystemPromptComboBox2.SelectedItem.ToString()))
            {
                var _instructionList = AppSettings.InstructionListSetting?.Cast<string>().Where((s, i) => i % 2 == 0).ToArray();
                var _index = Array.IndexOf(_instructionList, SystemPromptComboBox2.SelectedItem.ToString());
                _selectInstructionContent = AppSettings.InstructionListSetting[_index, 1];
            }

            SystemPromptContentsTextBox.Text = _selectInstructionContent;
            UnsavedLabel.Visibility = Visibility.Collapsed;
        }

        private void OnUserTextBoxMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Delta > 0 && UserTextBox.FontSize < 40)
                {
                    UserTextBox.FontSize += 2;
                }
                else if (e.Delta < 0 && UserTextBox.FontSize > 10)
                {
                    UserTextBox.FontSize -= 2;
                }
            }
        }

        private void OnTokenUsageMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var _window = new TokenUsageWindow();
            _window.Owner = this;
            _window.ShowDialog();
        }

        private void OnConfigurationSettingButtonClick(object sender, RoutedEventArgs e)
        {
            var _window = new ConfigSettingWindow();
            _window.Owner = this;
            _window.ShowDialog();
            ConfigurationComboBox.ItemsSource = AppSettings.ConfigDataTable.AsEnumerable().Select(x => x.Field<string>("ConfigurationName")).ToList();
            UpdateUIBasedOnVision();
        }

        private void OnInstructionSettingButtonClick(object sender, RoutedEventArgs e)
        {
            var _window = new InstructionSettingWindow(AppSettings.InstructionListSetting);
            _window.Owner = this;
            var _result = (bool)_window.ShowDialog();
            if (_result)
            {
                AppSettings.InstructionListSetting = _result ? _window.inputResult : null;
                var _instructionList = AppSettings.InstructionListSetting?.Cast<string>().Where((s, i) => i % 2 == 0).ToArray();
                Array.Resize(ref _instructionList, _instructionList.Length + 1);
                _instructionList[_instructionList.Length - 1] = "";
                SystemPromptComboBox.ItemsSource = _instructionList;
                SystemPromptComboBox2.ItemsSource = _instructionList;
            }
        }

        private void OnColorMenuItemClick(object sender, RoutedEventArgs e)
        {
            var _window = new ColorSettings();
            _window.Owner = this;
            _window.ShowDialog();
        }

        private void OnTranslationApiMenuItemClick(object sender, RoutedEventArgs e)
        {
            var _window = new TranslationAPISettingWindow();
            _window.Owner = this;
            _window.ShowDialog();
        }

        private void OnTitleGenerationMenuItemClick(object sender, RoutedEventArgs e)
        {
            var _window = new TitleGenerationSettings();
            _window.Owner = this;
            _window.ShowDialog();
        }

        private void OnVersionInformationMenuItemClick(object sender, RoutedEventArgs e)
        {
            var _window = new VersionWindow();
            _window.Owner = this;
            _window.ShowDialog();
        }

        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            UserTextBox.Height = Math.Max(UserTextBox.ActualHeight + e.VerticalChange, UserTextBox.MinHeight);
        }

        private void OnUserTextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
            ConversationListBox.Focus();  
        }

        private void OnMessageGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Grid _messageGrid)
            {
                if (_messageGrid.ActualWidth * 0.8 > 1200)
                {
                    _messageGrid.ColumnDefinitions[1].Width = new GridLength(1200);
                }
                else
                {
                    _messageGrid.ColumnDefinitions[1].Width = new GridLength(_messageGrid.ActualWidth * 0.8);
                }
            }
        }

        private ContextMenu CreateContextMenu(string paragraphText = null)
        {
            var _contextMenu = new ContextMenu();
            var _copyTextMenuItem = new MenuItem();
            _copyTextMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Copy);
            _contextMenu.Opened += (s, e) => UpdateMenuItemButtonContent(_contextMenu.PlacementTarget, _copyTextMenuItem);
            var _copyTextAndCloseMenu = () =>
            {
                _contextMenu.IsOpen = false;
            };

            _copyTextMenuItem.Click += (s, e) => _copyTextAndCloseMenu();
            _copyTextMenuItem.Click += CopyTextToClipboard;
            _copyTextMenuItem.Header = "Copy Text";

            void CopyTextToClipboard(object sender, RoutedEventArgs e)
            {
                var _target = _contextMenu.PlacementTarget;
                if (_target is TextBox _textBox)
                {
                    var _textToCopy = _textBox.SelectedText.Length > 0 ? _textBox.SelectedText : _textBox.Text;
                    Clipboard.SetText(_textToCopy);
                }
                else if (_target is MarkdownScrollViewer _markdownScrollViewer)
                {
                    var _selectedTextRange = new TextRange(_markdownScrollViewer.Selection.Start, _markdownScrollViewer.Selection.End);
                    if (!string.IsNullOrEmpty(_selectedTextRange.Text))    
                    {
                        Clipboard.SetText(_selectedTextRange.Text);
                    }
                    else
                    {
                        var _mousePos = Mouse.GetPosition(_markdownScrollViewer);  
                        var _hitVisual = _markdownScrollViewer.InputHitTest(_mousePos) as Visual;
                        if (_hitVisual is ICSharpCode.AvalonEdit.Rendering.TextView _editor)    
                        {
                            Clipboard.SetText(_editor.Document.Text);     
                        }
                        else   
                        {
                            Clipboard.SetText(_markdownScrollViewer.Markdown);
                        }
                    }
                }
                else if (_target is ICSharpCode.AvalonEdit.Rendering.TextView _textView)
                {
                    var _mousePos = Mouse.GetPosition(_textView);  
                    var _hitVisual = _textView.InputHitTest(_mousePos) as Visual;
                    if (_hitVisual is ICSharpCode.AvalonEdit.Rendering.TextView _editor)
                    {
                        Clipboard.SetText(_editor.Document.Text);     
                    }
                    else
                    {
                        Clipboard.SetText(_textView.Document.Text);
                    }
                }
            }

            _contextMenu.Items.Add(_copyTextMenuItem);
            _contextMenu.Items.Add(new Separator());
            var _currentFontSizeMenuItem = new MenuItem();
            _currentFontSizeMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontSize);
            _currentFontSizeMenuItem.Header = $"Font Size: {Properties.Settings.Default.FontSize}pt";
            _contextMenu.Items.Add(_currentFontSizeMenuItem);
            var _increaseFontSizeMenuItem = new MenuItem();
            _increaseFontSizeMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontIncrease);
            var _increaseFontSizeButton = new Button { Content = "Increase Font Size", Background = Brushes.Transparent };
            _increaseFontSizeMenuItem.Header = _increaseFontSizeButton;
            _increaseFontSizeButton.Click += (s, e) => SetFontSize(Properties.Settings.Default.FontSize + 1, _currentFontSizeMenuItem);
            _increaseFontSizeMenuItem.Click += (s, e) => SetFontSize(Properties.Settings.Default.FontSize + 1, _currentFontSizeMenuItem);
            var _decreaseFontSizeMenuItem = new MenuItem();
            _decreaseFontSizeMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontDecrease);
            var _decreaseFontSizeButton = new Button { Content = "Decrease Font Size", Background = Brushes.Transparent };
            _decreaseFontSizeMenuItem.Header = _decreaseFontSizeButton;
            _decreaseFontSizeButton.Click += (s, e) => SetFontSize(Properties.Settings.Default.FontSize - 1, _currentFontSizeMenuItem);
            _decreaseFontSizeMenuItem.Click += (s, e) => SetFontSize(Properties.Settings.Default.FontSize - 1, _currentFontSizeMenuItem);
            var _defaultFontSizeMenuItem = new MenuItem { Header = "Default Font Size" };
            _defaultFontSizeMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Refresh);
            var _defaultFontSizeButton = new Button { Content = "Default Font Size", Background = Brushes.Transparent };
            _defaultFontSizeMenuItem.Header = _defaultFontSizeButton;
            _defaultFontSizeButton.Click += (s, e) => SetFontSize(16, _currentFontSizeMenuItem);
            _defaultFontSizeMenuItem.Click += (s, e) => SetFontSize(16, _currentFontSizeMenuItem);
            _currentFontSizeMenuItem.Items.Add(_increaseFontSizeMenuItem);
            _currentFontSizeMenuItem.Items.Add(_decreaseFontSizeMenuItem);
            _currentFontSizeMenuItem.Items.Add(_defaultFontSizeMenuItem);
            var _currentFontWeightMenuItem = new MenuItem();
            _currentFontWeightMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Font);
            _currentFontWeightMenuItem.Header = $"Font Weight: {Properties.Settings.Default.FontWeight}";
            _contextMenu.Items.Add(_currentFontWeightMenuItem);
            var _increaseFontWeightMenuItem = new MenuItem();
            _increaseFontWeightMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontIncrease);
            var _increaseFontWeightButton = new Button { Content = "Increase Font Weight", Background = Brushes.Transparent };
            _increaseFontWeightMenuItem.Header = _increaseFontWeightButton;
            _increaseFontWeightButton.Click += (s, e) => SetFontWeight(Properties.Settings.Default.FontWeight + 50, _currentFontWeightMenuItem);
            _increaseFontWeightMenuItem.Click += (s, e) => SetFontWeight(Properties.Settings.Default.FontWeight + 50, _currentFontWeightMenuItem);
            var _decreaseFontWeightMenuItem = new MenuItem();
            _decreaseFontWeightMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontDecrease);
            var _decreaseFontWeightButton = new Button { Content = "Decrease Font Weight", Background = Brushes.Transparent };
            _decreaseFontWeightMenuItem.Header = _decreaseFontWeightButton;
            _decreaseFontWeightButton.Click += (s, e) => SetFontWeight(Properties.Settings.Default.FontWeight - 50, _currentFontWeightMenuItem);
            _decreaseFontWeightMenuItem.Click += (s, e) => SetFontWeight(Properties.Settings.Default.FontWeight - 50, _currentFontWeightMenuItem);
            var _defaultFontWeightMenuItem = new MenuItem { Header = "Default Font Weight" };
            _defaultFontWeightMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Refresh);
            var _defaultFontWeightButton = new Button { Content = "Default Font Weight", Background = Brushes.Transparent };
            _defaultFontWeightMenuItem.Header = _defaultFontWeightButton;
            _defaultFontWeightButton.Click += (s, e) => SetFontWeight(400, _currentFontWeightMenuItem);
            _defaultFontWeightMenuItem.Click += (s, e) => SetFontWeight(400, _currentFontWeightMenuItem);
            _currentFontWeightMenuItem.Items.Add(_increaseFontWeightMenuItem);
            _currentFontWeightMenuItem.Items.Add(_decreaseFontWeightMenuItem);
            _currentFontWeightMenuItem.Items.Add(_defaultFontWeightMenuItem);

            void SetFontSize(int newSize, MenuItem menuItem)
            {
                var _minSize = 8;
                var _maxSize = 32;
                newSize = Math.Max(_minSize, Math.Min(_maxSize, newSize));
                Properties.Settings.Default.FontSize = newSize;
                Properties.Settings.Default.Save();
                foreach (var _item in MessagesPanel.Children)
                {
                    if (_item is Grid _grid)
                    {
                        foreach (var _child in _grid.Children)
                        {
                            if (_child is TextBox _textBox)
                            {
                                _textBox.FontSize = newSize;
                            }
                            else if (_child is MarkdownScrollViewer _markdownScrollViewer)
                            {
                                _markdownScrollViewer.Document.FontSize = newSize;
                            }
                        }
                    }
                }

                menuItem.Header = $"Font Size: {Properties.Settings.Default.FontSize}pt";
            }

            void SetFontWeight(int newWeight, MenuItem menuItem)
            {
                var _minSize = 300;
                var _maxSize = 600;
                newWeight = Math.Max(_minSize, Math.Min(_maxSize, newWeight));
                Properties.Settings.Default.FontWeight = newWeight;
                Properties.Settings.Default.Save();
                foreach (var _item in MessagesPanel.Children)
                {
                    if (_item is Grid _grid)
                    {
                        foreach (var _child in _grid.Children)
                        {
                            if (_child is TextBox _textBox)
                            {
                                _textBox.FontWeight = FontWeight.FromOpenTypeWeight(newWeight);
                            }
                            else if (_child is MarkdownScrollViewer _markdownScrollViewer)
                            {
                                _markdownScrollViewer.Document.FontWeight = FontWeight.FromOpenTypeWeight(newWeight);
                            }
                        }
                    }
                }

                menuItem.Header = $"Font Weight: {Properties.Settings.Default.FontWeight}";
            }

            if (paragraphText is not null && IsMermaidCode(paragraphText))
            {
                _contextMenu.Items.Add(new Separator());
                var _mermaidMenuItem = new MenuItem();
                _mermaidMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.AllApps);
                var _mermaidTextAndCloseMenu = () =>
                {
                    OnMermaidPreviewContextMenuClick(paragraphText);
                    _contextMenu.IsOpen = false;
                };

                _mermaidMenuItem.Click += (s, e) => _mermaidTextAndCloseMenu();
                _mermaidMenuItem.Header = "Mermaid Preview";
                _contextMenu.Items.Add(_mermaidMenuItem);
            }

            if (paragraphText is not null && IsMarkdownTable(paragraphText))
            {
                _contextMenu.Items.Add(new Separator());
                var _copyTableMenuItem = new MenuItem();
                _copyTableMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Copy);
                var _copyTableAndCloseMenu = () =>
                {
                    CopyMarkdownTableToClipboard(paragraphText);
                    _contextMenu.IsOpen = false;
                };

                _copyTableMenuItem.Click += (s, e) => _copyTableAndCloseMenu();
                _copyTableMenuItem.Header = "Copy Table to Clipboard";
                _contextMenu.Items.Add(_copyTableMenuItem);
                var _exportCsvMenuItem = new MenuItem();
                _exportCsvMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Download);
                var _exportCsvAndCloseMenu = () =>
                {
                    OnExportCsvContextMenuClick(paragraphText);
                    _contextMenu.IsOpen = false;
                };

                _exportCsvMenuItem.Click += (s, e) => _exportCsvAndCloseMenu();
                _exportCsvMenuItem.Header = "Export CSV";
                _contextMenu.Items.Add(_exportCsvMenuItem);
            }

            return _contextMenu;
        }

        private void OnMermaidPreviewContextMenuClick(string text)
        {
            ShowMermaidPreview(text);
        }

        private void ShowMermaidPreview(string mermaidCode)
        {
            string _theme;
            string _backgroundColor;
            if (ThemeManager.Current.ActualApplicationTheme == ModernWpf.ApplicationTheme.Dark)
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
            var _parentCenterX = this.Left + (this.Width / 2);
            var _parentCenterY = this.Top + (this.Height / 2);
            _previewWindow.Left = _parentCenterX - (_previewWindow.Width / 2);
            _previewWindow.Top = _parentCenterY - (_previewWindow.Height / 2);
            _previewWindow.Show();
        }

        public static bool IsMermaidCode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var _patterns = new string[]
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

            var _firstLine = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
            foreach (var _pattern in _patterns)
            {
                if (Regex.IsMatch(_firstLine, _pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    return true;
            }

            return false;
        }

        private void OnExportCsvContextMenuClick(string text)
        {
            var _lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var _csvLines = new System.Text.StringBuilder();
            foreach (var _line in _lines)
            {
                if (!_line.StartsWith("|")) continue;  
                if (_line.Contains("---")) continue;
                var _cleanedLine = _line.Trim('|', ' ');
                var _values = _cleanedLine.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var _csvLine = string.Join(",", _values.Select(v => v.Trim()));
                _csvLines.AppendLine(_csvLine);
            }

            var _dialog = new System.Windows.Forms.SaveFileDialog();
            _dialog.Title = "Please select an export file.";
            _dialog.FileName = DateTime.Now.ToString("yyyyMMdd") + "_output.csv";
            _dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            _dialog.DefaultExt = "csv";
            var _result = _dialog.ShowDialog();
            if (_result == System.Windows.Forms.DialogResult.OK)
            {
                if (ContainsJapanese(_csvLines.ToString()))
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var _sjisEncoding = Encoding.GetEncoding("shift_jis");
                    File.WriteAllText(_dialog.FileName, _csvLines.ToString(), _sjisEncoding);
                }
                else
                {
                    File.WriteAllText(_dialog.FileName, _csvLines.ToString());
                }

                ModernWpf.MessageBox.Show("Exported successfully.");
            }
        }

        public static void CopyMarkdownTableToClipboard(string markdownText)
        {
            var _lines = markdownText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var _tableData = new List<List<string>>();

            foreach (var _line in _lines)
            {
                if (!_line.StartsWith("|")) continue;  
                if (_line.Contains("---")) continue;  

                var _row = _line.Trim('|').Split('|').Select(s => s.Trim()).ToList();
                _tableData.Add(_row);
            }

            if (_tableData.Count > 0)
            {
                var _stringBuilder = new System.Text.StringBuilder();
                foreach (var _row in _tableData)
                {
                    _stringBuilder.AppendLine(string.Join("\t", _row));
                }
                Clipboard.SetText(_stringBuilder.ToString());
                Console.WriteLine("Table data has been copied to the clipboard. You can now paste it into Excel.");
            }
            else
            {
                Console.WriteLine("No table data found in the markdown text.");
            }
        }

        static bool IsMarkdownTable(string text)
        {
            var _pattern = @"^\|.*\|\s*\n\|\s*[-:]+\s*\|";
            return Regex.IsMatch(text, _pattern, RegexOptions.Multiline);
        }

        public static bool ContainsJapanese(string text)
        {
            return text.Any(c => (c >= 0x3040 && c <= 0x30FF) ||   
                                 (c >= 0x4E00 && c <= 0x9FAF) ||   
                                 (c >= 0xFF66 && c <= 0xFF9D));    
        }

        void UpdateMenuItemButtonContent(object target, MenuItem menuItem)
        {
            var _headerText = "Copy All Text";
            if (target is TextBox _textBox && !string.IsNullOrEmpty(_textBox.SelectedText))
            {
                _headerText = "Copy Selected Text";
            }
            else if (target is MarkdownScrollViewer _markdownScrollViewer)
            {
                var _selectedTextRange = new TextRange(_markdownScrollViewer.Selection.Start, _markdownScrollViewer.Selection.End);
                if (!string.IsNullOrEmpty(_selectedTextRange.Text))
                {
                    _headerText = "Copy Selected Text";
                }
                else
                {
                    var _mousePos = Mouse.GetPosition(_markdownScrollViewer);  
                    var _hitVisual = _markdownScrollViewer.InputHitTest(_mousePos) as Visual;
                    if (_hitVisual is ICSharpCode.AvalonEdit.Rendering.TextView _editor)
                    {
                        _headerText = "Copy Code Block Text";
                    }
                    else
                    {
                        _headerText = "Copy All Text";
                    }
                }
            }
            else if (target is ICSharpCode.AvalonEdit.Rendering.TextView _textView)
            {
                var _mousePos = Mouse.GetPosition(_textView);  
                var _hitVisual = _textView.InputHitTest(_mousePos) as Visual;
                if (_hitVisual is ICSharpCode.AvalonEdit.Rendering.TextView _editor)
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

        private void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var _element = sender as UIElement;
            while (_element != null)
            {
                _element = VisualTreeHelper.GetParent(_element) as UIElement;
                if (_element is ScrollViewer _scrollViewer)
                {
                    _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset - (e.Delta / 3));
                    e.Handled = true;
                    return;
                }
            }
        }

        private void OnMessageScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var _isAtBottom = MessageScrollViewer.VerticalOffset >= MessageScrollViewer.ScrollableHeight;
            BottomScrollButton.Visibility = _isAtBottom ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnBottomScrollButtonClick(object sender, RoutedEventArgs e)
        {
            MessageScrollViewer.ScrollToBottom();
        }

        private void OnMessageScrollViewerPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                MessageScrollViewer.ScrollToBottom();
                _gKeyPressed = false;
            }
            else if (e.Key == Key.G)
            {
                if (_gKeyPressed)
                {
                    MessageScrollViewer.ScrollToTop();
                    _gKeyPressed = false;
                }
                else
                {
                    _gKeyPressed = true;
                }
            }
            else if (e.Key == Key.Home)
            {
                MessageScrollViewer.ScrollToTop();
                _gKeyPressed = false;
            }
            else if (e.Key == Key.End)
            {
                MessageScrollViewer.ScrollToBottom();
                _gKeyPressed = false;
            }
            else if (e.Key == Key.U && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset - (MessageScrollViewer.ViewportHeight / 2);
                MessageScrollViewer.ScrollToVerticalOffset(_newVerticalOffset);
                _gKeyPressed = false;
            }
            else if (e.Key == Key.D && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset + (MessageScrollViewer.ViewportHeight / 2);
                MessageScrollViewer.ScrollToVerticalOffset(_newVerticalOffset);
                _gKeyPressed = false;
            }
            else if (e.Key == Key.E && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset(_newVerticalOffset);
                _gKeyPressed = false;
            }
            else if (e.Key == Key.Y && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset(_newVerticalOffset);
                _gKeyPressed = false;
            }
            else if (e.Key == Key.J)
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset(_newVerticalOffset);
                _gKeyPressed = false;
            }
            else if (e.Key == Key.K)
            {
                var _newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset(_newVerticalOffset);
                _gKeyPressed = false;
            }
            else
            {
                _gKeyPressed = false;
            }
        }

        private void OnOpenSytemPromptWindowButtonClick(object sender, RoutedEventArgs e)
        {
            if (SystemPromptGridColumn.Width.Value > 0)
            {
                Properties.Settings.Default.SystemPromptColumnWidth = SystemPromptGridColumn.Width.Value;
                Properties.Settings.Default.Save();
                SystemPromptGridColumn.Width = new GridLength(0);
                GridSplitterGridColumn.Width = new GridLength(0);
                SystemPromptSplitter.Visibility = Visibility.Hidden;
                OpenSytemPromptWindowButtonIcon.Symbol = ModernWpf.Controls.Symbol.OpenPane;
                AppSettings.IsSystemPromptColumnVisible = false;
            }
            else
            {
                SystemPromptGridColumn.Width = new GridLength(Properties.Settings.Default.SystemPromptColumnWidth);
                GridSplitterGridColumn.Width = new GridLength(1, GridUnitType.Auto);
                SystemPromptSplitter.Visibility = Visibility.Visible;
                OpenSytemPromptWindowButtonIcon.Symbol = ModernWpf.Controls.Symbol.ClosePane;
                AppSettings.IsSystemPromptColumnVisible = true;
                SystemPromptComboBox2.SelectedIndex = SystemPromptComboBox.SelectedIndex;
            }

            if (AppSettings.IsConversationColumnVisible == true)
            {
                ConversationHistorytGridColumn.Width = new GridLength(Properties.Settings.Default.ConversationColumnWidth);
                GridSplitterGridColumn2.Width = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                ConversationHistorytGridColumn.Width = new GridLength(0);
                GridSplitterGridColumn2.Width = new GridLength(0);
            }
        }

        private void OnSystemPromptContentsTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            UnsavedLabel.Visibility = Visibility.Visible;
        }

        private void OnNewChatButtonClick(object sender, RoutedEventArgs e)
        {
            MessagesPanel.Children.Clear();
            if (ConversationListBox.SelectedItem is ConversationHistory _selectedItem)
            {
                _selectedItem.IsSelected = false;
            }

            ConversationListBox.SelectedItem = null;
            UserTextBox.Focus();
            UserTextBox.CaretIndex = UserTextBox.Text.Length;
        }

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

            var _result = ModernWpf.MessageBox.Show(
                "Are you sure you want to delete this conversation?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question );

            if( _result == MessageBoxResult.Yes )
            {
                AppSettings.ConversationManager.Histories.Remove( _itemToDelete );
                ConversationListBox.Items.Refresh( );
            }
        }

        private void OnConversationTitleEditButtonClick(object sender, RoutedEventArgs e)
        {
            ConversationHistory _itemToDelete = null;
            if (sender is MenuItem)
            {
                _itemToDelete = (ConversationHistory)((MenuItem)sender).DataContext;
            }

            if (sender is ContextMenu)
            {
                _itemToDelete = (ConversationHistory)((ContextMenu)sender).DataContext;
            }

            var _currentTitle = _itemToDelete.Title;
            var _editWindow = new TitleEditWindow(_currentTitle);
            _editWindow.Owner = this;
            if (_editWindow.ShowDialog() == true)
            {
                var _newTitle = _editWindow.NewTitle;
                _itemToDelete.Title = _newTitle;
            }
        }

        private void OnConversationListBoxContextMenuPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                OnConversationFavoriteButtonClick(sender, e);
            }

            if (e.Key == Key.T)
            {
                OnConversationTitleEditButtonClick(sender, e);
            }

            if (e.Key == Key.D)
            {
                OnConversationDeleteButtonClick(sender, e);
            }
        }

        private void OnConversationFavoriteButtonClick(object sender, RoutedEventArgs e)
        {
            ConversationHistory _item = null;
            if (sender is MenuItem)
            {
                _item = (ConversationHistory)((MenuItem)sender).DataContext;
            }

            if (sender is ContextMenu)
            {
                _item = (ConversationHistory)((ContextMenu)sender).DataContext;
            }

            _item.Favorite = !_item.Favorite;
        }

        private void OnConversationListBoxMoreButtonClick(object sender, RoutedEventArgs e)
        {
            var _button = sender as Button;
            if (_button.ContextMenu != null)
            {
                _button.ContextMenu.IsOpen = false;
                _button.ContextMenu.PlacementTarget = _button;
                _button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                _button.ContextMenu.IsOpen = true;
            }
        }

        public void RefreshConversationList()
        {
            var _collectionViewSource = FindResource("SortedConversations") as CollectionViewSource;
            if (_collectionViewSource != null)
            {
                _collectionViewSource.Source = AppSettings.ConversationManager.Histories;
                _collectionViewSource.View.Refresh();
            }
        }

        private void OnToastNotificationManagerCompatActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Activate();
                this.Topmost = true;
                this.Topmost = false;
            });
        }

        private async void OnTranslateButtonClick(object sender, RoutedEventArgs e)
        {
            Storyboard? _animation = null;
            Color _initialTextColor;
            try
            {
                TranslateButton.IsEnabled = false;
                _animation = UtilityFunctions.CreateTextColorAnimation(UserTextBox, out _initialTextColor);
                _animation.Begin();
                var _resultText = await TranslateAPIRequestAsync(UserTextBox.Text, AppSettings.ToTranslationLanguage);
                UserTextBox.Text = _resultText;
                UserTextBox.CaretIndex = UserTextBox.Text.Length;
            }
            catch (Exception ex)
            {
                ModernWpf.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                TranslateButton.IsEnabled = true;
                _animation?.Stop();
                UserTextBox.Foreground = new SolidColorBrush(_initialTextColor);
            }
        }

        private void ApplyFilter(string filterText, bool? isFilteringByFavorite = null)
        {
            var _collectionViewSource = FindResource("SortedConversations") as CollectionViewSource;
            if (_collectionViewSource != null)
            {
                _collectionViewSource.View.Filter = item =>
                {
                    var _conversationHistory = item as ConversationHistory;
                    if (_conversationHistory != null)
                    {
                        var _matchesTextFilter = string.IsNullOrEmpty(filterText) || _conversationHistory.Messages.Any(message => message.Content.Contains(filterText, StringComparison.OrdinalIgnoreCase));
                        var _matchesFavoriteFilter = isFilteringByFavorite == null || isFilteringByFavorite.Value == false || _conversationHistory.Favorite == isFilteringByFavorite.Value;
                        return _matchesTextFilter && _matchesFavoriteFilter;
                    }

                    return false;
                };

                _collectionViewSource.View.Refresh();
            }
        }

        private void OnFilterTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            _isFiltering = true;  
            var _isFilteringByFavorite = FavoriteFilterToggleButton.IsChecked;
            ApplyFilter(FilterTextBox.Text, _isFilteringByFavorite);
            _isFiltering = false;
        }

        private void ToggleFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Visibility = FilterTextBox.Visibility == Visibility.Visible
                   ? Visibility.Collapsed
                   : Visibility.Visible;
            FilterTextBoxClearButton.Visibility = FilterTextBoxClearButton.Visibility == Visibility.Visible
                   ? Visibility.Collapsed
                   : Visibility.Visible;
            FavoriteFilterToggleButton.Visibility = FavoriteFilterToggleButton.Visibility == Visibility.Visible
                   ? Visibility.Collapsed
                   : Visibility.Visible;
            FilterTextBox.Text = string.Empty;
            FavoriteFilterToggleButton.IsChecked = false;
            ApplyFilter("", false);
        }

        private void OnClearTextButtonClick(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Text = string.Empty;
        }

        private void OnFavoriteFilterToggleButtonClick(object sender, RoutedEventArgs e)
        {
            var _toggleButton = sender as ToggleButton;
            var _isFilteringByFavorite = _toggleButton.IsChecked;
            ApplyFilter(FilterTextBox.Text, _isFilteringByFavorite);
            FavoriteFilterToggleButton.Content = FavoriteFilterToggleButton.IsChecked == true ? "★" : "☆";
        }

        private void OnAttachFileButtonClick(object sender, RoutedEventArgs e)
        {
            var _button = sender as Button;
            if (_button.ContextMenu != null)
            {
                _button.ContextMenu.IsOpen = false;
                _button.ContextMenu.PlacementTarget = _button;
                _button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
                _button.ContextMenu.IsOpen = true;
            }
        }

        private void OnSelectFileClick(object sender, RoutedEventArgs e)
        {
            var _openFileDialog = new OpenFileDialog();
            _openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.webp;*.gif)|*.png;*.jpeg;*.jpg;*.webp;*.gif";
            _openFileDialog.Multiselect = false;

            if (_openFileDialog.ShowDialog() == true)
            {
                ImageFilePath = _openFileDialog.FileName;
                ImageFilePathLabel.Content = ImageFilePath;
                clipboardImage = null;
            }
        }

        private void OnPasteFromClipboardClick(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                var _image = Clipboard.GetImage();
                using (var _memoryStream = new MemoryStream())
                {
                    var _encoder = new PngBitmapEncoder();
                    _encoder.Frames.Add(BitmapFrame.Create(_image));
                    _encoder.Save(_memoryStream);
                    clipboardImage = _memoryStream.ToArray();
                    ImageFilePath = null;
                    ImageFilePathLabel.Content = "clipboard";
                }
            }
            else
            {
                ModernWpf.MessageBox.Show("The clipboard does not contain any images.", "error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            var _imageAvailable = Clipboard.ContainsImage();
            PasteFromClipboardMenuItem.IsEnabled = _imageAvailable;
        }

        private void OnClearImageFilePathLabelButtonClick(object sender, RoutedEventArgs e)
        {
            ImageFilePath = null;
            ImageFilePathLabel.Content = string.Empty;
        }

        private void OnImageFilePathLabelMouseUp(object sender, MouseButtonEventArgs e)
        {
            var _argument = $"/select, \"{ImageFilePath}\"";
            System.Diagnostics.Process.Start("explorer.exe", _argument);
        }

        private void OnShowLargeTextInputWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var _currentText = UserTextBox.Text;
            var _window = new LargeUserTextInput(_currentText);
            _window.Owner = this;
            _window.ShowDialog();
            UserTextBox.Focus();
        }

        private void OnToggleVisibilityPromptTemplateButtonClick(object sender, RoutedEventArgs e)
        {
            var _isCollapsed = PromptTemplateListBox.Visibility == Visibility.Collapsed;
            PromptTemplateListBox.Visibility = _isCollapsed ? Visibility.Visible : Visibility.Collapsed;
            NewTemplateButton.Visibility = _isCollapsed ? Visibility.Visible : Visibility.Collapsed;
            ToggleVisibilityPromptTemplateButton.Content = _isCollapsed ? "▼" : "▲";
            if (_isCollapsed)  
            {
                ChatListGridRow.Height = new GridLength(AppSettings.ChatListGridRowHeightSetting, GridUnitType.Star);
                PromptTemplateGridRow.Height = new GridLength(AppSettings.PromptTemplateGridRowHeightSaveSetting, GridUnitType.Star);
            }
            else  
            {
                AppSettings.ChatListGridRowHeightSetting = ChatListGridRow.ActualHeight;
                AppSettings.PromptTemplateGridRowHeightSaveSetting = PromptTemplateGridRow.ActualHeight;
                PromptTemplateGridRow.Height = new GridLength(0);
            }

            AppSettings.IsPromptTemplateListVisible = _isCollapsed;
        }

        /// <summary>
        /// Called when [calculator menu option click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/>
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
        /// <param name="e">The <see cref="EventArgs"/>
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
        /// <param name="e">The <see cref="EventArgs"/>
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
        /// <param name="e">The <see cref="EventArgs"/>
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
        /// <param name="e">The <see cref="EventArgs"/>
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
        /// <param name="e">The <see cref="EventArgs"/>
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
        /// <param name="e">The <see cref="EventArgs"/>
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
        /// <param name="e">The <see cref="EventArgs"/>
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
        /// <param name="e">The <see cref="EventArgs"/>
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
