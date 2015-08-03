namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class ExpandAction : ActionItem
    {
        public ExpandAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.ExpandSectionsCaption;
            this.IconUri = As.ResourceUri("Expand.png");
            this.InputGesture = "F2";
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
        }
    }
}
