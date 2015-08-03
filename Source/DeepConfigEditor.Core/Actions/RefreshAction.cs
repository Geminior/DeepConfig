namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class RefreshAction : ActionItem
    {
        public RefreshAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.RefreshCaption;
            this.Tooltip = ActionsRes.RefreshTooltip;
            this.IconUri = As.ResourceUri("Refresh.png");
            this.InputGesture = "F5";
        }

        public override bool CanExecute
        {
            get
            {
                var cfgInfo = this.Context.CurrentConfiguration;

                return cfgInfo.Exists && !cfgInfo.IsNew;
            }
        }

        public override void Execute()
        {
            this.Context.Messenger.Publish(EnsureAllBindingsRequest.Instance);

            var msg = new ConfigRequest(ConfigRequest.Action.Refresh);

            this.Context.Messenger.Publish(msg);
        }
    }
}
