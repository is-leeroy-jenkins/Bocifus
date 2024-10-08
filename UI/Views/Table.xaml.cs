﻿using ModernWpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.ObjectModels.RequestModels;
using Bocifus.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Bocifus.MainWindow;

namespace Bocifus
{
    using Model;

    public partial class Table
    {
        public ConversationHistory UpdatedConversationHistory { get; private set; }
        public class DataTableItem
        {
            public string? Role { get; set; }
            public string? Content { get; set; }
            public string? ImageUrl { get; set; }
        }
        public class ViewModel
        {
            public ObservableCollection<string> ComboBoxItems { get; set; }
            public ViewModel()
            {
                ComboBoxItems = new ObservableCollection<string>();
            }
        }
        ViewModel viewModel;
        public Table(ConversationHistory conversationHistory)
        {
            InitializeComponent();
            viewModel = new ViewModel();
            this.DataContext = viewModel;
            viewModel.ComboBoxItems.Add("user");
            viewModel.ComboBoxItems.Add("assistant");

            ObservableCollection<DataTableItem> list = new ObservableCollection<DataTableItem>();
            foreach (var message in conversationHistory.Messages)
            {
                var result = UtilityFunctions.ExtractUserAndImageFromMessage(message.Content);
                list.Add(new DataTableItem() { Role = message.Role, Content = result.userMessage, ImageUrl = result.image });
            }
            DataTable.ItemsSource = list;
            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light)
            {
                System.Windows.Media.Brush brush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#19000000"));
                DataTable.AlternatingRowBackground = brush;
            }
            else
            {
                System.Windows.Media.Brush brush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#19FFFFFF"));
                DataTable.AlternatingRowBackground = brush;
            }

            SetHistoryCountButton.Content = $"Set Number of Past Conversations: {AppSettings.ConversationHistoryCountSetting}";

            var accentColor = ThemeManager.Current.AccentColor;
            if (accentColor == null)
            {
                accentColor = SystemParameters.WindowGlassColor;
            }
            var accentColorBrush = new SolidColorBrush((System.Windows.Media.Color)accentColor);
            SaveButton.Background = accentColorBrush;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<DataTableItem> list = (ObservableCollection<DataTableItem>)DataTable.ItemsSource;
            UpdatedConversationHistory = new ConversationHistory();
            foreach (DataTableItem item in list)
            {
                if (item.Role == "user")  
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                    };
                    string contentJson = System.Text.Json.JsonSerializer.Serialize(new List<VisionUserContentItem>
                    {
                        new VisionUserContentItem { type = "text", text = item.Content },
                        new VisionUserContentItem { type = "image_url", image_url = new Image_Url { url = item.ImageUrl, detail = "auto" } }
                    }, options);

