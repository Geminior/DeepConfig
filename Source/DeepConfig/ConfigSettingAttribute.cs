namespace DeepConfig
{
    using System;

    /// <summary>
    /// Marks a config class property for participation in configuration with the <see cref="Handlers.GenericTypeSectionHandler"/>.
    /// <para>Only properties marked by this will be saved and read from the config file.</para>
    /// <para>See Remarks section for details.</para>
    /// </summary>
    /// <remarks>
    /// Only properties of the following types are valid for decoration by this attribute.
    /// <list type="bullet">
    /// <item><description>Primitive types</description></item>
    /// <item><description>String</description></item>
    /// <item><description>DateTime</description></item>
    /// <item><description>TimeSpan</description></item>
    /// <item><description>Enum</description></item>
    /// <item><description>Guid</description></item>
    /// <item><description>Decimal</description></item>
    /// <item><description>ICollection&lt;T&gt; derivatives</description></item>
    /// <item><description>Types decorated with the <see cref="ConfigSectionAttribute"/></description></item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class ConfigSettingAttribute : Attribute
    {
        /// <overloads>Constructors</overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSettingAttribute"/> class. <see cref="SettingName"/> will be the name of the property decorated and <see cref="Encrypt"/> will be false.
        /// </summary>
        public ConfigSettingAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSettingAttribute"/> class. <see cref="Encrypt"/> will be false.
        /// </summary>
        /// <param name="settingName">The name (alias) to use for the decorated property.</param>
        public ConfigSettingAttribute(string settingName)
            : this(settingName, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSettingAttribute"/> class. <see cref="SettingName"/> will be the name of the property decorated.
        /// </summary>
        /// <param name="encrypt">true to encrypt the value of the property</param>
        public ConfigSettingAttribute(bool encrypt)
            : this(null, encrypt)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSettingAttribute"/> class.
        /// </summary>
        /// <param name="settingName">The name (alias) to use for the decorated property.</param>
        /// <param name="encrypt">true to encrypt the value of the property</param>
        public ConfigSettingAttribute(string settingName, bool encrypt)
        {
            this.SettingName = settingName;
            this.Encrypt = encrypt;
        }

        /// <summary>
        /// Gets the name of the Configuration setting
        /// </summary>
        public string SettingName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the description of the Configuration setting
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this setting is encrypted
        /// </summary>
        public bool Encrypt
        {
            get;
            private set;
        }
    }
}
