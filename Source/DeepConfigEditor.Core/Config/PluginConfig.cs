namespace DeepConfigEditor.Config
{
    using System.Collections.Generic;
    using DeepConfig;

    public sealed class PluginConfig : AutoSingletonConfig<PluginConfig>
    {
        private List<string> _activePlugins = new List<string>();

        public PluginConfig()
        {
            this.PluginFolder = "Plugins";
        }

        [ConfigSetting(Description = "The folder in which plugins are stored.")]
        public string PluginFolder { get; set; }

        [ConfigSetting(Description = "The currently active plugin file names.")]
        public IList<string> ActivePlugins
        {
            get { return _activePlugins; }
        }
    }
}
