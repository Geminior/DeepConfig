namespace DeepConfig.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml.Linq;
    using DeepConfig.Utilities;

    /// <summary>
    /// Implementation of <see cref="IConfigProvider"/> for xml config files
    /// </summary>
    [ConfigSection]
    [ConfigSectionUI(IsSubsection = true)]
    [Serializable]
    public class FileConfigProvider : IConfigProvider
    {
        /// <summary>
        /// Backing field for <see cref="ConfigFileName"/>
        /// </summary>
        private string _configFileName;

        /// <summary>
        /// Stores the time the config was loaded
        /// </summary>
        private DateTime _loadTime = DateTime.MaxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileConfigProvider"/> class.
        /// </summary>
        /// <param name="configFile">The full path of the config file to manage. If the file does not exist, it is created</param>
        public FileConfigProvider(string configFile)
        {
            this.ConfigFileName = configFile;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="FileConfigProvider"/> class from being created. This default constructor is for use with the config framework, since a parameterless default constructor is needed, and it can be private.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private FileConfigProvider()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the configuration has been altered by another source since this config provider was loaded.
        /// </summary>
        public bool ModifiedSinceLoad
        {
            get
            {
                if (File.Exists(_configFileName))
                {
                    DateTime lastModification = File.GetLastWriteTimeUtc(_configFileName);
                    return (_loadTime < lastModification);
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration is read-only, i.e. the file is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                if (File.Exists(_configFileName))
                {
                    FileAttributes fileAttribs = File.GetAttributes(_configFileName);
                    return ((fileAttribs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration file can be deleted.
        /// </summary>
        public bool CanDelete
        {
            get { return !IsReadOnly; }
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
            get { return _configFileName; }
        }

        /// <summary>
        /// Gets the name of the config file this provider instance is based on.
        /// </summary>
        [ConfigSetting(Description = "The name of the config file this provider instance is based on.")]
        public string ConfigFileName
        {
            get
            {
                return _configFileName;
            }

            private set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("ConfigFileName cannot be null or empty.", "value");
                }

                _configFileName = value;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _configFileName;
        }

        /// <summary>
        /// Initializes the provider. Any initialization code that would otherwise be in the constructor must be in this method.
        /// </summary>
        public void Initialize()
        {
            // NOOP
        }

        /// <summary>
        /// Loads the main configuration file
        /// </summary>
        /// <returns>An list with one Xml Document containing the configuration</returns>
        /// <exception cref="System.IO.IOException">If the config file cannot be read. . File can be non-existant though as this will just create the file.</exception>
        /// <exception cref="System.Xml.XmlException">
        /// If the file is not a valid xml file. File can be non-existant though as this will just create the file.
        /// </exception>
        public IEnumerable<XDocument> LoadConfig()
        {
            XDocument xmlDoc;

            if (File.Exists(_configFileName))
            {
                xmlDoc = XmlUtil.LoadXmlFile(_configFileName, true);
                _loadTime = File.GetLastWriteTimeUtc(_configFileName);
            }
            else
            {
                xmlDoc = XmlUtil.CreateEmptyConfig();
                _loadTime = DateTime.UtcNow;
            }

            yield return xmlDoc;
        }

        /// <summary>
        /// Save the configuration back to disk
        /// </summary>
        /// <param name="configDoc">The configuration xml to save.</param>
        /// <exception cref="System.IO.IOException">If <see cref="ConfigFileName"/> cannot be written to for some reason. File can be non-existant though this will just create the file.</exception>
        /// <exception cref="UnauthorizedAccessException">If the file is read-only or otherwise inaccessible.</exception>
        public void SaveConfig(XDocument configDoc)
        {
            if (configDoc == null)
            {
                return;
            }

            string folder = Path.GetDirectoryName(_configFileName);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            configDoc.Save(_configFileName);

            _loadTime = File.GetLastWriteTimeUtc(_configFileName);
        }

        /// <summary>
        /// Deletes the configuration file
        /// </summary>
        public void DeleteConfig()
        {
            if (!this.CanDelete)
            {
                throw new InvalidOperationException("The configuration cannot be deleted, please check the CanDelete property before attempting to delete a configuration.");
            }

            if (File.Exists(_configFileName))
            {
                File.Delete(_configFileName);
            }
        }
    }
}
