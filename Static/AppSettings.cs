// ******************************************************************************************
//     Assembly:                Bocifus
//     Author:                  Terry D. Eppler
//     Created:                 11-04-2024
// 
//     Last Modified By:        Terry D. Eppler
//     Last Modified On:        11-04-2024
// ******************************************************************************************
// <copyright file="AppSettings.cs" company="Terry D. Eppler">
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
//   AppSettings.cs
// </summary>
// ******************************************************************************************

namespace Bocifus
{
    using Model;
    using System.Data;
    using Properties;

    public static class AppSettings
    {
        /// <summary>
        /// Gets or sets the configuration data table.
        /// </summary>
        /// <value>
        /// The configuration data table.
        /// </value>
        public static DataTable ConfigDataTable { get; set; } =
            UtilityFunctions.DeserializeDataTable( Settings.Default.ConfigDataTable );

        /// <summary>
        /// Gets or sets the select configuration setting.
        /// </summary>
        /// <value>
        /// The select configuration setting.
        /// </value>
        public static string SelectConfigSetting { get; set; } = Settings.Default.SelectConfig;

        /// <summary>
        /// Gets or sets the instruction setting.
        /// </summary>
        /// <value>
        /// The instruction setting.
        /// </value>
        public static string InstructionSetting { get; set; } = Settings.Default.Instruction;

        /// <summary>
        /// Gets or sets the instruction list setting.
        /// </summary>
        /// <value>
        /// The instruction list setting.
        /// </value>
        public static string[ , ] InstructionListSetting { get; set; } =
            UtilityFunctions.DeserializeArray( Settings.Default.InstructionList );

        /// <summary>
        /// Gets or sets the token usage setting.
        /// </summary>
        /// <value>
        /// The token usage setting.
        /// </value>
        public static string[ , ] TokenUsageSetting { get; set; } =
            UtilityFunctions.DeserializeArray( Settings.Default.TokenUsage );

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system prompt column visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system prompt column visible; otherwise, <c>false</c>.
        /// </value>
        public static bool IsSystemPromptColumnVisible { get; set; } =
            Settings.Default.IsSystemPromptColumnVisible;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is conversation column visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is conversation column visible; otherwise, <c>false</c>.
        /// </value>
        public static bool IsConversationColumnVisible { get; set; } =
            Settings.Default.IsConversationColumnVisible;

        /// <summary>
        /// Gets or sets the conversation history count setting.
        /// </summary>
        /// <value>
        /// The conversation history count setting.
        /// </value>
        public static int ConversationHistoryCountSetting { get; set; } =
            Settings.Default.ConversationHistoryCount;

        /// <summary>
        /// The use conversation history FLG
        /// </summary>
        public static bool UseConversationHistoryFlg = Settings.Default.UseConversationHistory;

        /// <summary>
        /// Gets or sets the conversation manager.
        /// </summary>
        /// <value>
        /// The conversation manager.
        /// </value>
        public static ConversationManager ConversationManager { get; set; }

        /// <summary>
        /// Gets or sets the prompt template manager.
        /// </summary>
        /// <value>
        /// The prompt template manager.
        /// </value>
        public static PromptTemplateManager PromptTemplateManager { get; set; }

        /// <summary>
        /// The prompt template grid row heigh setting
        /// </summary>
        public static double PromptTemplateGridRowHeighSetting =
            Settings.Default.PromptTemplateGridRowHeigh;

        /// <summary>
        /// The chat list grid row height setting
        /// </summary>
        public static double ChatListGridRowHeightSetting = Settings.Default.ChatListGridRowHeight;

        /// <summary>
        /// The prompt template grid row height save setting
        /// </summary>
        public static double PromptTemplateGridRowHeightSaveSetting =
            Settings.Default.PromptTemplateGridRowHeightSave;

        /// <summary>
        /// The model for title generation setting
        /// </summary>
        public static string ModelForTitleGenerationSetting =
            Settings.Default.ModelForTitleGeneration;

        /// <summary>
        /// The title generation prompt setting
        /// </summary>
        public static string TitleGenerationPromptSetting = Settings.Default.TitleGenerationPrompt;

        /// <summary>
        /// The title language setting
        /// </summary>
        public static string TitleLanguageSetting = Settings.Default.TitleLanguage;

