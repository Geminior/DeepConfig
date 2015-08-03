namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class SaveAction : ActionItem
    {
        public SaveAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.SaveCaption;
            this.Tooltip = ActionsRes.SaveTooltip;
            this.IconUri = As.ResourceUri("Save.png");
            this.InputGesture = "Ctrl+S";
        }

        public override bool CanExecute
        {
            get
            {
                var cfgInfo = this.Context.CurrentConfiguration;

                return cfgInfo.Exists && !cfgInfo.IsReadOnly;
            }
        }

        public override void Execute()
        {
            this.Context.Messenger.Publish(EnsureAllBindingsRequest.Instance);

            var msg = new ConfigRequest(ConfigRequest.Action.Save);

            this.Context.Messenger.Publish(msg);
        }
    }
}
