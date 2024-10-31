// ******************************************************************************************
//     Assembly:                Baby
//     Author:                  Terry D. Eppler
//     Created:                 09-09-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        09-09-2024
// ******************************************************************************************
// <copyright file="BrowserTab.cs" company="Terry D. Eppler">
//     Baby is a light-weight, full-featured, web-browser built with .NET 6 and is written
//     in C#.  The baby browser is designed for budget execution and data analysis.
//     A tool for EPA analysts and a component that can be used for general browsing.
// 
//     Copyright ©  2020 Terry D. Eppler
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
//   BrowserTab.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using CefSharp.Wpf;
    using Syncfusion.Windows.Tools.Controls;

    /// <inheritdoc />
    /// <summary>
    /// POCO created for holding data per tab
    /// </summary>
    [ SuppressMessage( "ReSharper", "MemberCanBeInternal" ) ]
    [ SuppressMessage( "ReSharper", "MissingBlankLines" ) ]
    [ SuppressMessage( "ReSharper", "ClassCanBeSealed.Global" ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" ) ]
    public class BrowserTab : TabItemExt, INotifyPropertyChanged
    {
        /// <summary>
        /// The path
        /// </summary>
        private protected object _path = new object( );

        /// <summary>
        /// The busy
        /// </summary>
        private protected bool _busy;

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
        private protected readonly DarkMode _theme = new DarkMode( );

        /// <summary>
        /// The is selected
        /// </summary>
        private bool _isSelected;

        /// <summary>
        /// The is visible
        /// </summary>
        private bool _isVisible = true;

        /// <summary>
        /// The is open
        /// </summary>
        private protected bool _isOpen;

        /// <summary>
        /// The can close
        /// </summary>
        private protected bool _canClose;

        /// <summary>
        /// The image
        /// </summary>
        private protected Image _image;

        /// <summary>
        /// The original URL
        /// </summary>
        private protected string _originalUrl;

        /// <summary>
        /// The current URL
        /// </summary>
        private protected string _currentUrl;

        /// <summary>
        /// The title
        /// </summary>
        private protected string _title;

        /// <summary>
        /// The referring URL
        /// </summary>
        private protected string _referringUrl;

        /// <summary>
        /// The date created
        /// </summary>
        private protected DateTime _dateCreated;

        /// <summary>
        /// The timer
        /// </summary>
        private protected Timer _timer;

        /// <summary>
        /// The timer
        /// </summary>
        private protected TimerCallback _timerCallback;

        /// <summary>
        /// The browser
        /// </summary>
        private protected ChromiumWebBrowser _browser;

        /// <summary>
        /// The tag
        /// </summary>
        private protected object _tag;

        /// <summary>
        /// The tab
        /// </summary>
        private protected BrowserTab _tab;

        /// <inheritdoc />
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Baby.BrowserTab" /> class.
        /// </summary>
        public BrowserTab( )
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Baby.BrowserTab" /> class.
        /// </summary>
        /// <param name="window">
        /// The display control.
        /// </param>
        public BrowserTab( Window window )
            : this( string.Empty, window )
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Baby.BrowserTab" /> class.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="window">The display control.</param>
        public BrowserTab( string caption, Window window )
        {
            _isSelected = false;
            _isVisible = true;
            UpdateText( caption, window );
            if( window != null )
            {
                base.AddChild( window );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control
        /// and all its child controls are displayed.
        /// </summary>
        [ DefaultValue( true ) ]
        public new bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if( _isVisible != value )
                {
                    _isVisible = value;
                    OnPropertyChanged( nameof( IsVisible ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        [ DefaultValue( false ) ]
        [ Browsable( false ) ]
        public bool Selected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if( _isSelected != value )
                {
                    _isSelected = value;
                }
            }
        }

        /// <summary>
        /// Gets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        [ Browsable( false ) ]
        public string Caption
        {
            get
            {
                return _title;
            }
        }

        /// <summary>
        /// The is open
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                if( _isOpen != value )
                {
                    _isOpen = value;
                    OnPropertyChanged( nameof( IsOpen ) );
                }
            }
        }

        /// <summary>
        /// The original URL
        /// </summary>
        public string OriginalUrl
        {
            get
            {
                return _originalUrl;
            }
            set
            {
                if( _originalUrl != value )
                {
                    _originalUrl = value;
                    OnPropertyChanged( nameof( OriginalUrl ) );
                }
            }
        }

        /// <summary>
        /// The current URL
        /// </summary>
        public string CurrentUrl
        {
            get
            {
                return _currentUrl;
            }
            set
            {
                if( _currentUrl != value )
                {
                    _currentUrl = value;
                    OnPropertyChanged( nameof( CurrentUrl ) );
                }
            }
        }

        /// <summary>
        /// The title
        /// </summary>
        [ DefaultValue( "Name" ) ]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if( _title != value )
                {
                    _title = value;
                    OnPropertyChanged( nameof( Title ) );
                }
            }
        }

        /// <summary>
        /// The referring URL
        /// </summary>
        public string ReferringUrl
        {
            get
            {
                return _referringUrl;
            }
            set
            {
                if( _referringUrl != value )
                {
                    _referringUrl = value;
                    OnPropertyChanged( nameof( ReferringUrl ) );
                }
            }
        }

        /// <summary>
        /// The date created
        /// </summary>
        public DateTime DateCreated
        {
            get
            {
                return _dateCreated;
            }
            set
            {
                if( _dateCreated != value )
                {
                    _dateCreated = value;
                    OnPropertyChanged( nameof( DateCreated ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the tab.
        /// </summary>
        /// <value>
        /// The tab.
        /// </value>
        public BrowserTab Tab
        {
            get
            {
                return _tab;
            }
            set
            {
                if( _tab != value )
                {
                    _tab = value;
                    OnPropertyChanged(nameof(Tab));
                }
            }
        }

        /// <summary>
        /// The browser
        /// </summary>
        public ChromiumWebBrowser Browser
        {
            get
            {
                return _browser;
            }
            set
            {
                if(_browser != value)
                {
                    _browser = value;
                    OnPropertyChanged(nameof(Browser));
                }
            }
        }
        
        /// <summary>
        /// Assigns the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Assign( BrowserTab item )
        {
            IsVisible = item.IsVisible;
            Title = item.Title;
            CanClose = item.CanClose;
            Tag = item.Tag;
        }

        /// <inheritdoc />
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents this instance.
        /// </returns>
        public override string ToString( )
        {
            return _title;
        }

        /// <summary>
        /// Updates the text.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="displayControl">
        /// The display control.
        /// </param>
        private void UpdateText( string caption, Window displayControl )
        {
            if( caption.Length <= 0
                && displayControl != null )
            {
                Title = displayControl.Title;
            }
            else if( caption != null )
            {
                Title = caption;
            }
        }
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property.</param>
        protected void OnPropertyChanged( [ CallerMemberName ] string propertyName = null ) 
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks
        /// associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c>
        /// to release both managed
        /// and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                _timer?.Dispose();
            }
        }

        /// <summary>
        /// Fails the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private protected void Fail(Exception ex )
        {
            var _error = new ErrorWindow(ex);
            _error?.SetText();
            _error?.ShowDialog();
        }
    }
}