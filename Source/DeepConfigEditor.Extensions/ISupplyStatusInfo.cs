namespace DeepConfigEditor.Extensions
{
    /// <summary>
    /// Interface that indicates that a viewmodel supplies status text to show in the application status bar when activated.
    /// </summary>
    public interface ISupplyStatusInfo
    {
        /// <summary>
        /// Gets the status message to show in the main area of the status bar.
        /// </summary>
        /// <value>
        /// The status message.
        /// </value>
        string StatusMessage { get; }

        /// <summary>
        /// Gets the status to show in the right hand side of the status bar.
        /// </summary>
        /// <value>
        /// The state status.
        /// </value>
        string StatusState { get; }
    }
}
