namespace DeepConfig.Spec.Utilities
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using DeepConfig;
    using DeepConfig.Core;
    using DeepConfig.TestTypes;
    using DeepConfig.Utilities;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigHelperTests
    {
        [TestMethod]
        public void Creating_handlers_of_valid_types_should_succeed()
        {
            /* Arrange */
            var exType = typeof(HandlerCustom);

            /* Act */
            var h1 = ConfigHelper.CreateHandler(exType);

            /* Assert */
            h1.Should().BeOfType<HandlerCustom>();
        }

        [TestMethod]
        public void Trying_to_create_handlers_of_a_type_with_no_default_ctor_should_throw()
        {
            /* Arrange */
            var type = typeof(HandlerInvalidCtor);

            /* Act */
            Action a = () => ConfigHelper.CreateHandler(type);

            /* Assert */
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.SectionHandlerCreationFailed);
        }

        [TestMethod]
        public void Trying_to_create_handlers_of_a_type_that_does_not_implement_correct_interface_should_throw()
        {
            /* Arrange */
            var type = typeof(object);

            /* Act */
            Action a = () => ConfigHelper.CreateHandler(type);

            /* Assert */
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.SectionHandlerInvalid);
        }

        [TestMethod]
        public void Creating_instances_of_valid_types_should_succeed()
        {
            /* Arrange */
            var type = typeof(ConfigWithBasicProps);

            /* Act */
            var h1 = ConfigHelper.CreateInstance(type, false);
            var h2 = ConfigHelper.CreateInstance(type.AssemblyQualifiedName, false);

            /* Assert */
            h1.Should().NotBeSameAs(h2);
            h1.Should().BeOfType<ConfigWithBasicProps>();
            h2.Should().BeOfType<ConfigWithBasicProps>();
        }

        [TestMethod]
        public void Trying_to_create_instances_of_a_type_with_no_default_ctor_should_throw()
        {
            /* Arrange */
            var type = typeof(ConfigInvalidCtor);

            /* Act */
            Action a = () => ConfigHelper.CreateInstance(type, false);

            /* Assert */
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.ConfigTypeCreationFailed);
        }

        [TestMethod]
        public void Suppressing_exceptions_when_trying_to_create_instances_of_invalid_types_should_instead_return_null()
        {
            /* Arrange */
            var type = typeof(ConfigInvalidCtor);

            /* Act */
            var h1 = ConfigHelper.CreateInstance(type, true);
            var h2 = ConfigHelper.CreateInstance(type.AssemblyQualifiedName, true);
            var h3 = ConfigHelper.CreateInstance("InvalidType", true);

            /* Assert */
            h1.Should().BeNull();
            h2.Should().BeNull();
            h3.Should().BeNull();
        }

        [TestMethod]
        public void Resolving_valid_typenames_should_succeed()
        {
            /* Arrange */
            var type = typeof(ConfigWithBasicProps);

            /* Act */
            var t = ConfigHelper.ResolveType(type.AssemblyQualifiedName, false);

            /* Assert */
            t.Should().Be(type);
        }

        [TestMethod]
        public void Resolving_invalid_typenames_should_throw()
        {
            /* Act */
            Action a = () => ConfigHelper.ResolveType("NoSuchType", false);

            /* Assert */
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.TypeResolutionFailed);
        }

        [TestMethod]
        public void Resolving_null_should_throw()
        {
            /* Act */
            Action a = () => ConfigHelper.ResolveType(null, false);

            /* Assert */
            a.ShouldThrow<ConfigException>().And.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.TypeResolutionFailed);
        }

        [TestMethod]
        public void Suppressing_exceptions_when_trying_to_resolve_invalid_typenames_should_instead_return_null()
        {
            /* Act */
            var t1 = ConfigHelper.ResolveType("NoSuchType", true);
            var t2 = ConfigHelper.ResolveType(null, true);

            /* Assert */
            t1.Should().BeNull("nullOnError is true and an error was expected");
            t2.Should().BeNull("nullOnError is true and an error was expected");
        }

        [TestMethod]
        public void Writing_a_section_definition_to_a_document_with_no_section_container_should_create_the_correct_structure()
        {
            /* Arrange */
            //First test a valid .Net config with no section defined (the actual data is irrelevant)
            var xmlDoc = TestConfigFactory.CreateConfig();

            /* Act */
            var sectionContainer = SelectSectionsNode(xmlDoc);

            ConfigHelper.WriteSectionDefinition(xmlDoc, "TestSection", typeof(HandlerCustom));

            var sectionDef = SelectSectionDefinitionNode(xmlDoc, "TestSection");

            /* Assert */
            sectionContainer.Should().BeNull("it should NOT be there before the call to WriteSectionDefinition");
            sectionDef.Should().NotBeNull();
            sectionDef.GetAttributeValue(ConfigElement.SectionNameAttribute, null).Should().Be("TestSection");
            sectionDef.GetAttributeValue(ConfigElement.SectionTypeAttribute, null).Should().Be(typeof(HandlerCustom).AssemblyQualifiedName);
        }

        [TestMethod]
        public void Writing_a_section_definition_to_a_document_with_an_existing_section_container_should_create_the_correct_structure()
        {
            /* Arrange */
            //Test of document with an existing section container present
            var xmlDoc = TestConfigFactory.CreateConfig(ConfigElement.SectionsNode);

            /* Act */
            var sectionContainerPre = SelectSectionsNode(xmlDoc);

            ConfigHelper.WriteSectionDefinition(xmlDoc, "TestSection", typeof(HandlerCustom));

            var sectionContainerPost = SelectSectionsNode(xmlDoc);
            var sectionDef = SelectSectionDefinitionNode(xmlDoc, "TestSection");

            /* Assert */
            sectionContainerPre.Should().NotBeNull();
            sectionContainerPost.Should().NotBeNull();
            sectionDef.Should().NotBeNull();
            sectionDef.GetAttributeValue(ConfigElement.SectionNameAttribute, null).Should().Be("TestSection");
            sectionDef.GetAttributeValue(ConfigElement.SectionTypeAttribute, null).Should().Be(typeof(HandlerCustom).AssemblyQualifiedName);
        }

        [TestMethod]
        public void Writing_a_section_definition_to_a_document_where_that_section_is_already_defined_should_update_the_existing_definition_and_not_make_duplicates()
        {
            /* Arrange */
            var xmlDoc = TestConfigFactory.CreateConfig(ConfigElement.SectionsNode);

            /* Act */
            ConfigHelper.WriteSectionDefinition(xmlDoc, "TestSection", typeof(HandlerCustom));

            //Next test a valid .Net config with the section definition already there, the section definition should remain with no duplicate created
            ConfigHelper.WriteSectionDefinition(xmlDoc, "TestSection", typeof(object));
            var sectionDef = SelectSectionDefinitionNode(xmlDoc, "TestSection");

            /* Assert */
            sectionDef.Should().NotBeNull();
            sectionDef.GetAttributeValue(ConfigElement.SectionNameAttribute, null).Should().Be("TestSection");
            sectionDef.GetAttributeValue(ConfigElement.SectionTypeAttribute, null).Should().Be(typeof(object).AssemblyQualifiedName);
        }

        [TestMethod]
        public void Writing_a_section_definition_for_appSettings_should_do_nothing()
        {
            /* Arrange */
            var xmlDoc = TestConfigFactory.CreateConfig(ConfigElement.SectionsNode);

            /* Act */
            //Adding an app settings section should not result in an entry in the section container
            ConfigHelper.WriteSectionDefinition(xmlDoc, ConfigElement.AppSettingsNode, typeof(HandlerCustom));
            var sectionDef = SelectSectionDefinitionNode(xmlDoc, ConfigElement.AppSettingsNode);

            /* Assert */
            sectionDef.Should().BeNull();
        }

        [TestMethod]
        public void Writing_a_section_definition_to_a_document_with_no_root_should_throw()
        {
            /* Arrange */
            var xmlDoc = new XDocument();

            /* Act */
            //Adding an app settings section should not result in an entry in the section container
            Action a = () => ConfigHelper.WriteSectionDefinition(xmlDoc, "Whatever", typeof(object));

            /* Assert */
            a.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void Retrieving_attributes_of_a_type_should_work()
        {
            /* Arrange */
            var t1 = typeof(AttributedClass);
            var t2 = typeof(AttributedSubClass);

            /* Act */
            var attrib1 = ConfigHelper.GetAttribute<ConfigSectionAttribute>(t1);

            var attrib2 = ConfigHelper.GetAttribute<ConfigSectionAttribute>(t2);
            var attrib3 = ConfigHelper.GetAttribute<ConfigSectionAttribute>(t2, true);

            /* Assert */
            attrib1.Should().NotBeNull();

            attrib2.Should().BeNull();
            attrib3.Should().NotBeNull();
        }

        [TestMethod]
        public void Retrieving_attributes_of_a_property_should_work()
        {
            /* Arrange */
            var t1 = typeof(AttributedClass);
            var t2 = typeof(AttributedSubClass);

            var propAttrib = t1.GetProperty("PropWithAttrib");
            var propNoAttrib = t1.GetProperty("PropNoAttrib");
            var derivedPropAttrib = t2.GetProperty("PropWithAttrib");
            var derivedPropInheritedAttrib = t2.GetProperty("PropWithInheritedAttrib");

            /* Act */
            var attrib1 = ConfigHelper.GetAttribute<ConfigSettingAttribute>(propAttrib);
            var attrib2 = ConfigHelper.GetAttribute<ConfigSettingAttribute>(propNoAttrib);

            //ConfigSettingAttribute is not inherited
            var attrib3 = ConfigHelper.GetAttribute<ConfigSettingAttribute>(derivedPropAttrib);
            var attrib4 = ConfigHelper.GetAttribute<ConfigSettingAttribute>(derivedPropAttrib, true);

            var attrib5 = ConfigHelper.GetAttribute<InheritedPropAttribute>(derivedPropInheritedAttrib);
            var attrib6 = ConfigHelper.GetAttribute<InheritedPropAttribute>(derivedPropInheritedAttrib, true);

            /* Assert */
            attrib1.Should().NotBeNull();
            attrib2.Should().BeNull();

            attrib3.Should().BeNull();
            attrib4.Should().BeNull("ConfigSettingAttribute is not inherited");

            attrib5.Should().BeNull();
            attrib6.Should().NotBeNull("InheritedPropAttribute is inherited");
        }

        private static XElement SelectSectionsNode(XDocument xmlDoc)
        {
            var nodes = xmlDoc.Root.Elements(ConfigElement.SectionsNode);

            if (nodes.Count() > 1)
            {
                throw new AssertFailedException("Multiple section definition nodes in document.");
            }

            return nodes.FirstOrDefault();
        }

        private static XElement SelectSectionDefinitionNode(XDocument xmlDoc, string sectionName)
        {
            /* Arrange */
            var container = xmlDoc.Root.Element(ConfigElement.SectionsNode);
            if (container == null)
            {
                return null;
            }

            var nodes = container.Elements(ConfigElement.SectionNode).Where(e =>
                e.HasAttribute(ConfigElement.SectionNameAttribute, sectionName) && e.Attribute(ConfigElement.SectionTypeAttribute) != null);

            if (nodes.Count() > 1)
            {
                throw new AssertFailedException("Multiple same name section nodes in document.");
            }

            return nodes.FirstOrDefault();
        }
    }
}
