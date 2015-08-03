namespace DeepConfigEditor.Actions
{
    using System;
    using Caliburn.Micro;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Models;
    using DeepConfigEditor.Services;

    public sealed class ActionContext : IActionContext
    {
        private IConfigInfo _configInfo;

        public ActionContext(IEventAggregator messenger, IProcessService processInfo)
        {
            this.Messenger = messenger;
            this.IsAdministrator = processInfo.IsRunningAsAdministrator;
            _configInfo = new ConfigInfo();
        }

        public IEventAggregator Messenger
        {
            get;
            private set;
        }

        public bool IsAdministrator
        {
            get;
            private set;
        }

        public IConfigInfo CurrentConfiguration
        {
            get
            {
                return _configInfo;
            }

            set
            {
                if (value == null)
                {
                    _configInfo = new ConfigInfo();
                    return;
                }

                if (value.GetType() != typeof(ConfigInfo))
                {
                    throw new InvalidOperationException("Setting this value is not allowed.");
                }

                _configInfo = value;
            }
        }
    }
}
