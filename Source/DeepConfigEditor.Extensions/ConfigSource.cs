namespace DeepConfigEditor.Extensions
{
    using System;
    using DeepConfig;
    using DeepConfig.Providers;

    /// <summary>
    /// Represent a configuration source.
    /// </summary>
    [ConfigSection]
    [ConfigSectionUI(IsSubsection = true)]
    public class ConfigSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSource"/> class.
        /// </summary>
        /// <param name="provider">The provider this source encapsulates.</param>
        /// <param name="alias">The alias of the source, i.e. short friendly name the user can potentially supply.</param>
        /// <param name="iconUri">The icon URI associated with the source. this will most likely be the same as its creating <see cref="IProvideConfigSource"/>.</param>
        /// <exception cref="System.ArgumentNullException">provider;A provider cannot be null.</exception>
        public ConfigSource(IConfigProvider provider, string alias = null, Uri iconUri = null)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider", "A provider cannot be null.");
            }

            this.Provider = provider;
            this.Alias = alias;
            this.IconUri = iconUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSource"/> class.
        /// </summary>
        /// <param name="provider">The provider this source encapsulates.</param>
        /// <param name="iconUri">The icon URI associated with the source. this will most likely be the same as its creating <see cref="IProvideConfigSource"/>.</param>
        public ConfigSource(IConfigProvider provider, Uri iconUri)
            : this(provider, null, iconUri)
        {
        }

        //Need a default ctor for the config framework
        private ConfigSource()
        {
        }

        /// <summary>
        /// Gets or sets the alias of the source, i.e. short friendly name the user can potentially supply.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        [ConfigSetting]
        public string Alias
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the icon URI associated with the source. this will most likely be the same as its creating <see cref="IProvideConfigSource"/>.
        /// </summary>
        /// <value>
        /// The icon URI.
        /// </value>
        [ConfigSetting]
        public Uri IconUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the provider this source encapsulates.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        [ConfigSetting]
        public IConfigProvider Provider
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the display name. This will either be the <see cref="Alias"/> if set, or the <see cref="Provider"/>'s ToString()
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.Alias))
                {
                    return this.Provider.ToString();
                }

                return this.Alias;
            }
        }
    }
}
