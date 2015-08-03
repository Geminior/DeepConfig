namespace DeepConfig.Spec.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using DeepConfig.Providers;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MultiFileConfigProviderTests
    {
        [TestMethod]
        public void Ctor_null_arg_should_throw()
        {
            /* Arrange / Act */
            Action a1 = () => new MultiFileConfigProvider((string[])null);
            Action a2 = () => new MultiFileConfigProvider((IEnumerable<string>)null);

            /* Assert */
            a1.ShouldThrow<ArgumentNullException>();
            a2.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void Constant_props_should_return_expected_values()
        {
            /* Arrange */
            IConfigProvider p = new MultiFileConfigProvider("SomeFile");

            /* Assert */
            p.CanDelete.Should().BeFalse();
            p.IsReadOnly.Should().BeTrue();
        }

        [TestMethod]
        public void Source_dentifier_should_return_correct_value()
        {
            /* Arrange / Act */
            string[] fileNames = new[] { "SomeFile.xml", "SomeOtherFile.xml" };
            string expectedId = string.Join("_", fileNames);

            var p = new MultiFileConfigProvider(fileNames);

            /* Assert */
            p.SourceIdentifier.Should().Be(expectedId);
        }

        [TestMethod]
        public void Unsupported_methods_should_throw()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();
            IConfigProvider p = new MultiFileConfigProvider("SomeFile");

            /* Act */
            Action a1 = () => p.SaveConfig(doc);
            Action a2 = () => p.DeleteConfig();

            /* Assert */
            a1.ShouldThrow<NotSupportedException>();
            a2.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void Loading_files_should_work()
        {
            /* Arrange */
            string fileOne = "testfileone.xml";
            string fileTwo = "testfiletwo.xml";
            string fileTthree = "testfilethree.xml";
            TestConfigFactory.CreateXmlFile(fileOne, "<configuration><section1 /></configuration>", false);
            TestConfigFactory.CreateXmlFile(fileTwo, "<configuration><section2 /></configuration>", true);

            IConfigProvider p1 = new MultiFileConfigProvider(fileOne, fileTthree, fileTwo);
            IConfigProvider p2 = new MultiFileConfigProvider(new List<string> { fileOne, fileTthree, fileTwo });

            /* Act */
            p1.Initialize();
            var docs1 = p1.LoadConfig().ToList();
            var docs2 = p2.LoadConfig().ToList();

            /* Assert */
            docs1.Should().HaveCount(2);
            docs1[0].Should().HaveElement("section1");
            docs1[1].Should().HaveElement("section2");

            docs2.Should().HaveCount(2);
            docs2[0].Should().HaveElement("section1");
            docs2[1].Should().HaveElement("section2");

            p1.ModifiedSinceLoad.Should().BeFalse();
        }

        [TestMethod]
        public void Modified_since_load_should_work_as_expected()
        {
            /* Arrange */
            string fileOne = "testfilemultieditone.xml";
            string fileTwo = "testfilemultiedittwo.xml";

            TestConfigFactory.CreateXmlFile(fileOne, "<configuration><section1 /></configuration>", false);
            TestConfigFactory.CreateXmlFile(fileTwo, "<configuration><section1 /></configuration>", false);

            var p = new MultiFileConfigProvider(fileOne, fileTwo);

            /* Act */
            var docs = p.LoadConfig().ToList();

            Thread.Sleep(10);
            TestConfigFactory.CreateXmlFile(fileTwo, "<configuration><section2 /></configuration>", false);

            /* Assert */
            p.ModifiedSinceLoad.Should().BeTrue();
        }
    }
}
