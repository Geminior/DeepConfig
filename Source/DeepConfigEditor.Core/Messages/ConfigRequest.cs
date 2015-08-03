namespace DeepConfigEditor.Messages
{
    using System;
    using DeepConfigEditor.Extensions;

    public sealed class ConfigRequest
    {
        public ConfigRequest(Action a, ConfigSource source = null)
        {
            if (a == Action.Save || a == Action.New || a == Action.Refresh || a == Action.Delete || a == Action.StartScreen)
            {
                if (source != null)
                {
                    throw new ArgumentException(string.Format("The {0} request cannot have a source specified.", a));
                }
            }
            else if (source == null)
            {
                throw new ArgumentNullException("source", string.Format("The source must not be null for a {0} request.", a));
            }

            this.RequestedAction = a;
            this.Source = source;
        }

        public enum Action
        {
            New,
            Open,
            Save,
            SaveAs,
            Refresh,
            Delete,
            StartScreen
        }

        public Action RequestedAction
        {
            get;
            private set;
        }

        public ConfigSource Source
        {
            get;
            private set;
        }
    }
}
