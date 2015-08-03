namespace DeepConfigEditor.Actions
{
    using System.Collections.Generic;
    using DeepConfigEditor.Extensions;

    public interface IActionsManager
    {
        IEnumerable<IActionItem> MenuItems
        {
            get;
        }

        IEnumerable<IActionItem> ToolBarItems
        {
            get;
        }

        IEnumerable<IAction> AllExecutables
        {
            get;
        }

        IActionContext ActionContext
        {
            get;
        }

        void BuildActions(IEnumerable<IProvideActions> pluginActions);

        bool ExecuteCommandLine(string[] args);
    }
}
