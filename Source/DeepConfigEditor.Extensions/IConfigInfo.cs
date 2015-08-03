namespace DeepConfigEditor.Extensions
{
    /// <summary>
    /// Interface representing information on the current configuration open in the editor.
    /// </summary>
    public interface IConfigInfo
    {
        /// <summary>
        /// Gets a value indicating whether there is a configuration loaded in the editor.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a configuration is loaded; otherwise, <c>false</c>.
        /// </value>
        bool Exists { get; }

        /// <summary>
        /// Gets a value indicating whether the current configuration is readonly.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the current configuration is read only; otherwise, <c>false</c>.
        /// </value>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the current configuration is new, i.e. has not yet been saved to storage.
        /// </summary>
        /// <value>
        ///   <c>true</c> if current configuration is new; otherwise, <c>false</c>.
        /// </value>
        bool IsNew { get; }

        /// <summary>
        /// Gets a value indicating whether the current configuration can be deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the current configuration can be deleted; otherwise, <c>false</c>.
        /// </value>
        bool CanDelete { get; }

        /// <summary>
        /// Gets the friendly name of the current configuration.
        /// </summary>
        /// <value>
        /// The name of the current configuration.
        /// </value>
        string FriendlyName { get; }
    }
}
