namespace DeepConfigEditor.Config
{
    using DeepConfig;

    [ConfigSection]
    public class FileSourceConfig
    {
        [ConfigSetting]
        public string LastDirectory
        {
            get;
            set;
        }
    }
}
