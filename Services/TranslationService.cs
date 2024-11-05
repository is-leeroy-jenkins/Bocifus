// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-04-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-04-2024
// ******************************************************************************************
// <copyright file="TranslationApiService.cs" company="Terry D. Eppler">
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
//   TranslationApiService.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using Google.Cloud.Translation.V2;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.IDisposable" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="T:System.Windows.Markup.IStyleConnector" />
    public partial class MainWindow
    {
        /// <summary>
        /// Translates the API request asynchronous.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="targetLang">The target language.</param>
        /// <returns></returns>
        /// <exception cref="ABI.System.Exception">
        /// Translate API Key is not set.
        /// or
        /// Translate API URL is not set.
        /// or
        /// API request failed: {ex.Message}
        /// or
        /// Translation API Provider is not set.
        /// </exception>
        public async Task<string> TranslateApiRequestAsync( string inputText, string targetLang )
        {
            if( AppSettings.TranslationApiProvider == "DeepL" )
            {
                if( string.IsNullOrWhiteSpace( AppSettings.TranslationApiKeyDeepL ) )
                {
                    throw new Exception( "Translate API Key is not set." );
                }

                if( string.IsNullOrWhiteSpace( AppSettings.TranslationApiUrlDeepL ) )
                {
                    throw new Exception( "Translate API URL is not set." );
                }

                using var _client = new HttpClient( );
                try
                {
                    var _content = new FormUrlEncodedContent( new[ ]
                    {
                        new KeyValuePair<string, string>( "text", inputText ),
                        new KeyValuePair<string, string>( "target_lang", targetLang )
                    } );

                    _client.DefaultRequestHeaders.Add( "Authorization",
                        $"DeepL-Auth-Key {AppSettings.TranslationApiKeyDeepL}" );

                    var _response = await _client.PostAsync( AppSettings.TranslationApiUrlDeepL,
                        _content );

                    var _responseBody = await _response.Content.ReadAsStringAsync( );
                    var json = JObject.Parse( _responseBody );
                    return json[ "translations" ][ 0 ][ "text" ].ToString( );
                }
                catch( Exception ex )
                {
                    throw new Exception( $"API request failed: {ex.Message}" );
                }
            }
            else if( AppSettings.TranslationApiProvider == "Google" )
            {
                if( string.IsNullOrWhiteSpace( AppSettings.TranslationApiKeyGoogle ) )
                {
                    throw new Exception( "Translate API Key is not set." );
                }

                if( string.IsNullOrWhiteSpace( AppSettings.TranslationApiUrlGoogle ) )
                {
                    throw new Exception( "Translate API URL is not set." );
                }

                using var _client =
                    TranslationClient.CreateFromApiKey( AppSettings.TranslationApiKeyGoogle );

                try
                {
                    var _translationResult = await _client.TranslateTextAsync( inputText,
                        targetLang, model: TranslationModel.NeuralMachineTranslation );

                    return _translationResult.TranslatedText;
                }
                catch( Exception ex )
                {
                    throw new Exception( $"API request failed: {ex.Message}" );
                }
            }
            else
            {
                throw new Exception( "Translation API Provider is not set." );
            }
        }
    }
}