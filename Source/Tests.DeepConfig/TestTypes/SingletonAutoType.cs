namespace DeepConfig.TestTypes
{
    public class SingletonAutoType : AutoSingletonConfig<SingletonAutoType>
    {
        [ConfigSetting]
        public string Name { get; set; }
    }
}
