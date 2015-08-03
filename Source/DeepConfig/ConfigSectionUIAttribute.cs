namespace DeepConfig
{
    using System;

    /// <summary>
    /// Applies settings to a config class to be used by the configuration management UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public sealed class ConfigSectionUIAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the section should only be suggested as a subsection to another section in the config file.
        /// </summary>
        public bool IsSubsection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the type's properties should be sorted aplhabetically
        /// </summary>
        public bool SortProperties
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Property name of the decorated class than can be said to identify a class instance. The value of this will be used to represent the section in the UI under certain circumstances, such as when appearing in a list.
        /// </summary>
        public string IdPropertyName
        {
            get;
            set;
        }
    }
}
