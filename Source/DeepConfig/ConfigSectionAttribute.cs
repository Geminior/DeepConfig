namespace DeepConfig
{
    using System;
    using DeepConfig.Handlers;

    /// <summary>
    /// Marks a class as a config section. Only classes decorated by this attribute or a derivative, will be considered for participation in configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public sealed class ConfigSectionAttribute : Attribute
    {
        /// <summary>
        /// Backing field for <see cref="SectionHandlerType"/>
        /// </summary>
        private Type _sectionHandlerType = typeof(GenericTypeSectionHandler);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSectionAttribute"/> class.
        /// <para>Section handler used will be the <see cref="GenericTypeSectionHandler"/></para>
        /// </summary>
        public ConfigSectionAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSectionAttribute"/> class.
        /// </summary>
        /// <param name="sectionHandlerType">
        /// The type of the section handler that will be used to store and read the configuration settings of the section.This type must implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </param>
        public ConfigSectionAttribute(Type sectionHandlerType)
        {
            if ((sectionHandlerType != null) && !sectionHandlerType.Equals(typeof(GenericTypeSectionHandler)))
            {
                _sectionHandlerType = sectionHandlerType;
                this.HasCustomHandler = true;
            }
        }

        /// <summary>
        /// Gets the type of the section handler which will handle the section represented by the type decorated.
        /// </summary>
        public Type SectionHandlerType
        {
            get { return _sectionHandlerType; }
        }

        /// <summary>
        /// Gets a value indicating whether the type decorated will use a custom section handler or the default <see cref="GenericTypeSectionHandler"/>
        /// </summary>
        public bool HasCustomHandler
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the section will only be present once in any given config file, and that the section name will be fixed to the name of the section type.
        /// </summary>
        public bool IsSingleton
        {
            get;
            set;
        }
    }
}
