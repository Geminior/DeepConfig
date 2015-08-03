namespace DeepConfig.Spec
{
    using System.Collections.Generic;
    using DeepConfig.Providers;
    using DeepConfig.TestTypes;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigMasterTests
    {
        private List<ConfigChangedEventArgs> _raisedEvents = new List<ConfigChangedEventArgs>();

        [TestInitialize]
        public void Init()
        {
            ConfigMaster.ConfigChanged += ChangeHandler;
        }

        [TestCleanup]
        public void Cleanup()
        {
            ((ProviderMapping)ConfigMaster.ProviderMapping).UnmapAll();

            _raisedEvents.Clear();

            ConfigMaster.ConfigChanged -= ChangeHandler;
        }

        [TestMethod]
        public void Getting_and_setting_using_a_default_mapping_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            ConfigMaster.ProviderMapping.MapDefaultProvider(p);
            var settingsPre = ConfigMaster.GetSettings<ConfigComplex>();

            ConfigMaster.SetSettings(new ConfigComplex());
            var settingsPost = ConfigMaster.GetSettings<ConfigComplex>();

            var mgr = ConfigMaster.GetDefaultConfig();

            /* Assert */
            settingsPre.Should().BeNull();
            settingsPost.Should().NotBeNull();
            mgr.HasSection<ConfigComplex>().Should().BeTrue();
            A.CallTo(() => p.SaveConfig(doc)).MustHaveHappened(Repeated.Exactly.Once);

            _raisedEvents.Should().HaveCount(1);
            _raisedEvents.Should().ContainSingle(e => e.Cause == ConfigChangedEventArgs.ChangeCause.SectionChanged);
        }

        [TestMethod]
        public void Getting_and_setting_using_a_specific_mapping_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            ConfigMaster.ProviderMapping.Map<ConfigComplex>().To(p);
            var settingsPre = ConfigMaster.GetSettings<ConfigComplex>();

            ConfigMaster.SetSettings(new ConfigComplex());
            var settingsPost = ConfigMaster.GetSettings<ConfigComplex>();

            var mgr = ConfigMaster.GetConfig<ConfigComplex>();
            var mgrDefault = ConfigMaster.GetDefaultConfig();

            /* Assert */
            settingsPre.Should().BeNull();
            settingsPost.Should().NotBeNull();
            mgr.HasSection<ConfigComplex>().Should().BeTrue();
            mgrDefault.HasSection<ConfigComplex>().Should().BeFalse();
            A.CallTo(() => p.SaveConfig(doc)).MustHaveHappened(Repeated.Exactly.Once);

            _raisedEvents.Should().HaveCount(1);
            _raisedEvents.Should().ContainSingle(e => e.Cause == ConfigChangedEventArgs.ChangeCause.SectionChanged);
        }

        [TestMethod]
        public void Setting_and_then_getting_a_setting_should_return_the_same_instance()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingSource = new ConfigComplex();

            /* Act */
            ConfigMaster.ProviderMapping.Map<ConfigComplex>().To(p);
            ConfigMaster.SetSettings(settingSource);

            var settings = ConfigMaster.GetSettings<ConfigComplex>();

            /* Assert */
            settings.Should().BeSameAs(settingSource);
        }

        [TestMethod]
        public void Getting_a_setting_multiple_times_should_return_the_same_instance()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingSource = new ConfigComplex();

            /* Act */
            ConfigMaster.ProviderMapping.Map<ConfigComplex>().To(p);

            //Set the setings through the manager, to mimic loading from file
            var mgr = ConfigMaster.GetConfig(p);
            mgr.SetSettings(settingSource);

            var settingsFirst = ConfigMaster.GetSettings<ConfigComplex>();
            var settingsSecond = ConfigMaster.GetSettings<ConfigComplex>();

            /* Assert */
            settingsFirst.Should().NotBeSameAs(settingSource);
            settingsSecond.Should().BeSameAs(settingsFirst);
        }

        [TestMethod]
        public void File_provider_shortcut_should_work()
        {
            /* Arrange / Act */
            var mgr = ConfigMaster.GetConfig("MyConfig.config");

            /* Assert */
            mgr.ConfigProvider.Should().BeOfType<FileConfigProvider>();
        }

        [TestMethod]
        public void Redirects_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            ConfigMaster.ProviderMapping.MapDefaultProvider(p);

            var redir = new ConfigurationRedirect
            {
                ActualProvider = new FileConfigProvider("MyRedirected.config")
            };

            ConfigMaster.SetSettings<ConfigurationRedirect>(redir);

            /* Act */
            var mgr1 = ConfigMaster.GetConfig(p);
            var mgr2 = ConfigMaster.GetConfig(p, true);

            /* Assert */
            mgr1.ConfigProvider.Should().BeOfType<FileConfigProvider>();
            mgr2.ConfigProvider.Should().Be(p);
        }

        [TestMethod]
        public void Setting_or_removing_settings_via_a_manager_should_also_raise_static_event()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            ConfigMaster.ProviderMapping.MapDefaultProvider(p);

            /* Act */
            var mgr = ConfigMaster.GetDefaultConfig();

            mgr.SetSettings(new ConfigComplex());
            mgr.RemoveSection<ConfigComplex>();

            /* Assert */
            _raisedEvents.Should().HaveCount(2);
            _raisedEvents.Should().ContainSingle(e => e.Cause == ConfigChangedEventArgs.ChangeCause.SectionChanged);
            _raisedEvents.Should().ContainSingle(e => e.Cause == ConfigChangedEventArgs.ChangeCause.SectionRemoved);
        }

        [TestMethod]
        public void Changing_default_mappings_of_loaded_configs_should_raise_event()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p1 = A.Fake<IConfigProvider>();
            A.CallTo(() => p1.LoadConfig()).Returns(new[] { doc });

            var p2 = A.Fake<IConfigProvider>();
            A.CallTo(() => p2.LoadConfig()).Returns(new[] { doc });

            ConfigMaster.ProviderMapping.MapDefaultProvider(p1);

            /* Act */
            ConfigMaster.SetSettings(new ConfigComplex());

            ConfigMaster.ProviderMapping.MapDefaultProvider(p2);

            /* Assert */
            _raisedEvents.Should().HaveCount(2);
            _raisedEvents.Should().ContainSingle(e => e.Cause == ConfigChangedEventArgs.ChangeCause.MappingChanged);
        }

        [TestMethod]
        public void Changing_mappings_of_loaded_configs_should_raise_event()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p1 = A.Fake<IConfigProvider>();
            A.CallTo(() => p1.LoadConfig()).Returns(new[] { doc });

            var p2 = A.Fake<IConfigProvider>();
            A.CallTo(() => p2.LoadConfig()).Returns(new[] { doc });

            ConfigMaster.ProviderMapping.MapDefaultProvider(p1);

            /* Act */
            ConfigMaster.SetSettings(new ConfigComplex());

            ConfigMaster.ProviderMapping.Map<ConfigComplex>().To(p2);

            /* Assert */
            _raisedEvents.Should().HaveCount(2);
            _raisedEvents.Should().ContainSingle(e => e.Cause == ConfigChangedEventArgs.ChangeCause.MappingChanged);
        }

        private void ChangeHandler(object sender, ConfigChangedEventArgs e)
        {
            _raisedEvents.Add(e);
        }
    }
}
