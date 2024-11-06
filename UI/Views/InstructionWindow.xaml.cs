

namespace Bocifus
{
    using ModernWpf;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using static MainWindow;

    public partial class InstructionWindow
    {
        public string[,] InputResult => Items;
        string[,] Items { get; set; }
        public InstructionWindow(string[,] param)
        {
            InitializeComponent();
            Items = param;

            var _accentColor = ThemeManager.Current.AccentColor;
            if (_accentColor == null)
            {
                _accentColor = SystemParameters.WindowGlassColor;
            }
            var _accentColorBrush = new SolidColorBrush((Color)_accentColor);
            SaveButton.Background = _accentColorBrush;

            InstructionListBox.ContextMenu = new ContextMenu();
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
            InstructionListBox.ContextMenu.Items.Add(_upSwap);
            InstructionListBox.ContextMenu.Items.Add(_downSwap);
            if (Items == null)
            {
                Items = new string[1, 2];
                Items[0, 0] = "";
                Items[0, 1] = "";
            }

            for (var _i = 0; _i < Items.GetLength(0); _i++)
            {
                InstructionListBox.Items.Add(Items[_i, 0]);
            }

            for (var _i = 0; _i < Items.GetLength(0); _i++)
            {
                if (AppSettings.InstructionSetting == "" || AppSettings.InstructionSetting == null)
                {
                    InstructionListBox.SelectedIndex = 0;
                    break;
                }

                if (Items[_i, 0] == AppSettings.InstructionSetting)
                {
                    InstructionListBox.SelectedIndex = _i;
                    break;
                }
            }
        }

        private void UpdateInstructionListBox()
        {
            InstructionListBox.Items.Clear();
            for (var _i = 0; _i < Items.GetLength(0); _i++)
            {
                InstructionListBox.Items.Add(Items[_i, 0]);
            }
        }

        private void DuplicateControl()
        {
            for (var _i = 0; _i < Items.GetLength(0); _i++)
            {
                var _currentName = Items[_i, 0];
                for (var _j = 0; _j < Items.GetLength(0); _j++)
                {
                    if (_i == _j)
                    {
                        continue;
                    }

                    if (Items[_i, 0] == Items[_j, 0])
                    {
                        _currentName += "*";
                        Items[_j, 0] = _currentName;
                    }
                }
            }
        }

        private void Save( )
        {
            if (InstructionListBox.SelectedIndex == -1)
            {
                return;
            }

            if (InstructionTextBox.Text == "")
            {
                var _msg = "The instruction name has not been entered.";
                ModernWpf.MessageBox.Show( _msg, "Error", MessageBoxButton.OK );
                return;
            }

            var _index = InstructionListBox.SelectedIndex;
            Items[_index, 0] = InstructionTextBox.Text;
            Items[_index, 1] = ContentsTextBox.Text;
            DuplicateControl();
            UpdateInstructionListBox();
            InstructionListBox.SelectedIndex = _index;
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstructionListBox.SelectedItem == null) return;
            if (InstructionListBox.SelectedIndex == -1)
            {
                return;
            }

            InstructionTextBox.Text = Items[InstructionListBox.SelectedIndex, 0];
            ContentsTextBox.Text = Items[InstructionListBox.SelectedIndex, 1];
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
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

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            var _newItems = new string[Items.GetLength(0) + 1, 2];
            for (var _i = 0; _i < Items.GetLength(0); _i++)
            {
                _newItems[_i, 0] = Items[_i, 0];
                _newItems[_i, 1] = Items[_i, 1];
            }

            _newItems[Items.GetLength(0), 0] = "";
            _newItems[Items.GetLength(0), 1] = "";
            Items = _newItems;
            UpdateInstructionListBox();
            InstructionListBox.SelectedIndex = Items.GetLength(0) - 1;
        }

        private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            var _index = InstructionListBox.SelectedIndex;
            if (_index == -1)
            {
                return;
            }

