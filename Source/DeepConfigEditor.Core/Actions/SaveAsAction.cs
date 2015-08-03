namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;

    public sealed class SaveAsAction : ActionItem
    {
        public SaveAsAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.SaveAsCaption;
            this.Tooltip = ActionsRes.SaveAsTooltip;
        }

        public override bool CanExecute
        {
            get
            {
                var cfgInfo = this.Context.CurrentConfiguration;

                return cfgInfo.Exists;
            }
        }

        public override void Execute()
        {
            this.Context.Messenger.Publish(EnsureAllBindingsRequest.Instance);

            var msg = new ConfigSourceRequest(ConfigSourcePurpose.Save);

            this.Context.Messenger.Publish(msg);
        }
    }
}
