// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="PromptTemplateManager.cs" company="Terry D. Eppler">
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
//   PromptTemplateManager.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using Model;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using MessageBox = ModernWpf.MessageBox;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class MainWindow 
    {
        /// <summary>
        /// Called when [prompt template ListBox selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnPromptTemplateListBoxSelectionChanged( object sender,
            SelectionChangedEventArgs e )
        {
            var selectedTemplate = ( PromptTemplate )PromptTemplateListBox.SelectedItem;
            foreach( var item in PromptTemplateListBox.Items.OfType<PromptTemplate>( ) )
            {
                item.IsSelected = false;
            }

            if( selectedTemplate != null )
            {
                selectedTemplate.IsSelected = true;
            }

            foreach( PromptTemplate item in e.RemovedItems )
            {
                item.IsSelected = false;
            }
        }

        /// <summary>
        /// Called when [prompt template sort up button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnPromptTemplateSortUpButtonClick( object sender, RoutedEventArgs e )
        {
            var selectedItem = ( PromptTemplate )( ( Button )sender ).DataContext;
            var currentIndex = AppSettings.PromptTemplateManager.Templates.IndexOf( selectedItem );
            if( currentIndex > 0 )
            {
                var itemAbove = AppSettings.PromptTemplateManager.Templates[ currentIndex - 1 ];
                selectedItem.SortOrder -= 1;
                itemAbove.SortOrder += 1;
                SortTemplatesBySortOrder( );
                PromptTemplateListBox.Items.Refresh( );
                PromptTemplateListBox.SelectedItem = selectedItem;
                PromptTemplateListBox.ScrollIntoView( selectedItem );
            }
        }

        /// <summary>
        /// Called when [prompt template sort down button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnPromptTemplateSortDownButtonClick( object sender, RoutedEventArgs e )
        {
            var selectedItem = ( PromptTemplate )( ( Button )sender ).DataContext;
            var templates = AppSettings.PromptTemplateManager.Templates;
            var currentIndex = templates.IndexOf( selectedItem );
            if( currentIndex < templates.Count - 1 )
            {
                var itemBelow = templates[ currentIndex + 1 ];
                selectedItem.SortOrder += 1;
                itemBelow.SortOrder -= 1;
                SortTemplatesBySortOrder( );
                PromptTemplateListBox.Items.Refresh( );
                PromptTemplateListBox.SelectedItem = selectedItem;
                PromptTemplateListBox.ScrollIntoView( selectedItem );
            }
        }

        /// <summary>
        /// Sorts the templates by sort order.
        /// </summary>
        private void SortTemplatesBySortOrder( )
        {
            var sortedTemplates = AppSettings.PromptTemplateManager.Templates
                .OrderBy( t => t.SortOrder ).ToList( );

            AppSettings.PromptTemplateManager.Templates.Clear( );
            foreach( var template in sortedTemplates )
            {
                AppSettings.PromptTemplateManager.Templates.Add( template );
            }
        }

        /// <summary>
        /// Called when [prompt template insert button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnPromptTemplateInsertButtonClick( object sender, RoutedEventArgs e )
        {
            var selectedItem = ( PromptTemplate )( ( Button )sender ).DataContext;
            var prompt = selectedItem.Prompt;
            if( !string.IsNullOrEmpty( UserTextBox.Text ) )
            {
                var _msg = "The text box already contains text. Do you want to replace it?";
                var result = MessageBox.Show( _msg, "Confirm Replace", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning );

                if( result == MessageBoxResult.Yes )
                {
                    UserTextBox.Text = prompt;
                }
            }
            else
            {
                UserTextBox.Text = prompt;
            }
        }

        /// <summary>
        /// Called when [prompt template ListBox mouse leave].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/>
        /// instance containing the event data.</param>
        private void OnPromptTemplateListBoxMouseLeave( object sender, MouseEventArgs e )
        {
            PromptTemplateListBox.SelectedItem = null;
        }

        /// <summary>
        /// Called when [prompt template ListBox item mouse enter].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/>
        /// instance containing the event data.</param>
        private void OnPromptTemplateListBoxItemMouseEnter( object sender, MouseEventArgs e )
        {
            var item = sender as ListBoxItem;
            if( item != null
                && !item.IsSelected )
            {
                item.IsSelected = true;
            }
        }

        /// <summary>
        /// Called when [prompt template edit button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnPromptTemplateEditButtonClick( object sender, RoutedEventArgs e )
        {
            var item = ( PromptTemplate )( ( Button )sender ).DataContext;
            var dialog = new PromptInputWindow( item )
            {
                Owner = this
            };

            if( dialog.ShowDialog( ) == true )
            {
                var newTemplate = dialog.Result;
                item.Title = newTemplate.Title;
                item.Description = newTemplate.Description;
                item.Prompt = newTemplate.Prompt;
                item.LastUpdated = DateTime.Now;
                var index = AppSettings.PromptTemplateManager.Templates.IndexOf( item );
                AppSettings.PromptTemplateManager.Templates[ index ] = item;
                PromptTemplateListBox.SelectedItem = item;
                PromptTemplateListBox.ScrollIntoView( item );
                PromptTemplateListBox.Items.Refresh( );
            }
            else
            {
                PromptTemplateListBox.Items.Refresh( );
            }
        }

        /// <summary>
        /// Called when [new template button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        private void OnNewTemplateButtonClick( object sender, RoutedEventArgs e )
        {
            var dialog = new PromptInputWindow
            {
                Owner = this
            };

            if( dialog.ShowDialog( ) == true )
            {
                var newTemplate = dialog.Result;
                if( AppSettings.PromptTemplateManager.Templates.Count > 0 )
                {
                    var maxSortOrder =
                        AppSettings.PromptTemplateManager.Templates.Max( t => t.SortOrder );

                    newTemplate.SortOrder = maxSortOrder + 1;
                }
                else
                {
                    newTemplate.SortOrder = 1;
                }

                AppSettings.PromptTemplateManager.Templates.Add( newTemplate );
                PromptTemplateListBox.SelectedItem = newTemplate;
                PromptTemplateListBox.ScrollIntoView( newTemplate );
                PromptTemplateListBox.Focus( );
            }
        }
    }
}