                    UpdatedConversationHistory.Messages.Add(new ChatMessage(item.Role, contentJson));
                }
                else
                {
                    UpdatedConversationHistory.Messages.Add(new ChatMessage(item.Role, item.Content));
                }
            }

            DialogResult = true;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = DialogResult == true;
        }
        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            ((DataGridTextColumn)e.Column).EditingElementStyle = (Style)this.Resources["editingTextBoxStyle"];
        }
        private void editingTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Return == e.Key && 0 < (ModifierKeys.Shift & e.KeyboardDevice.Modifiers))
            {
                TextBox tb = (TextBox)sender;
                int caret = tb.CaretIndex;
                tb.Text = tb.Text.Insert(caret, "\r\n");
                tb.CaretIndex = caret + 1;
                e.Handled = true;
            }
        }
        private void DataTable_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataTable.Columns.Count > 0)
            {
                DataTable.Columns[1].SetValue(DataGridTextColumn.ElementStyleProperty, new Style(typeof(TextBlock))
                {
                    Setters = {
                        new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap),
                        new Setter(TextBlock.PaddingProperty, new Thickness(5,5,5,5))
                    }
                });
                DataTable.Columns[1].Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);

                DataTable.Columns[2].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToHeader);

                DataGridTemplateColumn comboBoxColumn = new DataGridTemplateColumn();
                comboBoxColumn.Header = DataTable.Columns[0].Header;

                FrameworkElementFactory comboBoxFactory = new FrameworkElementFactory(typeof(ComboBox));

                Binding itemsSourceBinding = new Binding
                {
                    Path = new PropertyPath("ComboBoxItems"),
                    Mode = BindingMode.OneWay,
                    Source = viewModel
                };
                comboBoxFactory.SetBinding(ComboBox.ItemsSourceProperty, itemsSourceBinding);

                Binding selectedItemBinding = new Binding
                {
                    Path = new PropertyPath("Role"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                comboBoxFactory.SetValue(ComboBox.SelectedItemProperty, selectedItemBinding);

                comboBoxFactory.SetValue(ComboBox.WidthProperty, 100.0);

                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = comboBoxFactory;

                comboBoxColumn.CellTemplate = cellTemplate;

                DataTable.Columns.RemoveAt(0);

                DataTable.Columns.Insert(0, comboBoxColumn);
            }

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "Add new row after selected row";
            menuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Add);
            menuItem.Click += AddNewRowBeforeSelected_Click;
            contextMenu.Items.Add(menuItem);

            MenuItem deleteMenuItem = new MenuItem();
            deleteMenuItem.Header = "Delete selected row";
            deleteMenuItem.Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Delete);
            deleteMenuItem.Click += DeleteSelectedRow_Click;
            contextMenu.Items.Add(deleteMenuItem);

            DataTable.ContextMenu = contextMenu;
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataTable.Items.Count <= 1)
            {
                ModernWpf.MessageBox.Show("No conversation history.");
                return;
            }
            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Title = "Please select an export file.";
            string fileName = DateTime.Now.ToString("yyyyMMdd") + "_";
            if (((DataTableItem)DataTable.Items[0]).Content.Length < 20)
            {
                fileName += ((DataTableItem)DataTable.Items[0]).Content.Substring(0, ((DataTableItem)DataTable.Items[0]).Content.Length).Replace("/", "").Replace(":", "");
            }
            else
            {
                fileName += ((DataTableItem)DataTable.Items[0]).Content.Substring(0, 20).Replace("/", "").Replace(":", "") + "~";
            }
            dialog.FileName = fileName;
            dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            dialog.DefaultExt = "json";

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string json = JsonConvert.SerializeObject(DataTable.ItemsSource);
                json = JToken.Parse(json).ToString(Formatting.Indented);
                string path = dialog.FileName;
                File.WriteAllText(path, json);
                ModernWpf.MessageBox.Show("Exported successfully.");
            }
        }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "Please select an import file.";
            dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;
                string json = File.ReadAllText(path);
                ObservableCollection<DataTableItem> list = JsonConvert.DeserializeObject<ObservableCollection<DataTableItem>>(json);
                DataTable.ItemsSource = list;
                DataTable.Columns.RemoveAt(1);
                DataTable_Loaded(null, null);
                ModernWpf.MessageBox.Show("Imported successfully.");
            }
        }
        private void AddNewRowBeforeSelected_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = DataTable.SelectedIndex;

            if (selectedIndex >= 0)
            {
                DataTableItem item = new DataTableItem();
                item.Role = "User";
                item.Content = "";
                (DataTable.ItemsSource as ObservableCollection<DataTableItem>).Insert(selectedIndex + 1, item);
            }
            else
            {
                DataTableItem item = new DataTableItem();
                item.Role = "User";
                item.Content = "";
                (DataTable.ItemsSource as ObservableCollection<DataTableItem>).Insert(0, item);
            }
        }
        private void DeleteSelectedRow_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = DataTable.SelectedIndex;

            if (selectedIndex >= 0)
            {
                (DataTable.ItemsSource as ObservableCollection<DataTableItem>).RemoveAt(selectedIndex);
            }
        }
        private void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height = this.Owner.Height;
            this.Top = this.Owner.Top;
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
        private void SetHistoryCountButton_Click(object sender, RoutedEventArgs e)
        {
            int conversationHistoryCount = AppSettings.ConversationHistoryCountSetting;
            var window = new Messagebox("Conversation History Setting", "Adjust the number of past conversation histories to include in the conversation.", conversationHistoryCount);
            window.Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (window.ShowDialog() == true)
            {
                AppSettings.ConversationHistoryCountSetting = window.resultInt;
                SetHistoryCountButton.Content = $"Set Number of Past Conversations: {AppSettings.ConversationHistoryCountSetting}";
            }
        }
    }
}
