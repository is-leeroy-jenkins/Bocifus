// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="TokenUsageWindow.xaml.cs" company="Terry D. Eppler">
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
//   TokenUsageWindow.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using ModernWpf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using Properties;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="T:System.Windows.Markup.IStyleConnector" />
    [ SuppressMessage( "ReSharper", "ClassCanBeSealed.Global" ) ]
    public partial class TokenUsageWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenUsageWindow"/> class.
        /// </summary>
        public TokenUsageWindow( )
        {
            InitializeComponent( );
            var _savedTokenUsage = UtilityFunctions.DeserializeArray( Settings.Default.TokenUsage );
            var _tokenUsageDisplayItems = new List<TokenUsageDisplayItem>( );
            for( var _i = 0; _i < _savedTokenUsage.GetLength( 0 ); _i++ )
            {
                var _tokenUsageDisplayItem = new TokenUsageDisplayItem
                {
                    Date = _savedTokenUsage[ _i, 0 ],
                    Provider = _savedTokenUsage[ _i, 1 ],
                    GptVersion = _savedTokenUsage[ _i, 2 ],
                    TotalTokenUsage = int.Parse( _savedTokenUsage[ _i, 3 ] ).ToString( "N0" )
                };

                var _inputTokens = 0;
                if( _savedTokenUsage.GetLength( 1 ) > 4
                    && int.TryParse( _savedTokenUsage[ _i, 4 ], out _inputTokens ) )
                {
                    _tokenUsageDisplayItem.InputTokenUsage = _inputTokens.ToString( "N0" );
                }
                else
                {
                    _tokenUsageDisplayItem.InputTokenUsage = "0";
                }

                var _outputTokens = 0;
                if( _savedTokenUsage.GetLength( 1 ) > 5
                    && int.TryParse( _savedTokenUsage[ _i, 5 ], out _outputTokens ) )
                {
                    _tokenUsageDisplayItem.OutputTokenUsage = _outputTokens.ToString( "N0" );
                }
                else
                {
                    _tokenUsageDisplayItem.OutputTokenUsage = "0";
                }

                _tokenUsageDisplayItems.Add( _tokenUsageDisplayItem );
            }

            TokenUsageDataGrid.ItemsSource = _tokenUsageDisplayItems;
            TokenUsageDataGrid.Items.SortDescriptions.Add(
                new SortDescription( "Date", ListSortDirection.Descending ) );

            AlertSettingButton.Content =
                $"Set Alert Threshold: {Settings.Default.dailyTokenThreshold}";

            var _accentColor = ThemeManager.Current.AccentColor;
            if( _accentColor == null )
            {
                _accentColor = SystemParameters.WindowGlassColor;
            }

            var _accentColorBrush = new SolidColorBrush( ( Color )_accentColor );
            OkButton.Background = _accentColorBrush;
        }

        /// <summary>
        /// Called when [window key down].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnWindowKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.Escape )
            {
                DialogResult = false;
            }
        }

        /// <summary>
        /// Called when [okay button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnOkayButtonClick( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
        }

        /// <summary>
        /// Called when [alert setting button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnAlertSettingButtonClick( object sender, RoutedEventArgs e )
        {
            var _msg = "You will be alerted when daily token usage exceeds this threshold.";
            var _threshold = Settings.Default.dailyTokenThreshold;
            var _window = new Messagebox( "Set an alert threshold", _msg, _threshold )
            {
                Owner = Application.Current.Windows?.OfType<MainWindow>( )?.FirstOrDefault( )
            };

            if( _window.ShowDialog( ) == true )
            {
                Settings.Default.dailyTokenThreshold = _window.resultInt;
                Settings.Default.Save( );
                AlertSettingButton.Content =
                    $"Set Alert Threshold: {Settings.Default.dailyTokenThreshold}";
            }
        }

        /// <summary>
        /// Called when [calculate button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCalculateButtonClick( object sender, RoutedEventArgs e )
        {
            var _button = sender as Button;
            var _dataContext = _button.DataContext;
            var _input =
                ( _dataContext as TokenUsageDisplayItem ).InputTokenUsage.Replace( ",", "" );

            var _output =
                ( _dataContext as TokenUsageDisplayItem ).OutputTokenUsage.Replace( ",", "" );

            ;
        }
    }
}