namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Config;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;

    public sealed class AlwaysOnTopAction : ActionItem
    {
        public AlwaysOnTopAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.AlwaysOnTopCaption;
            this.IsCheckable = true;
            this.IsChecked = AppearanceConfig.Instance.AlwaysOnTop;
        }

        public override void Execute()
        {
            this.Context.Messenger.Publish(new ShellMessage(ShellMessage.Action.ToggleAlwaysOnTop));
        }
    }
}
