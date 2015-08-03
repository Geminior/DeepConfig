namespace DeepConfig.Spec.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig.Core;
    using DeepConfig.Cryptography;
    using DeepConfig.Handlers;
    using DeepConfig.TestTypes;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GenericTypeSectionHandlerTests
    {
        [TestMethod]
        public void Handler_should_have_no_errors_on_creation()
        {
            /* Arrange */
            var handler = new GenericTypeSectionHandler();

            /* Assert */
            handler.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Writing_a_type_with_no_config_attribute_should_fail_gracefully()
        {
            /* Arrange */
            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", new object(), null, false);

            /* Assert */
            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.MissingConfigSectionAttribute);
            node.Should().BeNull();
        }

        [TestMethod]
        public void Writing_a_type_with_a_custom_handler_should_fail_gracefully()
        {
            /* Arrange */
            var handler = new GenericTypeSectionHandler();
            var source = new ConfigWithCustomHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, null, false);

            /* Assert */
            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.SectionHandlerMismatch);
            node.Should().BeNull();
        }

        [TestMethod]
        public void Reading_a_section_that_is_null_should_return_null()
        {
            /* Arrange */
            var handler = new GenericTypeSectionHandler();

            /* Act */
            var result = handler.ReadSection(null, null, false);

            /* Assert */
            result.Should().BeNull();
            handler.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Reading_an_invalid_section_should_fail_gracefully()
        {
            /* Arrange */
            var handler = new GenericTypeSectionHandler();

            /* Act */
            var result = handler.ReadSection(new XElement("InvalidNode"), null, false);

            /* Assert */
            result.Should().BeNull();
            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidNode);
        }

        [TestMethod]
        public void Reading_a_section_with_an_invalid_config_type_should_fail_gracefully()
        {
            /* Arrange */
            var handler = new GenericTypeSectionHandler();
            var element = new XElement("InvalidType", new XAttribute(ConfigElement.SectionTypeAttribute, "NonExistantType"));

            /* Act */
            var result = handler.ReadSection(element, null, false);

            /* Assert */
            result.Should().BeNull();
            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidNode);
        }

        [TestMethod]
        public void Handling_a_type_with_basic_props_should_work_regardless_of_culture()
        {
            /* Arrange */
            var source = new ConfigWithBasicProps
            {
                BoolValue = true,
                ByteValue = 10,
                DecimalValue = 34.45M,
                DoubleValue = 35.45,
                EnumValue = ConfigWithBasicProps.TestEnum.One,
                FloatValue = 36.45F,
                GuidValue = Guid.NewGuid(),
                IntValue = 11,
                LongValue = 12L,
                ShortValue = 13,
                StringValue = "SomeString",
                TimeSpanValue = new TimeSpan(5, 4, 3, 2, 1),
                UriValue = new Uri("http://somewhere.com")
            };

            //Make a crypter that simply echoes the value
            var crypter = CreateEcho();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, false);

            ConfigWithBasicProps result;
            using (new CultureContext())
            {
                result = handler.ReadSection(node, crypter, false) as ConfigWithBasicProps;
            }

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);

            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => crypter.Decrypt(A<string>.Ignored)).MustNotHaveHappened();
        }

        [TestMethod]
        public void Handling_a_type_with_basic_props_with_forced_encryption_should_work()
        {
            /* Arrange */
            var source = new ConfigWithBasicProps
            {
                BoolValue = true,
                ByteValue = 10,
                DecimalValue = 34.45M,
                DoubleValue = 35.45,
                EnumValue = ConfigWithBasicProps.TestEnum.One,
                FloatValue = 36.45F,
                GuidValue = Guid.NewGuid(),
                IntValue = 11,
                LongValue = 12L,
                ShortValue = 13,
                StringValue = "SomeString",
                TimeSpanValue = new TimeSpan(5, 4, 3, 2, 1),
                UriValue = new Uri("http://somewhere.com")
            };

            //Make a crypter that simply echoes the value
            var crypter = CreateEcho();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, true);
            var result = handler.ReadSection(node, crypter, true) as ConfigWithBasicProps;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);

            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(13));
            A.CallTo(() => crypter.Decrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(13));
        }

        [TestMethod]
        public void Handling_a_type_with_basic_props_uninitialized_should_work()
        {
            /* Arrange */
            var source = new ConfigWithBasicProps();

            //Make a crypter that simply echoes the value
            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, false);
            var result = handler.ReadSection(node, crypter, false) as ConfigWithBasicProps;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
        }

        [TestMethod]
        public void Reading_a_section_with_basic_props_where_all_values_are_empty_should_work()
        {
            /* Arrange */
            var source = new ConfigWithBasicProps();

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, false);

            //Now remove all values and read it
            SetAllValues(node, string.Empty);
            var result = handler.ReadSection(node, crypter, false) as ConfigWithBasicProps;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
        }

        [TestMethod]
        public void Reading_a_section_with_basic_props_where_values_are_invalid_should_fail_gracefully()
        {
            /* Arrange */
            var source = new ConfigWithBasicProps()
            {
                StringValue = "something",
                UriValue = new Uri("http://somewhere.com")
            };

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, false);

            //Now remove all values and read it
            SetAllValues(node, "garbage");
            var result = handler.ReadSection(node, crypter, false) as ConfigWithBasicProps;

            /* Assert */
            //All but the string property should fail
            handler.Errors.Should().HaveCount(12);
        }

        [TestMethod]
        public void Handling_a_type_with_list_props_should_work()
        {
            /* Arrange */
            var source = new ConfigWithLists();
            source.StringsColOne = new List<string> { "A", "B", "C" };
            source.StringsColTwo = new List<string> { "D", "E", "F" };
            source.StringsColLazyOne.Add("G", "H", "I");
            source.StringsColLazyTwo.Add("J", "K", "L");
            source.DerivedColOne = new ConfigWithLists.DerivedList { 1, 2, 3 };
            source.DerivedColTwo = new ConfigWithLists.DerivedList { 4, 5, 6 };
            source.DerivedColLazyOne.Add(7, 8, 9);

            var crypter = A.Fake<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestListSection", source, crypter, false);
            var result = handler.ReadSection(node, crypter, false) as ConfigWithLists;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
            result.DerivedColOne.Should().BeOfType<ConfigWithLists.DerivedList>();

            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => crypter.Decrypt(A<string>.Ignored)).MustNotHaveHappened();
        }

        [TestMethod]
        public void Handling_a_type_with_list_props_with_forced_encryption_should_work()
        {
            /* Arrange */
            var source = new ConfigWithLists();
            source.StringsColOne = new List<string> { "A", "B", "C" };
            source.StringsColTwo = new List<string> { "D", "E", "F" };
            source.StringsColLazyOne.Add("G", "H", "I");
            source.StringsColLazyTwo.Add("J", "K", "L");
            source.DerivedColOne = new ConfigWithLists.DerivedList { 1, 2, 3 };
            source.DerivedColTwo = new ConfigWithLists.DerivedList { 4, 5, 6 };
            source.DerivedColLazyOne.Add(7, 8, 9);

            var crypter = CreateEcho();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestListSection", source, crypter, true);
            var result = handler.ReadSection(node, crypter, true) as ConfigWithLists;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
            result.DerivedColOne.Should().BeOfType<ConfigWithLists.DerivedList>();

            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(21));
            A.CallTo(() => crypter.Decrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(21));
        }

        [TestMethod]
        public void Handling_a_type_with_list_props_uninitialized_should_work()
        {
            /* Arrange */
            var source = new ConfigWithLists();

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestListSection", source, crypter, false);
            var result = handler.ReadSection(node, crypter, false) as ConfigWithLists;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
        }

        [TestMethod]
        public void Handling_a_complex_type_should_work()
        {
            /* Arrange */
            var source = new ConfigComplex
            {
                Name = "NotASetting",
                Encrypted = "EncryptedValue",
                SubAbstract = new ConfigComplexSub { Name = "Abstract", NumberPlain = 1, OnBaseOnly = "NotASetting" },
                SubConcrete = new ConfigComplexSub { Name = "Concrete", NumberPlain = 2, OnBaseOnly = "NotASetting" },
                SubEncrypted = new ConfigComplexSub { Name = "Encrypted", NumberPlain = 3, OnBaseOnly = "NotASetting" },
                SubInterface = new ConfigComplexSub { Name = "Interface", NumberPlain = 4, OnBaseOnly = "NotASetting" }
            };

            var crypter = CreateEcho();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestComplexSection", source, crypter, false);
            var result = handler.ReadSection(node, crypter, false) as ConfigComplex;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(
                source,
                o => o
                .Excluding(c => c.Name)
                .Excluding(c => c.SubAbstract.OnBaseOnly)
                .Excluding(c => c.SubConcrete.OnBaseOnly)
                .Excluding(c => c.SubEncrypted.OnBaseOnly)
                .Excluding(c => c.SubLazy.OnBaseOnly)
                .Excluding(c => c.SubInterface));
            result.Name.Should().BeNull();
            result.SubAbstract.OnBaseOnly.Should().BeNull();
            result.SubConcrete.OnBaseOnly.Should().BeNull();
            result.SubEncrypted.OnBaseOnly.Should().BeNull();
            result.SubLazy.OnBaseOnly.Should().BeNull();
            result.SubInterface.Should().BeOfType<ConfigComplexSub>();
            (result.SubInterface as ConfigComplexSub).OnBaseOnly.Should().BeNull();

            //So this calculation is a tad complex
            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(6));
            A.CallTo(() => crypter.Decrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(6));
        }

        [TestMethod]
        public void Writing_a_complex_type_with_custom_named_props_and_descriptions_should_work()
        {
            /* Arrange */
            var source = new ConfigComplexSub() { Name = "MyName" };

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            var expectedElementName = XmlConvert.EncodeLocalName("Custom Name");
            var expectedDescription = "This is the name";

            /* Act */
            var node = handler.WriteSection("TestComplexSection", source, crypter, false);

            var nameNode = node.Element(expectedElementName);
            XComment descriptionNode = null;
            if (nameNode != null)
            {
                descriptionNode = nameNode.PreviousNode as XComment;
            }

            /* Assert */
            handler.Errors.Should().BeEmpty();
            nameNode.Should().NotBeNull();
            descriptionNode.Should().NotBeNull();
            descriptionNode.Value.Should().Be(expectedDescription);
        }

        [TestMethod]
        public void Handling_a_badly_implemented_type_should_fail_gracefully()
        {
            /* Arrange */
            var source = new ConfigSilly(new ConfigWithBasicProps());

            var crypter = A.Dummy<ICryptoProvider>();

            var handlerWrite = new GenericTypeSectionHandler();
            var handlerRead = new GenericTypeSectionHandler();

            /* Act */
            var node = handlerWrite.WriteSection("TestSection", source, crypter, false);

            //Now remove all values and read it
            var result = handlerRead.ReadSection(node, crypter, false) as ConfigWithBasicProps;

            /* Assert */
            handlerWrite.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigSettingAttribute);
            handlerWrite.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.MissingGetter);
            handlerWrite.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigValue);
            handlerWrite.Errors.Where(e => e.Code == ConfigErrorCode.InvalidConfigType).Should().HaveCount(2);

            handlerRead.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.MissingSetter);
            handlerRead.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.ConfigTypeCreationFailed);
        }

        [TestMethod]
        public void Handling_a_type_with_a_circular_reference_should_fail_gracefully()
        {
            /* Arrange */
            var source = new ConfigWithCircularReference();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            Action a = () => handler.WriteSection("TestSection", source, null, false);

            /* Assert */
            a.ShouldThrow<ConfigException>();
        }

        [TestMethod]
        public void Handling_a_complex_type_with_invalid_complex_settings_should_fail_gracefully()
        {
            /* Arrange */
            var doc = XDocument.Load("TestXml\\InvalidSections.xml");
            var sillyNode = doc.Root.Element("TestSillySection");
            var abstractNode = doc.Root.Element("TestComplexSection");
            var listNode = doc.Root.Element("TestListSection");

            var crypter = A.Dummy<ICryptoProvider>();

            var handler1 = new GenericTypeSectionHandler();
            var handler2 = new GenericTypeSectionHandler();
            var handler3 = new GenericTypeSectionHandler();

            /* Act */
            handler1.ReadSection(sillyNode, crypter, false);
            handler2.ReadSection(abstractNode, crypter, false);
            var lists = handler3.ReadSection(listNode, crypter, false) as ConfigWithLists;

            /* Assert */
            handler1.Errors.Where(e => e.Code == ConfigErrorCode.InvalidConfigType).Should().HaveCount(2);
            handler2.Errors.Where(e => e.Code == ConfigErrorCode.InvalidConfigType).Should().HaveCount(2);
            handler3.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigType);
            handler3.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.ConfigTypeCreationFailed);
            lists.Should().NotBeNull();
            lists.DerivedColOne.Should().BeOfType<List<int>>();
        }

        [TestMethod]
        public void Handling_a_complex_property_with_a_custom_handler_should_work()
        {
            /* Arrange */
            var source = new ConfigWithCustomHandler
            {
                //This is purely for code coverage
                ErrorsNullOnRead = true,
                ErrorsNullOnWrite = true
            };

            var parent = new ConfigComplex
            {
                CustomHandler = source
            };

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestComplexSection", parent, crypter, false);
            var result = handler.ReadSection(node, crypter, false) as ConfigComplex;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeNull();

            source.WrittenByHandler.Should().BeFalse();
            source.ReadByHandler.Should().BeFalse();
            result.CustomHandler.WrittenByHandler.Should().BeTrue();
            result.CustomHandler.ReadByHandler.Should().BeTrue();
        }

        [TestMethod]
        public void Handling_a_complex_property_with_a_custom_handler_that_has_errors_should_pick_up_those_errors()
        {
            /* Arrange */
            var source = new ConfigWithCustomHandler
            {
                HasErrorOnRead = true,
                HasErrorOnWrite = true
            };

            var parent = new ConfigComplex
            {
                CustomHandler = source
            };

            var crypter = A.Dummy<ICryptoProvider>();

            var handlerWrite = new GenericTypeSectionHandler();
            var handlerRead = new GenericTypeSectionHandler();

            /* Act */
            var node = handlerWrite.WriteSection("TestComplexSection", parent, crypter, false);
            handlerRead.ReadSection(node, crypter, false);

            /* Assert */
            handlerWrite.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigType);
            handlerRead.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigType);
        }

        [TestMethod]
        public void Writing_a_complex_property_with_a_custom_handler_that_fails_should_fail_gracefully()
        {
            /* Arrange */
            var source = new ConfigWithCustomHandler
            {
                ThrowOnWrite = true
            };

            var parent = new ConfigComplex
            {
                CustomHandler = source
            };

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();
            var handlerRead = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestComplexSection", parent, crypter, false);

            /* Assert */
            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.SectionHandlerMismatch);
        }

        [TestMethod]
        public void Reading_a_complex_property_with_a_custom_handler_that_fails_should_fail_gracefully()
        {
            /* Arrange */
            var source = new ConfigWithCustomHandler
            {
                ThrowOnRead = true
            };

            var parent = new ConfigComplex
            {
                CustomHandler = source
            };

            var crypter = A.Dummy<ICryptoProvider>();

            var handlerWrite = new GenericTypeSectionHandler();
            var handlerRead = new GenericTypeSectionHandler();

            /* Act */
            var node = handlerWrite.WriteSection("TestComplexSection", parent, crypter, false);
            handlerRead.ReadSection(node, crypter, false);

            /* Assert */
            handlerRead.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.SectionHandlerMismatch);
        }

        [TestMethod]
        public void Handling_a_complex_get_only_property_with_a_custom_handler_should_fail_gracefully()
        {
            /* Arrange */
            var source = new ConfigComplexWithCustomGetOnly();

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestComplexSection", source, crypter, false);
            handler.ReadSection(node, crypter, false);

            /* Assert */
            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.MissingSetter);
        }

        [TestMethod]
        public void Handling_a_type_with_a_utc_datetime_should_work()
        {
            DoDateHandlingTest(DateTime.UtcNow);
        }

        [TestMethod]
        public void Handling_a_type_with_a_local_datetime_should_work()
        {
            DoDateHandlingTest(DateTime.Now);
        }

        [TestMethod]
        public void Handling_a_type_with_an_unspecified_datetime_should_work()
        {
            var dt = new DateTime(DateTime.Now.Ticks);
            DoDateHandlingTest(dt);
        }

        [TestMethod]
        public void Handling_a_type_with_a_datetime_uninitialized_should_work()
        {
            DoDateHandlingTest(null);
        }

        [TestMethod]
        public void Handling_a_type_with_a_datetime_with_force_encryption_should_work()
        {
            var source = new ConfigWithDateTimeProp
            {
                DateTimeValue = DateTime.Now
            };

            var crypter = CreateEcho();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, true);

            ConfigWithDateTimeProp result;
            using (new CultureContext())
            {
                result = handler.ReadSection(node, crypter, true) as ConfigWithDateTimeProp;
            }

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);

            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => crypter.Decrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestMethod]
        public void Reading_a_section_with_date_where_value_is_empty_should_work()
        {
            /* Arrange */
            var source = new ConfigWithDateTimeProp();

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, false);

            //Now remove all values and read it
            SetAllValues(node, string.Empty);
            var result = handler.ReadSection(node, crypter, false) as ConfigWithDateTimeProp;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
        }

        [TestMethod]
        public void Reading_a_section_with_date_where_value_is_invalid_should_fail_gracefully()
        {
            /* Arrange */
            var source = new ConfigWithDateTimeProp();

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, false);

            //Now remove all values and read it
            SetAllValues(node, "garbage");
            var result = handler.ReadSection(node, crypter, false) as ConfigWithDateTimeProp;

            /* Assert */
            handler.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.InvalidConfigValue);
        }

        [TestMethod]
        public void Handling_a_type_with_nullable_props_that_are_not_null_should_work()
        {
            /* Arrange */
            var source = new ConfigWithNullables
            {
                NullableInt = 10,
                NullableGuid = Guid.NewGuid(),
                NullableDateTime = DateTime.Now.AddHours(-2),
                NullableTimeSpan = new TimeSpan(1, 2, 3, 4, 5)
            };

            /* Act */
            var crypter = A.Fake<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            var node = handler.WriteSection("TestNullableSection", source, crypter, false);

            ConfigWithNullables result;
            using (new CultureContext())
            {
                result = handler.ReadSection(node, crypter, false) as ConfigWithNullables;
            }

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
        }

        [TestMethod]
        public void Handling_a_type_with_nullable_props_that_are_null_should_work()
        {
            /* Arrange */
            var source = new ConfigWithNullables();

            /* Act */
            var crypter = A.Fake<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            var node = handler.WriteSection("TestNullableSection", source, crypter, false);

            ConfigWithNullables result;
            using (new CultureContext())
            {
                result = handler.ReadSection(node, crypter, false) as ConfigWithNullables;
            }

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
        }

        [TestMethod]
        public void Reading_a_section_with_nullable_props_where_all_values_are_empty_should_work()
        {
            /* Arrange */
            var source = new ConfigWithNullables
            {
                NullableInt = 10,
                NullableGuid = Guid.NewGuid(),
                NullableDateTime = DateTime.Now.AddHours(-2),
                NullableTimeSpan = new TimeSpan(1, 2, 3, 4, 5)
            };

            var expected = new ConfigWithNullables();

            var crypter = A.Dummy<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestNullableSection", source, crypter, false);

            //Now remove all values and read it
            SetAllValues(node, string.Empty);
            var result = handler.ReadSection(node, crypter, false) as ConfigWithNullables;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(expected);
        }

        [TestMethod]
        public void Handling_encryption_with_a_null_encrypter_should_work()
        {
            var source = new ConfigWithDateTimeProp
            {
                DateTimeValue = DateTime.Now
            };

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, null, true);
            var result = handler.ReadSection(node, null, true) as ConfigWithDateTimeProp;

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
        }

        [TestMethod]
        public void Reading_a_section_with_one_encryption_and_writing_it_with_another_must_work()
        {
            /* Arrange */
            var source = new ConfigComplexSub { Name = "EncryptMePlease" };

            var cryptoOne = A.Fake<ICryptoProvider>();
            A.CallTo(() => cryptoOne.Encrypt(A<string>.Ignored)).ReturnsLazily(h => Convert.ToBase64String(Encoding.UTF8.GetBytes(h.Arguments[0].ToString())));
            A.CallTo(() => cryptoOne.Decrypt(A<string>.Ignored)).ReturnsLazily(h => Encoding.UTF8.GetString(Convert.FromBase64String(h.Arguments[0].ToString())));

            var cryptoTwo = A.Fake<ICryptoProvider>();
            A.CallTo(() => cryptoTwo.Encrypt(A<string>.Ignored)).ReturnsLazily(h => new string(h.Arguments[0].ToString().Reverse().ToArray()));
            A.CallTo(() => cryptoTwo.Decrypt(A<string>.Ignored)).ReturnsLazily(h => new string(h.Arguments[0].ToString().Reverse().ToArray()));

            var handler1 = new GenericTypeSectionHandler();
            var handler2 = new GenericTypeSectionHandler();

            /* Act */
            var node = handler1.WriteSection("TestSection", source, cryptoOne, false);
            var midResult = handler1.ReadSection(node, cryptoOne, false);

            node = handler2.WriteSection("TestSection", midResult, cryptoTwo, false);
            var result = handler2.ReadSection(node, cryptoTwo, false) as ConfigComplexSub;

            /* Assert */
            A.CallTo(() => cryptoOne.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => cryptoOne.Decrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => cryptoTwo.Encrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => cryptoTwo.Decrypt(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);

            result.ShouldBeEquivalentTo(source);
        }

        private static void DoDateHandlingTest(DateTime? dt)
        {
            /* Arrange */
            var source = new ConfigWithDateTimeProp();
            if (dt.HasValue)
            {
                source.DateTimeValue = dt.Value;
            }

            var crypter = A.Fake<ICryptoProvider>();

            var handler = new GenericTypeSectionHandler();

            /* Act */
            var node = handler.WriteSection("TestSection", source, crypter, false);

            ConfigWithDateTimeProp result;
            using (new CultureContext())
            {
                result = handler.ReadSection(node, crypter, false) as ConfigWithDateTimeProp;
            }

            /* Assert */
            handler.Errors.Should().BeEmpty();
            result.Should().NotBeSameAs(source);
            result.ShouldBeEquivalentTo(source);
        }

        private static ICryptoProvider CreateEcho()
        {
            var crypter = A.Fake<ICryptoProvider>();
            A.CallTo(() => crypter.Encrypt(A<string>.Ignored)).ReturnsLazily(h => h.Arguments[0].ToString());
            A.CallTo(() => crypter.Decrypt(A<string>.Ignored)).ReturnsLazily(h => h.Arguments[0].ToString());

            return crypter;
        }

        private static void SetAllValues(XElement element, string value)
        {
            var q = from e in element.Elements()
                    let a = e.Attribute(ConfigElement.SettingValueAttribute)
                    where a != null
                    select a;

            foreach (var a in q)
            {
                a.Value = value;
            }
        }

        //TODO: Clean this up
        //[TestMethod]
        //public void Creating_a_config_section_with_a_valid_handler_should_invoke_the_handler_correctly()
        //{
        //    var extendedHandler = A.Fake<IExtendedConfigurationSectionHandler>();
        //    A.CallTo(() => extendedHandler.ReadSection(null, null, A<bool>.Ignored)).Returns(new object());

        //    //The element and the crypto provider are irrelevant since its not the handler that is being tested. As long as the element is not null.
        //    var s1 = ConfigHelper.CreateConfigSectionInstance(extendedHandler, null, null, true);
        //    var s2 = ConfigHelper.CreateConfigSectionInstance(basicHandler, null, null, true);

        //    s1.Should().NotBeNull();
        //    s2.Should().NotBeNull();
        //    A.CallTo(() => extendedHandler.Create(null, null, true)).MustHaveHappened(Repeated.Exactly.Once);
        //    A.CallTo(() => basicHandler.Create(null, null, null)).MustHaveHappened(Repeated.Exactly.Once);
        //}

        //[TestMethod]
        //public void Creating_a_config_section_with_an_invalid_handler_should_throw()
        //{
        //    //Calling with a handler that does not implement either of the required interfaces. The remaining args are irrelevant.
        //    Action a = () => ConfigHelper.CreateConfigSectionInstance(new object(), null, null, false);

        //    a.ShouldThrow<ConfigException>().And.Cause.Should().Be(ConfigErrorCode.SectionHandlerInvalid);
        //}

        //[TestMethod]
        //public void Creating_a_config_section_with_a_failing_handler_should_invoke_the_handler_and_throw()
        //{
        //    var extendedHandler = A.Fake<IExtendedConfigurationSectionHandler>();
        //    A.CallTo(() => extendedHandler.Create(null, null, A<bool>.Ignored)).Throws(new DeepConfigTestException());

        //    var basicHandler = A.Fake<IConfigurationSectionHandler>();
        //    A.CallTo(() => basicHandler.Create(null, null, null)).Throws(new DeepConfigTestException());

        //    //The element and the crypto provider are irrelevant since its not the handler that is being tested.
        //    Action a = () => ConfigHelper.CreateConfigSectionInstance(extendedHandler, null, null, false);
        //    Action b = () => ConfigHelper.CreateConfigSectionInstance(basicHandler, null, null, false);

        //    a.ShouldThrow<ConfigException>().And.Cause.Should().Be(ConfigErrorCode.SectionHandlerMismatch);
        //    b.ShouldThrow<ConfigException>().And.Cause.Should().Be(ConfigErrorCode.SectionHandlerMismatch);
        //    A.CallTo(() => extendedHandler.Create(null, null, false)).MustHaveHappened(Repeated.Exactly.Once);
        //    A.CallTo(() => basicHandler.Create(null, null, null)).MustHaveHappened(Repeated.Exactly.Once);
        //}
    }
}
