// ******************************************************************************************
//     Assembly:                Badger
//     Author:                  Terry D. Eppler
//     Created:                 10-26-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        10-26-2024
// ******************************************************************************************
// <copyright file="WebControl.cs" company="Terry D. Eppler">
//   An open source data analysis application for EPA Analysts developed
//   in C-Sharp using WPF and released under the MIT license
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
//   WebControl.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;
    using Point = System.Drawing.Point;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:Badger.MetroTabControl" />
    /// <seealso cref="T:System.ComponentModel.ISupportInitialize" />
    [ SuppressMessage( "ReSharper", "FieldCanBeMadeReadOnly.Global" ) ]
    [ SuppressMessage( "ReSharper", "FieldCanBeMadeReadOnly.Local" ) ]
    [ SuppressMessage( "ReSharper", "UnusedParameter.Global" ) ]
    [ SuppressMessage( "ReSharper", "UnusedVariable" ) ]
    [ SuppressMessage( "ReSharper", "ConvertIfStatementToSwitchStatement" ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBeInternal" ) ]
    [ DefaultEvent( "TabStripItemSelectionChanged" ) ]
    [ DefaultProperty( "Items" ) ]
    [ ToolboxItem( true ) ]
    [ SuppressMessage( "ReSharper", "LocalVariableHidesMember" ) ]
    [ SuppressMessage( "ReSharper", "IntroduceOptionalParameters.Global" ) ]
    [ SuppressMessage( "ReSharper", "UnusedType.Global" ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" ) ]
    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    [ SuppressMessage( "ReSharper", "ConvertIfStatementToNullCoalescingExpression" ) ]
    [ SuppressMessage( "ReSharper", "RedundantAssignment" ) ]
    [ SuppressMessage( "ReSharper", "AssignNullToNotNullAttribute" ) ]
    [ SuppressMessage( "ReSharper", "MergeIntoPattern" ) ]
    [ SuppressMessage( "ReSharper", "PossibleNullReferenceException" ) ]
    [ SuppressMessage( "ReSharper", "RedundantExtendsListEntry" ) ]
    public class WebControl : MetroTabControl, ISupportInitialize, IDisposable
    {
        /// <summary>
        /// The text left margin
        /// </summary>
        private const int _TEXT_LEFT_MARGIN = 15;

        /// <summary>
        /// The text right margin
        /// </summary>
        private const int _TEXT_RIGHT_MARGIN = 10;

        /// <summary>
        /// The definition header height
        /// </summary>
        private const int _DEF_HEADER_HEIGHT = 28;

        /// <summary>
        /// The definition button height
        /// </summary>
        private const int _DEF_BUTTON_HEIGHT = 28;

        /// <summary>
        /// The definition glyph width
        /// </summary>
        private const int _DEF_GLYPH_WIDTH = 40;

        /// <summary>
        /// The start position
        /// </summary>
        private int _startPosition = 10;

        /// <summary>
        /// The format string
        /// </summary>
        private StringFormat _formatString;

        /// <summary>
        /// The initializing
        /// </summary>
        private bool _initializing;

        /// <summary>
        /// The menu
        /// </summary>
        private protected readonly ContextMenuStrip _menu;

        /// <summary>
        /// The open
        /// </summary>
        private protected bool _open;

        /// <summary>
        /// The selected item
        /// </summary>
        private BrowserTab _selectedItem;

        /// <summary>
        /// The rectangle
        /// </summary>
        private Rectangle _rectangle = Rectangle.Empty;

        /// <summary>
        /// The add button width
        /// </summary>
        public int AddButtonWidth = 40;

        /// <summary>
        /// The maximum tab size
        /// </summary>
        public int MaxTabSize = 200;

        /// <summary>
        /// Occurs when [tab strip item closing].
        /// </summary>
        public event TabItemClosing TabStripItemClosing;

        /// <summary>
        /// Occurs when [tab strip item selection changed].
        /// </summary>
        public event TabItemChange TabStripItemSelectionChanged;

        /// <summary>
        /// Occurs when [menu items loading].
        /// </summary>
        public event HandledEventHandler MenuItemsLoading;

        /// <summary>
        /// Occurs when [menu items loaded].
        /// </summary>
        public event EventHandler MenuItemsLoaded;

        /// <summary>
        /// Occurs when [tab strip item closed].
        /// </summary>
        public event EventHandler TabStripItemClosed;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Badger.MetroTabControl" /> class.
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc />
        public WebControl( )
        {
            BeginInit( );
            Items = new BrowserTabCollection( );
            Items.CollectionChanged += OnCollectionChanged;
            _formatString = new StringFormat( );
            EndInit( );
            UpdateFormat( );
        }

        /// <summary>
        /// Gets or sets the first item in the current selection
        /// or returns null if the selection is empty.
        /// </summary>
        public new BrowserTab SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if( _selectedItem == value )
                {
                    return;
                }

                if( value == null
                    && Items.Count > 0 )
                {
                    var _fATabStripItem = Items[ 0 ];
                    if( _fATabStripItem.IsVisible )
                    {
                        _selectedItem = _fATabStripItem;
                        _selectedItem.Selected = true;
                    }
                }
                else
                {
                    _selectedItem = value;
                }

                foreach( BrowserTab _item in Items )
                {
                    if( _item == _selectedItem )
                    {
                        SelectItem( _item );
                        _item.IsVisible = true;
                    }
                    else
                    {
                        UnSelectItem( _item );
                        _item.IsVisible = false;
                    }
                }

                SelectItem( _selectedItem );
                if( !_selectedItem.IsLoaded )
                {
                    Items.MoveTo( 0, _selectedItem );
                }

                OnBrowserTabItemChanged( new BrowserTabChangedEventArgs( _selectedItem,
                    ChangeType.SelectionChanged ) );
            }
        }

        /// <summary>
        /// Gets the collection used to generate the content of the
        /// <see cref="T:System.Windows.Controls.ItemsControl" />.
        /// </summary>
        public new BrowserTabCollection Items { get; set; }

        /// <summary>
        /// Adds the tab.
        /// </summary>
        /// <param name="tabItem">The tab item.</param>
        public void AddTab( BrowserTab tabItem )
        {
            AddTab( tabItem, false );
        }

        /// <summary>
        /// Adds the tab.
        /// </summary>
        /// <param name="tabItem">The tab item.</param>
        /// <param name="autoSelect">if set to <c>true</c> [automatic select].</param>
        public void AddTab( BrowserTab tabItem, bool autoSelect )
        {
            Items.Add( tabItem );
            if( ( autoSelect && tabItem.IsVisible )
                || ( tabItem.IsVisible && Items.DrawnCount < 1 ) )
            {
                SelectedItem = tabItem;
                SelectItem( tabItem );
            }
        }

        /// <summary>
        /// Removes the tab.
        /// </summary>
        /// <param name="tabItem">The tab item.</param>
        public void RemoveTab( BrowserTab tabItem )
        {
            var _num = Items.IndexOf( tabItem );
            if( _num >= 0 )
            {
                UnSelectItem( tabItem );
                Items.Remove( tabItem );
            }

            if( Items.Count > 0 )
            {
                if( Items[ _num - 1 ] != null )
                {
                    SelectedItem = Items[ _num - 1 ];
                }
                else
                {
                    SelectedItem = Items.FirstVisible;
                }
            }
        }

        /// <summary>
        /// Gets the tab item by point.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <returns></returns>
        public BrowserTab GetTabItemByPoint( Point pt )
        {
            BrowserTab _result = null;
            var _flag = false;
            for( var _i = 0; _i < Items.Count; _i++ )
            {
                var _fATabStripItem = Items[ _i ];
                if( _fATabStripItem.IsVisible
                    && _fATabStripItem.IsLoaded )
                {
                    _result = _fATabStripItem;
                    _flag = true;
                }

                if( _flag )
                {
                    break;
                }
            }

            return _result;
        }

        /// <inheritdoc />
        /// <summary>
        /// Indicates that the initialization of the
        /// <see cref="T:System.Windows.Controls.ItemsControl" />
        /// object is about to start.
        /// </summary>
        public override void BeginInit( )
        {
            _initializing = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Indicates that the initialization of the
        /// <see cref="T:System.Windows.Controls.ItemsControl" />
        /// object is complete.
        /// </summary>
        public override void EndInit( )
        {
            _initializing = false;
        }

        /// <summary>
        /// Ensures that all visual child elements of this
        /// element are properly updated for layout.
        /// </summary>
        protected virtual void UpdateFormat( )
        {
            if( _formatString != null )
            {
                _formatString.Trimming = StringTrimming.EllipsisCharacter;
                _formatString.FormatFlags |= StringFormatFlags.NoWrap;
                _formatString.FormatFlags &= StringFormatFlags.DirectionRightToLeft;
            }
        }

        /// <summary>
        /// Selects the item.
        /// </summary>
        /// <param name="tabItem">The tab item.</param>
        public void SelectItem( BrowserTab tabItem )
        {
            tabItem.IsVisible = true;
            tabItem.Selected = true;
        }

        /// <summary>
        /// Uns the select item.
        /// </summary>
        /// <param name="tabItem">The tab item.</param>
        public void UnSelectItem( BrowserTab tabItem )
        {
            tabItem.Selected = false;
        }

        /// <summary>
        /// Sets the default selection.
        /// </summary>
        private void SetDefaultSelection( )
        {
            if( _selectedItem == null
                && Items.Count > 0 )
            {
                _selectedItem = Items[ 0 ];
            }

            for( var _i = 0; _i < Items.Count; _i++ )
            {
                var _fATabStripItem = Items[ _i ];
            }
        }

        /// <summary>
        /// Raises the <see cref="E:TabStripItemClosing" /> event.
        /// </summary>
        /// <param name="e">The <see cref="BrowserTabClosingEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnTabStripItemClosing( BrowserTabClosingEventArgs e )
        {
            TabStripItemClosing?.Invoke( e );
        }

        /// <summary>
        /// Raises the <see cref="E:TabStripItemClosed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnTabStripItemClosed( RoutedEventArgs e )
        {
            _selectedItem = null;
            TabStripItemClosed?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:MenuItemsLoading" /> event.
        /// </summary>
        /// <param name="e">The <see cref="HandledEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnMenuItemsLoading( HandledEventArgs e )
        {
            MenuItemsLoading?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:MenuItemsLoaded" /> event.
        /// </summary>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnMenuItemsLoaded( RoutedEventArgs e )
        {
            MenuItemsLoaded?.Invoke( this, e );
        }

        /// <summary>
        /// Raises the <see cref="E:BrowserTabItemChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="BrowserTabChangedEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnBrowserTabItemChanged( BrowserTabChangedEventArgs e )
        {
            TabStripItemSelectionChanged?.Invoke( e );
        }

        /// <summary>
        /// Raises the <see cref="E:RightToLeftChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnRightToLeftChanged( RoutedEventArgs e )
        {
            UpdateFormat( );
        }

        /// <summary>
        /// Raises the <see cref="E:SizeChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="RoutedEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnSizeChanged( RoutedEventArgs e )
        {
            if( !_initializing )
            {
                UpdateFormat( );
            }
        }

        /// <summary>
        /// Called when [collection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CollectionChangeEventArgs"/>
        /// instance containing the event data.</param>
        private void OnCollectionChanged( object sender, CollectionChangeEventArgs e )
        {
            var _tab = ( BrowserTab )e.Element;
            if( e.Action == CollectionChangeAction.Add )
            {
                OnBrowserTabItemChanged( new BrowserTabChangedEventArgs( _tab, ChangeType.Added ) );
            }
            else if( e.Action == CollectionChangeAction.Remove )
            {
                OnBrowserTabItemChanged(
                    new BrowserTabChangedEventArgs( _tab, ChangeType.Removed ) );
            }
            else
            {
                OnBrowserTabItemChanged(
                    new BrowserTabChangedEventArgs( _tab, ChangeType.Changed ) );
            }

            UpdateFormat( );
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose( bool disposing )
        {
            if( disposing )
            {
                Items.CollectionChanged -= OnCollectionChanged;
                for( var _i = 0; _i < Items.Count; _i++ )
                {
                    var _item = Items[ _i ];
                    _item?.Dispose( );
                }

                if( _menu != null
                    && !_menu.IsDisposed )
                {
                    _menu.Dispose( );
                }

                _formatString?.Dispose( );
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks
        /// associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose( )
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
    }
}