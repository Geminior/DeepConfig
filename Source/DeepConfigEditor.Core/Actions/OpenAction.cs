namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class OpenAction : ActionItem
    {
        public OpenAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.OpenCaption;
            this.Tooltip = ActionsRes.OpenTooltip;
            this.IconUri = As.ResourceUri("Open.png");
            this.InputGesture = "Ctrl+O";
        }

        public override void Execute()
        {
            this.Context.Messenger.Publish(EnsureAllBindingsRequest.Instance);

            var msg = new ConfigSourceRequest(ConfigSourcePurpose.Open);

            this.Context.Messenger.Publish(msg);
        }
    }
}
