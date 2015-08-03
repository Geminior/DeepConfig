namespace DeepConfigEditor.Actions
{
    using System;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;

    public class MruAction : ActionItem
    {
        private readonly ConfigSource _source;

        public MruAction(IActionContext context, ConfigSource source)
            : base(context)
        {
            _source = source;
            this.Caption = _source.DisplayName;
            this.IconUri = _source.IconUri;
        }

        public override void Execute()
        {
            this.Context.Messenger.Publish(EnsureAllBindingsRequest.Instance);

            var msg = new ConfigRequest(ConfigRequest.Action.Open, _source);

            this.Context.Messenger.Publish(msg);
        }
    }
}
