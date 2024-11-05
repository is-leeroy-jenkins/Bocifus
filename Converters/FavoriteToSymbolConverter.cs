// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="FavoriteToSymbolConverter.cs" company="Terry D. Eppler">
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
//   FavoriteToSymbolConverter.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using ModernWpf.Controls;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:System.Windows.Data.IValueConverter" />
    public class FavoriteToSymbolConverter : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="FavoriteToSymbolConverter"/> class.
        /// </summary>
        public FavoriteToSymbolConverter( )
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />,
        /// the valid null value is used.
        /// </returns>
        public object Convert( object value, Type targetType, object parameter,
            CultureInfo culture )
        {
            var _isFavorite = value is bool && ( bool )value;
            var _symbol = _isFavorite
                ? Symbol.Favorite
                : Symbol.OutlineStar;

            return new SymbolIcon
            {
                Symbol = _symbol
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />,
        /// the valid null value is used.
        /// </returns>
        /// <exception cref="T:System.NotImplementedException"></exception>
        public object ConvertBack( object value, Type targetType, object parameter,
            CultureInfo culture )
        {
            throw new NotImplementedException( );
        }
    }
}