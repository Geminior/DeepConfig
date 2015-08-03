namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class CollapseAction : ActionItem
    {
        public CollapseAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.CollapseSectionsCaption;
            this.IconUri = As.ResourceUri("Collapse.png");
            this.InputGesture = "F3";
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
