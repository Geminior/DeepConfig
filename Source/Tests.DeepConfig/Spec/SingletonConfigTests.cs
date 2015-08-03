namespace DeepConfig.Spec
{
    using System;
    using DeepConfig.Providers;
    using DeepConfig.TestTypes;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SingletonConfigTests
    {
        [TestMethod]
        public void Getting_a_singleton_that_does_not_exists_should_behave_accordingly()
        {
            /* Arrange */
            var defaultVal = new SingletonType();

            /* Act */
            var settings = SingletonType.TryGetInstance(() => defaultVal, false);

            Action a = () => settings = SingletonType.Instance;

            /* Assert */
            SingletonType.Exists.Should().BeFalse();
            settings.Should().Be(defaultVal);
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.ConfigurationNotFound);
        }

        [TestMethod]
        public void Getting_a_singleton_that_does_not_exists_should_behave_accordingly_take_two()
        {
            /* Arrange */
            var defaultVal = new SingletonType();

            /* Act */
            var settings = SingletonType.TryGetInstance(() => defaultVal, true);

            var settings2 = SingletonType.Instance;

            /* Assert */
            SingletonType.Exists.Should().BeTrue();
            settings.Should().Be(defaultVal);
            settings2.Should().Be(defaultVal);
        }

        [TestMethod]
        public void Getting_an_existing_singleton_should_work()
        {
            /* Arrange */
            var settingSource = new SingletonType();

            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            ConfigMaster.ProviderMapping.MapDefaultProvider(p);

            ConfigMaster.SetSettings(settingSource);

            /* Act */
            var settings1 = SingletonType.TryGetInstance(() => null, false);
            var settings2 = SingletonType.Instance;

            /* Assert */
            SingletonType.Exists.Should().BeTrue();
            settings1.Should().BeSameAs(settingSource);
            settings2.Should().BeSameAs(settingSource);
        }

        [TestMethod]
        public void Reloading_should_refresh_the_singleton_instance()
        {
            /* Arrange */
            var settingSource = new SingletonType { Name = "I am single" };

            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            ConfigMaster.ProviderMapping.MapDefaultProvider(p);

            ConfigMaster.SetSettings(settingSource);

            /* Act */
            var settings1 = SingletonType.Instance;

            SingletonType.Reload();

            var settings2 = SingletonType.Instance;

            /* Assert */
            SingletonType.Exists.Should().BeTrue();
            settings1.Should().BeSameAs(settingSource);
            settings2.Should().NotBeSameAs(settingSource);
            settings2.ShouldBeEquivalentTo(settingSource);
        }
    }
}
