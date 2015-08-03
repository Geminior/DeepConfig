namespace DeepConfigEditor.Actions
{
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;

    public sealed class AboutAction : ActionItem
    {
        public AboutAction(IActionContext context)
            : base(context)
        {
            this.Caption = string.Format(ActionsRes.AboutCaption, CommonRes.AppName);      
        }

        public override void Execute()
        {
        }
    }
}
