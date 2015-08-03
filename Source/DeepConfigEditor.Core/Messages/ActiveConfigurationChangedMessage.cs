namespace DeepConfigEditor.Messages
{
    using DeepConfigEditor.Extensions;

    public class ActiveConfigurationChangedMessage
    {
        public ActiveConfigurationChangedMessage(IActionContext currentContext)
        {
            this.Context = currentContext;
        }

        public IActionContext Context
        {
            get;
            private set;
        }
    }
}
