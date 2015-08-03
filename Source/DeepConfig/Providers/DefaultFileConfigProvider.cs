namespace DeepConfig.Providers
{
    using System;

    /// <summary>
    /// Implementation of <see cref="IConfigProvider"/> that gives access to the settings stored in the default config file for the application.
    /// </summary>
    /// <remarks>
    /// The file used is either web.config for web application or applicationname.exe.config for other applications.
    /// </remarks>
    public sealed class DefaultFileConfigProvider : FileConfigProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFileConfigProvider"/> class.
        /// </summary>
        public DefaultFileConfigProvider()
            : base(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)
        {
        }
    }
}
