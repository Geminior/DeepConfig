namespace DeepConfigEditor.ViewModels
{
    using System.Collections.Generic;
    using Caliburn.Micro;
    using DeepConfigEditor.Config;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;

    public class StartScreenViewModel : Screen
    {
        private readonly IEventAggregator _messenger;

        public StartScreenViewModel(IEventAggregator messenger)
        {
            _messenger = messenger;
        }

        public IEnumerable<ConfigSource> RecentSources
        {
            get
            {
                return EditorSettings.Instance.RecentSources; 
            }
        }

        public void Open()
        {
            var msg = new ConfigSourceRequest(ConfigSourcePurpose.Open);

            _messenger.Publish(msg);
        }

        public void New()
        {
            var msg = new ConfigRequest(ConfigRequest.Action.New);

            _messenger.Publish(msg);
        }

        public void SelectSource(ConfigSource source)
        {
            var msg = new ConfigRequest(ConfigRequest.Action.Open, source);

            _messenger.Publish(msg);
        }

        public void RemoveRecentSource(ConfigSource source)
        {
            EditorSettings.Instance.RecentSources.Remove(source);
            _messenger.Publish(new MruListChangedMessage());
        }
    }
}
