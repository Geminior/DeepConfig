namespace DeepConfigEditor.Extensions
{
    using Caliburn.Micro;

    /// <summary>
    /// The context available to all action. It provides various services the action can use.
    /// </summary>
    public interface IActionContext
    {
        /// <summary>
        /// Gets the messenger that can be used to send messages to potential subscribers.
        /// </summary>
        /// <value>
        /// The messenger.
        /// </value>
        IEventAggregator Messenger { get; }

        /// <summary>
        /// Gets a value indicating whether the user is running the application with administrator privileges.
        /// </summary>
        /// <value>
        ///   <c>true</c> if administrator; otherwise, <c>false</c>.
        /// </value>
        bool IsAdministrator { get; }

        /// <summary>
        /// Gets or sets the info regarding the current configuration. This should not be changed.
        /// </summary>
        /// <value>
        /// The current configuration information.
        /// </value>
        IConfigInfo CurrentConfiguration
        {
            get;
            set;
        }
    }
}
