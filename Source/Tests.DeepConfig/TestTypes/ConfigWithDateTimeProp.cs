namespace DeepConfig.TestTypes
{
    using System;

    [ConfigSection]
    public class ConfigWithDateTimeProp
    {
        [ConfigSetting]
        public DateTime DateTimeValue
        {
            get;
            set;
        }
    }
}
