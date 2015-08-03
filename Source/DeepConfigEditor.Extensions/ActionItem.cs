namespace DeepConfigEditor.Extensions
{
    using System;

    /// <summary>
    /// Represents an actual action that can be <see cref="ActionItem.Execute"/>d
    /// </summary>
    public class ActionItem : ActionItemBase, IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItem"/> class.
        /// </summary>
        /// <param name="context">The action context.</param>
        public ActionItem(IActionContext context)
            : base(context)
        {  
        }

        /// <summary>
        /// Gets or sets the icon URI.
        /// </summary>
        /// <value>
        /// The icon URI.
        /// </value>
        public Uri IconUri { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item can be checked
        /// </summary>
        /// <value>
        ///   <c>true</c> if checkable; otherwise, <c>false</c>.
        /// </value>
        public bool IsCheckable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is checked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Gets or sets the key combination that will trigger this action, e.g. Ctrl+S.
        /// </summary>
        /// <value>
        /// The input gesture.
        /// </value>
        public string InputGesture { get; set; }

        /// <summary>
        /// Gets a value indicating whether this action can execute.
        /// </summary>
        /// <value>
        /// <c>true</c> if the action is valid to execute; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanExecute
        {
            get { return true; }
        }

        /// <summary>
        /// Executes this action.
        /// </summary>
        public virtual void Execute()
        {
        }

        /// <summary>
        /// Called by the ActionsManager when the current configuration changes, e.g. when a new configuration is opened or a new one is saved. See remarks for details.
        /// </summary>
        /// <remarks>
        /// The default implementation if this simply notifies a change for CanExecute, so all actions whose execution validity is dependent on the state of the current configuration get updated.
        /// </remarks>
        public virtual void HandleCurrentConfigurationChanged()
        {
            NotifyOfPropertyChange(() => CanExecute);
        }
    }
}
