namespace DeepConfig.Providers
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Interface for providers of configuration data
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Gets a value indicating whether the configuration has been altered by another source since this config provider was loaded.
        /// </summary>
        bool ModifiedSinceLoad
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the configuration is read-only.
        /// </summary>
        bool IsReadOnly
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the configuration can be deleted.
        /// </summary>
        bool CanDelete
        {
            get;
        }

        /// <summary>
        /// Gets the source identifier, which is a string that precisely identifies the source of the configuration returned by the provider.
        /// <para>
        /// If two providers of the same type, have the same SourceIdentifier they are considered equal.
        /// </para>
        /// </summary>
        /// <value>The source identifier.</value>
        string SourceIdentifier
        {
            get;
        }

        /// <summary>
        /// Initializes the provider. Any initialization code that would otherwise be in the constructor must be in this method.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Loads xml configuration from storage
        /// </summary>
        /// <returns>A list of XDocuments containing the configuration</returns>
        IEnumerable<XDocument> LoadConfig();

        /// <summary>
        /// Saves the configuration back to storage, unless <see cref="IsReadOnly"/> in which case an exception should be thrown.
        /// </summary>
        /// <param name="configDoc">The configuration XML to save</param>
        void SaveConfig(XDocument configDoc);

        /// <summary>
        /// Deletes the configuration from storage. If implemented as NOOP or to throw exception <see cref="CanDelete"/> should return false.
        /// </summary>
        void DeleteConfig();
    }
}
