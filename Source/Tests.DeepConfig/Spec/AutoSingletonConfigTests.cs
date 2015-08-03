namespace DeepConfig.Spec
{
    using System;
    using DeepConfig.Providers;
    using DeepConfig.TestTypes;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AutoSingletonConfigTests
    {
        [TestInitialize]
        public void Init()
        {
            ConfigMaster.ProviderMapping.UnmapDefaultProvider();
            ConfigMaster.RevokeInstance<SingletonAutoType>();
        }

        [TestMethod]
        public void Getting_a_singleton_that_does_not_exists_should_behave_accordingly()
        {
            /* Act */
            var existing = ConfigMaster.GetSettings<SingletonAutoType>();

            var settings = SingletonAutoType.Instance;

            /* Assert */
            existing.Should().BeNull();
            settings.Should().NotBeNull();
        }

        [TestMethod]
        public void Getting_an_existing_singleton_should_work()
        {
            /* Arrange */
            var settingSource = new SingletonAutoType();

            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            ConfigMaster.ProviderMapping.MapDefaultProvider(p);

            ConfigMaster.SetSettings(settingSource);

            /* Act */
            var settings = SingletonAutoType.Instance;

            /* Assert */
            settings.Should().BeSameAs(settingSource);
        }

        [TestMethod]
        public void Reloading_should_refresh_the_singleton_instance()
        {
            /* Act */
            var settings1 = SingletonAutoType.Instance;

            SingletonAutoType.Reload();

            var settings2 = SingletonAutoType.Instance;

            /* Assert */
            settings1.Should().NotBeSameAs(settings2);
        }
    }
}
