namespace DeepConfig.Core
{
    using System;

    /// <summary>
    /// Exposes names of common .Net configuration xml elements
    /// </summary>
    public static class ConfigElement
    {
        /// <summary>
        /// The root node of a .Net config file
        /// </summary>
        public const string RootNode = "configuration";

        /// <summary>
        /// The name of the xml element containing configuration section definitions
        /// </summary>
        public const string SectionsNode = "configSections";

        /// <summary>
        /// The name of the xml element defining a configuration section
        /// </summary>
        public const string SectionNode = "section";

        /// <summary>
        /// The name of the xml element defining a basic key/value setting
        /// </summary>
        public const string KeyValueSettingNode = "add";

        /// <summary>
        /// The name of the xml element defining a list element
        /// </summary>
        public const string ListElementNode = "item";

        /// <summary>
        /// The name of the xml attribute that stores the name of a configuration section
        /// </summary>
        public const string SectionNameAttribute = "name";

        /// <summary>
        /// The name of the xml attribute that stores the type of a configuration section handler
        /// </summary>
        public const string SectionTypeAttribute = "type";

        /// <summary>
        /// The name of the xml attribute that stores the key of a key/value setting
        /// </summary>
        public const string SettingKeyAttribute = "key";

        /// <summary>
        /// The name of the xml attribute that stores the value of a key/value setting
        /// </summary>
        public const string SettingValueAttribute = "value";

        /// <summary>
        /// The name of the xml element that is the default configuration section
        /// </summary>
        public const string AppSettingsNode = "appSettings";

        /// <summary>
        /// The name of the xml element that defines the cryptography provider
        /// </summary>
        public const string CryptoSettingNode = "DeepConfigCryptography";

        /// <summary>
        /// Default XML for .Net config contents.
        /// </summary>
        public const string DefaultConfigContents = "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration></configuration>";
    }
}
