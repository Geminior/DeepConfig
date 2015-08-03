namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;
    using DeepConfigEditor.ViewModels;

    public sealed class OptionsAction : ActionItem
    {
        public OptionsAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.OptionsCaption;
            this.Tooltip = ActionsRes.OptionsTooltip;
            this.IconUri = As.ResourceUri("Options.png");
        }

        public override void Execute()
        {
            var msg = ViewRequest.CreateForType<OptionsViewModel>();

            this.Context.Messenger.Publish(msg);
        }
    }
}
