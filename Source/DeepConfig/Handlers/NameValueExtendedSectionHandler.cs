namespace DeepConfig.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Xml.Linq;
    using DeepConfig.Core;
    using DeepConfig.Cryptography;
    using DeepConfig.Utilities;

    /// <summary>
    /// Handler for basic NameValue configuration settings, e.g. appSettings.
    /// </summary>
    public sealed class NameValueExtendedSectionHandler : IExtendedConfigurationSectionHandler
    {
        /// <summary>
        /// Backing field for <see cref="Errors"/>
        /// </summary>
        private IList<ConfigError> _errors;

        /// <summary>
        /// Gets a list of errors encountered while reading or writing a section.
        /// </summary>
        public IEnumerable<ConfigError> Errors
        {
            get
            {
                if (_errors == null)
                {
                    yield break;
                }

                foreach (var e in _errors)
                {
                    yield return e;
                }
            }
        }

        /// <summary>
        /// Writes configuration settings from a <see cref="NameValueSettings"/> or <see cref="System.Collections.Specialized.NameValueCollection"/> instance.
        /// </summary>
        /// <param name="sectionXmlName">The xml name of the section to be written.</param>
        /// <param name="sectionSource">A <see cref="NameValueSettings"/> or <see cref="System.Collections.Specialized.NameValueCollection"/> instance.</param>
        /// <param name="cryptoProvider">An <see cref="ICryptoProvider"/> instance that can be used to encrypt settings. This is only used if <c>encryptAll</c> is true.</param>
        /// <param name="encryptAll">If <see langword="true" /> all settings will be encrypted (if not already encrypted). Only applies if sectionSource is a <see cref="NameValueSettings"/>.</param>
        /// <returns>The xml element for the section.</returns>
        /// <exception cref="ArgumentNullException">If sectionName is null or empty, or sectionSource is null</exception>
        /// <exception cref="ConfigException">If sectionSource is not of type <see cref="NameValueSettings"/> or <see cref="System.Collections.Specialized.NameValueCollection"/></exception>
        public XElement WriteSection(string sectionXmlName, object sectionSource, ICryptoProvider cryptoProvider, bool encryptAll)
        {
            Ensure.ArgumentNotNullOrEmpty(sectionXmlName, "sectionName");
            Ensure.ArgumentNotNull(sectionSource, "sectionSource");

            _errors = null;

            var nvs = sectionSource as NameValueSettings;
            if (nvs != null)
            {
                return WriteSettings(sectionXmlName, nvs, cryptoProvider, encryptAll);
            }

            var nvc = sectionSource as NameValueCollection;
            if (nvc != null)
            {
                nvs = new NameValueSettings(nvc);
                return WriteSettings(sectionXmlName, nvs, cryptoProvider, encryptAll);
            }

            _errors = new[] { new ConfigError(ConfigErrorCode.InvalidConfigType, "This handler can only process instances of type DeepConfig.NameValueSettings or System.Collections.Specialized.NameValueCollection or a derivative thereof.") };
            return null;
        }

        /// <summary>
        /// Creates a <see cref="NameValueSettings"/> from an xml element of a config file.
        /// <para>
        /// NOTE: The settings collection is case sensitive.
        /// </para>
        /// </summary>
        /// <param name="section">The XElement that contains the configuration information from the configuration file. Provides direct access to the XML contents of the configuration section</param>
        /// <param name="cryptoProvider">An <see cref="ICryptoProvider"/> instance that can be used to decrypt settings.</param>
        /// <param name="decryptAll">If <see langword="true" /> all settings will be decrypted. This is ignored since the <see cref="NameValueSettings"/> handles this by itself.</param>
        /// <returns>A <see cref="NameValueSettings"/> instance containing the configuration data, or empty if section is null or in fact empty.</returns>
        public object ReadSection(XElement section, ICryptoProvider cryptoProvider, bool decryptAll)
        {
            Ensure.ArgumentNotNull(section, "section");

            _errors = null;

            //Create a case sensitive collection
            var data = new NameValueSettings();

            if (section != null)
            {
                var settings = from node in section.Elements(ConfigElement.KeyValueSettingNode)
                               let key = node.Attribute(ConfigElement.SettingKeyAttribute)
                               let val = node.Attribute(ConfigElement.SettingValueAttribute)
                               where key != null && val != null
                               select new
                               {
                                   Key = key.Value,
                                   Value = val.Value,
                                   Comment = node.PreviousNode as XComment
                               };

                foreach (var node in settings)
                {
                    string val = node.Value;
                    bool encrypted = false;

                    if (cryptoProvider != null && cryptoProvider.IsEncrypted(val))
                    {
                        try
                        {
                            val = cryptoProvider.Decrypt(val);
                            encrypted = true;
                        }
                        catch
                        {
                            if (_errors == null)
                            {
                                _errors = new List<ConfigError>();
                            }

                            _errors.Add(
                                new ConfigError(
                                    ConfigErrorCode.InvalidConfigValue,
                                    string.Format("Reading value of setting {0} failed, value appears to be encrypted, but decryption failed.", node.Key)));

                            continue;
                        }
                    }

                    data.SetSetting(node.Key, val, encrypted);

                    if (node.Comment != null)
                    {
                        data.SetDescription(node.Key, node.Comment.Value);
                    }
                }
            }

            return data;
        }

        private static XElement WriteSettings(string sectionXmlName, NameValueSettings sectionSource, ICryptoProvider cryptoProvider, bool encryptAll)
        {
            //Write the settings
            var section = new XElement(sectionXmlName);

            var rawSource = sectionSource.RawSettings;
            foreach (string key in rawSource.AllKeys)
            {
                //Add the description if appropriate
                string description = sectionSource.GetDescription(key);
                if (description.Length > 0)
                {
                    section.Add(new XComment(description));
                }

                //Encrypt if appropriate
                string val = rawSource[key];
                if (cryptoProvider != null && (encryptAll || sectionSource.IsEncrypted(key)))
                {
                    val = cryptoProvider.Encrypt(val);
                }

                section.Add(new XElement(
                    ConfigElement.KeyValueSettingNode,
                    new XAttribute(ConfigElement.SettingKeyAttribute, key),
                    new XAttribute(ConfigElement.SettingValueAttribute, val)));
            }

            return section;
        }
    }
}