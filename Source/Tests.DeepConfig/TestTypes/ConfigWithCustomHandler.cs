namespace DeepConfig.TestTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [ConfigSection(typeof(HandlerCustom))]
    public class ConfigWithCustomHandler
    {
        public bool ReadByHandler { get; set; }

        public bool WrittenByHandler { get; set; }

        public bool ThrowOnRead { get; set; }

        public bool ThrowOnWrite { get; set; }

        public bool HasErrorOnRead { get; set; }

        public bool HasErrorOnWrite { get; set; }

        public bool ErrorsNullOnWrite { get; set; }

        public bool ErrorsNullOnRead { get; set; }
    }
}
