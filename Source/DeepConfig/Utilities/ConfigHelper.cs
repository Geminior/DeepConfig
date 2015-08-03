namespace DeepConfig.Utilities
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;
    using DeepConfig.Core;
    using DeepConfig.Handlers;

    /// <summary>
    /// Helper class for common config related tasks.
    /// </summary>
    public static class ConfigHelper
    {
        /// <summary>
        /// Creates a config handler.
        /// </summary>
        /// <param name="handlerType">The type of the handler to create.</param>
        /// <exception cref="ArgumentNullException">If handlerType is null</exception>
        /// <exception cref="ConfigException">If handler creation fails.</exception>
        /// <returns>A handler of the specified type</returns>
        public static IExtendedConfigurationSectionHandler CreateHandler(Type handlerType)
        {
            Ensure.ArgumentNotNull(handlerType, "handlerType");

            var handler = CreateInstance(handlerType, false, ConfigErrorCode.SectionHandlerCreationFailed) as IExtendedConfigurationSectionHandler;

            if (handler == null)
            {
                throw new ConfigException(Msg.Text("Handler creation failed, type '{0}' does not implement IExtendedConfigurationSectionHandler.", handlerType.FullName), ConfigErrorCode.SectionHandlerInvalid);
            }

            return handler;
        }

        /// <summary>
        /// Creates a config type instance.
        /// </summary>
        /// <param name="typeName">The assembly qualified name of the type to create.</param>
        /// <param name="nullOnError">If <see langword="true"/> instantiation errors will not throw an exception but simply return <see langword="null"/></param>
        /// <exception cref="ArgumentNullException">If typeName is null</exception>
        /// <exception cref="ConfigException">If object creation fails and nullOnError is <see langword="false"/>.</exception>
        /// <returns>An instance of the specified type</returns>
        public static object CreateInstance(string typeName, bool nullOnError)
        {
            Ensure.ArgumentNotNullOrEmpty(typeName, "typeName");

            return CreateInstance(typeName, nullOnError, ConfigErrorCode.ConfigTypeCreationFailed);
        }

        /// <summary>
        /// Creates a config type instance.
        /// </summary>
        /// <param name="configType">The type to create an instance of.</param>
        /// <param name="nullOnError">If <see langword="true"/> instantiation errors will not throw an exception but simply return <see langword="null"/></param>
        /// <exception cref="ArgumentNullException">If configType is null</exception>
        /// <exception cref="ConfigException">If object creation fails and nullOnError is <see langword="false"/>.</exception>
        /// <returns>An instance of the specified type</returns>
        public static object CreateInstance(Type configType, bool nullOnError)
        {
            Ensure.ArgumentNotNull(configType, "configType");

            return CreateInstance(configType, nullOnError, ConfigErrorCode.ConfigTypeCreationFailed);
        }

        /// <summary>
        /// Resolves a type name to a concrete type
        /// </summary>
        /// <param name="typeName">Name of the type to resolve</param>
        /// <param name="nullOnError">If <see langword="true"/> resolution errors will not throw an exception but simply return <see langword="null"/></param>
        /// <exception cref="ConfigException">If typeName resolution fails and nullOnError is <see langword="false"/>.</exception>
        /// <returns>The type refered to by typeName</returns>
        public static Type ResolveType(string typeName, bool nullOnError)
        {
            try
            {
                return Type.GetType(typeName, true);
            }
            catch (Exception e)
            {
                if (nullOnError)
                {
                    return null;
                }

                throw new ConfigException(Msg.Text("Type resolution failed ({0}).", typeName), ConfigErrorCode.TypeResolutionFailed, e);
            }
        }

        /// <summary>
        /// Attempts to resolve the storage location for the application (Entry Assembly) under a given special folder. The path will be [root]\[company]\[product] [version] or any variation therepf depending on arguments and available meta data.
        /// </summary>
        /// <param name="root">The root special folder.</param>
        /// <param name="includeMinorVersion">if set to <c>true</c> the location path will include the minor version number in addition to the major.</param>
        /// <returns>The resolved path, or <see cref="string.Empty"/> if resolution did not succeed.</returns>
        public static string ResolveStorageLocation(Environment.SpecialFolder root, bool includeMinorVersion)
        {
            var asm = Assembly.GetEntryAssembly();
            if (asm == null)
            {
                return string.Empty;
            }

            var asmName = asm.GetName();

            var path = new StringBuilder(Environment.GetFolderPath(root).TrimEnd('\\'));
            if (path.Length == 0)
            {
                return string.Empty;
            }

            path.Append('\\');

            //We use filename chars since we also want to remove chars such as \ from each path token
            var invalidChars = Path.GetInvalidFileNameChars();

            var comp = GetAttrib<AssemblyCompanyAttribute>(asm);
            if (comp != null && !string.IsNullOrWhiteSpace(comp.Company))
            {
                path.Append(
                    EnsureValidPathToken(comp.Company, invalidChars));
                path.Append('\\');
            }

            var prod = GetAttrib<AssemblyProductAttribute>(asm);
            if (prod == null || string.IsNullOrWhiteSpace(prod.Product))
            {
                path.Append(
                    EnsureValidPathToken(asmName.Name, invalidChars));
            }
            else
            {
                path.Append(
                    EnsureValidPathToken(prod.Product, invalidChars));
            }

            path.Append(" ");
            path.Append(asmName.Version.Major);
            if (includeMinorVersion)
            {
                path.Append('.');
                path.Append(asmName.Version.Minor);
            }

            return path.ToString();
        }

        /// <summary>
        /// Get a specific attribute from a property if it exists
        /// </summary>
        /// <typeparam name="T">The type of attribute to look for</typeparam>
        /// <param name="prop">The property to inspect</param>
        /// <param name="inherit">Whether to accept inherited attributes</param>
        /// <returns>The attribute if it exists, otherwise null.</returns>
        public static T GetAttribute<T>(this PropertyInfo prop, bool inherit = false) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(prop, typeof(T), inherit);
        }

        /// <summary>
        /// Get a specific attribute from a type if it exists
        /// </summary>
        /// <typeparam name="T">The type of attribute to look for</typeparam>
        /// <param name="t">The type to inspect</param>
        /// <param name="inherit">Whether to accept inherited attributes</param>
        /// <returns>The attribute if it exists, otherwise null.</returns>
        public static T GetAttribute<T>(this Type t, bool inherit = false) where T : Attribute
        {
            return (T)t.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        /// <summary>
        /// Writes a Section definition <c>&lt;section name="NameOfSection" type="SectionHandlerAssemblyQualifiedName" /&gt;</c>
        /// </summary>
        /// <param name="xmlDoc">The Xml Document to write to.</param>
        /// <param name="sectionXmlName">The xml name of the section to write a definition for.</param>
        /// <param name="handlerType">The type of the handler to write a definition for.</param>
        /// <exception cref="ArgumentException">If either argument is null or empty or if the xml document has no root</exception>
        internal static void WriteSectionDefinition(XDocument xmlDoc, string sectionXmlName, Type handlerType)
        {
            Ensure.ArgumentNotNull(sectionXmlName, "sectionXmlName");
            Ensure.ArgumentNotNull(handlerType, "handlerType");
            Ensure.ArgumentNotNull(xmlDoc, "xmlDoc");
            Ensure.ArgumentNotNull(xmlDoc.Root, "xmlDoc.Root");

            //Check if there is a header to write
            if (sectionXmlName == ConfigElement.AppSettingsNode)
            {
                return;
            }

            //Get the sections node or create it
            var sectionsNode = xmlDoc.Root.Element(ConfigElement.SectionsNode);
            if (sectionsNode == null)
            {
                sectionsNode = new XElement(ConfigElement.SectionsNode);
                xmlDoc.Root.AddFirst(sectionsNode);
            }

            //Check if its already there
            var section = sectionsNode.Element(ConfigElement.SectionNode, e => e.HasAttribute(ConfigElement.SectionNameAttribute, sectionXmlName));

            //Only create if it does not already exist
            if (section == null)
            {
                sectionsNode.Add(new XElement(
                    ConfigElement.SectionNode,
                    new XAttribute(ConfigElement.SectionNameAttribute, sectionXmlName),
                    new XAttribute(ConfigElement.SectionTypeAttribute, handlerType.AssemblyQualifiedName)));
            }
            else
            {
                //Make sure the handler is correct
                section.SetAttributeValue(ConfigElement.SectionTypeAttribute, handlerType.AssemblyQualifiedName);
            }
        }

        private static string EnsureValidPathToken(string candidate, char[] invalidChars)
        {
            var path = new StringBuilder(candidate.Trim());

            foreach (var c in invalidChars)
            {
                path.Replace(c, '_');
            }

            return path.ToString();
        }

        private static T GetAttrib<T>(Assembly a) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(a, typeof(T));
        }

        private static object CreateInstance(string typeName, bool nullOnError, ConfigErrorCode errCause)
        {
            Type configType = ResolveType(typeName, nullOnError);

            if (configType == null)
            {
                //nullOnError is always true in this case since otherwise an exception would have been thrown by the call to ResolveType
                return null;
            }

            return CreateInstance(configType, nullOnError, errCause);
        }

        private static object CreateInstance(Type configType, bool nullOnError, ConfigErrorCode errCause)
        {
            try
            {
                return Activator.CreateInstance(configType, true);
            }
            catch (Exception e)
            {
                if (nullOnError)
                {
                    return null;
                }

                throw new ConfigException(Msg.Text("Object creation failed ({0}), see nested exception for details.", configType.FullName), errCause, e);
            }
        }
    }
}
