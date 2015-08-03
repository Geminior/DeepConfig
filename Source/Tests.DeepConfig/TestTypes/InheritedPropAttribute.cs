namespace DeepConfig.TestTypes
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class InheritedPropAttribute : Attribute
    {
    }
}
