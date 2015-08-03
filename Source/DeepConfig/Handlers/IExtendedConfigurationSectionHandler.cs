namespace DeepConfig.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using DeepConfig.Core;
    using DeepConfig.Cryptography;

    /// <summary>
    /// The base interface all configuration handlers must implement
    /// </summary>
    public interface IExtendedConfigurationSectionHandler
    {
        /// <summary>
        /// Gets a list of errors encountered while reading or writing a section.
        /// </summary>
        IEnumerable<ConfigError> Errors { get; }

        /// <summary>
        /// Creates a specialized data object from an xml element.
        /// </summary>
        /// <param name="section">The XElement that contains the configuration information from the configuration file. Provides direct access to the XML contents of the configuration section</param>
        /// <param name="cryptoProvider">An <see cref="ICryptoProvider"/> instance that can be used to decrypt settings. May be null</param>
        /// <param name="decryptAll">If <see langword="true" /> all settings should be decrypted.</param>
        /// <returns>A specialized data object instance containing the configuration data or null.</returns>
        /// <exception cref="ArgumentNullException">May throw if section is null</exception>
        /// <exception cref="Exception">May throw an Exception of some sort if section does not match the handler.</exception>
        object ReadSection(XElement section, ICryptoProvider cryptoProvider, bool decryptAll);

        /// <summary>
        /// Creates an xml section from its data object.
        /// </summary>
        /// <param name="sectionXmlName">The xml name of the section to be written.</param>
        /// <param name="sectionSource">The specialized data object containing the settings to be put in the section.</param>
        /// <param name="cryptoProvider">An <see cref="ICryptoProvider"/> instance that can be used to encrypt settings. May be null</param>
        /// <param name="encryptAll">If <see langword="true" /> all settings should be encrypted (if not already encrypted)</param>
        /// <returns>The XElement representing the section written.</returns>
        XElement WriteSection(string sectionXmlName, object sectionSource, ICryptoProvider cryptoProvider, bool encryptAll);
    }
}
