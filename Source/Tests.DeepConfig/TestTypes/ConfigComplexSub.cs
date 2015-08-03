namespace DeepConfig.TestTypes
{
    //Config section decoration is inherited
    public class ConfigComplexSub : ConfigComplexAbstract
    {
        [ConfigSetting("Custom Name", true, Description = "This is the name")]
        public override string Name
        {
            get;
            set;
        }

        [ConfigSetting]
        public int NumberPlain { get; set; }

        //This one is not decorated as a configsetting now
        public override string OnBaseOnly
        {
            get;
            set;
        }
    }
}
