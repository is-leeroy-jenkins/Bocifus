// ******************************************************************************************
//     Assembly:                Booger
//     Author:                  Terry D. Eppler
//     Created:                 10-19-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        10-19-2024
// ******************************************************************************************
// <copyright file="MetroTabItem.cs" company="Terry D. Eppler">
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
//   MetroTabItem.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    [ SuppressMessage( "ReSharper", "UnusedType.Global" ) ]
    public class MetroTabItem : TabItem
    {
        /// <summary>
        /// The theme
        /// </summary>
        private protected DarkMode _theme = new DarkMode( );

        /// <summary>
        /// Initializes a new instance of the <see cref="MetroTabItem"/> class.
        /// </summary>
        public MetroTabItem( )
            : base( )
        {
            Width = 100;
            Height = 24;
            Margin = new Thickness( 0 );
            Padding = new Thickness( 6, 2, 6, 5 );
            Background = _theme.TransparentBrush;
            BorderBrush = _theme.TransparentBrush;
            Foreground = _theme.Foreground;
        }
    }
}