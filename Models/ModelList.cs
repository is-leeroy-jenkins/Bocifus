

namespace Bocifus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ModelList
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ModelList"/> class.
        /// </summary>
        public ModelList( )
        {
        }

        /// <summary>
        /// Gets or sets the name of the configuration.
        /// </summary>
        /// <value>
        /// The name of the configuration.
        /// </value>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the deployment identifier.
        /// </summary>
        /// <value>
        /// The deployment identifier.
        /// </value>
        public string DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets the base domain.
        /// </summary>
        /// <value>
        /// The base domain.
        /// </value>
        public string BaseDomain { get; set; }

        /// <summary>
        /// Gets or sets the API version.
        /// </summary>
        /// <value>
        /// The API version.
        /// </value>
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the temperature.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public string Temperature { get; set; }

        /// <summary>
        /// Gets or sets the maximum tokens.
        /// </summary>
        /// <value>
        /// The maximum tokens.
        /// </value>
        public string MaxTokens { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="ModelList"/> is vision.
        /// </summary>
        /// <value>
        ///   <c>true</c> if vision; otherwise, <c>false</c>.
        /// </value>
        public bool Vision { get; set; }
    }
}
