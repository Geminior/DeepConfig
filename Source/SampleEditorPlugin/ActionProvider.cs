namespace SampleEditorPlugin
{
    using System.Collections.Generic;
    using DeepConfigEditor.Extensions;

    public class ActionProvider : IProvideActions
    {
        public IEnumerable<IActionItem> GetMenuActions(IActionContext context)
        {
            var container = new ActionItemParent(context, "SampleActions")
                                .AddRange(
                                    new SimpleViewAction(context));

            yield return container;
        }

        public IEnumerable<IActionItem> GetToolBarActions(IActionContext context)
        {
            yield break;
        }
    }
}
