namespace SampleEditorPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DeepConfig.Providers;
    using DeepConfigEditor.Extensions;

    public class StaticConfigProviderViewModel : IProvideConfigSource
    {
        public Uri IconUri { get; set; }

        public string DisplayName { get; set; }

        public bool CanGetSource
        {
            get { return true; }
        }

        public bool Initialize(ConfigSourcePurpose p, Action<ConfigSource> provideSource)
        {
            return true;
        }

        public ConfigSource GetSource()
        {
            return null;
        }
    }
}
