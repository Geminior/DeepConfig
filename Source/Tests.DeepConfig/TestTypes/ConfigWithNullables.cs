namespace DeepConfig.TestTypes
{
    using System;

    [ConfigSection]
    public class ConfigWithNullables
    {
        [ConfigSetting]
        public int? NullableInt
        {
            get;
            set;
        }

        [ConfigSetting]
        public DateTime? NullableDateTime
        {
            get;
            set;
        }

        [ConfigSetting]
        public TimeSpan? NullableTimeSpan
        {
            get;
            set;
        }

        [ConfigSetting]
        public Guid? NullableGuid
        {
            get;
            set;
        }
    }
}
