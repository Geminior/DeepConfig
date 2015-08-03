namespace DeepConfig.TestTypes
{
    [ConfigSection]
    public class ConfigComplexWithCustomGetOnly
    {
        private ConfigWithCustomHandler _getOnlyCustom;

        [ConfigSetting]
        public ConfigWithCustomHandler CustomHandlerGetOnly
        {
            get
            {
                if (_getOnlyCustom == null)
                {
                    _getOnlyCustom = new ConfigWithCustomHandler();
                }

                return _getOnlyCustom;
            }
        }
    }
}
