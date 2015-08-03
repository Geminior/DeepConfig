namespace DeepConfig
{
    using System;

    /// <summary>
    /// Defines the various error codes describing the type of error encountered.
    /// </summary>
    [Serializable]
    public enum ConfigErrorCode
    {
        /// <summary>
        /// Cause is not known. Do not use this.
        /// </summary>
        Unknown,

        /// <summary>
        /// Some error condition that falls outside the ones defined here.
        /// </summary>
        Generic,

        /// <summary>
        /// The specified configuration file is not a valid .Net config file
        /// </summary>
        InvalidConfigurationFile,

        /// <summary>
        /// The XmlNode passed to the method responsible for the Exception is not valid for the purpose of that method
        /// </summary>
        InvalidNode,

        /// <summary>
        /// The handler specified for a section in the config file could not be created. Likely due to the containing Assembly not being loaded or insufficuent privileges.
        /// </summary>
        SectionHandlerCreationFailed,

        /// <summary>
        /// The handler specified for a section in the config file does not implement the minimum required interface <see cref="Handlers.IExtendedConfigurationSectionHandler"/>.
        /// </summary>
        SectionHandlerInvalid,

        /// <summary>
        /// Either the handler specified for a section in the config file can not be resolved or the handler is not specified at all."/>.
        /// </summary>
        SectionHandlerMissing,

        /// <summary>
        /// The handler specified for a section in the config file is invalid for the section.
        /// </summary>
        SectionHandlerMismatch,

        /// <summary>
        /// An instance of the config type could not be created
        /// </summary>
        ConfigTypeCreationFailed,

        /// <summary>
        /// Failed to resolve a type from a type string
        /// </summary>
        TypeResolutionFailed,

        /// <summary>
        /// Failed to resolve or create the specified <see cref="DeepConfig.Cryptography.ICryptoProvider"/>
        /// </summary>
        CryptographyProviderCreationFailed,

        /// <summary>
        /// The config object has not been marked with the <see cref="ConfigSectionAttribute"/>
        /// </summary>
        MissingConfigSectionAttribute,

        /// <summary>
        /// The property has been decorated with an inappropriate attribute
        /// </summary>
        InvalidConfigSettingAttribute,

        /// <summary>
        /// The Type of the configuration object is invalid, either in general or in relation to its handler.
        /// </summary>
        InvalidConfigType,

        /// <summary>
        /// The value for a given element in the config file is invalid or does not match its mapped type.
        /// </summary>
        InvalidConfigValue,

        /// <summary>
        /// The config class property does not have a getter and can thus not be read.
        /// </summary>
        MissingGetter,

        /// <summary>
        /// The config class property does not have a setter and can thus not be set.
        /// </summary>
        MissingSetter,

        /// <summary>
        /// The config type most likely has a circular reference.
        /// </summary>
        ConfigTypeCircularReferenceSuspected,

        /// <summary>
        /// The <see cref="Providers.IConfigProvider"/> failed to load the configuration
        /// </summary>
        FailedToLoad,

        /// <summary>
        /// The <see cref="Providers.IConfigProvider"/> failed to save the configuration
        /// </summary>
        FailedToSave,

        /// <summary>
        /// The requested configuration could not be found
        /// </summary>
        ConfigurationNotFound
    }
}
