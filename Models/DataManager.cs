// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="DataManager.cs" company="Terry D. Eppler">
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
//   DataManager.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using Model;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.Encodings.Web;
    using System.Text.Json;

    [ SuppressMessage( "ReSharper", "ClassNeverInstantiated.Global" ) ]
    public class DataManager
    {
        /// <summary>
        /// Loads the conversations from json.
        /// </summary>
        /// <returns></returns>
        public static ConversationManager LoadConversationsFromJson( )
        {
            var _documentsPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
            var _dataDirectory =
                Path.Combine( _documentsPath, "Bocifus", "ConversationHistory" );

            var _manager = new ConversationManager
            {
                Histories = new ObservableCollection<ConversationHistory>( )
            };

            Directory.CreateDirectory( _dataDirectory );
            var _files = Directory.GetFiles( _dataDirectory, "Conversation_*.json" );
            for( var _i = 0; _i < _files.Length; _i++ )
            {
                var _file = _files[ _i ];
                var _jsonString = File.ReadAllText( _file );
                var _conversation = JsonSerializer.Deserialize<ConversationHistory>( _jsonString );
                if( _conversation != null )
                {
                    _manager.Histories.Add( _conversation );
                }
            }

            return _manager;
        }

        /// <summary>
        /// Saves the conversations as json.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public static void SaveConversationsAsJson( ConversationManager manager )
        {
            var _documentsPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
            var _dataDirectory =
                Path.Combine( _documentsPath, "Bocifus", "ConversationHistory" );

            Directory.CreateDirectory( _dataDirectory );
            foreach( var _file in Directory.EnumerateFiles( _dataDirectory, "*.json" ) )
            {
                File.Delete( _file );
            }

            var _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            };

            foreach( var _conversation in manager.Histories )
            {
                var _formattedLastUpdated = _conversation.LastUpdated.ToString( "yyyyMMddHHmmss" );
                var _filePath = Path.Combine( _dataDirectory,
                    $"Conversation_{_formattedLastUpdated}_{_conversation.ID}.json" );

                var _jsonString = JsonSerializer.Serialize( _conversation, _options );
                File.WriteAllText( _filePath, _jsonString );
            }
        }

        /// <summary>
        /// Saves the prompt template as json.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public static void SavePromptTemplateAsJson( PromptTemplateManager manager )
        {
            var _documentsPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
            var _dataDirectory = Path.Combine( _documentsPath, "Bocifus", "PromptTemplate" );
            Directory.CreateDirectory( _dataDirectory );
            foreach( var _file in Directory.EnumerateFiles( _dataDirectory, "*.json" ) )
            {
                File.Delete( _file );
            }

            var _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            };

            foreach( var _template in manager.Templates )
            {
                var _formattedLastUpdated = _template.LastUpdated.ToString( "yyyyMMddHHmmss" );
                var _filePath = Path.Combine( _dataDirectory,
                    $"PromptTemplate_{_template.SortOrder}_{_formattedLastUpdated}_{_template.ID}.json" );

                var _jsonString = JsonSerializer.Serialize( _template, _options );
                File.WriteAllText( _filePath, _jsonString );
            }
        }

        /// <summary>
        /// Loads the prompt template from json.
        /// </summary>
        /// <returns></returns>
        public static PromptTemplateManager LoadPromptTemplateFromJson( )
        {
            var _documentsPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
            var _dataDirectory = Path.Combine( _documentsPath, "Bocifus", "PromptTemplate" );
            var _manager = new PromptTemplateManager
            {
                Templates = new ObservableCollection<PromptTemplate>( )
            };

            Directory.CreateDirectory( _dataDirectory );
            var _files = Directory.GetFiles( _dataDirectory, "PromptTemplate_*.json" );
            for( var _index = 0; _index < _files.Length; _index++ )
            {
                var _file = _files[ _index ];
                var _jsonString = File.ReadAllText( _file );
                var _templates = JsonSerializer.Deserialize<PromptTemplate>( _jsonString );
                if( _templates != null )
                {
                    _manager.Templates.Add( _templates );
                }
            }

            return _manager;
        }
    }
}