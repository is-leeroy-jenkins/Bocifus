// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 10-31-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        10-31-2024
// ******************************************************************************************
// <copyright file="CollectionWithEvents.cs" company="Terry D. Eppler">
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
//   CollectionWithEvents.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System.Diagnostics.CodeAnalysis;
    using System.Collections;
    using System.ComponentModel;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:System.Collections.CollectionBase" />
    [ SuppressMessage( "ReSharper", "PublicConstructorInAbstractClass" ) ]
    public abstract class CollectionWithEvents : CollectionBase
    {
        /// <summary>
        /// The suspend count
        /// </summary>
        private int _suspendCount;

        /// <summary>
        /// Gets a value indicating whether this instance is suspended.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is suspended; otherwise, <c>false</c>.
        /// </value>
        [ Browsable( false ) ]
        public bool IsSuspended
        {
            get { return _suspendCount > 0; }
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:CollectionWithEvents" /> class.
        /// </summary>
        public CollectionWithEvents( )
        {
            _suspendCount = 0;
        }

        /// <summary>
        /// Occurs when [clearing].
        /// </summary>
        [ Browsable( false ) ]
        public event CollectionClear Clearing;

        /// <summary>
        /// Occurs when [cleared].
        /// </summary>
        [ Browsable( false ) ]
        public event CollectionClear Cleared;

        /// <summary>
        /// Occurs when [inserting].
        /// </summary>
        [ Browsable( false ) ]
        public event CollectionChanged Inserting;

        /// <summary>
        /// Occurs when [inserted].
        /// </summary>
        [ Browsable( false ) ]
        public event CollectionChanged Inserted;

        /// <summary>
        /// Occurs when [removing].
        /// </summary>
        [ Browsable( false ) ]
        public event CollectionChanged Removing;

        /// <summary>
        /// Occurs when [removed].
        /// </summary>
        [ Browsable( false ) ]
        public event CollectionChanged Removed;

        /// <summary>
        /// Suspends the events.
        /// </summary>
        public void SuspendEvents( )
        {
            _suspendCount++;
        }

        /// <summary>
        /// Resumes the events.
        /// </summary>
        public void ResumeEvents( )
        {
            _suspendCount--;
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs additional custom processes when
        /// clearing the contents of the
        /// <see cref="T:System.Collections.CollectionBase" /> instance.
        /// </summary>
        protected override void OnClear( )
        {
            if( !IsSuspended
                && Clearing != null )
            {
                Clearing( );
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs additional custom processes after
        /// clearing the contents of the
        /// <see cref="T:System.Collections.CollectionBase" /> instance.
        /// </summary>
        protected override void OnClearComplete( )
        {
            if( !IsSuspended
                && Cleared != null )
            {
                Cleared( );
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs additional custom processes before
        /// inserting a new element into the
        /// <see cref="T:System.Collections.CollectionBase" /> instance.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which to insert
        /// <paramref name="value" />.
        /// </param>
        /// <param name="value">
        /// The new value of the element at
        /// <paramref name="index" />.
        /// </param>
        protected override void OnInsert( int index, object value )
        {
            if( !IsSuspended
                && Inserting != null )
            {
                Inserting( index, value );
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs additional custom processes after
        /// inserting a new element into the
        /// <see cref="T:System.Collections.CollectionBase" /> instance.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which to insert
        /// <paramref name="value" />.
        /// </param>
        /// <param name="value">
        /// The new value of the element at
        /// <paramref name="index" />.
        /// </param>
        protected override void OnInsertComplete( int index, object value )
        {
            if( !IsSuspended
                && Inserted != null )
            {
                Inserted( index, value );
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs additional custom processes when
        /// removing an element from the
        /// <see cref="T:System.Collections.CollectionBase" /> instance.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which
        /// <paramref name="value" /> can be found.
        /// </param>
        /// <param name="value">
        /// The value of the element to remove from
        /// <paramref name="index" />.
        /// </param>
        protected override void OnRemove( int index, object value )
        {
            if( !IsSuspended
                && Removing != null )
            {
                Removing( index, value );
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs additional custom processes after removing
        /// an element from the
        /// <see cref="T:System.Collections.CollectionBase" /> instance.
        /// </summary>
        /// <param name="index">The zero-based index at which
        /// <paramref name="value" /> can be found.</param>
        /// <param name="value">The value of the element to
        /// remove from <paramref name="index" />.</param>
        protected override void OnRemoveComplete( int index, object value )
        {
            if( !IsSuspended
                && Removed != null )
            {
                Removed( index, value );
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the
        /// <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <param name="value">The object to locate in the
        /// <see cref="T:System.Collections.IList" />.</param>
        /// <returns>
        /// The index of
        /// <paramref name="value" />
        /// if found in the list; otherwise, -1.
        /// </returns>
        protected int IndexOf( object value )
        {
            return List.IndexOf( value );
        }
    }
}