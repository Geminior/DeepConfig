namespace DeepConfig.Cryptography
{
    using System;
    using System.Security.Cryptography;
    using System.Xml.Linq;
    using DeepConfig;
    using DeepConfig.Core;
    using DeepConfig.Utilities;

    /// <summary>
    /// Handler for Cryptography related configuration settings
    /// </summary>
    public sealed class CryptoConfigSectionHandler
    {
        /// <summary>
        /// Writes Cryptography related configuration settings from a <see cref="ICryptoProvider"/> type.
        /// </summary>
        /// <param name="providerType">The <see cref="ICryptoProvider"/> type to use for config cryptography.</param>
        /// <returns>The xml element for the section.</returns>
        /// <exception cref="ConfigException">If providerType does not implement <see cref="ICryptoProvider"/></exception>
        /// <exception cref="ArgumentNullException">If providerType is null</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This method is accessed on an instance and canot be static, regardless of not accessing instance state.")]
        public XElement WriteSection(Type providerType)
        {
            Ensure.ArgumentNotNull(providerType, "providerType");

            if (!typeof(ICryptoProvider).IsAssignableFrom(providerType))
            {
                throw new ConfigException("The supplied type must implement DeepConfig.Cryptography.ICryptoProvider.", ConfigErrorCode.InvalidConfigType);
            }

            var mangledTypeName = new CryptoCryptoProvider().Encrypt(providerType.AssemblyQualifiedName);

            return new XElement(
                ConfigElement.CryptoSettingNode,
                new XAttribute(ConfigElement.SectionTypeAttribute, mangledTypeName));
        }

        /// <summary>
        /// Creates an <see cref="ICryptoProvider"/> from the cryptography section of a config file.
        /// </summary>
        /// <param name="section">The xml element that contains the configuration information from the configuration file.</param>
        /// <returns>A <see cref="ICryptoProvider"/> instance or null if the section does not contain crypto provider data.</returns>
        /// <exception cref="DeepConfig.ConfigException">If instantiation of the provider fails, or the instantiated type does not implement <see cref="ICryptoProvider"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This method is accessed on an instance and canot be static, regardless of not accessing instance state.")]
        public ICryptoProvider ReadSection(XElement section)
        {
            if (section == null)
            {
                return null;
            }

            string mangledTypeName = section.GetAttributeValue(ConfigElement.SectionTypeAttribute);
            if (mangledTypeName == null)
            {
                return null;
            }

            string typeName = new CryptoCryptoProvider().Decrypt(mangledTypeName);

            var result = ConfigHelper.CreateInstance(typeName, true) as ICryptoProvider;
            if (result == null)
            {
                throw new ConfigException(Msg.Text("Unable to resolve or create Cryptoprovider ({0}).", typeName), ConfigErrorCode.CryptographyProviderCreationFailed);
            }

            return result;
        }

        /// <summary>
        /// Internal Crypto implementation
        /// </summary>
        private class CryptoCryptoProvider : SymmetricalWithPassword
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CryptoCryptoProvider"/> class.
            /// </summary>
            public CryptoCryptoProvider()
                : base("adc!KEF@4t94r9fwfp3mR4ervsa", 64, 100)
            {
            }

            /// <summary>
            /// Factory method for creating the specific symmetric crypto service provider to use
            /// </summary>
            /// <returns>
            /// An instance of a <see cref="AesManaged"/> class
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Obviously not")]
            protected override SymmetricAlgorithm CreateServiceProvider()
            {
                return new AesManaged
                {
                    BlockSize = 128,
                    KeySize = 256,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };
            }
        }
    }
}
