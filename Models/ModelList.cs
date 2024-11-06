

namespace Bocifus
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ SuppressMessage( "ReSharper", "ClassCanBeSealed.Global" ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" ) ]
    public class ModelList : PropertyChangeBase
    {
        /// <summary>
        /// The configuration name
        /// </summary>
        private protected string _configurationName;

        /// <summary>
        /// The provider
        /// </summary>
        private protected string _provider;

        /// <summary>
        /// The model
        /// </summary>
        private protected string _model;

        /// <summary>
        /// The API key
        /// </summary>
        private protected string _apiKey;

        /// <summary>
        /// The deployment identifier
        /// </summary>
        private protected string _deploymentId;

        /// <summary>
        /// The base domain
        /// </summary>
        private protected string _baseDomain;

        /// <summary>
        /// The API version
        /// </summary>
        private protected string _apiVersion;

        /// <summary>
        /// The temperature
        /// </summary>
        private protected string _temperature;

        /// <summary>
        /// The maximum tokens
        /// </summary>
        private protected string _maxTokens;

        /// <summary>
        /// The vision
        /// </summary>
        private protected bool _vision;

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
        public string ConfigurationName
        {
            get
            {
                return _configurationName;
            }
            set
            {
                if( _configurationName != value )
                {
                    _configurationName = value;
                    OnPropertyChanged( nameof( ConfigurationName ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value
        /// 
        public string Provider
        {
            get
            {
                return _provider;
            }
            set
            {
                if( _provider != value )
                {
                    _provider = value;
                    OnPropertyChanged( nameof( Provider ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public string Model
        {
            get
            {
                return _model;
            }
            set
            {
                if( _model != value )
                {
                    _model = value;
                    OnPropertyChanged( nameof( Model ) ); 
                }
            }
        }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public string ApiKey
        {
            get
            {
                return _apiKey;
            }
            set
            {
                if( _apiKey != value )
                {
                    _apiKey = value;
                    OnPropertyChanged( nameof( ApiKey ) ); 
                }
            }
        }

        /// <summary>
        /// Gets or sets the deployment identifier.
        /// </summary>
        /// <value>
        /// The deployment identifier.
        /// </value>
        public string DeploymentId
        {
            get
            {
                return _deploymentId;
            }
            set
            {
                if( _deploymentId != value )
                {
                    _deploymentId = value;
                    OnPropertyChanged( nameof( DeploymentId ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the base domain.
        /// </summary>
        /// <value>
        /// The base domain.
        /// </value>
        public string BaseDomain
        {
            get
            {
                return _baseDomain;
            }
            set
            {
                if( _baseDomain != value )
                {
                    _baseDomain = value;
                    OnPropertyChanged( nameof( BaseDomain ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the API version.
        /// </summary>
        /// <value>
        /// The API version.
        /// </value>
        public string ApiVersion
        {
            get
            {
                return _apiVersion;
            }
            set
            {
                if( _apiVersion != value )
                {
                    _apiVersion = value;
                    OnPropertyChanged( nameof( ApiVersion ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the temperature.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public string Temperature
        {
            get
            {
                return _temperature;
            }
            set
            {
                if( _temperature != value )
                {
                    _temperature = value;
                    OnPropertyChanged( nameof( Temperature ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum tokens.
        /// </summary>
        /// <value>
        /// The maximum tokens.
        /// </value>
        public string MaxTokens
        {
            get
            {
                return _maxTokens;
            }
            set
            {
                if( _maxTokens != value )
                {
                    _maxTokens = value;
                    OnPropertyChanged( nameof( MaxTokens ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// <see cref="ModelList"/> is vision.
        /// </summary>
        /// <value>
        ///   <c>true</c> if vision; otherwise,
        /// <c>false</c>.
        /// </value>
        public bool Vision
        {
            get
            {
                return _vision;
            }
            set
            {
                if( _vision != value )
                {
                    _vision = value;
                    OnPropertyChanged( nameof( Vision ) );
                }
            }
        }
    }
}
