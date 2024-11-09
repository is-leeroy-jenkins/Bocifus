// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="TitleGenerationSettings.xaml.cs" company="Terry D. Eppler">
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
//   TitleGenerationSettings.xaml.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System.ComponentModel;
    using Model;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using MessageBox = ModernWpf.MessageBox;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    [ SuppressMessage( "ReSharper", "ClassCanBeSealed.Global" ) ]
    public partial class TitleGenerationWindow
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TitleGenerationWindow"/> class.
        /// </summary>
        public TitleGenerationWindow( )
        {
            InitializeComponent( );
            var cultures = CultureInfo.GetCultures( CultureTypes.NeutralCultures );
            foreach( var culture in cultures )
            {
                var englishCulture = new CultureInfo( "en-US" );
                Thread.CurrentThread.CurrentCulture = englishCulture;
                Thread.CurrentThread.CurrentUICulture = englishCulture;
                var englishDisplayName = culture.DisplayName;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture;
                LanguageComboBox.Items.Add( englishDisplayName );
            }

            ModelComboBox.ItemsSource = AppSettings.ConfigDataTable?.AsEnumerable( )
                ?.Select( x => x.Field<string>( "ConfigurationName" ) )?.ToList( );

            EnableToggleSwitch.IsOn = AppSettings.UseTitleGenerationSetting;
            ModelComboBox.Text = AppSettings.ModelForTitleGenerationSetting;
            LanguageComboBox.Text = AppSettings.TitleLanguageSetting;
            PromptTextBox.Text = AppSettings.TitleGenerationPromptSetting;
        }

        /// <summary>
        /// Called when [window loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowLoaded( object sender, RoutedEventArgs e )
        {
        }

        /// <summary>
        /// Called when [ok button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnOkButtonClick( object sender, RoutedEventArgs e )
        {
            if( !PromptTextBox.Text.Contains( "{Prompt}" ) )
            {
                MessageBox.Show( "PromptTextBox must include \"{Prompt}\"." );
                return;
            }

            AppSettings.UseTitleGenerationSetting = EnableToggleSwitch.IsOn;
            AppSettings.ModelForTitleGenerationSetting = ModelComboBox.Text;
            AppSettings.TitleLanguageSetting = LanguageComboBox.Text;
            AppSettings.TitleGenerationPromptSetting = PromptTextBox.Text;
            DialogResult = true;
        }

        /// <summary>
        /// Called when [cancel button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnCancelButtonClick( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
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

            if( e.Key == Key.Enter
                && ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
            {
                OnOkButtonClick( sender, e );
            }
        }

        /// <summary>
        /// Called when [window closing].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/>
        /// instance containing the event data.</param>
        private void OnWindowClosing( object sender, CancelEventArgs e )
        {
            DialogResult = DialogResult == true;
        }

        /// <summary>
        /// Called when [prompt text box preview mouse wheel].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/>
        /// instance containing the event data.</param>
        private void OnPromptTextBoxPreviewMouseWheel( object sender, MouseWheelEventArgs e )
        {
            if( Keyboard.Modifiers == ModifierKeys.Control )
            {
                var textBox = sender as TextBox;
                if( e.Delta > 0
                    && textBox.FontSize < 40 )
                {
                    textBox.FontSize += 2;
                }
                else if( e.Delta < 0
                    && textBox.FontSize > 10 )
                {
                    textBox.FontSize -= 2;
                }

                e.Handled = true;
            }
        }
    }
}