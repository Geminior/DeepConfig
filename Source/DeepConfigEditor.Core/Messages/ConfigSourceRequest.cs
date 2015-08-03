namespace DeepConfigEditor.Messages
{
    using System;
    using DeepConfig.Providers;
    using DeepConfigEditor.Extensions;

    public sealed class ConfigSourceRequest
    {
        public ConfigSourceRequest(ConfigSourcePurpose p, Action<ConfigSource, bool> sourceSelectedCallback = null)
        {
            this.RequestPurpose = p;
            this.SourceSelectedCallback = sourceSelectedCallback;
        }

        public ConfigSourcePurpose RequestPurpose
        {
            get;
            private set;
        }

        public Action<ConfigSource, bool> SourceSelectedCallback
        {
            get;
            private set;
        }
    }
}
