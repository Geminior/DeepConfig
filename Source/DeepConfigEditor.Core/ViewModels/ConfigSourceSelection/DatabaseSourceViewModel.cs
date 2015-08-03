namespace DeepConfigEditor.ViewModels.ConfigSourceSelection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DeepConfig.Providers;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Utilities;

    public class DatabaseSourceViewModel : IProvideConfigSource
    {
        public Uri IconUri
        {
            get { return As.ResourceUri("Database.png"); }
        }

        public string DisplayName
        {
            get { return CommonRes.Database; }
        }

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
            var p = new MemoryConfigProvider("<root>This is database config, oh yeah</root>");
            return new ConfigSource(p);
        }
    }
}
