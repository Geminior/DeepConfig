namespace DeepConfig.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using DeepConfig.Utilities;

    /// <summary>
    /// This is a read-only provider that allows loading several config files at once, merging them into one set of configuration settings.
    /// </summary>
    /// <remarks>
    /// Loading one or more files that contain the same sections will result in only the section of the first file being loaded. I.e. the first file loaded will take precedence over later files if they contain the same section(s).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Perfectly fine name")]
    public class MultiFileConfigProvider : IConfigProvider
    {
        /// <summary>
        /// The list of file paths, representing the files to load
        /// </summary>
        private IList<string> _configFilePaths;

        /// <summary>
        /// The time the files were last loaded
        /// </summary>
        private DateTime _loadTime = DateTime.MaxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFileConfigProvider"/> class.
        /// </summary>
        /// <param name="configFilePaths">The list of config files (file path) to load inorder of precedence.</param>
        public MultiFileConfigProvider(params string[] configFilePaths)
            : this((IEnumerable<string>)configFilePaths)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFileConfigProvider"/> class.
        /// </summary>
        /// <param name="configFilePaths">The list of config files (file path) to load in order of precedence. A local copy is created of the list.</param>
        public MultiFileConfigProvider(IEnumerable<string> configFilePaths)
        {
            Ensure.ArgumentNotNull(configFilePaths, "configFilePaths");

            _configFilePaths = configFilePaths.ToList();
        }

        /// <summary>
        /// Gets a value indicating whether one or more of the source files have changed since they were loaded.
        /// </summary>
        public bool ModifiedSinceLoad
        {
            get
            {
                foreach (var cfgName in _configFilePaths)
                {
                    string fullPath = cfgName;
                    if (!Path.IsPathRooted(cfgName))
                    {
                        fullPath = Path.GetFullPath(cfgName);
                    }

                    if (File.Exists(fullPath))
                    {
                        DateTime lastModification = File.GetLastWriteTimeUtc(fullPath);
                        if (_loadTime < lastModification)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration is read-only.
        /// </summary>
        /// <value></value>
        public virtual bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration can be deleted.
        /// </summary>
        /// <value></value>
        public virtual bool CanDelete
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
            get { return string.Join("_", _configFilePaths); }
        }

        /// <summary>
        /// Gets the list configuration file paths. This list should not be altered after instantiation.
        /// </summary>
        /// <value>
        /// The configuration files.
        /// </value>
        protected IList<string> ConfigFiles
        {
            get { return _configFilePaths; }
        }

        /// <summary>
        /// Gets or sets the load time.
        /// </summary>
        /// <value>
        /// The load time.
        /// </value>
        protected DateTime LoadTime
        {
            get { return _loadTime; }
            set { _loadTime = value; }
        }

        /// <summary>
        /// Initializes the provider. Any initialization code that would otherwise be in the constructor must be in this method.
        /// </summary>
        public virtual void Initialize()
        {
            /* NOOP */
        }

        /// <summary>
        /// Loads xml configuration from storage. IF one or more of the files do not exist, they are simply ignored.
        /// </summary>
        /// <returns>
        /// A list of XDocuments containing the configuration
        /// </returns>
        public IEnumerable<XDocument> LoadConfig()
        {
            _loadTime = DateTime.UtcNow;

            foreach (var cfgName in _configFilePaths)
            {
                string fullPath = cfgName;
                if (!Path.IsPathRooted(cfgName))
                {
                    fullPath = Path.GetFullPath(cfgName);
                }

                if (File.Exists(fullPath))
                {
                    yield return XmlUtil.LoadXmlFile(fullPath, true);
                }
            }
        }

        /// <summary>
        /// Saves the configuration back to storage, unless <see cref="IsReadOnly"/> in which case an exception should be thrown.
        /// </summary>
        /// <param name="configDoc">The configuration XML to save</param>
        public virtual void SaveConfig(XDocument configDoc)
        {
            throw new NotSupportedException("Cannot save using this Provider.");
        }

        /// <summary>
        /// Deletes the configuration from storage. If implemented as NOOP or to throw exception <see cref="CanDelete"/> should return false.
        /// </summary>
        public virtual void DeleteConfig()
        {
            throw new NotSupportedException("Cannot delete using this Provider.");
        }
    }
}
