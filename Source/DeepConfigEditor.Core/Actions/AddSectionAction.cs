namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class AddSectionAction : ActionItem
    {
        public AddSectionAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.AddSectionCaption;
            this.Tooltip = ActionsRes.AddSectionTooltip;
            this.IconUri = As.ResourceUri("AddLg.png");           
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
        }
    }
}
