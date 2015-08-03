namespace DeepConfig.Spec
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using DeepConfig;
    using DeepConfig.Cryptography;
    using DeepConfig.Handlers;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NameValueExtendedSectionHandlerTests
    {
        [TestMethod]
        public void Handler_should_have_no_errors_on_creation()
        {
            /* Arrange */
            var handler = new NameValueExtendedSectionHandler();

            /* Assert */
            handler.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Reading_a_section_that_is_null_should_throw()
        {
            /* Arrange */
            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            Action a = () => handler.ReadSection(null, null, false);

            /* Assert */
            a.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void Writing_an_invalid_type_or_value_should_fail()
        {
            /* Arrange */
            var handler = new NameValueExtendedSectionHandler();
            var crypto = A.Dummy<ICryptoProvider>();

            /* Act */
            Action a1 = () => handler.WriteSection("SomeSettings1", null, crypto, false);
            Action a2 = () => handler.WriteSection(string.Empty, new object(), crypto, false);
            Action a3 = () => handler.WriteSection("SomeSettings2", new object(), crypto, false);

            /* Assert */
            a1.ShouldThrow<ArgumentNullException>();
            a2.ShouldThrow<ArgumentException>();
            a3.ShouldNotThrow();

            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigType);
        }

        [TestMethod]
        public void Normal_write_read_namevaluesettings_handling_should_work()
        {
            /* Arrange */
            var crypter = A.Fake<ICryptoProvider>();
            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).ReturnsLazily(h => h.Arguments[0].ToString());
            A.CallTo(() => crypter.Decrypt("PlainText")).Throws(new Exception());
            A.CallTo(() => crypter.Decrypt("EncryptedText")).Returns("EncryptedText");
            A.CallTo(() => crypter.IsEncrypted("PlainText")).Returns(false);
            A.CallTo(() => crypter.IsEncrypted("EncryptedText")).Returns(true);

            var nv = new NameValueSettings();
            nv.SetSetting("Plain", "PlainText");
            nv.SetEncryptedSetting("Encrypted", "EncryptedText");
            nv.SetDescription("Encrypted", "This is some encrypted text");

            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", nv, crypter, false);
            var result = handler.ReadSection(node, crypter, false) as NameValueSettings;
            var val1 = result.Get("Plain", null);
            var val2 = result.Get("Encrypted", null);
            var desc = result.GetDescription("Encrypted");
            var isEnc1 = result.IsEncrypted("Plain");
            var isEnc2 = result.IsEncrypted("Encrypted");

            /* Assert */
            val1.Should().Be("PlainText");
            val2.Should().Be("EncryptedText");
            desc.Should().Be("This is some encrypted text");
            isEnc1.Should().BeFalse();
            isEnc2.Should().BeTrue();
        }

        [TestMethod]
        public void Normal_write_read_handling_with_null_encrypter_should_work()
        {
            /* Arrange */
            var nv = new NameValueSettings();
            nv.SetSetting("Plain", "PlainText");
            nv.SetEncryptedSetting("Encrypted", "EncryptedText");

            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", nv, null, false);
            var result = handler.ReadSection(node, null, false) as NameValueSettings;
            var val1 = result.Get("Plain", null);
            var val2 = result.Get("Encrypted", null);
            var isEnc1 = result.IsEncrypted("Plain");
            var isEnc2 = result.IsEncrypted("Encrypted");

            /* Assert */
            val1.Should().Be("PlainText");
            val2.Should().Be("EncryptedText");
            isEnc1.Should().BeFalse();
            isEnc2.Should().BeFalse();
        }

        [TestMethod]
        public void Normal_write_read_namevaluecollection_handling_should_work()
        {
            /* Arrange */
            var crypto = CreateEcho();

            var nv = new NameValueCollection();
            nv.Add("PlainOne", "PlainTextOne");
            nv.Add("PlainTwo", "PlainTextTwo");

            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", nv, crypto, false);
            var result = handler.ReadSection(node, crypto, false) as NameValueSettings;
            var val1 = result.Get("PlainOne", null);
            var val2 = result.Get("PlainTwo", null);

            /* Assert */
            val1.Should().Be("PlainTextOne");
            val2.Should().Be("PlainTextTwo");
        }

        [TestMethod]
        public void Reading_an_empty_section_should_work()
        {
            /* Arrange */
            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            var result = handler.ReadSection(new XElement("EmptyData"), null, false) as NameValueSettings;

            /* Assert */
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void Handling_namevaluesettings_forced_encryption_should_work()
        {
            /* Arrange */
            var crypto = A.Fake<ICryptoProvider>();
            A.CallTo(() => crypto.Encrypt(A<string>.Ignored)).ReturnsLazily(h => Convert.ToBase64String(Encoding.UTF8.GetBytes(h.Arguments[0].ToString())));
            A.CallTo(() => crypto.Decrypt(A<string>.Ignored)).ReturnsLazily(h => Encoding.UTF8.GetString(Convert.FromBase64String(h.Arguments[0].ToString())));
            A.CallTo(() => crypto.IsEncrypted(A<string>.Ignored)).Returns(true);

            var nv = new NameValueSettings();
            nv.SetSetting("Plain", "PlainText");
            nv.SetEncryptedSetting("Encrypted", "EncryptedText");

            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", nv, crypto, true);
            var result = handler.ReadSection(node, crypto, true) as NameValueSettings;
            var val1 = result.Get("Plain", null);
            var val2 = result.Get("Encrypted", null);
            var allEncrypted = result.IsEncrypted("Plain") && result.IsEncrypted("Encrypted");

            /* Assert */
            val1.Should().Be("PlainText");
            val2.Should().Be("EncryptedText");
            allEncrypted.Should().BeTrue();
        }

        [TestMethod]
        public void Handling_namevaluecollection_forced_encryption_should_work()
        {
            /* Arrange */
            var crypto = A.Fake<ICryptoProvider>();
            A.CallTo(() => crypto.Encrypt(A<string>.Ignored)).ReturnsLazily(h => Convert.ToBase64String(Encoding.UTF8.GetBytes(h.Arguments[0].ToString())));
            A.CallTo(() => crypto.Decrypt(A<string>.Ignored)).ReturnsLazily(h => Encoding.UTF8.GetString(Convert.FromBase64String(h.Arguments[0].ToString())));
            A.CallTo(() => crypto.IsEncrypted(A<string>.Ignored)).Returns(true);

            var nv = new NameValueCollection();
            nv.Add("PlainOne", "PlainTextOne");
            nv.Add("PlainTwo", "PlainTextTwo");

            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", nv, crypto, true);
            var result = handler.ReadSection(node, crypto, true) as NameValueSettings;
            var val1 = result.Get("PlainOne", null);
            var val2 = result.Get("PlainTwo", null);
            var allEncrypted = result.IsEncrypted("PlainOne") && result.IsEncrypted("PlainTwo");

            /* Assert */
            val1.Should().Be("PlainTextOne");
            val2.Should().Be("PlainTextTwo");
            allEncrypted.Should().BeTrue();
        }

        [TestMethod]
        public void Reading_a_section_with_one_encryption_and_writing_it_with_another_must_work()
        {
            /* Arrange */
            var cryptoOne = A.Fake<ICryptoProvider>();
            A.CallTo(() => cryptoOne.Encrypt(A<string>.Ignored)).ReturnsLazily(h => Convert.ToBase64String(Encoding.UTF8.GetBytes(h.Arguments[0].ToString())));
            A.CallTo(() => cryptoOne.Decrypt(A<string>.Ignored)).ReturnsLazily(h => Encoding.UTF8.GetString(Convert.FromBase64String(h.Arguments[0].ToString())));
            A.CallTo(() => cryptoOne.IsEncrypted(A<string>.Ignored)).Returns(true);

            var cryptoTwo = A.Fake<ICryptoProvider>();
            A.CallTo(() => cryptoTwo.Encrypt(A<string>.Ignored)).ReturnsLazily(h => new string(h.Arguments[0].ToString().Reverse().ToArray()));
            A.CallTo(() => cryptoTwo.Decrypt(A<string>.Ignored)).ReturnsLazily(h => new string(h.Arguments[0].ToString().Reverse().ToArray()));
            A.CallTo(() => cryptoTwo.IsEncrypted(A<string>.Ignored)).Returns(true);

            var source = new NameValueSettings();
            source.SetEncryptedSetting("Setting1", "SomeText");

            var handler1 = new NameValueExtendedSectionHandler();
            var handler2 = new NameValueExtendedSectionHandler();

            /* Act */
            var node = handler1.WriteSection("TestSection", source, cryptoOne, false);
            var midResult = handler1.ReadSection(node, cryptoOne, false) as NameValueSettings;

            node = handler2.WriteSection("TestSection", midResult, cryptoTwo, false);
            var result = handler2.ReadSection(node, cryptoTwo, false) as NameValueSettings;
            var val = result.Get("Setting1", null);

            /* Assert */
            A.CallTo(() => cryptoOne.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => cryptoOne.Decrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => cryptoTwo.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => cryptoTwo.Decrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);

            val.Should().Be("SomeText");
        }

        [TestMethod]
        public void Reading_a_value_that_where_decryption_fails_must_fail_gracefully()
        {
            /* Arrange */
            var crypto = A.Fake<ICryptoProvider>();
            A.CallTo(() => crypto.Encrypt(A<string>.Ignored)).ReturnsLazily(h => Convert.ToBase64String(Encoding.UTF8.GetBytes(h.Arguments[0].ToString())));
            A.CallTo(() => crypto.Decrypt(A<string>.Ignored)).Throws(new Exception());
            A.CallTo(() => crypto.IsEncrypted(A<string>.Ignored)).Returns(true);

            var nv = new NameValueSettings();
            nv.SetEncryptedSetting("Encrypted", "EncryptedText");

            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", nv, crypto, true);
            var result = handler.ReadSection(node, crypto, true) as NameValueSettings;

            /* Assert */
            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigValue);
        }

        [TestMethod]
        public void Handling_section_with_invalid_namevalue_entries_should_ignore_them()
        {
            /* Arrange */
            var doc = XDocument.Load("TestXml\\InvalidNameValueEntries.xml");
            var sectionWithInvalidElements = doc.Root.Element("appSettings");

            var handler = new NameValueExtendedSectionHandler();

            /* Act */
            var result = handler.ReadSection(sectionWithInvalidElements, null, false) as NameValueSettings;

            /* Assert */
            result.Count.Should().Be(1);
        }

        private static ICryptoProvider CreateEcho()
        {
            var crypter = A.Fake<ICryptoProvider>();
            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).ReturnsLazily(h => h.Arguments[0].ToString());
            A.CallTo(() => crypter.Decrypt(A<string>.Ignored)).ReturnsLazily(h => h.Arguments[0].ToString());

            return crypter;
        }
    }
}
