namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class AssignCryptoAction : ActionItem
    {
        public AssignCryptoAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.AssignCryptoCaption;
            this.Tooltip = ActionsRes.AssignCryptoTooltip;
            this.IconUri = As.ResourceUri("Key.png");
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
        }
    }
}
