namespace DeepConfig.TestTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [ConfigSection]
    public class ConfigWithCircularReference
    {
        public ConfigWithCircularReference()
        {
            this.Child = new InnerClass { Parent = this };
        }

        [ConfigSetting]
        public InnerClass Child { get; set; }

        [ConfigSection]
        public class InnerClass
        {
            [ConfigSetting]
            public ConfigWithCircularReference Parent { get; set; }
        }
    }
}
