namespace DeepConfig.TestTypes
{
    [ConfigSection]
    public abstract class ConfigComplexAbstract : IComplexType
    {
        public abstract string Name { get; set; }

        [ConfigSetting]
        public abstract string OnBaseOnly { get; set; }
    }
}
