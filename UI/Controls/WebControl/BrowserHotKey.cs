// ******************************************************************************************
//     Assembly:                Booger
//     Author:                  Terry D. Eppler
//     Created:                 09-09-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        09-09-2024
// ******************************************************************************************
// <copyright file="BrowserHotKey.cs" company="Terry D. Eppler">
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
//   BrowserHotKey.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;

    /// <summary>
    /// POCO for holding hot-key data
    /// </summary>
    [ SuppressMessage( "ReSharper", "FieldCanBeMadeReadOnly.Global" ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBeInternal" ) ]
    [ SuppressMessage( "ReSharper", "ClassCanBeSealed.Global" ) ]
    public class BrowserHotKey
    {
        /// <summary>
        /// The key
        /// </summary>
        public Keys Key;

        /// <summary>
        /// The key code
        /// </summary>
        public int KeyCode;

        /// <summary>
        /// The control
        /// </summary>
        public bool Ctrl;

        /// <summary>
        /// The shift
        /// </summary>
        public bool Shift;

        /// <summary>
        /// The alt
        /// </summary>
        public bool Alt;

        /// <summary>
        /// The callback
        /// </summary>
        public Action Callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserHotKey"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="key">The key.</param>
        /// <param name="ctrl">if set to <c>true</c> [control].</param>
        /// <param name="shift">if set to <c>true</c> [shift].</param>
        /// <param name="alt">if set to <c>true</c> [alt].</param>
        public BrowserHotKey( Action callback, Keys key, bool ctrl = false,
            bool shift = false, bool alt = false )
        {
            Callback = callback;
            Key = key;
            KeyCode = ( int )key;
            Ctrl = ctrl;
            Shift = shift;
            Alt = alt;
        }
    }
}