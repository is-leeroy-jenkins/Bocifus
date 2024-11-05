
namespace Bocifus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public static class ForTokenCalc
    {
        /// <summary>
        /// Gets or sets the old conversations token.
        /// </summary>
        /// <value>
        /// The old conversations token.
        /// </value>
        public static string OldConversationsToken { get; set; } = "";

        /// <summary>
        /// Gets or sets the system prompt token.
        /// </summary>
        /// <value>
        /// The system prompt token.
        /// </value>
        public static string SystemPromptToken { get; set; } = "";

        /// <summary>
        /// Gets or sets the user prompt token.
        /// </summary>
        /// <value>
        /// The user prompt token.
        /// </value>
        public static string UserPromptToken { get; set; } = "";

        /// <summary>
        /// Gets or sets the response token.
        /// </summary>
        /// <value>
        /// The response token.
        /// </value>
        public static string ResponseToken { get; set; } = "";
    }
}
