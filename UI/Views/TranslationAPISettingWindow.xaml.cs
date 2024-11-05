// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-04-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-04-2024
// ******************************************************************************************
// <copyright file="TranslationAPISettingWindow.xaml.cs" company="Terry D. Eppler">
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
//   TranslationAPISettingWindow.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <inheritdoc />
    /// <summary>
    /// TranslateAPISettingWindow.xaml
    /// </summary>
    public partial class TranslationApiSettingWindow
    {
        /// <summary>
        /// The main window
        /// </summary>
        private MainWindow _mainWindow = ( MainWindow )Application.Current.MainWindow;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TranslationApiSettingWindow"/> class.
        /// </summary>
        public TranslationApiSettingWindow( )
        {
            InitializeComponent( );
            TranslationApiProviderComboBox.Items.Add( "DeepL" );
            TranslationApiProviderComboBox.Items.Add( "Google" );
            TranslationApiProviderComboBox.SelectedItem = AppSettings.TranslationApiProvider;
            if( AppSettings.TranslationApiProvider == "DeepL" )
            {
                ApiUrlTextBox.Text = AppSettings.TranslationApiUrlDeepL;
                ApiKeyPasswordBox.Password = AppSettings.TranslationApiKeyDeepL;
            }
            else if( AppSettings.TranslationApiProvider == "Google" )
            {
                ApiUrlTextBox.Text = AppSettings.TranslationApiUrlGoogle;
                ApiUrlTextBox.IsEnabled = false;
                ApiKeyPasswordBox.Password = AppSettings.TranslationApiKeyGoogle;
            }

            UseTranslateApiToggleSwitch.IsOn = AppSettings.TranslationApiUseFlg;
            PopulateLanguageComboBox( );
            FromTranslationLanguageComboBox.SelectedItem = AppSettings.FromTranslationLanguage;
            ToTranslationLanguageComboBox.SelectedItem = AppSettings.ToTranslationLanguage;
        }

        /// <summary>
        /// Populates the language ComboBox.
        /// </summary>
        private void PopulateLanguageComboBox( )
        {
            var _languageCodes = new List<string>
            {
                "BG",
                "CS",
                "DA",
                "DE",
                "EL",
                "EN",
                "ES",
                "ET",
                "FI",
                "FR",
                "HU",
                "ID",
                "IT",
                "JA",
                "KO",
                "LT",
                "LV",
                "NB",
                "NL",
                "PL",
                "PT",
                "RO",
                "RU",
                "SK",
                "SL",
                "SV",
                "TR",
                "UK",
                "ZH"
            };

            foreach( var _code in _languageCodes )
            {
                FromTranslationLanguageComboBox.Items.Add( _code );
                ToTranslationLanguageComboBox.Items.Add( _code );
            }
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowClosing( object sender, CancelEventArgs e )
        {
            if( AppSettings.TranslationApiUseFlg == false )
            {
                UseTranslateApiToggleSwitch.IsOn = false;
                _mainWindow.TranslateButton.Visibility = Visibility.Collapsed;
                _mainWindow.UserTextBox.Padding = new Thickness( 10, 10, 10, 10 );
            }
            else
            {
                UseTranslateApiToggleSwitch.IsOn = true;
                _mainWindow.TranslateButton.Visibility = Visibility.Visible;
                _mainWindow.UserTextBox.Padding = new Thickness( 10, 10, 30, 10 );
            }
        }

        /// <summary>
        /// Handles the KeyDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowKeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == Key.Escape )
            {
                DialogResult = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnSaveButtonClick( object sender, RoutedEventArgs e )
        {
            if( TranslationApiProviderComboBox.Text == "DeepL" )
            {
                AppSettings.TranslationApiUrlDeepL = ApiUrlTextBox.Text;
                AppSettings.TranslationApiKeyDeepL = ApiKeyPasswordBox.Password;
            }
            else if( TranslationApiProviderComboBox.Text == "Google" )
            {
                AppSettings.TranslationApiUrlGoogle = ApiUrlTextBox.Text;
                AppSettings.TranslationApiKeyGoogle = ApiKeyPasswordBox.Password;
            }

            AppSettings.FromTranslationLanguage =
                FromTranslationLanguageComboBox.SelectedItem.ToString( );

            AppSettings.ToTranslationLanguage =
                ToTranslationLanguageComboBox.SelectedItem.ToString( );

            AppSettings.TranslationApiUseFlg = UseTranslateApiToggleSwitch.IsOn;
            AppSettings.TranslationApiProvider =
                TranslationApiProviderComboBox.SelectedItem.ToString( );
        }

        /// <summary>
        /// Handles the Click event of the CloseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnCloseButtonClick( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Toggled event of the UseTranslateAPIToggleSwitch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnUseTranslateApiToggleSwitchToggled( object sender, RoutedEventArgs e )
        {
            if( UseTranslateApiToggleSwitch.IsOn == false )
            {
                _mainWindow.TranslateButton.Visibility = Visibility.Collapsed;
                _mainWindow.UserTextBox.Padding = new Thickness( 10, 10, 10, 10 );
            }
            else
            {
                _mainWindow.TranslateButton.Visibility = Visibility.Visible;
                _mainWindow.UserTextBox.Padding = new Thickness( 10, 10, 30, 10 );
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the TranslationAPIProviderComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnTranslationApiProviderComboBoxSelectionChanged( object sender,
            SelectionChangedEventArgs e )
        {
            if( TranslationApiProviderComboBox.SelectedItem.ToString( ) == "DeepL" )
            {
                ApiUrlTextBox.Text = AppSettings.TranslationApiUrlDeepL;
                ApiUrlTextBox.IsEnabled = true;
                ApiKeyPasswordBox.Password = AppSettings.TranslationApiKeyDeepL;
            }
            else if( TranslationApiProviderComboBox.SelectedItem.ToString( ) == "Google" )
            {
                ApiUrlTextBox.Text = AppSettings.TranslationApiUrlGoogle;
                ApiUrlTextBox.IsEnabled = false;
                ApiKeyPasswordBox.Password = AppSettings.TranslationApiKeyGoogle;
            }
        }
    }
}