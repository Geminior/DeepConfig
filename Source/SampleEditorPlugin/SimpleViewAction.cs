namespace SampleEditorPlugin
{
    using DeepConfigEditor.Extensions;

    public class SimpleViewAction : ActionItem
    {
        public SimpleViewAction(IActionContext context)
            : base(context)
        {
            this.Caption = "Go to simple view";
            this.Tooltip = "Navigates to the simple view";
        }

        public override void Execute()
        {
            var msg = ViewRequest.CreateForType<SimpleViewModel>();

            this.Context.Messenger.Publish(msg);
        }
    }
}
