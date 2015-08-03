namespace DeepConfig.Spec.Utilities
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig.Utilities;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class XmlUtilTests
    {
        [TestMethod]
        public void Removing_children_should_succeed()
        {
            /* Arrange */
            var root = TestConfigFactory.CreateConfig("Section1", "Section2", "Section1").Root;

            /* Act */
            root.RemoveChildren("Section1");

            var c1 = root.Element("Section1");
            var c2 = root.Element("Section2");

            /* Assert */
            c1.Should().BeNull("elements should have been removed");
            c2.Should().NotBeNull();
        }

        [TestMethod]
        public void Removing_children_should_not_remove_deeper_descendants()
        {
            /* Arrange */
            var root = TestConfigFactory.CreateConfigEx(
                new XElement("Section1", new XElement("Section2")),
                new XElement("SectionStay", new XElement("Section1"))).Root;

            /* Act */
            root.RemoveChildren("Section1");

            var c1 = root.Element("Section1");
            var c2 = root.Descendants("Section1").FirstOrDefault();

            /* Assert */
            c1.Should().BeNull("elements should have been removed");
            c2.Should().NotBeNull();
        }

        [TestMethod]
        public void Removing_children_by_predicate_should_succeed()
        {
            /* Arrange */
            var root = TestConfigFactory.CreateConfigEx(
                new XElement("Section1", new XAttribute("Value", 1)),
                new XElement("Section1", new XAttribute("Value", 2)),
                new XElement("Section1", new XAttribute("Value", 1))).Root;

            /* Act */
            root.RemoveChildren("Section1", e => e.Attribute("Value").Value == "1");

            var rest = root.Elements("Section1");

            /* Assert */
            rest.Should().HaveCount(1);
        }

        [TestMethod]
        public void Creating_an_empty_config_document_should_work()
        {
            /* Act */
            var doc = XmlUtil.CreateEmptyConfig();

            /* Assert */
            doc.Root.Should().NotBeNull();
            doc.Declaration.Encoding.Should().Be("utf-8");
            doc.Declaration.Version.Should().Be("1.0");
        }

        [TestMethod]
        public void Getting_a_documents_encoding_should_work()
        {
            /* Arrange */
            var doc1 = new XDocument();
            var doc2 = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var doc3 = new XDocument(new XDeclaration("1.0", "ascii", null));
            var doc4 = new XDocument(new XDeclaration("1.0", string.Empty, null));

            /* Act */
            var enc1 = doc1.GetEncoding(null);
            var enc2 = doc1.GetEncoding(Encoding.ASCII);
            var enc3 = doc2.GetEncoding(null);
            var enc4 = doc3.GetEncoding(null);
            var enc5 = doc4.GetEncoding(Encoding.UTF8);

            /* Assert */
            enc1.Should().BeNull();
            enc2.HeaderName.Should().Be("us-ascii");
            enc3.Should().NotBeNull();
            enc4.Should().NotBeNull();
            enc3.HeaderName.Should().Be("utf-8");
            enc4.HeaderName.Should().Be("us-ascii");
            enc5.HeaderName.Should().Be("utf-8");
        }

        [TestMethod]
        public void Getting_an_element_with_optional_creation_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig("Section1");

            /* Act */
            var e1 = doc.Element("Section1", false);
            var e2 = doc.Element("Section1", true);
            var e3 = doc.Element("Section2", false);
            var e4 = doc.Element("Section2", true);

            var sectionOnes = doc.Root.Elements("Section1");
            var sectionTwos = doc.Root.Elements("Section2");

            /* Assert */
            e1.Should().NotBeNull();
            e2.Should().NotBeNull();
            e3.Should().BeNull();
            e4.Should().NotBeNull();

            sectionOnes.Should().HaveCount(1);
            sectionTwos.Should().HaveCount(1);
        }

        [TestMethod]
        public void Getting_an_element_by_predicate_should_work()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfigEx(
                new XElement("Section1", new XAttribute("value", 1)));

            /* Act */
            var e1 = doc.Root.Element("Section1", e => e.Attribute("value").Value == "1");
            var e2 = doc.Root.Element("Section1", e => e.Attribute("value").Value == "2");

            /* Assert */
            e1.Should().NotBeNull();
            e2.Should().BeNull();
        }

        [TestMethod]
        public void Checking_for_an_attribute_value_should_work()
        {
            /* Arrange */
            var root = TestConfigFactory.CreateConfigEx(
                new XElement("Section1", new XAttribute("value", 1)),
                new XElement("Section2")).Root;

            /* Act */
            var b1 = root.Element("Section1").HasAttribute("value", "1");
            var b2 = root.Element("Section1").HasAttribute("value", "2");
            var b3 = root.Element("Section2").HasAttribute("value", "1");
            var b4 = root.Element("Section3").HasAttribute("value", "1");

            /* Assert */
            b1.Should().BeTrue();
            b2.Should().BeFalse();
            b3.Should().BeFalse();
            b4.Should().BeFalse();
        }

        [TestMethod]
        public void Getting_an_attribute_value_should_work()
        {
            /* Arrange */
            var root = TestConfigFactory.CreateConfigEx(
                new XElement("Section1", new XAttribute("value", 1)),
                new XElement("Section2")).Root;

            /* Act */
            var v1 = root.Element("Section1").GetAttributeValue("value");
            var v2 = root.Element("Section1").GetAttributeValue("type", "no");
            var v3 = root.Element("Section2").GetAttributeValue("value");
            var v4 = root.Element("Section3").GetAttributeValue("value", "no");

            /* Assert */
            v1.Should().Be("1");
            v2.Should().Be("no");
            v3.Should().BeNull();
            v4.Should().Be("no");
        }

        [TestMethod]
        public void Loading_xml_without_namespaces_on_elements_and_saving_should_work()
        {
            /* Arrange */
            var source = "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><Section1 xmlns=\"http://www.default.com\" /><Section2 xmlns=\"http://www.p.com\" /></configuration>";

            var expectedOutput = "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><Section1 xmlns=\"http://www.default.com\" /><Section2 xmlns=\"http://www.p.com\" /><Section3 /></configuration>";

            ///* Act */
            var res = XmlUtil.LoadXmlString(source, true);
            res.Root.Add(new XElement("Section3"));

            var e1 = res.Root.Element("Section1");

            var output = StripWhiteSpace(res.ToFormattedString());

            ///* Assert */
            e1.Should().NotBeNull();
            output.Should().Be(expectedOutput);
        }

        [TestMethod]
        public void Loading_xml_with_named_namespaces_should_work()
        {
            /* Arrange */
            XNamespace ns = "http://www.default.com";

            var source = "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration xmlns=\"http://www.default.com\" xmlns:p=\"http://www.p.com\" ><Section1 /><p:Section2 /></configuration>";

            ///* Act */
            var res1 = XmlUtil.LoadXmlString(source, false);
            var e1 = res1.Root.Element(ns + "Section1");

            var res2 = XmlUtil.LoadXmlString(source, true);
            var e2 = res2.Root.Element("Section1");

            ///* Assert */
            e1.Should().NotBeNull();
            e2.Should().NotBeNull();
        }

        [TestMethod]
        public void Saving_and_loading_xml_to_and_from_file_should_work()
        {
            /* Arrange */
            XNamespace ns = "http://www.default.com";
            XNamespace nsp = "http://www.p.com";
            XNamespace nsx = "http://www.x.com";

            var source = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(
                    ns + "configuration",
                    new XAttribute("xmlns", "http://www.default.com"),
                    new XAttribute(XNamespace.Xmlns + "p", "http://www.p.com"),
                    new XElement(
                        ns + "Section1",
                        new XAttribute("Section1Attrib", "1"),
                        new XComment("SectionOne"),
                        new XElement("Section1Child")),
                    new XElement(
                        nsp + "Section2",
                        new XAttribute("Section2Attrib", "2"),
                        new XComment("SectionTwo"),
                        new XElement(nsp + "Section2Child")),
                    new XElement(
                        nsx + "Section3",
                        new XAttribute("Section3Attrib", "3"),
                        new XComment("SectionThree"),
                        new XElement(nsx + "Section3Child"))));

            ///* Act */
            source.Save("test.xml");

            var res1 = XmlUtil.LoadXmlFile("test.xml", true);
            var res2 = XmlUtil.LoadXmlFile("test.xml", false);

            var e1 = res1.Root.Element("Section1");
            var e2 = res2.Root.Element(ns + "Section1");

            ///* Assert */
            e1.Should().NotBeNull();
            e2.Should().NotBeNull();
        }

        [TestMethod]
        public void Loading__rubbish_xml_should_fail_as_expected()
        {
            /* Arrange */
            string rubbishXml = "jflsakd";
            string rubbishFile = "TestXml\\RubbishXml.xml";

            /* Act */
            Action a1 = () => XmlUtil.LoadXmlString(rubbishXml, false);
            Action a2 = () => XmlUtil.LoadXmlFile(rubbishFile, false);

            /* Assert */
            a1.ShouldThrow<XmlException>();
            a2.ShouldThrow<XmlException>();
        }

        [TestMethod]
        public void Loading__empty_xml_should_work()
        {
            /* Arrange */
            string emptyFile = "TestXml\\Empty.xml";

            /* Act */
            var doc = XmlUtil.LoadXmlFile(emptyFile, false);

            /* Assert */
            doc.Should().NotBeNull();
        }

        [TestMethod]
        public void Stripping_a_document_of_namespaces_should_work()
        {
            /* Arrange */
            XNamespace ns = "http://www.default.com";
            XNamespace nsp = "http://www.p.com";
            XNamespace nsx = "http://www.x.com";

            var source = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(
                    ns + "configuration",
                    new XAttribute("xmlns", "http://www.default.com"),
                    new XAttribute(XNamespace.Xmlns + "p", "http://www.p.com"),
                    new XElement(
                        ns + "Section1",
                        new XAttribute("Section1Attrib", "1"),
                        new XComment("SectionOne"),
                        new XElement("Section1Child")),
                    new XElement(
                        nsp + "Section2",
                        new XAttribute("Section2Attrib", "2"),
                        new XComment("SectionTwo"),
                        new XElement(nsp + "Section2Child")),
                    new XElement(
                        nsx + "Section3",
                        new XAttribute("Section3Attrib", "3"),
                        new XComment("SectionThree"),
                        new XElement(nsx + "Section3Child"))));

            /* Act */
            source = source.StripNameSpaces(true);

            var e1 = source.Root.Element("Section1");
            var e2 = source.Root.Element("Section2");
            var e3 = source.Root.Element("Section3");

            var c1 = e1.Element("Section1Child");
            var c2 = e2.Element("Section2Child");
            var c3 = e3.Element("Section3Child");

            /* Assert */
            c1.Should().NotBeNull();
            c2.Should().NotBeNull();
            c3.Should().NotBeNull();

            e1.Attribute("Section1Attrib").Value.Should().Be("1");
            e2.Attribute("Section2Attrib").Value.Should().Be("2");
            e3.Attribute("Section3Attrib").Value.Should().Be("3");

            c1.PreviousNode.Should().BeOfType<XComment>().And.Match<XComment>(c => c.Value == "SectionOne");
            c2.PreviousNode.Should().BeOfType<XComment>().And.Match<XComment>(c => c.Value == "SectionTwo");
            c3.PreviousNode.Should().BeOfType<XComment>().And.Match<XComment>(c => c.Value == "SectionThree");
        }

        private static string StripWhiteSpace(string s)
        {
            return Regex.Replace(s, @"(>|^)\s*(<|$)", "$1$2");
        }
    }
}
