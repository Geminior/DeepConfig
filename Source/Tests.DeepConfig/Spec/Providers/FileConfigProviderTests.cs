namespace DeepConfig.Spec.Providers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig.Core;
    using DeepConfig.Providers;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FileConfigProviderTests
    {
        [TestMethod]
        public void Accessing_properties_on_a_non_existant_file_should_work_returning_expected_defaults()
        {
            /* Arrange */
            string fileName = "testnoexist.xml";
            var p = new FileConfigProvider(fileName);

            /* Act */
            string str = p.ToString();

            /* Assert */
            p.CanDelete.Should().BeTrue();
            p.ConfigFileName.Should().Be(fileName);
            p.IsReadOnly.Should().BeFalse();
            p.ModifiedSinceLoad.Should().BeFalse();
            str.Should().Be(fileName);
        }

        [TestMethod]
        public void Source_dentifier_should_return_correct_value()
        {
            /* Arrange / Act */
            string fileName = "SomeFile.xml";
            var p = new FileConfigProvider(fileName);

            /* Assert */
            p.SourceIdentifier.Should().Be(fileName);
        }

        [TestMethod]
        public void Accessing_a_readonly_file_should_work_as_expected()
        {
            /* Arrange */
            var doc = new XDocument();
            string fileName = "testro.xml";
            string newFileName = "testronew.xml";
            TestConfigFactory.CreateXmlFile(fileName, "<configuration />", true);
            TestConfigFactory.CreateXmlFile(newFileName, "<configuration />", true);

            var p = new FileConfigProvider(fileName);

            /* Act */
            Action a1 = () => p.DeleteConfig();
            Action a2 = () => p.SaveConfig(doc);

            /* Assert */
            p.CanDelete.Should().BeFalse();
            p.IsReadOnly.Should().BeTrue();
            p.ModifiedSinceLoad.Should().BeFalse();

            a1.ShouldThrow<InvalidOperationException>();
            a2.ShouldThrow<UnauthorizedAccessException>();
        }

        [TestMethod]
        public void Creating_a_provider_with_a_null_or_empty_fileName_should_fail()
        {
            /* Arrange */
            IConfigProvider p;

            /* Act */
            Action a1 = () => p = new FileConfigProvider(null);
            Action a2 = () => p = new FileConfigProvider(string.Empty);

            /* Assert */
            a1.ShouldThrow<ArgumentException>();
            a2.ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void Deleting_a_writable_file_should_wok()
        {
            /* Arrange */
            string fileName = "testdel.xml";
            TestConfigFactory.CreateXmlFile(fileName, "<configuration />", false);

            var p = new FileConfigProvider(fileName);

            /* Act */
            bool existsPre = File.Exists(fileName);
            p.DeleteConfig();
            bool existsPost = File.Exists(fileName);

            /* Assert */
            existsPre.Should().BeTrue();
            existsPost.Should().BeFalse();
        }

        [TestMethod]
        public void Loading_and_saving_a_file_should_work()
        {
            /* Arrange */
            var fileName = "testexisting.xml";
            TestConfigFactory.CreateXmlFile(fileName, "<configuration><section1 /></configuration>", false);
            var p = new FileConfigProvider(fileName);

            /* Act */
            Action a1 = () =>
            {
                //Calling init for code coverage
                p.Initialize();
                var doc = p.LoadConfig().First();
                doc.Root.FirstNode.Remove();
                p.SaveConfig(doc);
            };

            Action a2 = () => p.SaveConfig(null);

            /* Assert */
            a1.ShouldNotThrow();
            a2.ShouldNotThrow();
            p.ModifiedSinceLoad.Should().BeFalse();
        }

        [TestMethod]
        public void Loading_and_saving_a_new_file_should_work()
        {
            /* Arrange */
            var fileName = "testnew.xml";
            File.Delete(fileName);
            var p = new FileConfigProvider(fileName);

            /* Act */
            var existsPre = File.Exists(fileName);
            var doc = p.LoadConfig().First();
            p.SaveConfig(doc);
            var existsPost = File.Exists(fileName);

            /* Assert */
            existsPre.Should().BeFalse();
            existsPost.Should().BeTrue();
            doc.Root.Name.LocalName.Should().Be(ConfigElement.RootNode);
        }

        [TestMethod]
        public void Loading_invalid_xml_should_fail()
        {
            /* Arrange */
            var fileName = "testfail.xml";
            TestConfigFactory.CreateXmlFile(fileName, "garblegarble", false);
            var p = new FileConfigProvider(fileName);

            /* Act */
            Action a1 = () => p.LoadConfig().First();

            /* Assert */
            a1.ShouldThrow<XmlException>();
        }

        [TestMethod]
        public void Modified_since_load_should_work_as_expected()
        {
            /* Arrange */
            var fileName = "testmultiedit.xml";
            TestConfigFactory.CreateXmlFile(fileName, "<configuration><section1 /></configuration>", false);
            var p1 = new FileConfigProvider(fileName);
            var p2 = new FileConfigProvider(fileName);

            /* Act */
            var doc1 = p1.LoadConfig().First();
            var doc2 = p2.LoadConfig().First();

            Thread.Sleep(10);
            p1.SaveConfig(doc1);

            /* Assert */
            p1.ModifiedSinceLoad.Should().BeFalse();
            p2.ModifiedSinceLoad.Should().BeTrue();
        }
    }
}
