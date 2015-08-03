namespace DeepConfig.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using DeepConfig.Utilities;

    /// <summary>
    /// This is a read-only provider that allows loading the configuration from several providers at once, merging them into one set of configuration settings.
    /// </summary>
    /// <remarks>
    /// Loading two or more configurations that contain the same sections will result in only the section of the configuration file being loaded. I.e. the first configuration loaded will take precedence over later configurations if they contain the same section(s).
    /// </remarks>
    [ConfigSection]
    [ConfigSectionUI(IsSubsection = true)]
    public sealed class CompositeConfigProvider : IConfigProvider
    {
        /// <summary>
        /// The list of file paths, representing the files to load
        /// </summary>
        private IList<IConfigProvider> _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeConfigProvider"/> class.
        /// </summary>
        public CompositeConfigProvider()
        {
            _providers = new List<IConfigProvider>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeConfigProvider"/> class.
        /// </summary>
        /// <param name="providers">The config providers to load in order of precedence.</param>
        public CompositeConfigProvider(params IConfigProvider[] providers)
            : this((IEnumerable<IConfigProvider>)providers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeConfigProvider"/> class.
        /// </summary>
        /// <param name="providers">The list of config providers to load in order of precedence.</param>
        public CompositeConfigProvider(IEnumerable<IConfigProvider> providers)
        {
            Ensure.ArgumentNotNull(providers, "providers");

            _providers = new List<IConfigProvider>(providers);
        }

        /// <summary>
        /// Gets a value indicating whether one or more of the source configs have changed since they were loaded.
        /// </summary>
        public bool ModifiedSinceLoad
        {
            get
            {
                foreach (var p in _providers)
                {
                    if (p.ModifiedSinceLoad)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration is read-only.
        /// </summary>
        /// <value></value>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration can be deleted.
        /// </summary>
        /// <value></value>
        public bool CanDelete
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the source identifier, which is a string that precisely identifies the source of the configuration returned by the provider.
        /// <para>
        /// If two providers of the same type, have the same SourceIdentifier they are considered equal.
        /// </para>
        /// </summary>
        /// <value>The source identifier.</value>
        public string SourceIdentifier
        {
            get { return string.Join("_", _providers.Select(p => p.SourceIdentifier)); }
        }

        /// <summary>
        /// Gets the list of providers.
        /// </summary>
        /// <value>
        /// The providers.
        /// </value>
        public IList<IConfigProvider> Providers
        {
            get { return _providers; }
        }

        /// <summary>
        /// Initializes the provider. Any initialization code that would otherwise be in the constructor must be in this method.
        /// </summary>
        void IConfigProvider.Initialize()
        {
            foreach (var p in _providers)
            {
                p.Initialize();
            }
        }

        /// <summary>
        /// Loads xml configurations.
        /// </summary>
        /// <returns>
        /// A list of XDocuments containing the configuration
        /// </returns>
        public IEnumerable<XDocument> LoadConfig()
        {
            foreach (var p in _providers)
            {
                var docs = p.LoadConfig();
                if (docs == null)
                {
                    continue;
                }

                foreach (var doc in docs)
                {
                    if (doc != null)
                    {
                        yield return doc;
                    }
                }
            }
        }

        /// <summary>
        /// Saves the configuration back to storage, unless <see cref="IsReadOnly"/> in which case an exception should be thrown.
        /// </summary>
        /// <param name="configDoc">The configuration XML to save</param>
        void IConfigProvider.SaveConfig(XDocument configDoc)
        {
            throw new NotSupportedException("Cannot save using this Provider.");
        }

        /// <summary>
        /// Deletes the configuration from storage. If implemented as NOOP or to throw exception <see cref="CanDelete"/> should return false.
        /// </summary>
        void IConfigProvider.DeleteConfig()
        {
            throw new NotSupportedException("Cannot delete using this Provider.");
        }
    }
}
