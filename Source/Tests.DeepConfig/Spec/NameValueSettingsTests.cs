namespace DeepConfig.Spec
{
    using System;
    using DeepConfig;
    using DeepConfig.Cryptography;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public enum TestSetting
    {
        StringValue,
        IntegerValue,
        BooleanValue,
        DateTimeValue,
        EnumValue,
        TimeSpanValue,
        GuidValue
    }

    [TestClass]
    public class NameValueSettingsTests
    {
        [TestMethod]
        public void Get_and_set_should_work_regardless_of_culture()
        {
            /* Arrange */
            var nv = new NameValueSettings();

            var expectedDate = DateTime.UtcNow;
            var expectedTime = new TimeSpan(5, 4, 3, 2, 1);
            var expectedGuid = Guid.NewGuid();

            /* Act */
            using (new CultureContext())
            {
                nv.SetSetting("StringValue", "SomeString");
                nv.SetEncryptedSetting("StringValueTwo", "SomeOtherString");
                nv.SetSetting(TestSetting.IntegerValue, 10);
                nv.SetSetting(TestSetting.BooleanValue, true);
                nv.SetSetting(TestSetting.DateTimeValue, expectedDate);
                nv.SetSetting(TestSetting.EnumValue, TestSetting.EnumValue);
                nv.SetSetting(TestSetting.TimeSpanValue, expectedTime);
                nv.SetEncryptedSetting(TestSetting.GuidValue, expectedGuid);
            }

            bool encryptedDate = nv.IsEncrypted(TestSetting.DateTimeValue);
            bool encryptedGuid = nv.IsEncrypted(TestSetting.GuidValue);
            bool valueExists = nv.HasSetting(TestSetting.BooleanValue);
            bool valueNoExists = nv.HasSetting("SomeOtherVal");

            string stringVal = nv.Get("StringValue", null);
            string stringVal2 = nv.Get<string>("StringValueTwo", null);
            int intVal = nv.Get<int>(TestSetting.IntegerValue, 0);
            bool boolVal = nv.Get<bool>(TestSetting.BooleanValue, false);
            DateTime dateVal = nv.Get<DateTime>(TestSetting.DateTimeValue, DateTime.MaxValue);
            TestSetting enumVal = nv.Get<TestSetting>(TestSetting.EnumValue, TestSetting.BooleanValue);
            TimeSpan timeVal = nv.Get<TimeSpan>(TestSetting.TimeSpanValue, TimeSpan.Zero);
            Guid guidVal = nv.Get<Guid>(TestSetting.GuidValue, Guid.Empty);

            /* Assert */
            stringVal.Should().Be("SomeString");
            stringVal2.Should().Be("SomeOtherString");
            intVal.Should().Be(10);
            boolVal.Should().Be(true);
            dateVal.Should().Be(expectedDate);
            enumVal.Should().Be(TestSetting.EnumValue);
            timeVal.Should().Be(expectedTime);
            guidVal.Should().Be(expectedGuid);

            encryptedDate.Should().BeFalse();
            encryptedGuid.Should().BeTrue();
            valueExists.Should().BeTrue();
            valueNoExists.Should().BeFalse();
        }

        [TestMethod]
        public void Getting_defaults_for_non_existant_values_should_work()
        {
            /* Arrange */
            var nv = new NameValueSettings();

            /* Act */
            var s1 = nv.Get("NoSuch1", "Def1");
            var s2 = nv.Get<string>("NoSuch2", "Def2");
            var s3 = nv.Get<bool>("NoSuch3", true);

            /* Assert */
            s1.Should().Be("Def1");
            s2.Should().Be("Def2");
            s3.Should().BeTrue();
        }

        [TestMethod]
        public void Setting_empty_values_should_work()
        {
            /* Arrange */
            var nv = new NameValueSettings();

            /* Act */
            nv.SetSetting(TestSetting.StringValue, "SomeVal");
            nv.SetSetting(TestSetting.BooleanValue, true);

            nv.SetSetting(TestSetting.StringValue, null);
            nv.SetSetting(TestSetting.BooleanValue, string.Empty);

            var s1 = nv.Get(TestSetting.StringValue, "Def1");
            var s2 = nv.Get<bool>(TestSetting.BooleanValue, false);

            /* Assert */
            s1.Should().Be("Def1");
            s2.Should().BeFalse();
        }

        [TestMethod]
        public void Removing_a_setting_should_work()
        {
            /* Arrange */
            var nv = new NameValueSettings();

            /* Act */
            nv.SetSetting(TestSetting.StringValue, "SomeVal");
            nv.SetSetting(TestSetting.BooleanValue, true);

            nv.RemoveSetting(TestSetting.StringValue);
            nv.RemoveSetting((Enum)null);
            nv.RemoveSetting((string)null);

            var s1 = nv.Get(TestSetting.StringValue, "Def1");
            var s2 = nv.Get<bool>(TestSetting.BooleanValue, false);

            /* Assert */
            s1.Should().Be("Def1");
            s2.Should().BeTrue();
        }

        [TestMethod]
        public void Get_and_set_and_remove_description_Should_work()
        {
            /* Arrange */
            var nv = new NameValueSettings();
            string expectedDescription = "this is a description";

            /* Act */
            nv.SetDescription(TestSetting.GuidValue, expectedDescription);
            nv.SetDescription(TestSetting.TimeSpanValue, expectedDescription);
            nv.RemoveDescription(TestSetting.TimeSpanValue);
            nv.RemoveDescription((Enum)null);
            nv.RemoveDescription((string)null);

            string desc1 = nv.GetDescription(TestSetting.GuidValue);
            string desc2 = nv.GetDescription(TestSetting.IntegerValue);
            string desc3 = nv.GetDescription(TestSetting.TimeSpanValue);
            /* Assert */
            desc1.Should().Be(expectedDescription);
            desc2.Should().BeEmpty();
            desc3.Should().BeEmpty();
        }

        [TestMethod]
        public void Props_should_return_expected_values()
        {
            /* Arrange */
            var nv = new NameValueSettings();

            /* Act */
            nv.SetSetting(TestSetting.StringValue, "somestring");
            nv.SetSetting(TestSetting.BooleanValue, false);

            /* Assert */
            nv.Count.Should().Be(2);
            nv.RawSettings.Count.Should().Be(2);
            nv.SettingNames.Count.Should().Be(2);
        }

        [TestMethod]
        public void Getting_an_invalid_type_should_throw()
        {
            /* Arrange */
            var nv = new NameValueSettings();

            /* Act */
            nv.SetSetting(TestSetting.StringValue, "somestring");

            Action a1 = () => nv.Get<int>(TestSetting.StringValue, 0);
            Action a2 = () => nv.Get<object>(TestSetting.StringValue, null);

            /* Assert */
            a1.ShouldThrow<ConfigException>();
            a2.ShouldThrow<ConfigException>();
        }
    }
}
