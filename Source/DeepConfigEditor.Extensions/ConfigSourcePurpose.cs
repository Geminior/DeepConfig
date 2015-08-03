namespace DeepConfigEditor.Extensions
{
    /// <summary>
    /// The purpose of a request for a config source.
    /// </summary>
    public enum ConfigSourcePurpose
    {
        /// <summary>
        /// The config source will be used to open an existing configuration.
        /// </summary>
        Open,

        /// <summary>
        /// The config source will be used to save a new configuration.
        /// </summary>
        Save
    }
}
