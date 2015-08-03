namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class NewAction : ActionItem
    {
        public NewAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.NewCaption;
            this.Tooltip = ActionsRes.NewTooltip;
            this.IconUri = As.ResourceUri("New.png");
            this.InputGesture = "Ctrl+N";
        }

        public override void Execute()
        {
            this.Context.Messenger.Publish(EnsureAllBindingsRequest.Instance);

            var msg = new ConfigRequest(ConfigRequest.Action.New);

            this.Context.Messenger.Publish(msg);
        }
    }
}
