namespace DeepConfig.Spec.Providers
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using DeepConfig.Providers;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MemoryConfigProviderTests
    {
        [TestMethod]
        public void Invoking_irrelevant_methods_should_do_nothing_or_throw()
        {
            /* Arrange */
            IConfigProvider p = new MemoryConfigProvider();

            /* Act */
            Action a1 = () => p.Initialize();
            Action a2 = () => p.DeleteConfig();
            Action a3 = () => p.SaveConfig(new XDocument());

            /* Assert */
            a1.ShouldNotThrow();
            a2.ShouldThrow<NotSupportedException>();
            a3.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void Properties_Should_return_expected_values()
        {
            /* Arrange */
            IConfigProvider p = new MemoryConfigProvider();

            /* Act */
            string name = p.ToString();

            /* Assert */
            p.CanDelete.Should().BeFalse();
            p.IsReadOnly.Should().BeTrue();
            p.ModifiedSinceLoad.Should().BeFalse();

            name.Should().Be(typeof(MemoryConfigProvider).Name);
        }

        [TestMethod]
        public void Source_dentifier_should_return_correct_value()
        {
            /* Arrange / Act */
            var p = new MemoryConfigProvider("<xml />");

            /* Assert */
            p.SourceIdentifier.Should().Be("<xml />");
        }

        [TestMethod]
        public void Loading_empty_should_return_an_empty_document()
        {
            /* Arrange */
            IConfigProvider p = new MemoryConfigProvider();

            /* Act */
            var docs = p.LoadConfig();
            var doc = docs.FirstOrDefault();

            /* Assert */
            docs.Should().HaveCount(1);
            doc.Root.Elements().Should().BeEmpty();
        }

        [TestMethod]
        public void Loading_xml_should_work()
        {
            /* Arrange */
            var expected = TestConfigFactory.CreateConfig("Section1", "Section2", "Section3");

            IConfigProvider p = new MemoryConfigProvider(expected.Declaration.ToString() + expected.ToString(SaveOptions.DisableFormatting));

            /* Act */
            var docs = p.LoadConfig();
            var doc = docs.FirstOrDefault();

            /* Assert */
            docs.Should().HaveCount(1);
            doc.Root.Element("Section1").Should().NotBeNull();
            doc.Root.Element("Section2").Should().NotBeNull();
            doc.Root.Element("Section3").Should().NotBeNull();
        }
    }
}
