// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="OpenAIApiService .cs" company="Terry D. Eppler">
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
//   OpenAIApiService .cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using Model;
    using MdXaml;
    using Microsoft.Toolkit.Uwp.Notifications;
    using OpenAI;
    using OpenAI.Managers;
    using OpenAI.ObjectModels.RequestModels;
    using OpenAI.Tokenizer.GPT3;
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using OpenAI.ObjectModels.ResponseModels;
    using Properties;
    using static UtilityFunctions;
    using MessageBox = ModernWpf.MessageBox;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <seealso cref="T:SourceChord.FluentWPF.AcrylicWindow" />
    /// <seealso cref="T:System.IDisposable" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="T:System.Windows.Markup.IStyleConnector" />
    [ SuppressMessage( "ReSharper", "PossibleNullReferenceException" ) ]
    [ SuppressMessage( "ReSharper", "BadParensLineBreaks" ) ]
    [ SuppressMessage( "ReSharper", "AssignNullToNotNullAttribute" ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" ) ]
    public partial class MainWindow
    {
        /// <summary>
        /// The result FLG
        /// </summary>
        private bool _resultFlg = true;

        /// <summary>
        /// The is processing
        /// </summary>
        private bool _isProcessing;

        /// <summary>
        /// Dummies the sub.
        /// </summary>
        private void DummySub( ) { }

        /// <summary>
        /// Flushes the windows message queue.
        /// </summary>
        private void FlushWindowsMessageQueue( )
        {
            Application.Current.Dispatcher.Invoke( new Action( DummySub ),
                DispatcherPriority.Background, new Object[ ]
                {
                } );
        }

        /// <summary>
        /// The user message
        /// </summary>
        private string _userMessage = "";

        /// <summary>
        /// The response text
        /// </summary>
        private string _responseText = "";

        /// <summary>
        /// The binary image
        /// </summary>
        private byte[ ] _binaryImage;

        /// <summary>
        /// The clipboard image
        /// </summary>
        private byte[ ] _clipboardImage;

        /// <summary>
        /// The generated title
        /// </summary>
        private string _generatedTitle = "";

        /// <summary>
        /// The title generating
        /// </summary>
        private bool _titleGenerating;

        /// <summary>
        /// The alert FLG
        /// </summary>
        private bool _alertFlg;

        /// <summary>
        /// The daily total
        /// </summary>
        private int _dailyTotal;

        /// <summary>
        /// The today string
        /// </summary>
        private string _todayString;

        /// <summary>
        /// The new identifier
        /// </summary>
        private Guid _newId;

        /// <summary>
        /// The temporary messages
        /// </summary>
        private List<ChatMessage> _tempMessages = new List<ChatMessage>( );

        /// <summary>
        /// Processes the open ai asynchronous.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <exception cref="Exception">
        /// ConfigurationName is not set.
        /// or
        /// </exception>
        private async Task ProcessOpenAiAsync( string prompt )
        {
            Debug.Print( "===== Start processing =====" );
            if( _isProcessing )
            {
                MessageBox.Show( "Processing is in progress." );
                return;
            }

            _isProcessing = true;
            _resultFlg = true;
            Prepare( );
            MessageScrollViewer.ScrollToBottom( );
            try
            {
                if( !RetrieveConfiguration( ) )
                {
                    throw new Exception( "ConfigurationName is not set." );
                }

                _binaryImage = null;
                if( _imageFilePath != null )
                {
                    _binaryImage = await File.ReadAllBytesAsync( _imageFilePath );
                }
                else if( _clipboardImage != null )
                {
                    _binaryImage = _clipboardImage;
                }

                var _openAiService = CreateOpenAiService( AppSettings.ProviderSetting,
                    AppSettings.ModelSetting, AppSettings.ApiKeySetting,
                    AppSettings.BaseDomainSetting, AppSettings.DeploymentIdSetting,
                    AppSettings.ApiVersionSetting );

                _userMessage = prompt.Trim( );
                var _messages = PrepareMessages( prompt, _binaryImage );
                _tempMessages = _messages;
                _generatedTitle = "";
                if( AppSettings.UseTitleGenerationSetting
                    && ConversationListBox.SelectedIndex == -1 )
                {
                    _ = Task.Run( async ( ) =>
                    {
                        var _prompt = AppSettings.TitleGenerationPromptSetting
                            .Replace( "{Language}", AppSettings.TitleLanguageSetting )
                            .Replace( "{Prompt}", _userMessage );

                        Debug.Print( "----- Title generation Prompt -----" );
                        Debug.Print( _prompt );
                        Debug.Print( "-----------------------------------" );
                        await GenerateTitleAsync( _prompt );
                    } );
                }

                if( 1 == 2 )
                {
                    var _completionResult = await _openAiService.ChatCompletion.CreateCompletion(
                        new ChatCompletionCreateRequest( )
                        {
                            Messages = _messages,
                            Temperature = AppSettings.TemperatureSetting,
                            MaxTokens = AppSettings.MaxTokensSetting
                        } );

                    HandleCompletionResult( _completionResult );
                }
                else
                {
                    var _request = new ChatCompletionCreateRequest
                    {
                        Messages = _messages,
                        Temperature = AppSettings.TemperatureSetting,
                        MaxTokens = AppSettings.MaxTokensSetting
                    };

                    var _completionResult = 
                        _openAiService.ChatCompletion.CreateCompletionAsStream( _request );

                    _cancellationTokenSource = new CancellationTokenSource( );
                    Task.Run( async ( ) =>
                    {
                        await HandleCompletionResultStream( _completionResult,
                            _cancellationTokenSource.Token );
                    } );
                }
            }
            catch( Exception ex )
            {
                Reset( );
                MessageBox.Show( ex.ToString( ) );
                throw new Exception( $"{ex.Message}" );
            }
            finally
            {
                Debug.Print( "===== End of process =====" );
            }
        }

        /// <summary>
        /// Prepares this instance.
        /// </summary>
        private void Prepare( )
        {
            _stopWatch.Start( );
            TimeLabel.Content = "";
            TokensLabel.Content = "";
            ProgressRing.IsActive = true;
            UserTextBox.Text = "";
            ConversationListBox.IsEnabled = false;
            NewChatButton.IsEnabled = false;
            ConversationHistoryButton.IsEnabled = false;
            ConversationHistoryClearButton.IsEnabled = false;
            _responseText = "";
            ExecButton.IsEnabled = false;
            TranslateButton.IsEnabled = false;
            ForTokenCalc.OldConversationsToken = "";
            ForTokenCalc.SystemPromptToken = "";
            ForTokenCalc.UserPromptToken = "";
            ForTokenCalc.ResponseToken = "";
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        private void Reset( )
        {
            _stopWatch.Stop( );
            TimeLabel.Content = $"{_stopWatch.ElapsedMilliseconds.ToString( "N0" )} ms";
            _stopWatch.Reset( );
            ExecButton.IsEnabled = true;
            TranslateButton.IsEnabled = true;
            ProgressRing.IsActive = false;
            ConversationListBox.IsEnabled = true;
            NewChatButton.IsEnabled = true;
            ConversationHistoryButton.IsEnabled = true;
            ConversationHistoryClearButton.IsEnabled = true;
            _isProcessing = false;
            _imageFilePath = null;
            _clipboardImage = null;
            ImageFilePathLabel.Content = "";
        }

        /// <summary>
        /// Retrieves the configuration.
        /// </summary>
        /// <returns></returns>
        private bool RetrieveConfiguration( )
        {
            var _configName = ConfigurationComboBox.Text;
            var _rows =
                AppSettings.ConfigDataTable.Select( "ConfigurationName = '" + _configName + "'" );

            if( _rows.Length > 0 )
            {
                AppSettings.ProviderSetting = _rows[ 0 ][ "Provider" ].ToString( );
                AppSettings.ModelSetting = _rows[ 0 ][ "Model" ].ToString( );
                AppSettings.ApiKeySetting = _rows[ 0 ][ "APIKey" ].ToString( );
                AppSettings.DeploymentIdSetting = _rows[ 0 ][ "DeploymentId" ].ToString( );
                AppSettings.BaseDomainSetting = _rows[ 0 ][ "BaseDomain" ].ToString( );
                AppSettings.ApiVersionSetting = _rows[ 0 ][ "ApiVersion" ].ToString( );
                if( string.IsNullOrEmpty( _rows[ 0 ][ "Temperature" ].ToString( ) ) == false )
                {
                    AppSettings.TemperatureSetting =
                        float.Parse( _rows[ 0 ][ "Temperature" ].ToString( ) );
                }
                else
                {
                    AppSettings.TemperatureSetting = 1;
                }

                if( string.IsNullOrEmpty( _rows[ 0 ][ "MaxTokens" ].ToString( ) ) == false )
                {
                    AppSettings.MaxTokensSetting =
                        int.Parse( _rows[ 0 ][ "MaxTokens" ].ToString( ) );
                }
                else
                {
                    AppSettings.MaxTokensSetting = 2048;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the open ai service.
        /// </summary>
        /// <param name="providerSetting">The provider setting.</param>
        /// <param name="model">The model.</param>
        /// <param name="targetApiKey">The target API key.</param>
        /// <param name="targetBaseDomain">The target base domain.</param>
        /// <param name="targetDeploymentId">The target deployment identifier.</param>
        /// <param name="targetApiVersion">The target API version.</param>
        /// <returns></returns>
        private OpenAIService CreateOpenAiService( string providerSetting, string model,
            string targetApiKey, string targetBaseDomain, string targetDeploymentId,
            string targetApiVersion )
        {
            var _targetType = new ProviderType( );
            var _tempTargetApiKey = "";
            string _tempTargetBaseDomain = null;
            string _tempTargetDeploymentId = null;
            string _tempTargetApiVersion = null;
            switch( providerSetting )
            {
                case "OpenAI":
                {
                    _targetType = ProviderType.OpenAi;
                    _tempTargetApiKey = targetApiKey;
                    break;
                }
                case "Azure":
                {
                    _targetType = ProviderType.Azure;
                    _tempTargetApiKey = targetApiKey;
                    _tempTargetBaseDomain = targetBaseDomain;
                    _tempTargetDeploymentId = targetDeploymentId;
                    _tempTargetApiVersion = string.IsNullOrEmpty( targetApiVersion )
                        ? null
                        : targetApiVersion;

                    break;
                }
            }

            var _openAiService = new OpenAIService( new OpenAiOptions( )
            {
                ProviderType = _targetType,
                ApiKey = _tempTargetApiKey,
                BaseDomain = _tempTargetBaseDomain,
                DeploymentId = _tempTargetDeploymentId,
                ApiVersion = _tempTargetApiVersion
            } );

            _openAiService.SetDefaultModelId( model );
            return _openAiService;
        }

        /// <summary>
        /// Prepares the messages.
        /// </summary>
        /// <param name="userMessage">The user message.</param>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        private List<ChatMessage> PrepareMessages( string userMessage, byte[ ] image )
        {
            if( AppSettings.IsSystemPromptColumnVisible )
            {
                _selectInstructionContent = SystemPromptContentsTextBox.Text;
            }
            else if( !String.IsNullOrEmpty( AppSettings.InstructionSetting ) )
            {
                var _instructionList = AppSettings.InstructionListSetting?.Cast<string>( )
                    .Where( ( s, i ) => i % 2 == 0 ).ToArray( );

                var _index = Array.IndexOf( _instructionList, AppSettings.InstructionSetting );
                _selectInstructionContent = AppSettings.InstructionListSetting[ _index, 1 ];
            }
            else
            {
                _selectInstructionContent = "";
            }

            Debug.Print( "----- Parameter -----" );
            Debug.Print( $"Temperature:{AppSettings.TemperatureSetting}" );
            Debug.Print( "----- Contents of this message sent -----" );
            Debug.Print( _selectInstructionContent );
            Debug.Print( userMessage );
            var _messages = new List<ChatMessage>( );
            if( AppSettings.UseConversationHistoryFlg )
            {
                var _selectedItems = ConversationListBox.SelectedItems;
                if( _selectedItems.Count > 0 )
                {
                    var _selectedConversationHistory =
                        _selectedItems.Cast<ConversationHistory>( ).ToList( );

                    foreach( var _conversationHistory in _selectedConversationHistory )
                    {
                        if( _conversationHistory.Messages.Count
                            > AppSettings.ConversationHistoryCountSetting )
                        {
                            var _tempList = _conversationHistory.Messages.ToList( );
                            _tempList = _tempList
                                .Skip( _tempList.Count
                                    - AppSettings.ConversationHistoryCountSetting ).ToList( );

                            _messages.AddRange( _tempList );
                            foreach( var _token in _tempList )
                            {
                                ForTokenCalc.OldConversationsToken += _token.Content;
                            }
                        }
                        else
                        {
                            foreach( var _token in _conversationHistory.Messages )
                            {
                                ForTokenCalc.OldConversationsToken += _token.Content;
                            }

                            _messages.AddRange( _conversationHistory.Messages );
                        }
                    }
                }
            }

            _messages.Add( ChatMessage.FromSystem( _selectInstructionContent ) );
            if( image == null )
            {
                _messages.Add( ChatMessage.FromUser( userMessage ) );
            }
            else
            {
                _messages.Add( ChatMessage.FromUser( new List<MessageContent>
                {
                    MessageContent.TextContent( userMessage ),
                    MessageContent.ImageBinaryContent( image, "png" )
                } ) );
            }

            ForTokenCalc.SystemPromptToken = _selectInstructionContent;
            ForTokenCalc.UserPromptToken = userMessage;
            return _messages;
        }

        /// <summary>
        /// Handles the completion result.
        /// </summary>
        /// <param name="completionResult">The completion result.</param>
        /// <exception cref="Exception">Unknown Error</exception>
        private void HandleCompletionResult( ChatCompletionCreateResponse completionResult )
        {
            if( completionResult.Successful )
            {
                _responseText = completionResult.Choices.First( ).Message.Content;
                CaluculateTokenUsage( );
                if( AppSettings.NoticeFlgSetting )
                {
                    new ToastContentBuilder( ).AddText( "️AI responded back." ).Show( );
                }
            }
            else
            {
                if( completionResult.Error == null )
                {
                    throw new Exception( "Unknown Error" );
                }

                if( AppSettings.NoticeFlgSetting )
                {
                    new ToastContentBuilder( ).AddText( "️An error has occurred." ).Show( );
                }

                _resultFlg = false;
                MessageBox.Show(
                    $"{completionResult.Error.Code}: {completionResult.Error.Message}" );
            }

            Reset( );
        }

        /// <summary>
        /// Handles the completion result stream.
        /// </summary>
        /// <param name="result">The completion result.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        /// <exception cref="Exception">Unknown Error</exception>
        private async Task HandleCompletionResultStream(
            IAsyncEnumerable<ChatCompletionCreateResponse> result,
            CancellationToken cancelToken )
        {
            MarkdownScrollViewer _markdownScrollViewer = null;
            await Dispatcher.InvokeAsync( ( ) =>
            {
                try
                {
                    var _foundButtons = new List<Button>( );
                    foreach( var _child in GetAllChildren( MessagesPanel ) )
                    {
                        if( _child is Button _button
                            && ( string )_button.Tag == "RegenerateButton" )
                        {
                            _button.Visibility = Visibility.Collapsed;
                        }
                    }

                    var _messageElement = CreateMessageElement( _userMessage, true, false );
                    MessagesPanel.Children.Add( _messageElement );
                    if( _binaryImage != null )
                    {
                        var _imageString = Convert.ToBase64String( _binaryImage );
                        var _messageElementImage =
                            CreateMessageElement( _userMessage, false, false, _imageString );

                        MessagesPanel.Children.Add( _messageElementImage );
                    }

                    FrameworkElement _assistantMessageElement = null;
                    _assistantMessageElement = CreateMessageElement( "", false, true );
                    MessagesPanel.Children.Add( _assistantMessageElement );
                    var _assistantMessageGrid = _assistantMessageElement as Grid;
                    if( _assistantMessageGrid != null )
                    {
                        for( var _index = 0; _index < _assistantMessageGrid.Children.Count; _index++ )
                        {
                            var _child = _assistantMessageGrid.Children[ _index ];
                            if( _child is MarkdownScrollViewer )
                            {
                                _markdownScrollViewer = _child as MarkdownScrollViewer;
                                _markdownScrollViewer.Document.LineHeight = 1.0;
                                break;
                            }
                        }
                    }
                }
                catch( Exception ex )
                {
                    throw new Exception( $"{ex.Message}" );
                }
            } );

            var _resultText = "";
            try
            {
                await foreach( var _completion in result.WithCancellation( cancelToken ) )
                {
                    if( _completion.Successful )
                    {
                        var _firstChoice = _completion.Choices.FirstOrDefault( );
                        if( _firstChoice == null )
                        {
                            continue;
                        }

                        _resultText = _firstChoice.Message.Content;
                        await Dispatcher.InvokeAsync( ( ) =>
                        {
                            _responseText += $"{_resultText}";
                            _markdownScrollViewer.Markdown += _resultText;
                            _markdownScrollViewer.Document.FontSize = Settings.Default.FontSize;
                            FlushWindowsMessageQueue( );
                        } );
                    }
                    else
                    {
                        if( _completion.Error == null )
                        {
                            throw new Exception( "Unknown Error" );
                        }

                        _resultText = $"{_completion.Error.Code}: {_completion.Error.Message}";
                        MessageBox.Show( $"{_completion.Error.Code}: {_completion.Error.Message}" );
                        _resultFlg = false;
                    }
                }
            }
            catch( OperationCanceledException ) { }

            Debug.Print( "----- Conversation History -----" );
            _tempMessages.Add( ChatMessage.FromAssistant( _responseText ) );
            foreach( var _item in _tempMessages )
            {
                Debug.Print( $"{_item.Role}: {_item.Content}" );
            }

            await Dispatcher.InvokeAsync( ( ) =>
            {
                ForTokenCalc.ResponseToken = _responseText;
                if( _resultFlg )
                {
                    CaluculateTokenUsage( );
                }

                if( AppSettings.NoticeFlgSetting && _resultFlg )
                {
                    new ToastContentBuilder( ).AddText( "️AI responded back." ).Show( );
                }

                if( _alertFlg && Settings.Default.LastAlertDate != _todayString )
                {
                    var _result = MessageBox.Show(
                        $"Daily token usage of {_dailyTotal} exceeds the threshold of {Settings.Default.dailyTokenThreshold}! Do not show alerts for today again?",
                        "Token Usage Alert", MessageBoxButton.YesNo, MessageBoxImage.Question );

                    if( _result == MessageBoxResult.Yes )
                    {
                        Settings.Default.LastAlertDate = _todayString;
                        Settings.Default.Save( );
                    }
                }

                Reset( );
            } );
        }

        /// <summary>
        /// Caluculates the token usage.
        /// </summary>
        private void CaluculateTokenUsage( )
        {
            var _conversationResultTokens =
                TokenizerGpt3.Encode( ForTokenCalc.OldConversationsToken );

            var _instructionTokens = TokenizerGpt3.Encode( ForTokenCalc.SystemPromptToken );
            var _userTokens = TokenizerGpt3.Encode( ForTokenCalc.UserPromptToken );
            var _responseTokens = TokenizerGpt3.Encode( ForTokenCalc.ResponseToken );
            var _inputTokens = _conversationResultTokens.Count( ) + _instructionTokens.Count( )
                + _userTokens.Count( );

            var _outputTokens = _responseTokens.Count( );
            var _totalTokens = _inputTokens + _outputTokens;
            var _tooltip = "";
            _tooltip +=
                $"Conversation History Tokens : {_conversationResultTokens.Count( ).ToString( "N0" )}\r\n";

            _tooltip +=
                $"System Prompt Tokens : {_instructionTokens.Count( ).ToString( "N0" )}\r\n";

            _tooltip += $"User Message Tokens : {_userTokens.Count( ).ToString( "N0" )}\r\n";
            _tooltip += $"AI Response Tokens : {_responseTokens.Count( ).ToString( "N0" )}\r\n";
            _tooltip += $"Total Tokens : {_totalTokens.ToString( "N0" )}";
            TokensLabel.Content = _totalTokens.ToString( "N0" );
            TokensLabel.ToolTip = _tooltip;
            if( ConversationListBox.SelectedIndex != -1 )
            {
                var _selectedConversation = ( ConversationHistory )ConversationListBox.SelectedItem;
                var _selectedId = _selectedConversation.ID;
                var _conversation =
                    AppSettings.ConversationManager.Histories.FirstOrDefault( c =>
                        c.ID == _selectedId );

                if( _conversation != null )
                {
                    if( _binaryImage == null )
                    {
                        _conversation.Messages.Add( ChatMessage.FromUser( _userMessage ) );
                    }
                    else
                    {
                        _conversation.Messages.Add( ChatMessage.FromUser( new List<MessageContent>
                        {
                            MessageContent.TextContent( _userMessage ),
                            MessageContent.ImageBinaryContent( _binaryImage, "png" )
                        } ) );
                    }

                    _conversation.Messages.Add( ChatMessage.FromAssistant( _responseText ) );
                }

                RefreshConversationList( );
            }

            if( ConversationListBox.SelectedIndex == -1 )
            {
                var _cleanedUserMessage = _userMessage.Replace( "\n", "" ).Replace( "\r", "" );
                var _title = "";
                if( AppSettings.UseTitleGenerationSetting )
                {
                    if( !string.IsNullOrEmpty( _generatedTitle ) )
                    {
                        _title = _generatedTitle;
                    }
                    else
                    {
                        _title = "generating...";
                        _titleGenerating = true;
                    }
                }
                else
                {
                    _title = _cleanedUserMessage.Length > 20
                        ? _cleanedUserMessage.Substring( 0, 20 ) + "..."
                        : _cleanedUserMessage;
                }

                ConversationHistory _newHistory;
                if( _binaryImage == null )
                {
                    _newHistory = new ConversationHistory( )
                    {
                        Title = _title,
                        Messages = new ObservableCollection<ChatMessage>( )
                        {
                            ChatMessage.FromUser( _userMessage ),
                            ChatMessage.FromAssistant( _responseText )
                        }
                    };
                }
                else
                {
                    _newHistory = new ConversationHistory( )
                    {
                        Title = _title,
                        Messages = new ObservableCollection<ChatMessage>( )
                        {
                            ChatMessage.FromUser( new List<MessageContent>
                            {
                                MessageContent.TextContent( _userMessage ),
                                MessageContent.ImageBinaryContent( _binaryImage, "png" )
                            } ),
                            ChatMessage.FromAssistant( _responseText )
                        }
                    };
                }

                AppSettings.ConversationManager.Histories.Add( _newHistory );
                if( _titleGenerating )
                {
                    _newId = _newHistory.ID;
                }

                RefreshConversationList( );
                ConversationListBox.SelectedIndex = 0;
            }

            var _model = AppSettings.ModelSetting != ""
                ? AppSettings.ModelSetting
                : AppSettings.DeploymentIdSetting;

            AddTokenUsage( _totalTokens, _inputTokens, _outputTokens, _model,
                AppSettings.ProviderSetting );
        }

        /// <summary>
        /// Adds the token usage.
        /// </summary>
        /// <param name="totalToken">The total token.</param>
        /// <param name="inputTokens">The input tokens.</param>
        /// <param name="outputTokens">The output tokens.</param>
        /// <param name="model">The model.</param>
        /// <param name="provider">The provider.</param>
        private void AddTokenUsage( int totalToken, int inputTokens, int outputTokens,
            string model, string provider )
        {
            var _rowCount = AppSettings.TokenUsageSetting.GetLength( 0 );
            var _colCount = AppSettings.TokenUsageSetting.GetLength( 1 );
            if( AppSettings.TokenUsageSetting == null
                || _rowCount == 0
                || _colCount == 0 )
            {
                var _temp = new string[ 0, 5 ];
                AppSettings.TokenUsageSetting = _temp;
            }

            var _oldTokenUsage = AppSettings.TokenUsageSetting;
            var _rows = _oldTokenUsage.GetLength( 0 );
            var _cols = _oldTokenUsage.GetLength( 1 );
            var _newTokenUsage = new string[ _rows, _cols + 2 ];
            for( var _i = 0; _i < _rows; _i++ )
            {
                for( var _j = 0; _j < _cols; _j++ )
                {
                    _newTokenUsage[ _i, _j ] = _oldTokenUsage[ _i, _j ];
                }

                _newTokenUsage[ _i, _cols ] = "0";
                _newTokenUsage[ _i, _cols + 1 ] = "0";
            }

            _todayString = DateTime.Today.ToString( "yyyy/MM/dd" );
            var _tokenUsage = AppSettings.TokenUsageSetting;
            var _tokenUsageCount = _tokenUsage.GetLength( 0 );
            _dailyTotal = 0;
            var _todayTokenUsageExist = false;
            for( var _i = 0; _i < _tokenUsageCount; _i++ )
            {
                if( _tokenUsage[ _i, 0 ] == _todayString )
                {
                    _dailyTotal += int.Parse( _tokenUsage[ _i, 3 ] );
                    _dailyTotal += totalToken;
                    if( _tokenUsage[ _i, 1 ] == provider
                        && _tokenUsage[ _i, 2 ] == model )
                    {
                        {
                            _tokenUsage[ _i, 3 ] =
                                ( int.Parse( _tokenUsage[ _i, 3 ] ) + totalToken ).ToString( );

                            _tokenUsage[ _i, 4 ] =
                                ( int.Parse( _tokenUsage[ _i, 4 ] ) + inputTokens ).ToString( );

                            _tokenUsage[ _i, 5 ] =
                                ( int.Parse( _tokenUsage[ _i, 5 ] ) + outputTokens ).ToString( );

                            _todayTokenUsageExist = true;
                        }
                    }
                }
            }

            if( !_todayTokenUsageExist )
            {
                _tokenUsage = ResizeArray( _tokenUsage, _tokenUsageCount + 1, 6 );
                _tokenUsage[ _tokenUsageCount, 0 ] = _todayString;
                _tokenUsage[ _tokenUsageCount, 1 ] = provider;
                _tokenUsage[ _tokenUsageCount, 2 ] = model;
                _tokenUsage[ _tokenUsageCount, 3 ] = totalToken.ToString( );
                _tokenUsage[ _tokenUsageCount, 4 ] = inputTokens.ToString( );
                _tokenUsage[ _tokenUsageCount, 5 ] = outputTokens.ToString( );
                _dailyTotal += totalToken;
            }

            AppSettings.TokenUsageSetting = _tokenUsage;
            Settings.Default.TokenUsage = SerializeArray( AppSettings.TokenUsageSetting );
            Settings.Default.Save( );
            _alertFlg = false;
            if( _dailyTotal > Settings.Default.dailyTokenThreshold )
            {
                _alertFlg = true;
            }
        }

        /// <summary>
        /// Resizes the array.
        /// </summary>
        /// <param name="originalArray">The original array.</param>
        /// <param name="newRowCount">The new row count.</param>
        /// <param name="newColCount">The new col count.</param>
        /// <returns></returns>
        public static string[ , ] ResizeArray( string[ , ] originalArray, int newRowCount,
            int newColCount )
        {
            var _originalRowCount = originalArray.GetLength( 0 );
            var _originalColCount = originalArray.GetLength( 1 );
            var _newArray = new string[ newRowCount, newColCount ];
            for( var _i = 0; _i < Math.Min( _originalRowCount, newRowCount ); _i++ )
            {
                for( var _j = 0; _j < Math.Min( _originalColCount, newColCount ); _j++ )
                {
                    _newArray[ _i, _j ] = originalArray[ _i, _j ];
                }
            }

            return _newArray;
        }

        /// <summary>
        /// Generates the title asynchronous.
        /// </summary>
        /// <param name="userMessage">The user message.</param>
        public async Task GenerateTitleAsync( string userMessage )
        {
            try
            {
                Debug.WriteLine(
                    $"GenerateTitleAsync started on thread ID: {Thread.CurrentThread.ManagedThreadId}" );

                var _configName = AppSettings.ModelForTitleGenerationSetting;
                var _rows =
                    AppSettings.ConfigDataTable.Select(
                        "ConfigurationName = '" + _configName + "'" );

                var _providerSetting = _rows[ 0 ][ "Provider" ].ToString( );
                var _modelSetting = _rows[ 0 ][ "Model" ].ToString( );
                var _apiKeySetting = _rows[ 0 ][ "APIKey" ].ToString( );
                var _deploymentIdSetting = _rows[ 0 ][ "DeploymentId" ].ToString( );
                var _baseDomainSetting = _rows[ 0 ][ "BaseDomain" ].ToString( );
                var _apiVersionSetting = _rows[ 0 ][ "ApiVersion" ].ToString( );
                float _temperatureSetting;
                int _maxTokensSetting;
                if( string.IsNullOrEmpty( _rows[ 0 ][ "Temperature" ].ToString( ) ) == false )
                {
                    _temperatureSetting = float.Parse( _rows[ 0 ][ "Temperature" ].ToString( ) );
                }
                else
                {
                    _temperatureSetting = 1;
                }

                if( string.IsNullOrEmpty( _rows[ 0 ][ "MaxTokens" ].ToString( ) ) == false )
                {
                    _maxTokensSetting = int.Parse( _rows[ 0 ][ "MaxTokens" ].ToString( ) );
                }
                else
                {
                    _maxTokensSetting = 2048;
                }

                var _openAiService = CreateOpenAiService( _providerSetting, _modelSetting,
                    _apiKeySetting, _baseDomainSetting, _deploymentIdSetting, _apiVersionSetting );

                var _messages = new List<ChatMessage>
                {
                    ChatMessage.FromUser( userMessage )
                };

                var _completionResult = await _openAiService.ChatCompletion.CreateCompletion(
                    new ChatCompletionCreateRequest( )
                    {
                        Messages = _messages,
                        Temperature = _temperatureSetting,
                        MaxTokens = _maxTokensSetting
                    } );

                HandleCompletionResultForTitle( _completionResult );
                var _model = _modelSetting != ""
                    ? _modelSetting
                    : _deploymentIdSetting;

                var _userMessageTokens = TokenizerGpt3.Encode( userMessage );
                var _responseTokens = TokenizerGpt3.Encode( _generatedTitle );
                var _totalTokens = _userMessageTokens.Count( ) + _responseTokens.Count( );
                AddTokenUsage( _totalTokens, _userMessageTokens.Count( ), _responseTokens.Count( ),
                    _model, _providerSetting );

                if( _titleGenerating )
                {
                    var _historyToUpdate =
                        AppSettings.ConversationManager.Histories.FirstOrDefault( history =>
                            history.ID == _newId );

                    if( _historyToUpdate != null )
                    {
                        _historyToUpdate.Title = _generatedTitle;
                    }

                    await Dispatcher.InvokeAsync( ( ) =>
                    {
                        RefreshConversationList( );
                    } );
                }
            }
            catch( Exception ex )
            {
                await Dispatcher.InvokeAsync( ( ) =>
                {
                    MessageBox.Show( ex.Message );
                } );
            }
        }

        /// <summary>
        /// Handles the completion result for title.
        /// </summary>
        /// <param name="completionResult">The completion result.</param>
        /// <exception cref="Exception">
        /// Unknown Error
        /// or
        /// Title generation Error: {completionResult.Error.Message}
        /// </exception>
        private void HandleCompletionResultForTitle( ChatCompletionCreateResponse completionResult )
        {
            if( completionResult.Successful )
            {
                _generatedTitle = completionResult.Choices.First( ).Message.Content;
                Debug.Print( "===== Generated Title =====" );
                Debug.Print( _generatedTitle );
                Debug.Print( "===========================" );
            }
            else
            {
                _generatedTitle = "Error!";
                if( completionResult.Error == null )
                {
                    throw new Exception( "Unknown Error" );
                }
                else if( completionResult.Error.Message != null )
                {
                    throw new Exception(
                        $"Title generation Error: {completionResult.Error.Message}" );
                }
            }
        }
    }
}