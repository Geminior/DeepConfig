namespace DeepConfigEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;
    using Caliburn.Micro.Extras;
    using DeepConfig.Providers;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Services;

    public class ConfigSourceSelectViewModel : Conductor<IProvideConfigSource>.Collection.OneActive, ISupplyStatusInfo
    {
        private readonly IEventAggregator _messenger;
        private readonly IEnumerable<IProvideConfigSource> _sources;
        private ConfigSourcePurpose _purpose;
        private Action<ConfigSource, bool> _sourceSelectedCallback;

        public ConfigSourceSelectViewModel(IEnumerable<IProvideConfigSource> sources, IEventAggregator messenger)
        {
            _sources = sources;
            _messenger = messenger;
        }

        public string SelectCaption
        {
            get;
            set;
        }

        public string StatusMessage
        {
            get { return ConfigSourceSelectRes.ConfigSelectStatusMessage; }
        }

        public string StatusState
        {
            get { return string.Empty; }
        }

        public void Cancel()
        {
            Logger.Instance.Debug("Closing source select");
            this.TryClose();

            if (_sourceSelectedCallback != null)
            {
                Logger.Instance.Debug("Executing callback with cancel");
                _sourceSelectedCallback(null, true);
            }
        }

        public void Select()
        {
            var active = this.ActiveItem;
            if (active != null)
            {
                SelectSource(active.GetSource());
            }
            else
            {
                SelectSource(null);
            }
        }

        public void SelectSource(ConfigSource source)
        {
            bool isSourceNull = (source == null);
            if (isSourceNull)
            {
                Logger.Instance.Warn("The config source selection returned null: " + this.ActiveItem.GetType().ToString());
            }

            //If the request provided a callback use that, otherwise post a request
            if (_sourceSelectedCallback != null)
            {
                Logger.Instance.Debug("Closing source select");
                this.TryClose();

                Logger.Instance.Debug("Executing callback with source");
                _sourceSelectedCallback(source, isSourceNull);
            }
            else
            {
                var reqType = (_purpose == ConfigSourcePurpose.Open) ? ConfigRequest.Action.Open : ConfigRequest.Action.SaveAs;

                Logger.Instance.Debug("Closing source select");
                this.TryClose();

                if (!isSourceNull)
                {
                    Logger.Instance.Debug("Requesting operation " + reqType.ToString());
                    var msg = new ConfigRequest(reqType, source);
                    _messenger.Publish(msg);
                }
            }
        }

        internal void Initialize(ConfigSourceRequest request)
        {
            _purpose = request.RequestPurpose;
            _sourceSelectedCallback = request.SourceSelectedCallback;

            if (_purpose == ConfigSourcePurpose.Open)
            {
                this.SelectCaption = CommonRes.Open;
            }
            else
            {
                this.SelectCaption = CommonRes.Save;
            }

            foreach (var s in _sources)
            {
                if (s.Initialize(_purpose, SelectSource))
                {
                    this.Items.Add(s);
                }
            }
        }
    }
}
