namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;

    public sealed class ShowUnsupportedAction : ActionItem
    {
        public ShowUnsupportedAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.ShowUnsupportedCaption;
            this.Tooltip = ActionsRes.ShowUnsupportedTooltip;           
        }

        public override void Execute()
        {
        }
    }
}
