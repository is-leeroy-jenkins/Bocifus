// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 10-31-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        10-31-2024
// ******************************************************************************************
// <copyright file="BrowserTabClosingEventArgs.cs" company="Terry D. Eppler">
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
//   BrowserTabClosingEventArgs.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:System.EventArgs" />
    [ SuppressMessage( "ReSharper", "MemberCanBeInternal" ) ]
    public class BrowserTabClosingEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        public BrowserTab Item { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BrowserTabClosingEventArgs"/> is cancel.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Baby.TabStripItemClosingEventArgs" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public BrowserTabClosingEventArgs( BrowserTab item )
        {
            Item = item;
        }
    }
}