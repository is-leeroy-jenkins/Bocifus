// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-15-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-15-2024
// ******************************************************************************************
// <copyright file="DataTableSerializer.cs" company="Terry D. Eppler">
//    Bocifus is an open source windows (wpf) application for interacting with OpenAI GPT
//    that is based on NET 7 and written in C-Sharp.
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
//   DataTableSerializer.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using System;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// 
    /// </summary>
    public class DataTableSerializer
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataTableSerializer"/> class.
        /// </summary>
        public DataTableSerializer( )
        {
        }

        /// <summary>
        /// Serializes the specified data table.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        public string Serialize( DataTable dataTable )
        {
            try
            {
                ThrowIf.Null( dataTable, nameof( dataTable ) );
                using var _stream = new MemoryStream( );
                var _serializer = new XmlSerializer( typeof( DataTable ) );
                _serializer.Serialize( _stream, dataTable );
                return Convert.ToBase64String( _stream.ToArray( ) );
            }
            catch( Exception ex )
            {
                Fail( ex );
                return string.Empty;
            }
        }

        /// <summary>
        /// Deserializes the specified serialized data table.
        /// </summary>
        /// <param name="serializedDataTable">The serialized data table.</param>
        /// <returns></returns>
        public DataTable Deserialize( string serializedDataTable )
        {
            try
            {
                ThrowIf.Empty( serializedDataTable, nameof( serializedDataTable ) );
                using var _stream =
                    new MemoryStream( Convert.FromBase64String( serializedDataTable ) );

                var _serializer = new XmlSerializer( typeof( DataTable ) );
                return ( DataTable )_serializer.Deserialize( _stream );
            }
            catch( Exception ex )
            {
                Fail( ex );
                return default( DataTable );
            }
        }

        /// <summary>
        /// Fails the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private protected void Fail( Exception ex )
        {
            var _error = new ErrorWindow( ex );
            _error?.SetText( );
            _error?.ShowDialog( );
        }
    }
}