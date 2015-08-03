namespace DeepConfig.Spec.Cryptography
{
    using System;
    using System.Xml.Linq;
    using DeepConfig;
    using DeepConfig.Core;
    using DeepConfig.Cryptography;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CryptoConfigSectionHandlerTests
    {
        [TestMethod]
        public void Handling_a_valid_crypto_provider_should_work()
        {
            /* Arrange */
            var handler = new CryptoConfigSectionHandler();

            /* Act */
            var node = handler.WriteSection(typeof(AesCryptoProvider));
            var nodeContents = node.Attribute(ConfigElement.SectionTypeAttribute).Value;
            var result = handler.ReadSection(node);

            /* Assert */
            result.Should().BeOfType<AesCryptoProvider>();
            nodeContents.Should().NotContain("AesCryptoProvider", "it must be encrypted");
        }

        [TestMethod]
        public void Passing_invalid_args_to_write_should_fail()
        {
            /* Arrange */
            var handler = new CryptoConfigSectionHandler();

            /* Act */
            Action a1 = () => handler.WriteSection(null);
            Action a2 = () => handler.WriteSection(typeof(object));

            /* Assert */
            a1.ShouldThrow<ArgumentNullException>();
            a2.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigType);
        }

        [TestMethod]
        public void Passing_invalid_args_to_read_should_fail()
        {
            /* Arrange */
            var invalidSection = new XElement("SomeElement");
            var invalidProvider = new XElement(ConfigElement.CryptoSettingNode, new XAttribute(ConfigElement.SectionTypeAttribute, "DE4U8wqsv5xKjAVuUYLbne14iM00e0yF3ZH1F2EIAn1OCU/fO1yAXf8SkvsjoQRUXRe0+s06u0NYBlPh9WuPRCrhCDxgrcapqhgecSISLApvPct7dYdW+/WCCN61ACfe8+Hm927njdW4wSTkwuJSlWiUkW1H849G4wMpHlKY1OkfraqcAMJb/05s5Se6QQ0oktGPBemsgpPVI6+baDiHHuIjPJhCcQ7IBSsQdAGTpNc="));

            var handler = new CryptoConfigSectionHandler();

            /* Act */
            var result1 = handler.ReadSection(null);
            var result2 = handler.ReadSection(invalidSection);

            Action a = () => handler.ReadSection(invalidProvider);

            /* Assert */
            result1.Should().BeNull();
            result2.Should().BeNull();
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.CryptographyProviderCreationFailed);
        }
    }
}