namespace DeepConfig.TestTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [ConfigSection]
    public class ConfigSilly
    {
        private ConfigWithBasicProps _getOnlyProp;
        private object _invalidType;

        public ConfigSilly()
        {
        }

        public ConfigSilly(ConfigWithBasicProps getOnlyProp)
        {
            _getOnlyProp = getOnlyProp;
        }

        [ConfigSetting]
        public string StringGetOnly
        {
            get { return "getonly"; }
        }

        [ConfigSetting]
        public ConfigErrorCode EnumGetOnly
        {
            get { return ConfigErrorCode.Generic; }
        }

        [ConfigSetting]
        public string StringSetOnly
        {
            set { }
        }

        [ConfigSetting]
        public ConfigSilly SelfRef
        {
            get { return this; }
        }

        [ConfigSetting]
        public ConfigWithBasicProps ComplexGetOnlyNull
        {
            get { return _getOnlyProp; }
        }

        [ConfigSetting]
        public object ComplexNotAConfigType
        {
            get { return new object(); }
            set { throw new Exception("Not expected to be called"); }
        }

        [ConfigSetting]
        public object ComplexNotAConfigTypeGetOnly
        {
            get
            {
                if (_invalidType == null)
                {
                    _invalidType = new object();
                }

                return _invalidType;
            }
        }

        [ConfigSetting]
        public object ComplexInvalidCtor
        {
            get { return new ConfigInvalidCtor("something"); }
            set { throw new Exception("Not expected to be called"); }
        }

        public string NotASetting
        {
            get { throw new Exception("Not expected to be called"); }
            set { throw new Exception("Not expected to be called"); }
        }

        [ConfigSetting]
        public string this[int idx]
        {
            get { return "indexer"; }
            set { throw new Exception("Not expected to be called"); }
        }
    }
}
