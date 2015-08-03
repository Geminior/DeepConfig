namespace SampleEditorPlugin
{
    using Caliburn.Micro;
    using DeepConfigEditor.Extensions;

    public class SimpleViewModel : IPlugin
    {
        private IEventAggregator _messenger;

        public SimpleViewModel(IEventAggregator messenger)
        {
            _messenger = messenger;
        }

        /// <summary>
        /// Even though this example relies on the IEventAggregator to provide a way to get back to the mmain view, you can leverage the conventions of Caliburn.Micro without referencing it at all.
        /// Of course in order to close your custom view, it must be possible to send the ViewRequest.MainView from somewhere, but that could also be from an action you display in a custom menu or whatever.
        /// </summary>
        public void Close()
        {
            _messenger.Publish(ViewRequest.MainView);
        }
    }
}
