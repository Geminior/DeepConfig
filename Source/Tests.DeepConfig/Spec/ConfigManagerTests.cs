namespace DeepConfig.Spec
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig;
    using DeepConfig.Cryptography;
    using DeepConfig.Providers;
    using DeepConfig.TestTypes;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit test for the ConfigManager class
    /// </summary>
    [TestClass]
    public class ConfigManagerTests
    {
        public enum TestSection
        {
            SectionOne,
            SectionTwo
        }

        [TestMethod]
        public void Getting_name_value_sections_should_work()
        {
            /* Arrange */
            var doc = XDocument.Load("TestXml\\MgrConfigWithNameValue.xml");

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            var mgr = ConfigManager.Create(p);

            var appSettings = mgr.GetAppSettings();
            var basicSettings = mgr.GetSettings("SectionBasic") as NameValueSettings;
            var extendedSettings = mgr.GetSettings<NameValueSettings>("SectionExtended");

            /* Assert */
            appSettings.Should().NotBeNull();
            appSettings.HasSetting("AppSetting1").Should().BeTrue();
            basicSettings.Should().NotBeNull();
            extendedSettings.Should().NotBeNull();
        }

        [TestMethod]
        public void Basic_initialization_should_work_as_expected()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            var mgr = ConfigManager.Create(p);

            /* Assert */
            mgr.ConfigProvider.Should().BeSameAs(p);
            mgr.CryptoProvider.Should().NotBeNull();
            mgr.Sections.Should().BeEmpty();
            mgr.SupportedSections.Should().HaveCount(1); //AppSettings
            mgr.ModifiedSinceLoad.Should().BeFalse();
        }

        [TestMethod]
        public void Provider_returning_null_should_result_in_an_empty_manager()
        {
            /* Arrange */
            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(null);

            /* Act */
            var mgr = ConfigManager.Create(p);

            /* Assert */
            mgr.ConfigProvider.Should().BeSameAs(p);
            mgr.CryptoProvider.Should().NotBeNull();
            mgr.Sections.Should().BeEmpty();
        }

        [TestMethod]
        public void Setting_and_getting_appsettings_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingsOne = new NameValueCollection();
            settingsOne.Add("A", "ValueA");

            var settingsTwo = new NameValueSettings();
            settingsTwo.SetSetting("B", "ValueB");

            /* Act */
            var mgr = ConfigManager.Create(p);

            var preSet = mgr.GetAppSettings();

            mgr.SetAppSettings(settingsOne);
            mgr.SetAppSettings(settingsTwo);

            var postSet = mgr.GetAppSettings();

            mgr.Reload();

            var postReload = mgr.GetAppSettings();

            int sectionCount = mgr.Sections.Count();

            /* Assert */
            A.CallTo(() => p.SaveConfig(doc)).MustNotHaveHappened();

            preSet.Should().NotBeNull();
            preSet.Get("A", null).Should().BeNull();

            postSet.Should().NotBeNull();
            postSet.Should().NotBeSameAs(settingsTwo);
            postSet.Get("B", null).Should().Be("ValueB");

            postReload.Should().NotBeNull();
            postReload.Get("B", null).Should().Be("ValueB");

            sectionCount.Should().Be(1);
        }

        [TestMethod]
        public void Setting_and_getting_name_value_settings_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingsOne = new NameValueCollection();
            settingsOne.Add("A", "ValueA");

            var settingsTwo = new NameValueSettings();
            settingsTwo.SetSetting("B", "ValueB");

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.SetSettings(TestSection.SectionOne, settingsOne);
            mgr.SetSettings(TestSection.SectionTwo, settingsTwo);

            var readSectionOne = mgr.GetSettings<NameValueSettings>(TestSection.SectionOne);
            var readSectionTwo = mgr.GetSettings<NameValueSettings>(TestSection.SectionTwo);

            mgr.Reload();

            var readSectionOneAfterReload = mgr.GetSettings<NameValueSettings>(TestSection.SectionOne);
            var readSectionTwoAfterReload = mgr.GetSettings<NameValueSettings>(TestSection.SectionTwo);

            int sectionCount = mgr.Sections.Count();

            /* Assert */
            A.CallTo(() => p.SaveConfig(doc)).MustNotHaveHappened();

            readSectionOne.Should().NotBeNull();
            readSectionOne.Get("A", null).Should().Be("ValueA");

            readSectionTwo.Should().NotBeNull();
            readSectionTwo.Should().NotBeSameAs(settingsTwo);
            readSectionTwo.Get("B", null).Should().Be("ValueB");

            readSectionOneAfterReload.Should().NotBeNull();
            readSectionOneAfterReload.Get("A", null).Should().Be("ValueA");

            readSectionTwoAfterReload.Should().NotBeNull();
            readSectionTwoAfterReload.Get("B", null).Should().Be("ValueB");

            sectionCount.Should().Be(2);
        }

        [TestMethod]
        public void Setting_and_getting_complex_settings_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingsOne = new ConfigWithBasicProps
            {
                StringValue = "One"
            };

            var settingsTwo = new ConfigWithBasicProps
            {
                StringValue = "Two"
            };

            var settingsThree = new ConfigComplex
            {
                Name = "Three"
            };

            /* Act */
            var mgr = ConfigManager.Create(p);
            mgr.MonitorEvents();

            mgr.SetSettings(TestSection.SectionOne, settingsOne);
            mgr.SetSettings(settingsTwo);
            mgr.SetSettings(settingsThree);

            var readSectionOne = mgr.GetSettings<ConfigWithBasicProps>(TestSection.SectionOne);
            var readSectionTwo = mgr.GetSettings<ConfigWithBasicProps>();
            var allBasics = mgr.GetSettingsList<ConfigWithBasicProps>();

            mgr.Reload();

            var readSectionOneAfterReload = mgr.GetSettings<ConfigWithBasicProps>(TestSection.SectionOne);
            var readSectionTwoAfterReload = mgr.GetSettings<ConfigWithBasicProps>();

            int sectionCount = mgr.Sections.Count();

            /* Assert */
            A.CallTo(() => p.SaveConfig(doc)).MustNotHaveHappened();

            readSectionOne.Should().NotBeNull();
            readSectionOne.Should().NotBeSameAs(settingsOne);
            readSectionOne.ShouldBeEquivalentTo(settingsOne);

            readSectionTwo.Should().NotBeNull();
            readSectionTwo.Should().NotBeSameAs(settingsTwo);
            readSectionTwo.ShouldBeEquivalentTo(settingsTwo);

            readSectionOneAfterReload.Should().NotBeNull();
            readSectionOneAfterReload.ShouldBeEquivalentTo(settingsOne);

            readSectionTwoAfterReload.Should().NotBeNull();
            readSectionTwoAfterReload.ShouldBeEquivalentTo(settingsTwo);

            allBasics.Count.Should().Be(2);
            sectionCount.Should().Be(3);

            mgr.ShouldRaise("ConfigChanged").WithArgs<ConfigChangedEventArgs>(e => e.SectionName == "SectionOne" && e.Cause == ConfigChangedEventArgs.ChangeCause.SectionChanged && e.SectionType == typeof(ConfigWithBasicProps));
            mgr.ShouldRaise("ConfigChanged").WithArgs<ConfigChangedEventArgs>(e => e.SectionName == "ConfigWithBasicProps" && e.Cause == ConfigChangedEventArgs.ChangeCause.SectionChanged);
            mgr.ShouldRaise("ConfigChanged").WithArgs<ConfigChangedEventArgs>(e => e.SectionName == "ConfigComplex" && e.Cause == ConfigChangedEventArgs.ChangeCause.SectionChanged);
        }

        [TestMethod]
        public void Setting_a_section_multiple_times_should_only_set_the_last_value()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingsOne = new ConfigWithBasicProps
            {
                StringValue = "One"
            };

            var settingsTwo = new ConfigWithBasicProps
            {
                StringValue = "Two"
            };

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.SetSettings(TestSection.SectionOne, settingsOne);
            mgr.SetSettings(TestSection.SectionOne, settingsOne);
            mgr.SetSettings(TestSection.SectionOne, settingsTwo);

            var readSectionOne = mgr.GetSettings<ConfigWithBasicProps>(TestSection.SectionOne);

            int sectionCount = mgr.Sections.Count();

            /* Assert */
            A.CallTo(() => p.SaveConfig(doc)).MustNotHaveHappened();

            readSectionOne.Should().NotBeNull();
            readSectionOne.ShouldBeEquivalentTo(settingsTwo);

            sectionCount.Should().Be(1);
        }

        [TestMethod]
        public void Section_removal_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingsOne = new ConfigWithBasicProps
            {
                StringValue = "One"
            };

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.MonitorEvents();

            mgr.SetSettings(TestSection.SectionOne, settingsOne);
            mgr.SetSettings(TestSection.SectionTwo, settingsOne);

            bool existsPre = mgr.HasSection(TestSection.SectionOne);

            mgr.RemoveSection(TestSection.SectionOne);
            mgr.RemoveSection("NoSuchSection");
            mgr.RemoveSection((Enum)null);
            mgr.RemoveSection(string.Empty);

            bool existsPost = mgr.HasSection(TestSection.SectionOne);

            var readSectionOne = mgr.GetSettings<ConfigWithBasicProps>(TestSection.SectionOne);

            int sectionCount = mgr.Sections.Count();

            /* Assert */
            A.CallTo(() => p.SaveConfig(doc)).MustNotHaveHappened();

            readSectionOne.Should().BeNull();

            existsPre.Should().BeTrue();
            existsPost.Should().BeFalse();
            sectionCount.Should().Be(1);

            mgr.ShouldRaise("ConfigChanged").WithArgs<ConfigChangedEventArgs>(e => e.SectionName == "SectionOne" && e.Cause == ConfigChangedEventArgs.ChangeCause.SectionRemoved);
        }

        [TestMethod]
        public void Section_removal_should_not_raise_event_if_section_does_not_exist()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.MonitorEvents();

            mgr.RemoveSection(TestSection.SectionOne);

            /* Assert */
            mgr.ShouldNotRaise("ConfigChanged");
        }

        [TestMethod]
        public void Has_section_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingsOne = new ConfigWithBasicProps
            {
                StringValue = "One"
            };

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.SetSettings(settingsOne);

            bool exists = mgr.HasSection<ConfigWithBasicProps>();

            /* Assert */
            exists.Should().BeTrue();
        }

        [TestMethod]
        public void Setting_an_invalid_setting_should_fail_appropriately()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var invalidSetting = new object();

            /* Act */
            var mgr = ConfigManager.Create(p);
            mgr.MonitorEvents();

            Action a = () => mgr.SetSettings(TestSection.SectionOne, invalidSetting);

            /* Assert */
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.MissingConfigSectionAttribute);
            mgr.ShouldNotRaise("ConfigChanged");
        }

        [TestMethod]
        public void An_invalid_doc_should_fail_appropriately()
        {
            /* Arrange */
            var doc1 = new XDocument();
            var doc2 = new XDocument(new XElement("InvalidRoot"));

            var p1 = A.Fake<IConfigProvider>();
            A.CallTo(() => p1.LoadConfig()).Returns(new[] { doc1 });

            var p2 = A.Fake<IConfigProvider>();
            A.CallTo(() => p2.LoadConfig()).Returns(new[] { doc2 });

            /* Act */
            Action a1 = () => ConfigManager.Create(p1);
            Action a2 = () => ConfigManager.Create(p2);

            /* Assert */
            a1.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigurationFile);
            a2.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigurationFile);
        }

        [TestMethod]
        public void Handler_errors_should_fail_appropriately()
        {
            /* Arrange */
            var doc = XDocument.Load("TestXml\\MgrConfigWithErrors.xml");

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            var mgr = ConfigManager.Create(p);

            Action a1 = () => mgr.GetSettings("SectionInvalidHandler");
            Action a2 = () => mgr.GetSettings("SectionHandlerThrowsOnRead");
            Action a3 = () => mgr.GetSettings("SectionHandlerErrorsOnRead");

            Action a4 = () => mgr.SetSettings("SectionHandlerThrowsOnWrite", new ConfigWithCustomHandler { ThrowOnWrite = true });
            Action a5 = () => mgr.SetSettings("SectionHandlerErrorsOnWrite", new ConfigWithCustomHandler { HasErrorOnWrite = true });

            /* Assert */
            a1.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.TypeResolutionFailed);
            a2.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.SectionHandlerMismatch);
            a3.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigType);

            a4.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.SectionHandlerMismatch);
            a5.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigType);
        }

        [TestMethod]
        public void Persisting_changes_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.PersistChanges();

            /* Assert */
            A.CallTo(() => p.SaveConfig(doc)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestMethod]
        public void If_provider_fails_to_persist_it_should_fail_appropriately()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });
            A.CallTo(() => p.SaveConfig(doc)).Throws(new Exception());

            /* Act */
            var mgr = ConfigManager.Create(p);

            Action a = () => mgr.PersistChanges();

            /* Assert */
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.FailedToSave);
        }

        [TestMethod]
        public void Persiting_to_new_provider_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var pnew = A.Fake<IConfigProvider>();

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.PersistTo(pnew);

            /* Assert */
            A.CallTo(() => pnew.Initialize()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => pnew.SaveConfig(doc)).MustHaveHappened(Repeated.Exactly.Once);
            mgr.ConfigProvider.Should().BeSameAs(pnew);
        }

        [TestMethod]
        public void Provider_failure_should_fail_with_config_exception()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p1 = A.Fake<IConfigProvider>();
            A.CallTo(() => p1.LoadConfig()).Throws(new XmlException());

            var p2 = A.Fake<IConfigProvider>();
            A.CallTo(() => p2.LoadConfig()).Throws(new Exception());

            /* Act */
            Action a1 = () => ConfigManager.Create(p1);
            Action a2 = () => ConfigManager.Create(p2);

            /* Assert */
            a1.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigurationFile);
            a2.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.FailedToLoad);
        }

        [TestMethod]
        public void Setting_the_cryptoprovider_to_the_same_type_should_do_nothing()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            var mgr = ConfigManager.Create(p);
            var curCrypto = mgr.CryptoProvider;

            mgr.SetCryptoProvider(typeof(AesCryptoProvider));

            /* Assert */
            mgr.CryptoProvider.Should().BeSameAs(curCrypto);
        }

        [TestMethod]
        public void Setting_the_cryptoprovider_to_an_invalid_type_should_fail()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            /* Act */
            var mgr = ConfigManager.Create(p);

            Action a = () => mgr.SetCryptoProvider(typeof(object));

            /* Assert */
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.ConfigTypeCreationFailed);
        }

        [TestMethod]
        public void Setting_the_cryptoprovider_that_causes_failure_should_restore_the_original_provider()
        {
            /* Arrange */
            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).ReturnsLazily(_ => new[] { TestConfigFactory.CreateConfig() });

            var cryptoNew = A.Fake<ICryptoProvider>();
            A.CallTo(() => cryptoNew.Encrypt(A<string>.Ignored)).Throws(new Exception());

            var settingOne = new ConfigComplex
            {
                Encrypted = "One"
            };

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.SetSettings(TestSection.SectionOne, settingOne);

            Action a = () => mgr.SetCryptoProvider(cryptoNew);

            /* Assert */
            a.ShouldThrow<Exception>();
            mgr.CryptoProvider.Should().BeOfType<AesCryptoProvider>();
        }

        [TestMethod]
        public void Setting_the_cryptoprovider_should_work_as_expected()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingOne = new ConfigComplex
            {
                Encrypted = "One"
            };

            var settingTwo = new ConfigComplex
            {
                Encrypted = "Two"
            };

            var cryptoNew = A.Fake<ICryptoProvider>();
            A.CallTo(() => cryptoNew.Encrypt(A<string>.Ignored)).ReturnsLazily(h => h.Arguments[0].ToString());

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.SetSettings(TestSection.SectionOne, settingOne);
            mgr.SetSettings(TestSection.SectionTwo, settingTwo);

            mgr.SetCryptoProvider(cryptoNew);

            /* Assert */
            A.CallTo(() => cryptoNew.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
        }

        [TestMethod]
        public void Setting_the_cryptoprovider_by_type_should_work_as_expected()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();

            var p = A.Fake<IConfigProvider>();
            A.CallTo(() => p.LoadConfig()).Returns(new[] { doc });

            var settingOne = new ConfigComplex
            {
                Encrypted = "One"
            };

            /* Act */
            var mgr = ConfigManager.Create(p);

            mgr.SetSettings(TestSection.SectionOne, settingOne);

            mgr.SetCryptoProvider(typeof(CryptoCustom));

            mgr.Reload();

            /* Assert */
            mgr.CryptoProvider.Should().BeOfType<CryptoCustom>();
        }
    }
}
