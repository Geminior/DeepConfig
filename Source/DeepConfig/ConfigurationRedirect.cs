namespace DeepConfig
{
    using DeepConfig.Providers;

    /// <summary>
    /// When inserterted into a config file, it will redirect the <see cref="ConfigManager"/> to another provider to use instead.
    /// </summary>
    [ConfigSection(IsSingleton = true)]
    public sealed class ConfigurationRedirect
    {
        /// <summary>
        /// Gets or sets the actual provider to redirect to.
        /// </summary>
        /// <value>The actual provider.</value>
        [ConfigSetting(Description = "The actual config provider to use to provide configuration for the application.")]
        public IConfigProvider ActualProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Tries to get a section of this type from the config.
        /// </summary>
        /// <param name="mgr">The config manager.</param>
        /// <param name="provider">The provider to redirect to, if found.</param>
        /// <returns><see langword="true"/> if found, otherwise <see langword="false"/></returns>
        internal static bool TryGetRedirect(IConfigManager mgr, out IConfigProvider provider)
        {
            var cfgRedir = mgr.GetSettings<ConfigurationRedirect>();

            provider = null;

            if (cfgRedir != null)
            {
                provider = cfgRedir.ActualProvider;
            }

            return (provider != null);
        }
    }
}
