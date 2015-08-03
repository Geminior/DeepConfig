namespace DeepConfigEditor.Actions
{
    using System.Reflection;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public sealed class AssociateAction : ActionItem
    {
        private const string Extension = ".config";
        private const string CommandName = "EditWithDeepConfig";

        private readonly FileAssociationHandler _handler;
        private string _commandLine;

        public AssociateAction(IActionContext context)
            : base(context)
        {
            this.Caption = ActionsRes.AssociateWithCaption;
            this.Tooltip = ActionsRes.AssociateWithTooltip;
            this.IsCheckable = true;

            _handler = new FileAssociationHandler("DeepConfigEditor", false);

            _commandLine = string.Format("\"{0}\" \"%1\"", Assembly.GetEntryAssembly().Location);

            this.IsChecked = _handler.IsAssociated(Extension, CommandName, _commandLine);
        }

        public override void Execute()
        {
            if (this.IsChecked)
            {
                _handler.Associate(Extension, CommandName, CommonRes.ShellAssociatedCommandCaption, _commandLine);
            }
            else
            {
                _handler.Disassociate(Extension, CommandName);
            }
        }
    }
}
