namespace DeepConfigEditor.Extensions
{
    /// <summary>
    /// Interface for an executable action
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Gets a value indicating whether this action can execute.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the action is valid to execute; otherwise, <c>false</c>.
        /// </value>
        bool CanExecute
        {
            get;
        }

        /// <summary>
        /// Gets or sets the key combination that will trigger this action, e.g. Ctrl+S.
        /// </summary>
        /// <value>
        /// The input gesture.
        /// </value>
        string InputGesture
        {
            get;
            set;
        }

        /// <summary>
        /// Executes this action.
        /// </summary>
        void Execute();

        /// <summary>
        /// Called by the ActionsManager when the current configuration changes, e.g. when a new configuration is opened or a new one is saved. See remarks for details.
        /// </summary>
        /// <remarks>
        /// The default implementation if this simply notifies a change for CanExecute, so all actions whose execution validity is dependent on the state of the current configuration get updated.
        /// </remarks>
        void HandleCurrentConfigurationChanged();
    }
}
