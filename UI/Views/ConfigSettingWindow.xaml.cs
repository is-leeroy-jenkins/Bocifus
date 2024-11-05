// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="ConfigSettingWindow.xaml.cs" company="Terry D. Eppler">
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
//   ConfigSettingWindow.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using ModernWpf;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using HorizontalAlignment = System.Windows.HorizontalAlignment;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using MessageBox = ModernWpf.MessageBox;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    [ SuppressMessage( "ReSharper", "ClassCanBeSealed.Global" ) ]
    public partial class ConfigSettingWindow
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ConfigSettingWindow"/> class.
        /// </summary>
        public ConfigSettingWindow( )
        {
            InitializeComponent( );
            ConfigListBox.ContextMenu = new ContextMenu( );
            var _upSwap = new MenuItem( );
            _upSwap.Header = "⬆";
            _upSwap.Click += OnUpSwapClick;
            _upSwap.HorizontalAlignment = HorizontalAlignment.Center;
            var _downSwap = new MenuItem( );
            _downSwap.Header = "⬇";
            _downSwap.Click += OnDownSwapClick;
            _downSwap.HorizontalAlignment = HorizontalAlignment.Center;
            ConfigListBox.ContextMenu.Items.Add( _upSwap );
            ConfigListBox.ContextMenu.Items.Add( _downSwap );
            var _accentColor = ThemeManager.Current.AccentColor;
            if( _accentColor == null )
            {
                _accentColor = SystemParameters.WindowGlassColor;
            }

            var _accentColorBrush = new SolidColorBrush( ( Color )_accentColor );
            SaveButton.Background = _accentColorBrush;
            ModelComboBox.Items.Add( "gpt-3.5-turbo" );
            ModelComboBox.Items.Add( "gpt-3.5-turbo-16k" );
            ModelComboBox.Items.Add( "gpt-3.5-turbo-instruct" );
            ModelComboBox.Items.Add( "gpt-3.5-turbo-1106" );
            ModelComboBox.Items.Add( "gpt-3.5-turbo-0125" );
            ModelComboBox.Items.Add( "gpt-4" );
            ModelComboBox.Items.Add( "gpt-4-32k" );
            ModelComboBox.Items.Add( "gpt-4-0613" );
            ModelComboBox.Items.Add( "gpt-4-1106-preview" );
            ModelComboBox.Items.Add( "gpt-4-0125-preview" );
            ModelComboBox.Items.Add( "gpt-4-vision-preview" );
            ModelComboBox.Items.Add( "gpt-4-turbo" );
            ModelComboBox.Items.Add( "gpt-4-turbo-2024-04-09" );
            ModelComboBox.Items.Add( "gpt-4o" );
            ModelComboBox.Items.Add( "gpt-4o-mini" );
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

            foreach( DataRow _row in AppSettings.ConfigDataTable.Rows )
            {
                ConfigListBox.Items.Add( _row[ "ConfigurationName" ] );
            }

            var _datarow = AppSettings.ConfigDataTable
                .AsEnumerable( )
                .Where( a => a.Field<string>( 0 ) == AppSettings.SelectConfigSetting )
                .FirstOrDefault( );

            var _index = AppSettings.ConfigDataTable.Rows.IndexOf( _datarow );
            ConfigListBox.SelectedIndex = _index;
        }

        /// <summary>
        /// Duplicates the control.
        /// </summary>
        private void DuplicateControl( )
        {
            for( var _i = 0; _i < AppSettings.ConfigDataTable.Rows.Count; _i++ )
            {
                var _currentName = AppSettings.ConfigDataTable.Rows[ _i ][ "ConfigurationName" ]
                    .ToString( );

                for( var _j = 0; _j < AppSettings.ConfigDataTable.Rows.Count; _j++ )
                {
                    if( _i == _j )
                    {
                        continue;
                    }

                    if( AppSettings.ConfigDataTable.Rows[ _i ][ "ConfigurationName" ].ToString( )
                        == AppSettings.ConfigDataTable.Rows[ _j ][ "ConfigurationName" ]
                            .ToString( ) )
                    {
                        _currentName += "*";
                        AppSettings.ConfigDataTable.Rows[ _j ][ "ConfigurationName" ] =
                            _currentName;
                    }
                }
            }
        }

        /// <summary>
        /// Swaps the items.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="isUp">if set to <c>true</c> [is up].</param>
        private void SwapItems( int index, bool isUp )
        {
            if( isUp )
            {
                if( index == 0 )
                {
                    return;
                }

                var _row = AppSettings.ConfigDataTable.NewRow( );
                _row.ItemArray =
                    AppSettings.ConfigDataTable.Rows[ index - 1 ].ItemArray.Clone( ) as object[ ];

                AppSettings.ConfigDataTable.Rows[ index - 1 ].ItemArray =
                    AppSettings.ConfigDataTable.Rows[ index ].ItemArray.Clone( ) as object[ ];

                AppSettings.ConfigDataTable.Rows[ index ].ItemArray =
                    _row.ItemArray.Clone( ) as object[ ];

                var _name = ConfigListBox.Items[ index - 1 ].ToString( );
                ConfigListBox.Items[ index - 1 ] = ConfigListBox.Items[ index ];
                ConfigListBox.Items[ index ] = _name;
                ConfigListBox.SelectedIndex = index - 1;
            }
            else
            {
                if( index == ConfigListBox.Items.Count - 1 )
                {
                    return;
                }

                var _row = AppSettings.ConfigDataTable.NewRow( );
                _row.ItemArray =
                    AppSettings.ConfigDataTable.Rows[ index + 1 ].ItemArray.Clone( ) as object[ ];

                AppSettings.ConfigDataTable.Rows[ index + 1 ].ItemArray =
                    AppSettings.ConfigDataTable.Rows[ index ].ItemArray.Clone( ) as object[ ];

                AppSettings.ConfigDataTable.Rows[ index ].ItemArray =
                    _row.ItemArray.Clone( ) as object[ ];

                var _name = ConfigListBox.Items[ index + 1 ].ToString( );
                ConfigListBox.Items[ index + 1 ] = ConfigListBox.Items[ index ];
                ConfigListBox.Items[ index ] = _name;
                ConfigListBox.SelectedIndex = index + 1;
            }
        }

        /// <summary>
        /// Ups the swap.
        /// </summary>
        private void UpSwap( )
        {
            var _index = ConfigListBox.SelectedIndex;
            SwapItems( _index, true );
        }

        /// <summary>
        /// Downs the swap.
        /// </summary>
        private void DownSwap( )
        {
            var _index = ConfigListBox.SelectedIndex;
            SwapItems( _index, false );
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        private void Save( )
        {
            if( ConfigListBox.SelectedIndex == -1 )
            {
                return;
            }

            if( ConfigurationNameTextBox.Text == "" )
            {
                MessageBox.Show( "The Configuration name has not been entered.", "Error",
                    MessageBoxButton.OK );

                return;
            }

            var _index = ConfigListBox.SelectedIndex;
            AppSettings.ConfigDataTable.Rows[ _index ][ "ConfigurationName" ] =
                ConfigurationNameTextBox.Text;

            AppSettings.ConfigDataTable.Rows[ _index ][ "Provider" ] = ProviderComboBox.Text;
            AppSettings.ConfigDataTable.Rows[ _index ][ "APIKey" ] = ApiKeyPasswordbox.Password;
            AppSettings.ConfigDataTable.Rows[ _index ][ "Model" ] = ModelComboBox.Text;
            AppSettings.ConfigDataTable.Rows[ _index ][ "DeploymentId" ] = DeploymentIdTextbox.Text;
            AppSettings.ConfigDataTable.Rows[ _index ][ "BaseDomain" ] = BaseDomainTextbox.Text;
            AppSettings.ConfigDataTable.Rows[ _index ][ "ApiVersion" ] = ApiVersionTextbox.Text;
            AppSettings.ConfigDataTable.Rows[ _index ][ "Temperature" ] = TemperatureNumberbox.Text;
            AppSettings.ConfigDataTable.Rows[ _index ][ "MaxTokens" ] = MaxTokensNumberbox.Text;
            AppSettings.ConfigDataTable.Rows[ _index ][ "Vision" ] = VisionToggleSwitch.IsOn;
            DuplicateControl( );
            ConfigListBox.SelectedIndex = _index;
            ConfigListBox.Items[ _index ] =
                AppSettings.ConfigDataTable.Rows[ _index ][ "ConfigurationName" ];
        }

        /// <summary>
        /// Called when [ListView selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnListViewSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if( ConfigListBox.SelectedItem == null )
            {
                return;
            }

            if( ConfigListBox.SelectedIndex == -1 )
            {
                return;
            }

            ConfigurationNameTextBox.Text =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][
                    "ConfigurationName" ].ToString( );

            ProviderComboBox.Text =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "Provider" ]
                    .ToString( );

            ApiKeyPasswordbox.Password =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "APIKey" ]
                    .ToString( );

            ModelComboBox.Text =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "Model" ]
                    .ToString( );

            DeploymentIdTextbox.Text =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "DeploymentId" ]
                    .ToString( );

            BaseDomainTextbox.Text =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "BaseDomain" ]
                    .ToString( );

            ApiVersionTextbox.Text =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "ApiVersion" ]
                    .ToString( );

            TemperatureNumberbox.Text =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "Temperature" ]
                    .ToString( );

            MaxTokensNumberbox.Text =
                AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "MaxTokens" ]
                    .ToString( );

            var _rows = AppSettings.ConfigDataTable.Rows[ ConfigListBox.SelectedIndex ][ "Vision" ];
            VisionToggleSwitch.IsOn = Convert.ToBoolean( _rows );
        }

        /// <summary>
        /// Called when [add button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnAddButtonClick( object sender, RoutedEventArgs e )
        {
            var _row = AppSettings.ConfigDataTable.NewRow( );
            _row[ "ConfigurationName" ] = "New Item";
            _row[ "Provider" ] = "OpenAI";
            _row[ "Temperature" ] = "1";
            _row[ "MaxTokens" ] = "2048";
            _row[ "Vision" ] = false;
            AppSettings.ConfigDataTable.Rows.Add( _row );
            ConfigListBox.Items.Add( _row[ "ConfigurationName" ].ToString( ) );
            ConfigListBox.SelectedIndex = ConfigListBox.Items.Count - 1;
        }

        /// <summary>
        /// Called when [remove button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnRemoveButtonClick( object sender, RoutedEventArgs e )
        {
            var _index = ConfigListBox.SelectedIndex;
            if( _index == -1 )
            {
                return;
            }

            ConfigListBox.Items.RemoveAt( _index );
            AppSettings.ConfigDataTable.Rows[ _index ].Delete( );
            ConfigListBox.SelectedIndex = _index - 1;
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
        /// Called when [export button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnExportButtonClick( object sender, RoutedEventArgs e )
        {
            try
            {
                var _items = new List<ModelList>( );
                foreach( DataRow _row in AppSettings.ConfigDataTable.Rows )
                {
                    var _item = new ModelList( );
                    _item.ConfigurationName = _row[ "ConfigurationName" ].ToString( );
                    _item.Provider = _row[ "Provider" ].ToString( );
                    _item.Model = _row[ "Model" ].ToString( );
                    _item.ApiKey = _row[ "APIKey" ].ToString( );
                    _item.DeploymentId = _row[ "DeploymentId" ].ToString( );
                    _item.BaseDomain = _row[ "BaseDomain" ].ToString( );
                    _item.ApiVersion = _row[ "ApiVersion" ].ToString( );
                    _item.Temperature = _row[ "Temperature" ].ToString( );
                    _item.MaxTokens = _row[ "MaxTokens" ].ToString( );
                    _item.Vision = Convert.ToBoolean( _row[ "Vision" ].ToString( ) );
                    _items.Add( _item );
                }

                var _json = JsonConvert.SerializeObject( _items );
                _json = JToken.Parse( _json ).ToString( Formatting.Indented );
                var _dialog = new SaveFileDialog( );
                _dialog.Title = "Please select an export file.";
                _dialog.FileName = DateTime.Now.ToString( "yyyyMMdd" ) + "_config.json";
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
                        var _items = JsonConvert.DeserializeObject<List<ModelList>>( _json );
                        AppSettings.ConfigDataTable.Rows.Clear( );
                        foreach( var _item in _items )
                        {
                            var _row = AppSettings.ConfigDataTable.NewRow( );
                            _row[ "ConfigurationName" ] = _item.ConfigurationName;
                            _row[ "Provider" ] = _item.Provider;
                            _row[ "Model" ] = _item.Model;
                            _row[ "APIKey" ] = _item.ApiKey;
                            _row[ "DeploymentId" ] = _item.DeploymentId;
                            _row[ "BaseDomain" ] = _item.BaseDomain;
                            _row[ "ApiVersion" ] = _item.ApiVersion;
                            _row[ "Temperature" ] = _item.Temperature;
                            _row[ "MaxTokens" ] = _item.MaxTokens;
                            _row[ "Vision" ] = _item.Vision;
                            AppSettings.ConfigDataTable.Rows.Add( _row );
                        }

                        ConfigListBox.Items.Clear( );
                        foreach( DataRow _row in AppSettings.ConfigDataTable.Rows )
                        {
                            ConfigListBox.Items.Add( _row[ "ConfigurationName" ] );
                        }

                        MessageBox.Show( "Imported successfully." );
                    }
                }

                DuplicateControl( );
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        /// <summary>
        /// Called when [configuration ListBox key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnConfigListBoxKeyDown( object sender, KeyEventArgs e )
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
            DuplicateControl( );
            DialogResult = true;
        }

        /// <summary>
        /// Called when [window key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.Escape )
            {
                DialogResult = false;
            }

            if( e.Key == Key.Delete )
            {
                OnRemoveButtonClick( sender, e );
            }

            if( e.Key == Key.D
                && Keyboard.Modifiers == ModifierKeys.Control )
            {
                OnAddButtonClick( sender, e );
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
        /// Called when [provider ComboBox selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnProviderComboBoxSelectionChanged( object sender,
            SelectionChangedEventArgs e )
        {
            if( ProviderComboBox.SelectedItem == null )
            {
                return;
            }

            if( ProviderComboBox.SelectedItem.ToString( )
                == "System.Windows.Controls.ComboBoxItem: OpenAI" )
            {
                ModelComboBox.IsEnabled = true;
                DeploymentIdTextbox.IsEnabled = false;
                ApiVersionTextbox.IsEnabled = false;
                BaseDomainTextbox.IsEnabled = false;
            }
            else
            {
                ModelComboBox.IsEnabled = false;
                DeploymentIdTextbox.IsEnabled = true;
                ApiVersionTextbox.IsEnabled = true;
                BaseDomainTextbox.IsEnabled = true;
            }

            ConfigurationNameTextBox.Focus( );
        }
    }
}