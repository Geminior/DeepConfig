namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class DeleteAction : ActionItem
    {
        public DeleteAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.DeleteCaption;
            this.Tooltip = ActionsRes.DeleteTooltip;
            this.IconUri = As.ResourceUri("Delete.png");           
        }

        public override bool CanExecute
        {
            get
            {
                var cfgInfo = this.Context.CurrentConfiguration;

                return cfgInfo.Exists && cfgInfo.CanDelete && !cfgInfo.IsReadOnly;
            }
        }

        public override void Execute()
        {
            var msg = new ConfigRequest(ConfigRequest.Action.Delete);

            this.Context.Messenger.Publish(msg);
        }
    }
}
