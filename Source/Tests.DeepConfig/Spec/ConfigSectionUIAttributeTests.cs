namespace DeepConfig.Spec
{
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigSectionUIAttributeTests
    {
        [TestMethod]
        public void All_section_attrib_ui_props_should_work_as_expected()
        {
            /* Arrange / Act */
            var adef = new ConfigSectionUIAttribute();

            var a = new ConfigSectionUIAttribute
            {
                IdPropertyName = "SomeProp",
                IsSubsection = true,
                SortProperties = true
            };

            /* Assert */
            adef.IdPropertyName.Should().BeNull();
            adef.IsSubsection.Should().BeFalse();
            adef.SortProperties.Should().BeFalse();

            a.IdPropertyName.Should().Be("SomeProp");
            a.IsSubsection.Should().BeTrue();
            a.SortProperties.Should().BeTrue();
        }
    }
}
