namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class ExitAction : ActionItem
    {
        public ExitAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.ExitCaption;
            this.IconUri = As.ResourceUri("Close.png");
            this.InputGesture = "Alt+F4";
        }

        public override void Execute()
        {
            this.Context.Messenger.Publish(EnsureAllBindingsRequest.Instance);

            this.Context.Messenger.Publish(new ShellMessage(ShellMessage.Action.Exit));
        }
    }
}
