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

namespace Bocifus
{
    using Model;

    public partial class MainWindow
    {
        /// <summary>
        /// The busy
        /// </summary>
        private protected bool _busy;

        /// <summary>
        /// The path
        /// </summary>
        private protected object _path = new object();

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

        string selectInstructionContent = "";
        Stopwatch stopWatch = new Stopwatch();
        private bool gKeyPressed;
        private bool isFiltering = false;
        public string? imageFilePath = null;
        public bool visionEnabled = false;

        public MainWindow()
        {
            InitializeComponent();
            DataManagement.SettingsManager.InitializeSettings();
            InitializeUI();
            RecoverWindowBounds();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var collectionViewSource = FindResource("SortedConversations") as CollectionViewSource;
            if (collectionViewSource != null)
            {
                collectionViewSource.Source = AppSettings.ConversationManager.Histories;
                ConversationListBox.ItemsSource = collectionViewSource.View;
            }
            var promptTemplateSource = FindResource("SortedPromptTemplates") as CollectionViewSource;
            if (promptTemplateSource != null)
            {
                promptTemplateSource.Source = AppSettings.PromptTemplateManager.Templates;
                PromptTemplateListBox.ItemsSource = promptTemplateSource.View;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
            var settings = Properties.Settings.Default;
            settings.WindowMaximized = WindowState == WindowState.Maximized;
            WindowState = WindowState.Normal;  
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            if (SystemPromptGridColumn.Width.Value > 0)
            {
                Properties.Settings.Default.SystemPromptColumnWidth = SystemPromptGridColumn.Width.Value;
            }
            if (ConversationHistorytGridColumn.Width.Value > 0)
            {
                Properties.Settings.Default.ConversationColumnWidth = ConversationHistorytGridColumn.Width.Value;
            }
            settings.Save();
        }

        void RecoverWindowBounds()
        {
            var settings = Properties.Settings.Default;
            if (settings.WindowLeft >= 0 &&
                (settings.WindowLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
            { Left = settings.WindowLeft; }
            if (settings.WindowTop >= 0 &&
                (settings.WindowTop + settings.WindowHeight) < SystemParameters.VirtualScreenHeight)
            { Top = settings.WindowTop; }
            if (settings.WindowWidth > 0 &&
                settings.WindowWidth <= SystemParameters.WorkArea.Width)
            { Width = settings.WindowWidth; }
            if (settings.WindowHeight > 0 &&
                settings.WindowHeight <= SystemParameters.WorkArea.Height)
            { Height = settings.WindowHeight; }
            if (settings.WindowMaximized)
            {
                Loaded += (o, e) => WindowState = WindowState.Maximized;
            }
        }

        private void InitializeUI()
        {
            UtilityFunctions.InitialColorSet();
            ToastNotificationManagerCompat.OnActivated += this.ToastNotificationManagerCompat_OnActivated;
            UserTextBox.Focus();
            NoticeToggleSwitch.IsOn = AppSettings.NoticeFlgSetting;

            if (AppSettings.ConversationManager.Histories == null)
            {
                AppSettings.ConversationManager.Histories = new ObservableCollection<ConversationHistory>();
            }
            else
            {
                var selectedConversation = AppSettings.ConversationManager.Histories.FirstOrDefault(ch => ch.IsSelected);
                if (selectedConversation != null)
                {
                    ConversationListBox.SelectedItem = selectedConversation;
                    ConversationListBox.ScrollIntoView(selectedConversation);
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
            var appSettings = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal);
            Debug.Print("Path to save the configuration file:" + appSettings.FilePath);
            UtilityFunctions.InitializeConfigDataTable();
            UtilityFunctions.EnsureColumnsForType(AppSettings.ConfigDataTable, typeof(ConfigSettingWindow.ModelList));
            ConfigurationComboBox.ItemsSource = AppSettings.ConfigDataTable.AsEnumerable().Select(x => x.Field<string>("ConfigurationName")).ToList();
            ConfigurationComboBox.Text = AppSettings.SelectConfigSetting;
            UseConversationHistoryToggleSwitch.IsOn = AppSettings.UseConversationHistoryFlg;
            MessageScrollViewer.ScrollToBottom();
            InitializeSystemPromptColumn();
            bool isCollapsed = !(AppSettings.IsPromptTemplateListVisible);
            PromptTemplateListBox.Visibility = isCollapsed ? Visibility.Collapsed : Visibility.Visible;
            NewTemplateButton.Visibility = isCollapsed ? Visibility.Collapsed : Visibility.Visible;
            ToggleVisibilityPromptTemplateButton.Content = isCollapsed ? "▲" : "▼";
            var currentPadding = UserTextBox.Padding;
            if (AppSettings.TranslationAPIUseFlg == true)
            {
                TranslateButton.Visibility = Visibility.Visible;
                UserTextBox.Padding = new Thickness(currentPadding.Left, currentPadding.Top, 30, currentPadding.Bottom);
            }
            else
            {
                TranslateButton.Visibility = Visibility.Collapsed;
                UserTextBox.Padding = new Thickness(currentPadding.Left, currentPadding.Top, 10, currentPadding.Bottom);
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                string currentText = UserTextBox.Text;
                var window = new LargeUserTextInput(currentText);
                window.Owner = this;
                window.ShowDialog();
                UserTextBox.Focus();
            }
            if (e.Key == Key.F3)
            {
                ShowTable();
            }
        }

        private void AcrylicWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                NewChatButton_Click(sender, e);
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                try
                {
                    DataManagement.DataManager.SaveConversationsAsJson(AppSettings.ConversationManager);
                    DataManagement.DataManager.SavePromptTemplateAsJson(AppSettings.PromptTemplateManager);
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    ModernWpf.MessageBox.Show("Saved to " + documentsPath + @"\OpenAIOnWPF\ConversationHistory"
                                                + "\r\n" + documentsPath + @"\OpenAIOnWPF\PromptTemplate"
                                                ,"Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    e.Handled = true;
                }
                catch
                (Exception ex)
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

        private void UserTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _ = ProcessOpenAIAsync(UserTextBox.Text);
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt))
            {
                if (AppSettings.TranslationAPIUseFlg == true)
                {
                    TranslateButton_Click(sender, e);
                }
            }
            else if (e.Key == Key.K && Keyboard.Modifiers == ModifierKeys.Control)
            {
                double newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
            }
            else if (e.Key == Key.J && Keyboard.Modifiers == ModifierKeys.Control)
            {
                double newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
            }
        }

        private void ExecButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(string.IsNullOrWhiteSpace(UserTextBox.Text)))
            {
                _ = ProcessOpenAIAsync(UserTextBox.Text);
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void AssistantMessageGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isProcessing)
            {
                CancelButton.Visibility = Visibility.Visible;
            }
        }

