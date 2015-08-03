namespace DeepConfig.Spec
{
    using DeepConfig.Handlers;
    using DeepConfig.TestTypes;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigSectionAttributeTests
    {
        [TestMethod]
        public void All_section_attrib_ctors_should_work_as_expected()
        {
            /* Arrange / Act */
            var a1 = new ConfigSectionAttribute();
            var a2 = new ConfigSectionAttribute(null);
            var a3 = new ConfigSectionAttribute(typeof(GenericTypeSectionHandler));
            var a4 = new ConfigSectionAttribute(typeof(HandlerCustom));

            var a5 = new ConfigSectionAttribute();
            a5.IsSingleton = true;

            /* Assert */
            a1.HasCustomHandler.Should().BeFalse();
            a1.IsSingleton.Should().BeFalse();
            a1.SectionHandlerType.Should().Be(typeof(GenericTypeSectionHandler));

            a2.HasCustomHandler.Should().BeFalse();
            a2.IsSingleton.Should().BeFalse();
            a2.SectionHandlerType.Should().Be(typeof(GenericTypeSectionHandler));

            a3.HasCustomHandler.Should().BeFalse();
            a3.IsSingleton.Should().BeFalse();
            a3.SectionHandlerType.Should().Be(typeof(GenericTypeSectionHandler));

            a4.HasCustomHandler.Should().BeTrue();
            a4.IsSingleton.Should().BeFalse();
            a4.SectionHandlerType.Should().Be(typeof(HandlerCustom));

            a5.IsSingleton.Should().BeTrue();
        }
    }
}