            var _newItems = new string[Items.GetLength(0) - 1, 2];
            for (var _i = 0; _i < _index; _i++)
            {
                _newItems[_i, 0] = Items[_i, 0];
                _newItems[_i, 1] = Items[_i, 1];
            }

            for (var _i = _index + 1; _i < Items.GetLength(0); _i++)
            {
                _newItems[_i - 1, 0] = Items[_i, 0];
                _newItems[_i - 1, 1] = Items[_i, 1];
            }
            
            Items = _newItems;
            UpdateInstructionListBox();
            InstructionListBox.SelectedIndex = Items.GetLength(0) - 1;
        }

        private void SwapItems(int index, bool isUp)
        {
            if (index == -1 || (isUp && index == 0) || (!isUp && index == Items.GetLength(0) - 1))
            {
                return;
            }

            var _newIndex = isUp ? index - 1 : index + 1;
            var _newItems = new string[Items.GetLength(0), 2];
            for (var _i = 0; _i < Items.GetLength(0); _i++)
            {
                if (_i == index)
                {
                    _newItems[_newIndex, 0] = Items[_i, 0];
                    _newItems[_newIndex, 1] = Items[_i, 1];
                }
                else if (_i == _newIndex)
                {
                    _newItems[index, 0] = Items[_i, 0];
                    _newItems[index, 1] = Items[_i, 1];
                }
                else
                {
                    _newItems[_i, 0] = Items[_i, 0];
                    _newItems[_i, 1] = Items[_i, 1];
                }
            }

            Items = _newItems;
            UpdateInstructionListBox();
            InstructionListBox.SelectedIndex = _newIndex;
        }

        private void UpSwap()
        {
            var _index = InstructionListBox.SelectedIndex;
            SwapItems(_index, true);
        }

        private void DownSwap()
        {
            var _index = InstructionListBox.SelectedIndex;
            SwapItems(_index, false);
        }

        void OnUpSwapClick(object sender, RoutedEventArgs e)
        {
            UpSwap();
        }

        void OnDownSwapClick(object sender, RoutedEventArgs e)
        {
            DownSwap();
        }

        private void OnInstructionListBoxKeyDown(object sender, KeyEventArgs e)
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

        private void OnExportButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var _json = JsonConvert.SerializeObject(Items);
                _json = JToken.Parse(_json).ToString(Formatting.Indented);

                var _dialog = new System.Windows.Forms.SaveFileDialog();
                _dialog.Title = "Please select an export file.";
                _dialog.FileName = DateTime.Now.ToString("yyyyMMdd") + "_SystemPrompt.json";
                _dialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                _dialog.DefaultExt = "json";
                var _result = _dialog.ShowDialog();
                if (_result == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(_dialog.FileName, _json);
                    ModernWpf.MessageBox.Show("Exported successfully.");
                }
            }
            catch(Exception ex)
            {
                ModernWpf.MessageBox.Show(ex.Message);
            }
        }

        private void OnImportButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var _okFlg = ModernWpf.MessageBox.Show("Overwrite with the contents of the selected json file. Are you sure?", "Question", MessageBoxButton.YesNo);
                if (_okFlg == MessageBoxResult.Yes)
                {
                    var _dialog = new System.Windows.Forms.OpenFileDialog();
                    _dialog.Title = "Please select a json file.";
                    _dialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                    _dialog.FilterIndex = 1;
                    _dialog.RestoreDirectory = true;
                    var _result = _dialog.ShowDialog();
                    if (_result == System.Windows.Forms.DialogResult.OK)
                    {
                        var _path = _dialog.FileName;
                        var _json = File.ReadAllText(_path);
                        Items = JsonConvert.DeserializeObject<string[,]>(_json);
                        UpdateInstructionListBox();
                        InstructionListBox.SelectedIndex = Items.GetLength(0) - 1;
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
