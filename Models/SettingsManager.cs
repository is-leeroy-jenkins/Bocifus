// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-05-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-05-2024
// ******************************************************************************************
// <copyright file="SettingsManager.cs" company="Terry D. Eppler">
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
//   SettingsManager.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using Model;
    using Model;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;
    using Properties;
    using static DataManager;
    using static UtilityFunctions;
    using MessageBox = ModernWpf.MessageBox;

    /// <summary>
    /// 
    /// </summary>
    [ SuppressMessage( "ReSharper", "ClassNeverInstantiated.Global" ) ]
    public class SettingsManager
    {
        /// <summary>
        /// Saves the settings.
        /// </summary>
        public static void SaveSettings( )
        {
            SaveGeneralSettings( );
            SaveUISettings( );
            SaveTranslationSettings( );
            TitleGenerationSettings( );
            SaveConversationSettings( );
            SaveTemplateSettings( );
            Settings.Default.Save( );
        }

        /// <summary>
        /// Saves the general settings.
        /// </summary>
        private static void SaveGeneralSettings( )
        {
            Settings.Default.ConfigDataTable = SerializeDataTable( AppSettings.ConfigDataTable );
            Settings.Default.SelectConfig = AppSettings.SelectConfigSetting;
            Settings.Default.UseConversationHistory = AppSettings.UseConversationHistoryFlg;
            Settings.Default.ConversationHistoryCount = AppSettings.ConversationHistoryCountSetting;
            Settings.Default.NoticeFlg = AppSettings.NoticeFlgSetting;
            Settings.Default.Instruction = AppSettings.InstructionSetting;
            Settings.Default.InstructionList = SerializeArray( AppSettings.InstructionListSetting );
        }

        /// <summary>
        /// Saves the UI settings.
        /// </summary>
        private static void SaveUISettings( )
        {
            Settings.Default.IsSystemPromptColumnVisible = AppSettings.IsSystemPromptColumnVisible;
            Settings.Default.IsConversationColumnVisible = AppSettings.IsConversationColumnVisible;
            Settings.Default.IsPromptTemplateListVisible = AppSettings.IsPromptTemplateListVisible;
            Settings.Default.PromptTemplateGridRowHeigh =
                AppSettings.PromptTemplateGridRowHeighSetting;

            Settings.Default.ChatListGridRowHeight = AppSettings.ChatListGridRowHeightSetting;
            Settings.Default.PromptTemplateGridRowHeightSave =
                AppSettings.PromptTemplateGridRowHeightSaveSetting;
        }

        /// <summary>
        /// Saves the translation settings.
        /// </summary>
        private static void SaveTranslationSettings( )
        {
            Settings.Default.TranslationAPIProvider = AppSettings.TranslationApiProvider;
            Settings.Default.TranslationAPIUseFlg = AppSettings.TranslationApiUseFlg;
            Settings.Default.FromTranslationLanguage = AppSettings.FromTranslationLanguage;
            Settings.Default.ToTranslationLanguage = AppSettings.ToTranslationLanguage;
            Settings.Default.TranslationAPIUrlDeepL = AppSettings.TranslationApiUrlDeepL;
            Settings.Default.TranslationAPIKeyDeepL = AppSettings.TranslationApiKeyDeepL;
            Settings.Default.TranslationAPIUrlGoogle = AppSettings.TranslationApiUrlGoogle;
            Settings.Default.TranslationAPIKeyGoogle = AppSettings.TranslationApiKeyGoogle;
        }

        /// <summary>
        /// Titles the generation settings.
        /// </summary>
        private static void TitleGenerationSettings( )
        {
            Settings.Default.ModelForTitleGeneration = AppSettings.ModelForTitleGenerationSetting;
            Settings.Default.TitleGenerationPrompt = AppSettings.TitleGenerationPromptSetting;
            Settings.Default.TitleLanguage = AppSettings.TitleLanguageSetting;
            Settings.Default.UseTitleGeneration = AppSettings.UseTitleGenerationSetting;
        }

        /// <summary>
        /// Saves the conversation settings.
        /// </summary>
        private static void SaveConversationSettings( )
        {
            SaveConversationsAsJson( AppSettings.ConversationManager );
        }

        /// <summary>
        /// Saves the template settings.
        /// </summary>
        private static void SaveTemplateSettings( )
        {
            SavePromptTemplateAsJson( AppSettings.PromptTemplateManager );
        }

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        public static void InitializeSettings( )
        {
            if( Settings.Default.UpgradeRequired )
            {
                Settings.Default.Upgrade( );
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save( );
            }

            try
            {
                AppSettings.ConversationManager = LoadConversationsFromJson( );
                AppSettings.PromptTemplateManager = LoadPromptTemplateFromJson( );
            }
            catch( Exception ex )
            {
                var documentsPath =
                    Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );

                var message = new StringBuilder( )
                    .AppendLine( "Failed to load conversation history." ).AppendLine( ex.Message )
                    .AppendLine( ).AppendLine( "Do you want to reset the conversation history?" )
                    .AppendLine( "If you choose No, the application will exit at this point." )
                    .AppendLine(
                        "Please re-launch the application after the problem with the folder where the conversation history is saved has been resolved." )
                    .AppendLine( ).AppendLine( $"{documentsPath}\\Bocifus\\ConversationHistory" )
                    .ToString( );

                var result = MessageBox.Show( message, "Error", MessageBoxButton.YesNo,
                    MessageBoxImage.Error );

                if( result == MessageBoxResult.Yes )
                {
                    AppSettings.ConversationManager = new ConversationManager( );
                }
                else
                {
                    Environment.Exit( 1 );
                }
            }
        }
    }
}