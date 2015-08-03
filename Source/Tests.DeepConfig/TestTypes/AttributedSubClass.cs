namespace DeepConfig.TestTypes
{
    public class AttributedSubClass : AttributedClass
    {
        public override int PropWithAttrib
        {
            get { return base.PropWithAttrib; }
            set { base.PropWithAttrib = value; }
        }

        public override int PropWithInheritedAttrib
        {
            get { return base.PropWithAttrib; }
            set { base.PropWithAttrib = value; }
        }
    }
}
