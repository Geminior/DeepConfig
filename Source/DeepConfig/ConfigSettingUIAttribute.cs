namespace DeepConfig
{
    using System;

    /// <summary>
    /// Applies settings to a config setting to be used by the configuration management UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class ConfigSettingUIAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether this setting should be visible in graphical editors
        /// </summary>
        public bool HideInUI
        {
            get;
            set;
        }
    }
}
