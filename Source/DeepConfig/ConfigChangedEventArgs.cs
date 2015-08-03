namespace DeepConfig
{
    using System;

    /// <summary>
    /// EventArgs for configuration change events.
    /// </summary>
    public sealed class ConfigChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The various possible causes why the change event was raised.
        /// </summary>
        public enum ChangeCause
        {
            /// <summary>
            /// Indicates that the config section's settings changed.
            /// </summary>
            SectionChanged,

            /// <summary>
            /// Indicates that the config section was removed.
            /// </summary>
            SectionRemoved,

            /// <summary>
            /// Indicates that the provider mapping for the config type has changed, <see cref="ConfigMaster.ProviderMapping"/>.
            /// </summary>
            MappingChanged
        }

        /// <summary>
        /// Gets the name of the section that was changed.
        /// </summary>
        /// <value>The name of the section. May be null in certain cases.</value>
        public string SectionName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the type of the section that was changed.
        /// </summary>
        /// <value>
        /// The type of the section. May be null.
        /// </value>
        public Type SectionType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the cause of the change.
        /// </summary>
        /// <value>
        /// The cause.
        /// </value>
        public ChangeCause Cause
        {
            get;
            internal set;
        }
    }
}
