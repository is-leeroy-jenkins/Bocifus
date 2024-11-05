

namespace Bocifus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics.CodeAnalysis;

    [ SuppressMessage( "ReSharper", "UnusedType.Global" ) ]
    public class TokenUsageDisplayItem
    {
        public TokenUsageDisplayItem( )
        {
        }

        public string Date { get; set; }

        public string Provider { get; set; }

        public string GptVersion { get; set; }

        public string TotalTokenUsage { get; set; }

        public string InputTokenUsage { get; set; }

        public string OutputTokenUsage { get; set; }
    }
}