        private void AssistantMessageGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            CancelButton.Visibility = Visibility.Collapsed;
        }

        private void UserTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var tokens = TokenizerGpt3.Encode(UserTextBox.Text);
            string tooltip = $"Tokens : {tokens.Count()}";
            UserTextBox.ToolTip = tooltip;
        }

        private void UserTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
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

        private void NoticeToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.NoticeFlgSetting = (bool)NoticeToggleSwitch.IsOn;
        }

        private void TokensLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UtilityFunctions.ShowMessagebox("Tokens", TokensLabel.ToolTip.ToString());
        }

        private void ConfigurationComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

            string selectedConfigName = ConfigurationComboBox.SelectedItem.ToString();
            var row = AppSettings.ConfigDataTable.AsEnumerable()
                        .FirstOrDefault(x => x.Field<string>("ConfigurationName") == selectedConfigName);

            if (row != null)
            {
                visionEnabled = row.Field<bool>("Vision");
            }

            AttachFileButton.Visibility = visionEnabled ? Visibility.Visible : Visibility.Collapsed;
            var currentPadding = UserTextBox.Padding;
            int leftPadding = visionEnabled ? 35 : 10;
            UserTextBox.Padding = new Thickness(leftPadding, currentPadding.Top, currentPadding.Right, currentPadding.Bottom);
        }

        private void SystemPromptComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SystemPromptComboBox2.SelectedIndex = SystemPromptComboBox.SelectedIndex;
            if (SystemPromptComboBox.SelectedItem == "")
            {
                AppSettings.InstructionSetting = "";
                return;
            }

            AppSettings.InstructionSetting = SystemPromptComboBox.SelectedItem.ToString();
            string selectInstructionContent = "";
            if (!String.IsNullOrEmpty(AppSettings.InstructionSetting))
            {
                string[] instructionList = AppSettings.InstructionListSetting?.Cast<string>().Where((s, i) => i % 2 == 0).ToArray();
                int index = Array.IndexOf(instructionList, AppSettings.InstructionSetting);
                selectInstructionContent = AppSettings.InstructionListSetting[index, 1];
            }

            SystemPromptComboBox.ToolTip = "# " 
                + AppSettings.InstructionSetting 
                + "\r\n"
                + selectInstructionContent;
        }

        private void SystemPromptComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SystemPromptComboBox.SelectedIndex = SystemPromptComboBox2.SelectedIndex;
            string selectInstructionContent = "";
            if (!String.IsNullOrEmpty(SystemPromptComboBox2.SelectedItem.ToString()))
            {
                string[] instructionList = AppSettings.InstructionListSetting?.Cast<string>().Where((s, i) => i % 2 == 0).ToArray();
                int index = Array.IndexOf(instructionList, SystemPromptComboBox2.SelectedItem.ToString());
                selectInstructionContent = AppSettings.InstructionListSetting[index, 1];
            }

            SystemPromptContentsTextBox.Text = selectInstructionContent;
            UnsavedLabel.Visibility = Visibility.Collapsed;
        }

        private void UserTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
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

        private void TokenUsage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var window = new TokenUsageWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ConfigurationSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ConfigSettingWindow();
            window.Owner = this;
            window.ShowDialog();
            ConfigurationComboBox.ItemsSource = AppSettings.ConfigDataTable.AsEnumerable().Select(x => x.Field<string>("ConfigurationName")).ToList();
            UpdateUIBasedOnVision();
        }

        private void InstructionSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new InstructionSettingWindow(AppSettings.InstructionListSetting);
            window.Owner = this;
            bool result = (bool)window.ShowDialog();
            if (result)
            {
                AppSettings.InstructionListSetting = result ? window.inputResult : null;
                string[] instructionList = AppSettings.InstructionListSetting?.Cast<string>().Where((s, i) => i % 2 == 0).ToArray();
                Array.Resize(ref instructionList, instructionList.Length + 1);
                instructionList[instructionList.Length - 1] = "";
                SystemPromptComboBox.ItemsSource = instructionList;
                SystemPromptComboBox2.ItemsSource = instructionList;
            }
        }

        private void ColorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new ColorSettings();
            window.Owner = this;
            window.ShowDialog();
        }

        private void TranslationAPIMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new TranslationAPISettingWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void TitleGenerationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new TitleGenerationSettings();
            window.Owner = this;
            window.ShowDialog();
        }

        private void VersionInformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new VersionWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            UserTextBox.Height = Math.Max(UserTextBox.ActualHeight + e.VerticalChange, UserTextBox.MinHeight);
        }

        private void UserTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
            ConversationListBox.Focus();  
        }

        private void MessageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Grid messageGrid)
            {
                if (messageGrid.ActualWidth * 0.8 > 1200)
                {
                    messageGrid.ColumnDefinitions[1].Width = new GridLength(1200);
                }
                else
                {
                    messageGrid.ColumnDefinitions[1].Width = new GridLength(messageGrid.ActualWidth * 0.8);
                }
            }
        }

        private ContextMenu CreateContextMenu(string paragraphText = null)
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem copyTextMenuItem = new MenuItem();
            copyTextMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Copy);
            contextMenu.Opened += (s, e) => UpdateMenuItemButtonContent(contextMenu.PlacementTarget, copyTextMenuItem);
            Action copyTextAndCloseMenu = () =>
            {
                contextMenu.IsOpen = false;
            };

            copyTextMenuItem.Click += (s, e) => copyTextAndCloseMenu();
            copyTextMenuItem.Click += CopyTextToClipboard;
            copyTextMenuItem.Header = "Copy Text";

            void CopyTextToClipboard(object sender, RoutedEventArgs e)
            {
                var target = contextMenu.PlacementTarget;
                if (target is TextBox textBox)
                {
                    string textToCopy = textBox.SelectedText.Length > 0 ? textBox.SelectedText : textBox.Text;
                    Clipboard.SetText(textToCopy);
                }
                else if (target is MarkdownScrollViewer markdownScrollViewer)
                {
                    TextRange selectedTextRange = new TextRange(markdownScrollViewer.Selection.Start, markdownScrollViewer.Selection.End);
                    if (!string.IsNullOrEmpty(selectedTextRange.Text))    
                    {
                        Clipboard.SetText(selectedTextRange.Text);
                    }
                    else
                    {
                        var mousePos = Mouse.GetPosition(markdownScrollViewer);  
                        Visual hitVisual = markdownScrollViewer.InputHitTest(mousePos) as Visual;
                        if (hitVisual is ICSharpCode.AvalonEdit.Rendering.TextView editor)    
                        {
                            Clipboard.SetText(editor.Document.Text);     
                        }
                        else   
                        {
                            Clipboard.SetText(markdownScrollViewer.Markdown);
                        }
                    }
                }
                else if (target is ICSharpCode.AvalonEdit.Rendering.TextView textView)
                {
                    var mousePos = Mouse.GetPosition(textView);  
                    Visual hitVisual = textView.InputHitTest(mousePos) as Visual;
                    if (hitVisual is ICSharpCode.AvalonEdit.Rendering.TextView editor)
                    {
                        Clipboard.SetText(editor.Document.Text);     
                    }
                    else
                    {
                        Clipboard.SetText(textView.Document.Text);
                    }
                }
            }

            contextMenu.Items.Add(copyTextMenuItem);
            contextMenu.Items.Add(new Separator());
            MenuItem currentFontSizeMenuItem = new MenuItem();
            currentFontSizeMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontSize);
            currentFontSizeMenuItem.Header = $"Font Size: {Properties.Settings.Default.FontSize}pt";
            contextMenu.Items.Add(currentFontSizeMenuItem);
            MenuItem increaseFontSizeMenuItem = new MenuItem();
            increaseFontSizeMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontIncrease);
            Button increaseFontSizeButton = new Button { Content = "Increase Font Size", Background = Brushes.Transparent };
            increaseFontSizeMenuItem.Header = increaseFontSizeButton;
            increaseFontSizeButton.Click += (s, e) => SetFontSize(Properties.Settings.Default.FontSize + 1, currentFontSizeMenuItem);
            increaseFontSizeMenuItem.Click += (s, e) => SetFontSize(Properties.Settings.Default.FontSize + 1, currentFontSizeMenuItem);
            MenuItem decreaseFontSizeMenuItem = new MenuItem();
            decreaseFontSizeMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontDecrease);
            Button decreaseFontSizeButton = new Button { Content = "Decrease Font Size", Background = Brushes.Transparent };
            decreaseFontSizeMenuItem.Header = decreaseFontSizeButton;
            decreaseFontSizeButton.Click += (s, e) => SetFontSize(Properties.Settings.Default.FontSize - 1, currentFontSizeMenuItem);
            decreaseFontSizeMenuItem.Click += (s, e) => SetFontSize(Properties.Settings.Default.FontSize - 1, currentFontSizeMenuItem);
            MenuItem defaultFontSizeMenuItem = new MenuItem { Header = "Default Font Size" };
            defaultFontSizeMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Refresh);
            Button defaultFontSizeButton = new Button { Content = "Default Font Size", Background = Brushes.Transparent };
            defaultFontSizeMenuItem.Header = defaultFontSizeButton;
            defaultFontSizeButton.Click += (s, e) => SetFontSize(16, currentFontSizeMenuItem);
            defaultFontSizeMenuItem.Click += (s, e) => SetFontSize(16, currentFontSizeMenuItem);
            currentFontSizeMenuItem.Items.Add(increaseFontSizeMenuItem);
            currentFontSizeMenuItem.Items.Add(decreaseFontSizeMenuItem);
            currentFontSizeMenuItem.Items.Add(defaultFontSizeMenuItem);
            MenuItem currentFontWeightMenuItem = new MenuItem();
            currentFontWeightMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Font);
            currentFontWeightMenuItem.Header = $"Font Weight: {Properties.Settings.Default.FontWeight}";
            contextMenu.Items.Add(currentFontWeightMenuItem);
            MenuItem increaseFontWeightMenuItem = new MenuItem();
            increaseFontWeightMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontIncrease);
            Button increaseFontWeightButton = new Button { Content = "Increase Font Weight", Background = Brushes.Transparent };
            increaseFontWeightMenuItem.Header = increaseFontWeightButton;
            increaseFontWeightButton.Click += (s, e) => SetFontWeight(Properties.Settings.Default.FontWeight + 50, currentFontWeightMenuItem);
            increaseFontWeightMenuItem.Click += (s, e) => SetFontWeight(Properties.Settings.Default.FontWeight + 50, currentFontWeightMenuItem);
            MenuItem decreaseFontWeightMenuItem = new MenuItem();
            decreaseFontWeightMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.FontDecrease);
            Button decreaseFontWeightButton = new Button { Content = "Decrease Font Weight", Background = Brushes.Transparent };
            decreaseFontWeightMenuItem.Header = decreaseFontWeightButton;
            decreaseFontWeightButton.Click += (s, e) => SetFontWeight(Properties.Settings.Default.FontWeight - 50, currentFontWeightMenuItem);
            decreaseFontWeightMenuItem.Click += (s, e) => SetFontWeight(Properties.Settings.Default.FontWeight - 50, currentFontWeightMenuItem);
            MenuItem defaultFontWeightMenuItem = new MenuItem { Header = "Default Font Weight" };
            defaultFontWeightMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Refresh);
            Button defaultFontWeightButton = new Button { Content = "Default Font Weight", Background = Brushes.Transparent };
            defaultFontWeightMenuItem.Header = defaultFontWeightButton;
            defaultFontWeightButton.Click += (s, e) => SetFontWeight(400, currentFontWeightMenuItem);
            defaultFontWeightMenuItem.Click += (s, e) => SetFontWeight(400, currentFontWeightMenuItem);
            currentFontWeightMenuItem.Items.Add(increaseFontWeightMenuItem);
            currentFontWeightMenuItem.Items.Add(decreaseFontWeightMenuItem);
            currentFontWeightMenuItem.Items.Add(defaultFontWeightMenuItem);

            void SetFontSize(int newSize, MenuItem menuItem)
            {
                int minSize = 8;
                int maxSize = 32;
                newSize = Math.Max(minSize, Math.Min(maxSize, newSize));
                Properties.Settings.Default.FontSize = newSize;
                Properties.Settings.Default.Save();
                foreach (var item in MessagesPanel.Children)
                {
                    if (item is Grid grid)
                    {
                        foreach (var child in grid.Children)
                        {
                            if (child is TextBox textBox)
                            {
                                textBox.FontSize = newSize;
                            }
                            else if (child is MarkdownScrollViewer markdownScrollViewer)
                            {
                                markdownScrollViewer.Document.FontSize = newSize;
                            }
                        }
                    }
                }

                menuItem.Header = $"Font Size: {Properties.Settings.Default.FontSize}pt";
            }

            void SetFontWeight(int newWeight, MenuItem menuItem)
            {
                int minSize = 300;
                int maxSize = 600;
                newWeight = Math.Max(minSize, Math.Min(maxSize, newWeight));
                Properties.Settings.Default.FontWeight = newWeight;
                Properties.Settings.Default.Save();
                foreach (var item in MessagesPanel.Children)
                {
                    if (item is Grid grid)
                    {
                        foreach (var child in grid.Children)
                        {
                            if (child is TextBox textBox)
                            {
                                textBox.FontWeight = FontWeight.FromOpenTypeWeight(newWeight);
                            }
                            else if (child is MarkdownScrollViewer markdownScrollViewer)
                            {
                                markdownScrollViewer.Document.FontWeight = FontWeight.FromOpenTypeWeight(newWeight);
                            }
                        }
                    }
                }

                menuItem.Header = $"Font Weight: {Properties.Settings.Default.FontWeight}";
            }

            if (paragraphText is not null && IsMermaidCode(paragraphText))
            {
                contextMenu.Items.Add(new Separator());
                MenuItem mermaidMenuItem = new MenuItem();
                mermaidMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.AllApps);
                Action mermaidTextAndCloseMenu = () =>
                {
                    MermaidPreviewContextMenu_Click(paragraphText);
                    contextMenu.IsOpen = false;
                };

                mermaidMenuItem.Click += (s, e) => mermaidTextAndCloseMenu();
                mermaidMenuItem.Header = "Mermaid Preview";
                contextMenu.Items.Add(mermaidMenuItem);
            }

            if (paragraphText is not null && IsMarkdownTable(paragraphText))
            {
                contextMenu.Items.Add(new Separator());
                MenuItem copyTableMenuItem = new MenuItem();
                copyTableMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Copy);
                Action copyTableAndCloseMenu = () =>
                {
                    CopyMarkdownTableToClipboard(paragraphText);
                    contextMenu.IsOpen = false;
                };

                copyTableMenuItem.Click += (s, e) => copyTableAndCloseMenu();
                copyTableMenuItem.Header = "Copy Table to Clipboard";
                contextMenu.Items.Add(copyTableMenuItem);
                MenuItem exportCsvMenuItem = new MenuItem();
                exportCsvMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Download);
                Action exportCsvAndCloseMenu = () =>
                {
                    ExportCsvContextMenu_Click(paragraphText);
                    contextMenu.IsOpen = false;
                };

                exportCsvMenuItem.Click += (s, e) => exportCsvAndCloseMenu();
                exportCsvMenuItem.Header = "Export CSV";
                contextMenu.Items.Add(exportCsvMenuItem);
            }

            return contextMenu;
        }

        private void MermaidPreviewContextMenu_Click(string text)
        {
            ShowMermaidPreview(text);
        }

        private void ShowMermaidPreview(string mermaidCode)
        {
            string theme;
            string backgroundColor;
            if (ThemeManager.Current.ActualApplicationTheme == ModernWpf.ApplicationTheme.Dark)
            {
                theme = "dark";
                backgroundColor = "#333";
            }
            else
            {
                theme = "default";
                backgroundColor = "#FFFFFF";
            }

            string htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ background-color: {backgroundColor}; }}
    </style>
    <script src='https://cdn.jsdelivr.net/npm/mermaid@10.9.0/dist/mermaid.min.js'></script>
    <script>mermaid.initialize({{startOnLoad:true, theme: '{theme}'}});</script>
