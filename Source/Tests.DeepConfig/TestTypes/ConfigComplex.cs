namespace DeepConfig.TestTypes
{
    [ConfigSection]
    public class ConfigComplex : IComplexType
    {
        private ConfigComplexSub _subLazy;

        public string Name
        {
            get;
            set;
        }

        [ConfigSetting(true)]
        public string Encrypted
        {
            get;
            set;
        }

        [ConfigSetting("Concrete Instance")]
        public ConfigComplexSub SubConcrete
        {
            get;
            set;
        }

        [ConfigSetting(true)]
        public ConfigComplexSub SubEncrypted
        {
            get;
            set;
        }

        [ConfigSetting]
        public ConfigComplexSub SubLazy
        {
            get
            {
                if (_subLazy == null)
                {
                    _subLazy = new ConfigComplexSub();
                }

                return _subLazy;
            }
        }

        [ConfigSetting]
        public IComplexType SubInterface
        {
            get;
            set;
        }

        [ConfigSetting]
        public ConfigComplexAbstract SubAbstract
        {
            get;
            set;
        }

        [ConfigSetting]
        public ConfigWithCustomHandler CustomHandler
        {
            get;
            set;
        }
    }
}
