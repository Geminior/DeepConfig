namespace DeepConfig.Spec
{
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigSettingAttributeTests
    {
        [TestMethod]
        public void All_setting_attrib_ctors_should_work_as_expected()
        {
            /* Arrange / Act */
            var a1 = new ConfigSettingAttribute();
            var a2 = new ConfigSettingAttribute(true);
            var a3 = new ConfigSettingAttribute("New Name");
            var a4 = new ConfigSettingAttribute("New Name", true);

            var a5 = new ConfigSettingAttribute
            {
                Description = "Some Description"
            };

            /* Assert */
            a1.Description.Should().BeNull();
            a1.Encrypt.Should().BeFalse();
            a1.SettingName.Should().BeNull();

            a2.Description.Should().BeNull();
            a2.Encrypt.Should().BeTrue();
            a2.SettingName.Should().BeNull();

            a3.Description.Should().BeNull();
            a3.Encrypt.Should().BeFalse();
            a3.SettingName.Should().Be("New Name");

            a4.Description.Should().BeNull();
            a4.Encrypt.Should().BeTrue();
            a4.SettingName.Should().Be("New Name");

            a5.Description.Should().Be("Some Description");
        }
    }
}
