namespace DeepConfigEditor.Extensions
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface to implement in order to supply the application with actions.
    /// </summary>
    public interface IProvideActions : IPlugin
    {
        /// <summary>
        /// Gets the menu actions.
        /// </summary>
        /// <param name="context">The action context that the actions can use if they need acces to the services it provides.</param>
        /// <returns>The list of actions to add to the menu.</returns>
        IEnumerable<IActionItem> GetMenuActions(IActionContext context);

        /// <summary>
        /// Gets the toolbar actions.
        /// </summary>
        /// <param name="context">The action context that the actions can use if they need acces to the services it provides.</param>
        /// <returns>The list of actions to add to the toolbar.</returns>
        IEnumerable<IActionItem> GetToolBarActions(IActionContext context);
    }
}
