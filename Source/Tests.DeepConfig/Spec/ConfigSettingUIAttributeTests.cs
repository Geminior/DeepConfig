namespace DeepConfig.Spec
{
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigSettingUIAttributeTests
    {
        [TestMethod]
        public void All_setting_attrib_ui_props_should_work_as_expected()
        {
            /* Arrange / Act */
            var adef = new ConfigSettingUIAttribute();

            var a = new ConfigSettingUIAttribute
            {
                HideInUI = true
            };

            /* Assert */
            adef.HideInUI.Should().BeFalse();

            a.HideInUI.Should().BeTrue();
        }
    }
}
