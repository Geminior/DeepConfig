namespace DeepConfig.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig.Utilities;

    /// <summary>
    /// Implementation of <see cref="IConfigProvider"/> for in memory config, i.e. non-persistent
    /// </summary>
    [Serializable]
    public sealed class MemoryConfigProvider : IConfigProvider
    {
        /// <summary>
        /// The xml to load
        /// </summary>
        private string _xml;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryConfigProvider"/> class.
        /// </summary>
        public MemoryConfigProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryConfigProvider"/> class.
        /// </summary>
        /// <param name="xml">The xml to load</param>
        public MemoryConfigProvider(string xml)
        {
            _xml = xml;
        }

        /// <summary>
        /// Gets a value indicating whether the configuration has been altered by another source since this config provider was loaded, this will always return false for this provider.
        /// </summary>
        public bool ModifiedSinceLoad
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration is read-only, this will always return true for this provider.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration can be deleted, this will always return false for this provider.
        /// </summary>
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
            get { return _xml; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "MemoryConfigProvider";
        }

        /// <summary>
        /// Loads a default empty config
        /// </summary>
        /// <returns>An XmlDocument containing the configuration</returns>
        public IEnumerable<XDocument> LoadConfig()
        {
            if (string.IsNullOrEmpty(_xml))
            {
                yield return XmlUtil.CreateEmptyConfig();
            }
            else
            {
                yield return XmlUtil.LoadXmlString(_xml, true);
            }
        }

        /// <summary>
        /// Initializes the provider. Any initialization code that would otherwise be in the constructor must be in this method.
        /// </summary>
        void IConfigProvider.Initialize()
        {
            // NOOP
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
