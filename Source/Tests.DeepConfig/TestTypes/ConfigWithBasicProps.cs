namespace DeepConfig.TestTypes
{
    using System;

    [ConfigSection]
    public class ConfigWithBasicProps
    {
        public enum TestEnum
        {
            None,
            One,
            Two
        }

        [ConfigSetting]
        public string StringValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public bool BoolValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public byte ByteValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public int IntValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public long LongValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public short ShortValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public float FloatValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public double DoubleValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public decimal DecimalValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public Guid GuidValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public TestEnum EnumValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public TimeSpan TimeSpanValue
        {
            get;
            set;
        }

        [ConfigSetting]
        public Uri UriValue
        {
            get;
            set;
        }
    }
}
