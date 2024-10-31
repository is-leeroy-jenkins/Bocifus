// ******************************************************************************************
//     Assembly:                Baby
//     Author:                  Terry D. Eppler
//     Created:                 09-09-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        09-09-2024
// ******************************************************************************************
// <copyright file="BrowserTabStripItemCollection.cs" company="Terry D. Eppler">
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
//   BrowserTabStripItemCollection.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:Baby.CollectionWithEvents" />
    [ SuppressMessage( "ReSharper", "UnusedMethodReturnValue.Global" ) ]
    [ SuppressMessage( "ReSharper", "UnusedParameter.Global" ) ]
    [ SuppressMessage( "ReSharper", "ParameterTypeCanBeEnumerable.Global" ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBeInternal" ) ]
    [ SuppressMessage( "ReSharper", "PossibleNullReferenceException" ) ]
    public class BrowserTabCollection : CollectionWithEvents
    {
        /// <summary>
        /// The lock update
        /// </summary>
        private int _lockUpdate;

        /// <summary>
        /// Gets or sets the
        /// <see cref="BrowserTab"/>
        /// at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="BrowserTab"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public BrowserTab this[ int index ]
        {
            get
            {
                return index < 0 || List.Count - 1 < index
                    ? null
                    : ( BrowserTab )List[ index ];
            }
            set
            {
                List[ index ] = value;
            }
        }

        /// <summary>
        /// Gets the drawn count.
        /// </summary>
        /// <value>
        /// The drawn count.
        /// </value>
        [ Browsable( false ) ]
        public virtual int DrawnCount
        {
            get
            {
                var _count = Count;
                var _num = 0;
                if( _count == 0 )
                {
                    return 0;
                }

                for( var _i = 0; _i < _count; _i++ )
                {
                    if( this[ _i ].IsLoaded )
                    {
                        _num++;
                    }
                }

                return _num;
            }
        }

        /// <summary>
        /// Gets the last visible.
        /// </summary>
        /// <value>
        /// The last visible.
        /// </value>
        public virtual BrowserTab LastVisible
        {
            get
            {
                for( var _num = Count - 1; _num > 0; _num-- )
                {
                    if( this[ _num ].IsVisible )
                    {
                        return this[ _num ];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the first visible.
        /// </summary>
        /// <value>
        /// The first visible.
        /// </value>
        public virtual BrowserTab FirstVisible
        {
            get
            {
                for( var _i = 0; _i < Count; _i++ )
                {
                    if( this[ _i ].IsVisible )
                    {
                        return this[ _i ];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the visible count.
        /// </summary>
        /// <value>
        /// The visible count.
        /// </value>
        [ Browsable( false ) ]
        public virtual int VisibleCount
        {
            get
            {
                var _count = Count;
                var _num = 0;
                if( _count == 0 )
                {
                    return 0;
                }

                for( var _i = 0; _i < _count; _i++ )
                {
                    if( this[ _i ].IsVisible )
                    {
                        _num++;
                    }
                }

                return _num;
            }
        }

        /// <summary>
        /// Occurs when [collection changed].
        /// </summary>
        [ Browsable( false ) ]
        public event CollectionChangeEventHandler CollectionChanged;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Bocifus.BrowserTabCollection" /> class.
        /// </summary>
        public BrowserTabCollection( )
        {
            _lockUpdate = 0;
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="items">The items.</param>
        public virtual void AddRange( BrowserTab[ ] items )
        {
            BeginUpdate( );
            try
            {
                foreach( var _value in items )
                {
                    List.Add( _value );
                }
            }
            finally
            {
                EndUpdate( );
            }
        }

        /// <summary>
        /// Assigns the specified collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public virtual void Assign( BrowserTabCollection collection )
        {
            BeginUpdate( );
            try
            {
                Clear( );
                for( var _i = 0; _i < collection.Count; _i++ )
                {
                    var _item = collection[ _i ];
                    var _fATabStripItem = new BrowserTab( );
                    _fATabStripItem.Assign( _item );
                    Add( _fATabStripItem );
                }
            }
            finally
            {
                EndUpdate( );
            }
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual int Add( BrowserTab item )
        {
            var _num = IndexOf( item );
            if( _num == -1 )
            {
                _num = List.Add( item );
            }

            return _num;
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public virtual void Remove( BrowserTab item )
        {
            if( List.Contains( item ) )
            {
                List.Remove( item );
            }
        }

        /// <summary>
        /// Moves to.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual BrowserTab MoveTo( int newIndex, BrowserTab item )
        {
            var _num = List.IndexOf( item );
            if( _num >= 0 )
            {
                RemoveAt( _num );
                Insert( 0, item );
                return item;
            }

            return null;
        }

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual int IndexOf( BrowserTab item )
        {
            return List.IndexOf( item );
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if [contains]
        /// [the specified item]; otherwise,
        /// <c>false</c>.
        /// </returns>
        public virtual bool Contains( BrowserTab item )
        {
            return List.Contains( item );
        }

        /// <summary>
        /// Inserts the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        public virtual void Insert( int index, BrowserTab item )
        {
            if( !Contains( item ) )
            {
                List.Insert( index, item );
            }
        }

        /// <summary>
        /// Raises the
        /// <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The
        /// <see cref="CollectionChangeEventArgs"/>
        /// instance containing the event data.
        /// </param>
        protected virtual void OnCollectionChanged( CollectionChangeEventArgs e )
        {
            CollectionChanged?.Invoke( this, e );
        }

        /// <summary>
        /// Begins the update.
        /// </summary>
        protected virtual void BeginUpdate( )
        {
            _lockUpdate++;
        }

        /// <summary>
        /// Ends the update.
        /// </summary>
        protected virtual void EndUpdate( )
        {
            if( --_lockUpdate == 0 )
            {
                var _args = new CollectionChangeEventArgs( CollectionChangeAction.Refresh, null );
                OnCollectionChanged( _args );
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Called when [insert complete].
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void OnInsertComplete( int index, object item )
        {
            var _fATabStripItem = item as BrowserTab;
            _fATabStripItem.PropertyChanged += OnItemChanged;
            OnCollectionChanged( new CollectionChangeEventArgs( CollectionChangeAction.Add, item ) );
        }

        /// <inheritdoc />
        /// <summary>
        /// Called when [remove].
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void OnRemove( int index, object item )
        {
            base.OnRemove( index, item );
            var _fATabStripItem = item as BrowserTab;
            _fATabStripItem.PropertyChanged -= OnItemChanged;
            OnCollectionChanged( new CollectionChangeEventArgs( CollectionChangeAction.Remove, item ) );
        }

        /// <inheritdoc />
        /// <summary>
        /// Called when [clear].
        /// </summary>
        protected override void OnClear( )
        {
            if( Count == 0 )
            {
                return;
            }

            BeginUpdate( );
            try
            {
                for( var _num = Count - 1; _num >= 0; _num-- )
                {
                    RemoveAt( _num );
                }
            }
            finally
            {
                EndUpdate( );
            }
        }

        /// <summary>
        /// Called when [item changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/>
        /// instance containing the event data.
        /// </param>
        protected virtual void OnItemChanged( object sender, EventArgs e )
        {
            var _args = new CollectionChangeEventArgs( CollectionChangeAction.Refresh, sender );
            OnCollectionChanged( _args );
        }
    }
}