namespace DeepConfig.TestTypes
{
    [ConfigSection]
    public class AttributedClass
    {
        [ConfigSetting]
        public virtual int PropWithAttrib { get; set; }

        [InheritedPropAttribute]
        public virtual int PropWithInheritedAttrib { get; set; }

        public int PropNoAttrib { get; set; }
    }
}
