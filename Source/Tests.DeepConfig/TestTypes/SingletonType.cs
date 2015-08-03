namespace DeepConfig.TestTypes
{
    public class SingletonType : SingletonConfig<SingletonType>
    {
        [ConfigSetting]
        public string Name { get; set; }
    }
}