</head>
<body>
    <div class='mermaid'>
{mermaidCode}
    </div>
</body>
</html>";
            var previewWindow = new WebBrowserPreview(htmlContent);
            double parentCenterX = this.Left + (this.Width / 2);
            double parentCenterY = this.Top + (this.Height / 2);
            previewWindow.Left = parentCenterX - (previewWindow.Width / 2);
            previewWindow.Top = parentCenterY - (previewWindow.Height / 2);
            previewWindow.Show();
        }

        public static bool IsMermaidCode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            string[] patterns = new string[]
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

            string firstLine = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(firstLine, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    return true;
            }

            return false;
        }

        private void ExportCsvContextMenu_Click(string text)
        {
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var csvLines = new System.Text.StringBuilder();
            foreach (var line in lines)
            {
                if (!line.StartsWith("|")) continue;  
                if (line.Contains("---")) continue;
                var cleanedLine = line.Trim('|', ' ');
                var values = cleanedLine.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var csvLine = string.Join(",", values.Select(v => v.Trim()));
                csvLines.AppendLine(csvLine);
            }

            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Title = "Please select an export file.";
            dialog.FileName = DateTime.Now.ToString("yyyyMMdd") + "_output.csv";
            dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.DefaultExt = "csv";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (ContainsJapanese(csvLines.ToString()))
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    Encoding sjisEncoding = Encoding.GetEncoding("shift_jis");
                    File.WriteAllText(dialog.FileName, csvLines.ToString(), sjisEncoding);
                }
                else
                {
                    File.WriteAllText(dialog.FileName, csvLines.ToString());
                }

                ModernWpf.MessageBox.Show("Exported successfully.");
            }
        }

        public static void CopyMarkdownTableToClipboard(string markdownText)
        {
            var lines = markdownText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var tableData = new List<List<string>>();

            foreach (var line in lines)
            {
                if (!line.StartsWith("|")) continue;  
                if (line.Contains("---")) continue;  

                var row = line.Trim('|').Split('|').Select(s => s.Trim()).ToList();
                tableData.Add(row);
            }

            if (tableData.Count > 0)
            {
                var stringBuilder = new System.Text.StringBuilder();
                foreach (var row in tableData)
                {
                    stringBuilder.AppendLine(string.Join("\t", row));
                }
                Clipboard.SetText(stringBuilder.ToString());
                Console.WriteLine("Table data has been copied to the clipboard. You can now paste it into Excel.");
            }
            else
            {
                Console.WriteLine("No table data found in the markdown text.");
            }
        }

        static bool IsMarkdownTable(string text)
        {
            string pattern = @"^\|.*\|\s*\n\|\s*[-:]+\s*\|";
            return Regex.IsMatch(text, pattern, RegexOptions.Multiline);
        }

        public static bool ContainsJapanese(string text)
        {
            return text.Any(c => (c >= 0x3040 && c <= 0x30FF) ||   
                                 (c >= 0x4E00 && c <= 0x9FAF) ||   
                                 (c >= 0xFF66 && c <= 0xFF9D));    
        }

        void UpdateMenuItemButtonContent(object target, MenuItem menuItem)
        {
            string headerText = "Copy All Text";
            if (target is TextBox textBox && !string.IsNullOrEmpty(textBox.SelectedText))
            {
                headerText = "Copy Selected Text";
            }
            else if (target is MarkdownScrollViewer markdownScrollViewer)
            {
                TextRange selectedTextRange = new TextRange(markdownScrollViewer.Selection.Start, markdownScrollViewer.Selection.End);
                if (!string.IsNullOrEmpty(selectedTextRange.Text))
                {
                    headerText = "Copy Selected Text";
                }
                else
                {
                    var mousePos = Mouse.GetPosition(markdownScrollViewer);  
                    Visual hitVisual = markdownScrollViewer.InputHitTest(mousePos) as Visual;
                    if (hitVisual is ICSharpCode.AvalonEdit.Rendering.TextView editor)
                    {
                        headerText = "Copy Code Block Text";
                    }
                    else
                    {
                        headerText = "Copy All Text";
                    }
                }
            }
            else if (target is ICSharpCode.AvalonEdit.Rendering.TextView textView)
            {
                var mousePos = Mouse.GetPosition(textView);  
                Visual hitVisual = textView.InputHitTest(mousePos) as Visual;
                if (hitVisual is ICSharpCode.AvalonEdit.Rendering.TextView editor)
                {
                    headerText = "Copy Code Block Text";
                }
                else
                {
                    headerText = "Copy All Text";
                }
            }

            menuItem.Header = headerText;
        }

        private void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIElement element = sender as UIElement;
            while (element != null)
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                if (element is ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (e.Delta / 3));
                    e.Handled = true;
                    return;
                }
            }
        }

        private void MessageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            bool isAtBottom = MessageScrollViewer.VerticalOffset >= MessageScrollViewer.ScrollableHeight;
            BottomScrollButton.Visibility = isAtBottom ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BottomScrollButton_Click(object sender, RoutedEventArgs e)
        {
            MessageScrollViewer.ScrollToBottom();
        }

        private void MessageScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                MessageScrollViewer.ScrollToBottom();
                gKeyPressed = false;
            }
            else if (e.Key == Key.G)
            {
                if (gKeyPressed)
                {
                    MessageScrollViewer.ScrollToTop();
                    gKeyPressed = false;
                }
                else
                {
                    gKeyPressed = true;
                }
            }
            else if (e.Key == Key.Home)
            {
                MessageScrollViewer.ScrollToTop();
                gKeyPressed = false;
            }
            else if (e.Key == Key.End)
            {
                MessageScrollViewer.ScrollToBottom();
                gKeyPressed = false;
            }
            else if (e.Key == Key.U && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                double newVerticalOffset = MessageScrollViewer.VerticalOffset - (MessageScrollViewer.ViewportHeight / 2);
                MessageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
                gKeyPressed = false;
            }
            else if (e.Key == Key.D && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                double newVerticalOffset = MessageScrollViewer.VerticalOffset + (MessageScrollViewer.ViewportHeight / 2);
                MessageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
                gKeyPressed = false;
            }
            else if (e.Key == Key.E && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                double newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
                gKeyPressed = false;
            }
            else if (e.Key == Key.Y && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                double newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
                gKeyPressed = false;
            }
            else if (e.Key == Key.J)
            {
                double newVerticalOffset = MessageScrollViewer.VerticalOffset + 20;
                MessageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
                gKeyPressed = false;
            }
            else if (e.Key == Key.K)
            {
                double newVerticalOffset = MessageScrollViewer.VerticalOffset - 20;
                MessageScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
                gKeyPressed = false;
            }
            else
            {
                gKeyPressed = false;
            }
        }

        private void OpenSytemPromptWindowButton_Click(object sender, RoutedEventArgs e)
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

        private void SystemPromptContentsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UnsavedLabel.Visibility = Visibility.Visible;
        }

        private void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            MessagesPanel.Children.Clear();
            if (ConversationListBox.SelectedItem is ConversationHistory selectedItem)
            {
                selectedItem.IsSelected = false;
            }

            ConversationListBox.SelectedItem = null;
            UserTextBox.Focus();
            UserTextBox.CaretIndex = UserTextBox.Text.Length;
        }

        private void ConversationDeleteButton_Click( object sender, RoutedEventArgs e )
        {
            ConversationHistory itemToDelete = null;
            if( sender is MenuItem )
            {
                itemToDelete = ( ConversationHistory )( ( MenuItem )sender ).DataContext;
            }

            if( sender is ContextMenu )
            {
                itemToDelete = ( ConversationHistory )( ( ContextMenu )sender ).DataContext;
            }

            var result = ModernWpf.MessageBox.Show(
                "Are you sure you want to delete this conversation?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question );

            if( result == MessageBoxResult.Yes )
            {
                AppSettings.ConversationManager.Histories.Remove( itemToDelete );
                ConversationListBox.Items.Refresh( );
            }
        }

        private void ConversationTitleEditButton_Click(object sender, RoutedEventArgs e)
        {
            ConversationHistory itemToDelete = null;
            if (sender is MenuItem)
            {
                itemToDelete = (ConversationHistory)((MenuItem)sender).DataContext;
            }

            if (sender is ContextMenu)
            {
                itemToDelete = (ConversationHistory)((ContextMenu)sender).DataContext;
            }

            string currentTitle = itemToDelete.Title;
            var editWindow = new TitleEditWindow(currentTitle);
            editWindow.Owner = this;
            if (editWindow.ShowDialog() == true)
            {
                string newTitle = editWindow.NewTitle;
                itemToDelete.Title = newTitle;
            }
        }

        private void ConversationListBoxContextMenu_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                ConversationFavoriteButton_Click(sender, e);
            }

            if (e.Key == Key.T)
            {
                ConversationTitleEditButton_Click(sender, e);
            }

            if (e.Key == Key.D)
            {
                ConversationDeleteButton_Click(sender, e);
            }
        }

        private void ConversationFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            ConversationHistory item = null;
            if (sender is MenuItem)
            {
                item = (ConversationHistory)((MenuItem)sender).DataContext;
            }

            if (sender is ContextMenu)
            {
                item = (ConversationHistory)((ContextMenu)sender).DataContext;
            }

            item.Favorite = !item.Favorite;
        }

        private void ConversationListBoxMoreButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.ContextMenu != null)
            {
                button.ContextMenu.IsOpen = false;
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                button.ContextMenu.IsOpen = true;
            }
        }

        public void RefreshConversationList()
        {
            var collectionViewSource = FindResource("SortedConversations") as CollectionViewSource;
            if (collectionViewSource != null)
            {
                collectionViewSource.Source = AppSettings.ConversationManager.Histories;
                collectionViewSource.View.Refresh();
            }
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Activate();
                this.Topmost = true;
                this.Topmost = false;
            });
        }

        private async void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard? animation = null;
            Color initialTextColor;
            try
            {
                TranslateButton.IsEnabled = false;
                animation = UtilityFunctions.CreateTextColorAnimation(UserTextBox, out initialTextColor);
                animation.Begin();
                string resultText = await TranslateAPIRequestAsync(UserTextBox.Text, AppSettings.ToTranslationLanguage);
                UserTextBox.Text = resultText;
                UserTextBox.CaretIndex = UserTextBox.Text.Length;
            }
            catch (Exception ex)
            {
                ModernWpf.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                TranslateButton.IsEnabled = true;
                animation?.Stop();
                UserTextBox.Foreground = new SolidColorBrush(initialTextColor);
            }
        }

        private void ApplyFilter(string filterText, bool? isFilteringByFavorite = null)
        {
            var collectionViewSource = FindResource("SortedConversations") as CollectionViewSource;
            if (collectionViewSource != null)
            {
                collectionViewSource.View.Filter = item =>
                {
                    var conversationHistory = item as ConversationHistory;
                    if (conversationHistory != null)
                    {
                        bool matchesTextFilter = string.IsNullOrEmpty(filterText) || conversationHistory.Messages.Any(message => message.Content.Contains(filterText, StringComparison.OrdinalIgnoreCase));
                        bool matchesFavoriteFilter = isFilteringByFavorite == null || isFilteringByFavorite.Value == false || conversationHistory.Favorite == isFilteringByFavorite.Value;
                        return matchesTextFilter && matchesFavoriteFilter;
                    }

                    return false;
                };

                collectionViewSource.View.Refresh();
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            isFiltering = true;  
            bool? isFilteringByFavorite = FavoriteFilterToggleButton.IsChecked;
            ApplyFilter(FilterTextBox.Text, isFilteringByFavorite);
            isFiltering = false;
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

        private void ClearTextButton_Click(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Text = string.Empty;
        }

        private void FavoriteFilterToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            bool? isFilteringByFavorite = toggleButton.IsChecked;
            ApplyFilter(FilterTextBox.Text, isFilteringByFavorite);
            FavoriteFilterToggleButton.Content = FavoriteFilterToggleButton.IsChecked == true ? "★" : "☆";
        }

        private void AttachFileButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.ContextMenu != null)
            {
                button.ContextMenu.IsOpen = false;
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.webp;*.gif)|*.png;*.jpeg;*.jpg;*.webp;*.gif";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                imageFilePath = openFileDialog.FileName;
                ImageFilePathLabel.Content = imageFilePath;
                clipboardImage = null;
            }
        }

        private void PasteFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                using (var memoryStream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(memoryStream);
                    clipboardImage = memoryStream.ToArray();
                    imageFilePath = null;
                    ImageFilePathLabel.Content = "clipboard";
                }
            }
            else
            {
                ModernWpf.MessageBox.Show("The clipboard does not contain any images.", "error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            bool imageAvailable = Clipboard.ContainsImage();
            PasteFromClipboardMenuItem.IsEnabled = imageAvailable;
        }

        private void ClearImageFilePathLabelButton_Click(object sender, RoutedEventArgs e)
        {
            imageFilePath = null;
            ImageFilePathLabel.Content = string.Empty;
        }

        private void ImageFilePathLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string argument = $"/select, \"{imageFilePath}\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void ShowLargeTextInputWindowButton_Click(object sender, RoutedEventArgs e)
        {
            string currentText = UserTextBox.Text;
            var window = new LargeUserTextInput(currentText);
            window.Owner = this;
            window.ShowDialog();
            UserTextBox.Focus();
        }

        private void ToggleVisibilityPromptTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            bool isCollapsed = PromptTemplateListBox.Visibility == Visibility.Collapsed;
            PromptTemplateListBox.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;
            NewTemplateButton.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;
            ToggleVisibilityPromptTemplateButton.Content = isCollapsed ? "▼" : "▲";
            if (isCollapsed)  
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

            AppSettings.IsPromptTemplateListVisible = isCollapsed;
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