        /// <summary>
        /// The use title generation setting
        /// </summary>
        public static bool UseTitleGenerationSetting = Settings.Default.UseTitleGeneration;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is prompt template list visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is prompt template list visible; otherwise, <c>false</c>.
        /// </value>
        public static bool IsPromptTemplateListVisible { get; set; } =
            Settings.Default.IsPromptTemplateListVisible;

        /// <summary>
        /// Gets or sets a value indicating whether [notice FLG setting].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [notice FLG setting]; otherwise, <c>false</c>.
        /// </value>
        public static bool NoticeFlgSetting { get; set; } = Settings.Default.NoticeFlg;

        /// <summary>
        /// Gets or sets the translation API provider.
        /// </summary>
        /// <value>
        /// The translation API provider.
        /// </value>
        public static string TranslationApiProvider { get; set; } =
            Settings.Default.TranslationAPIProvider;

        /// <summary>
        /// Gets or sets a value indicating whether [translation API use FLG].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [translation API use FLG]; otherwise, <c>false</c>.
        /// </value>
        public static bool TranslationApiUseFlg { get; set; } =
            Settings.Default.TranslationAPIUseFlg;

        /// <summary>
        /// Gets or sets from translation language.
        /// </summary>
        /// <value>
        /// From translation language.
        /// </value>
        public static string FromTranslationLanguage { get; set; } =
            Settings.Default.FromTranslationLanguage;

        /// <summary>
        /// Converts to translationlanguage.
        /// </summary>
        /// <value>
        /// To translation language.
        /// </value>
        public static string ToTranslationLanguage { get; set; } =
            Settings.Default.ToTranslationLanguage;

        /// <summary>
        /// Gets or sets the translation API URL deep l.
        /// </summary>
        /// <value>
        /// The translation API URL deep l.
        /// </value>
        public static string TranslationApiUrlDeepL { get; set; } =
            Settings.Default.TranslationAPIUrlDeepL;

        /// <summary>
        /// Gets or sets the translation API key deep l.
        /// </summary>
        /// <value>
        /// The translation API key deep l.
        /// </value>
        public static string TranslationApiKeyDeepL { get; set; } =
            Settings.Default.TranslationAPIKeyDeepL;

        /// <summary>
        /// Gets or sets the translation API URL google.
        /// </summary>
        /// <value>
        /// The translation API URL google.
        /// </value>
        public static string TranslationApiUrlGoogle { get; set; } =
            Settings.Default.TranslationAPIUrlGoogle;

        /// <summary>
        /// Gets or sets the translation API key google.
        /// </summary>
        /// <value>
        /// The translation API key google.
        /// </value>
        public static string TranslationApiKeyGoogle { get; set; } =
            Settings.Default.TranslationAPIKeyGoogle;

        /// <summary>
        /// Gets or sets the API key setting.
        /// </summary>
        /// <value>
        /// The API key setting.
        /// </value>
        public static string ApiKeySetting { get; set; }

        /// <summary>
        /// Gets or sets the model setting.
        /// </summary>
        /// <value>
        /// The model setting.
        /// </value>
        public static string ModelSetting { get; set; }

        /// <summary>
        /// Gets or sets the provider setting.
        /// </summary>
        /// <value>
        /// The provider setting.
        /// </value>
        public static string ProviderSetting { get; set; }

        /// <summary>
        /// Gets or sets the deployment identifier setting.
        /// </summary>
        /// <value>
        /// The deployment identifier setting.
        /// </value>
        public static string DeploymentIdSetting { get; set; }

        /// <summary>
        /// Gets or sets the base domain setting.
        /// </summary>
        /// <value>
        /// The base domain setting.
        /// </value>
        public static string BaseDomainSetting { get; set; }

        /// <summary>
        /// Gets or sets the API version setting.
        /// </summary>
        /// <value>
        /// The API version setting.
        /// </value>
        public static string ApiVersionSetting { get; set; }

        /// <summary>
        /// Gets or sets the maximum tokens setting.
        /// </summary>
        /// <value>
        /// The maximum tokens setting.
        /// </value>
        public static int MaxTokensSetting { get; set; }

        /// <summary>
        /// Gets or sets the temperature setting.
        /// </summary>
        /// <value>
        /// The temperature setting.
        /// </value>
        public static float TemperatureSetting { get; set; }
    }
}