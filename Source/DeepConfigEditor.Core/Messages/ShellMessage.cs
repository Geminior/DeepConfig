namespace DeepConfigEditor.Messages
{
    public class ShellMessage
    {
        public ShellMessage(Action action)
        {
            this.RequestedAction = action;
        }

        public enum Action
        {
            Exit,
            ToggleAlwaysOnTop,
            StartWait,
            StopWait
        }

        public Action RequestedAction
        {
            get;
            private set;
        }
    }
}
