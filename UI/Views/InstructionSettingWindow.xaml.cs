using ModernWpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Bocifus.MainWindow;

namespace Bocifus
{
    /// <summary>
    /// InstructionSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InstructionSettingWindow
    {
        public string[,] inputResult => items;
        string[,] items { get; set; }
        public InstructionSettingWindow(string[,] param)
        {
            InitializeComponent();
            items = param;

            var accentColor = ThemeManager.Current.AccentColor;
            if (accentColor == null)
            {
                accentColor = SystemParameters.WindowGlassColor;
            }
            var accentColorBrush = new SolidColorBrush((Color)accentColor);
            SaveButton.Background = accentColorBrush;

            InstructionListBox.ContextMenu = new ContextMenu();
            var UpSwap = new MenuItem();
            UpSwap.Header = "⬆";
            UpSwap.Click += UpSwap_Click;
            UpSwap.HorizontalAlignment = HorizontalAlignment.Center;
            var DownSwap = new MenuItem();
            DownSwap.Header = "⬇";
            DownSwap.Click += DownSwap_Click;
            DownSwap.HorizontalAlignment = HorizontalAlignment.Center;
            InstructionListBox.ContextMenu.Items.Add(UpSwap);
            InstructionListBox.ContextMenu.Items.Add(DownSwap);

            // itemsがnullなら[0,0]で初期化
            if (items == null)
            {
                items = new string[1, 2];
                items[0, 0] = "";
                items[0, 1] = "";
            }

            // itemsの1列目をInstructionListBoxに格納
            for (var i = 0; i < items.GetLength(0); i++)
            {
                InstructionListBox.Items.Add(items[i, 0]);
            }
            // コンボボックスで選択している指示を開く
            for (var i = 0; i < items.GetLength(0); i++)
            {
                if (AppSettings.InstructionSetting == "" || AppSettings.InstructionSetting == null)
                {
                    InstructionListBox.SelectedIndex = 0;
                    break;
                }
                if (items[i, 0] == AppSettings.InstructionSetting)
                {
                    InstructionListBox.SelectedIndex = i;
                    break;
                }
            }
        }
        private void UpdateInstructionListBox()
        {
            InstructionListBox.Items.Clear();
            for (var i = 0; i < items.GetLength(0); i++)
            {
                InstructionListBox.Items.Add(items[i, 0]);
            }
        }
        private void DuplicateControl()
        {
            for (var i = 0; i < items.GetLength(0); i++)
            {
                var currentName = items[i, 0];

                for (var j = 0; j < items.GetLength(0); j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    if (items[i, 0] == items[j, 0])
                    {
                        currentName += "*";
                        items[j, 0] = currentName;
                    }
                }
            }
        }
        private void Save()
        {
            if (InstructionListBox.SelectedIndex == -1)
            {
                return;
            }
            if (InstructionTextBox.Text == "")
            {
                ModernWpf.MessageBox.Show("The instruction name has not been entered.", "Error", MessageBoxButton.OK);
                return;
            }
            var index = InstructionListBox.SelectedIndex;
            items[index, 0] = InstructionTextBox.Text;
            items[index, 1] = ContentsTextBox.Text;

            DuplicateControl();
            UpdateInstructionListBox();
            InstructionListBox.SelectedIndex = index;
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstructionListBox.SelectedItem == null) return;
            if (InstructionListBox.SelectedIndex == -1)
            {
                return;
            }
            InstructionTextBox.Text = items[InstructionListBox.SelectedIndex, 0];
            ContentsTextBox.Text = items[InstructionListBox.SelectedIndex, 1];
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Save();
            }
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Save();
            }
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // itemsの行数を1増やす
            var newItems = new string[items.GetLength(0) + 1, 2];
            for (var i = 0; i < items.GetLength(0); i++)
            {
                newItems[i, 0] = items[i, 0];
                newItems[i, 1] = items[i, 1];
            }
            newItems[items.GetLength(0), 0] = "";
            newItems[items.GetLength(0), 1] = "";
            items = newItems;

            UpdateInstructionListBox();
            InstructionListBox.SelectedIndex = items.GetLength(0) - 1;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            //選択しているアイテムを削除
            var index = InstructionListBox.SelectedIndex;
            if (index == -1)
            {
                return;
            }
            var newItems = new string[items.GetLength(0) - 1, 2];
            for (var i = 0; i < index; i++)
            {
                newItems[i, 0] = items[i, 0];
                newItems[i, 1] = items[i, 1];
            }
            for (var i = index + 1; i < items.GetLength(0); i++)
            {
                newItems[i - 1, 0] = items[i, 0];
                newItems[i - 1, 1] = items[i, 1];
            }
            items = newItems;

            UpdateInstructionListBox();
            InstructionListBox.SelectedIndex = items.GetLength(0) - 1;
        }
        private void SwapItems(int index, bool isUp)
        {
            // 入力が不正であるか、先頭または末尾での入れ替えを試みる場合は何もしない
            if (index == -1 || (isUp && index == 0) || (!isUp && index == items.GetLength(0) - 1))
            {
                return;
            }
            // 入れ替え先のインデックスを計算
            var newIndex = isUp ? index - 1 : index + 1;
            var newItems = new string[items.GetLength(0), 2];

            for (var i = 0; i < items.GetLength(0); i++)
            {
                if (i == index)
                {
                    // 現在のアイテムを入れ替え先の位置に移動
                    newItems[newIndex, 0] = items[i, 0];
                    newItems[newIndex, 1] = items[i, 1];
                }
                else if (i == newIndex)
                {
                    // 入れ替え先のアイテムを現在の位置に移動
                    newItems[index, 0] = items[i, 0];
                    newItems[index, 1] = items[i, 1];
                }
                else
                {
                    // その他のアイテムはそのままコピー
                    newItems[i, 0] = items[i, 0];
                    newItems[i, 1] = items[i, 1];
                }
            }
            // 新しいアイテム配列をセット
            items = newItems;
            UpdateInstructionListBox();
            // 入れ替え後のインデックスを選択状態にする
            InstructionListBox.SelectedIndex = newIndex;
        }
        private void UpSwap()
        {
            var index = InstructionListBox.SelectedIndex;
            SwapItems(index, true);
        }
        private void DownSwap()
        {
            var index = InstructionListBox.SelectedIndex;
            SwapItems(index, false);
        }
        void UpSwap_Click(object sender, RoutedEventArgs e)
        {
            UpSwap();
        }
        void DownSwap_Click(object sender, RoutedEventArgs e)
        {
            DownSwap();
        }
        private void InstructionListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.K)
            {
                UpSwap();
            }
            if (e.Key == Key.J)
            {
                DownSwap();
            }
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 登録内容をjsonファイルに出力
                var json = JsonConvert.SerializeObject(items);
                json = JToken.Parse(json).ToString(Formatting.Indented);

                var dialog = new System.Windows.Forms.SaveFileDialog();
                dialog.Title = "Please select an export file.";
                dialog.FileName = DateTime.Now.ToString("yyyyMMdd") + "_SystemPrompt.json";
                dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                dialog.DefaultExt = "json";
                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, json);
                    ModernWpf.MessageBox.Show("Exported successfully.");
                }
            }
            catch(Exception ex)
            {
                ModernWpf.MessageBox.Show(ex.Message);
            }
        }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var okFlg = ModernWpf.MessageBox.Show("Overwrite with the contents of the selected json file. Are you sure?", "Question", MessageBoxButton.YesNo);
                if (okFlg == MessageBoxResult.Yes)
                {
                    // jsonファイルを読み込み
                    var dialog = new System.Windows.Forms.OpenFileDialog();
                    dialog.Title = "Please select a json file.";
                    dialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                    dialog.FilterIndex = 1;
                    dialog.RestoreDirectory = true;
                    var result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        var path = dialog.FileName;
                        var json = File.ReadAllText(path);
                        items = JsonConvert.DeserializeObject<string[,]>(json);
                        // ListBoxにアイテムを再セット
                        UpdateInstructionListBox();
                        InstructionListBox.SelectedIndex = items.GetLength(0) - 1;
                        ModernWpf.MessageBox.Show("Imported successfully.");
                    }
                }

                DuplicateControl();
                UpdateInstructionListBox();
            }
            catch (Exception ex)
            {
                ModernWpf.MessageBox.Show(ex.Message);
            }
        }
    }
}
