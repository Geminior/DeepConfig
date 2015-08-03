namespace DeepConfig.Handlers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig.Core;
    using DeepConfig.Cryptography;
    using DeepConfig.Utilities;

    /// <summary>
    /// Reads and writes config types marked with the <see cref="ConfigSectionAttribute"/>.
    /// </summary>
    public sealed class GenericTypeSectionHandler : IExtendedConfigurationSectionHandler
    {
        /// <summary>
        /// Limit to number of items a section can contain before a circular reference is suspected
        /// </summary>
        private const int MaxItemsLevel = 500;

        /// <summary>
        /// Counter used in the check for possible circular references
        /// </summary>
        private int _recursionGuard;

        /// <summary>
        /// The crypto provider in use
        /// </summary>
        private ICryptoProvider _cryptoProvider;

        /// <summary>
        /// Current list of errors encountered
        /// </summary>
        private List<ConfigError> _errorList;

        /// <summary>
        /// Gets a list of errors encountered while reading or writing a section.
        /// </summary>
        public IEnumerable<ConfigError> Errors
        {
            get
            {
                if (_errorList == null)
                {
                    yield break;
                }

                foreach (var e in _errorList)
                {
                    yield return e;
                }
            }
        }

        /// <summary>
        /// Writes configuration settings from a generic config object decorated with the <see cref="ConfigSectionAttribute"/> attribute.
        /// </summary>
        /// <param name="sectionXmlName">The xml name of the section to be written.</param>
        /// <param name="sectionSource">The generic config object containing the settings to be put in the section.</param>
        /// <param name="cryptoProvider">An <see cref="ICryptoProvider"/> instance that can be used to encrypt settings. May be null</param>
        /// <param name="encryptAll">If <see langword="true" /> all settings should be encrypted (if not already encrypted)</param>
        /// <returns>The xml element for the section.</returns>
        /// <exception cref="ConfigException">If sectionSource is not decorated with the <see cref="ConfigSectionAttribute"/> attribute or sectionSource has other config types nested more than 50 levels deep.</exception>
        /// <exception cref="ArgumentNullException">If sectionName is null or empty, or settingsSource is null</exception>
        public XElement WriteSection(string sectionXmlName, object sectionSource, ICryptoProvider cryptoProvider, bool encryptAll)
        {
            Ensure.ArgumentNotNullOrEmpty(sectionXmlName, "sectionXmlName");
            Ensure.ArgumentNotNull(sectionSource, "sectionSource");

            //Create the list to pick up errors during write
            _errorList = new List<ConfigError>();
            _recursionGuard = 0;

            var sectionAttrib = sectionSource.GetType().GetAttribute<ConfigSectionAttribute>(true);
            if (sectionAttrib == null)
            {
                RegisterError(
                    sectionXmlName,
                    "Only objects marked with the default 'ConfigSectionAttribute' can be handled by this handler",
                    ConfigErrorCode.MissingConfigSectionAttribute);

                return null;
            }
            else if (sectionAttrib.HasCustomHandler)
            {
                RegisterError(
                    sectionXmlName,
                    "Only objects marked with the default 'ConfigSectionAttribute' (no custom handler) can be handled by this handler",
                    ConfigErrorCode.SectionHandlerMismatch);

                return null;
            }

            _cryptoProvider = cryptoProvider;

            return WriteSettingsRecursive(sectionXmlName, sectionSource, encryptAll, true);
        }

        /// <summary>
        /// Creates a concrete config object from an xml element of a config file.
        /// </summary>
        /// <param name="section">The XElement that contains the configuration information from the configuration file. Provides direct access to the XML contents of the configuration section</param>
        /// <param name="cryptoProvider">An <see cref="ICryptoProvider"/> instance that can be used to decrypt settings. May be null</param>
        /// <param name="decryptAll">If <see langword="true" /> all settings will be decrypted.</param>
        /// <returns>A concrete config object instance containing the configuration data or null.</returns>
        /// <exception cref="ArgumentNullException">May throw if section is null</exception>
        /// <exception cref="ConfigException">If section does not have a <c>type</c> attribute or if the specified type cannot be created.</exception>
        public object ReadSection(XElement section, ICryptoProvider cryptoProvider, bool decryptAll)
        {
            if (section == null)
            {
                return null;
            }

            //Create the list to pick up errors during write
            _errorList = new List<ConfigError>();

            //Get the type to instantiate
            Type configType = GetConfigType(section, null);
            if (configType == null)
            {
                RegisterError(section.Name.LocalName, "Section either has no type attribute or it is empty or it referes to a type that cannot be resolved.", ConfigErrorCode.InvalidNode);
                return null;
            }

            _cryptoProvider = cryptoProvider;

            return CreateRecursive(section, configType, decryptAll);
        }

        private static Type GetConfigType(XElement section, Type defaultType)
        {
            var typeName = section.GetAttributeValue(ConfigElement.SectionTypeAttribute);
            if (string.IsNullOrEmpty(typeName))
            {
                return defaultType;
            }

            var t = ConfigHelper.ResolveType(typeName, true);

            if (t == null)
            {
                return defaultType;
            }

            return t;
        }

        private static bool IsListSetting(Type settingType)
        {
            //Checks for IEnumerable first to avoid doing the generics check unnecessarily
            return typeof(IEnumerable).IsAssignableFrom(settingType) && IsAssignableToGenericType(settingType, typeof(ICollection<>));
        }

        [ExcludeFromCodeCoverage]
        private static bool IsAssignableToGenericType(Type candidate, Type genericType)
        {
            if (candidate == null || genericType == null)
            {
                return false;
            }

            return candidate == genericType
              || MapsToGenericTypeDefinition(candidate, genericType)
              || HasInterfaceThatMapsToGenericTypeDefinition(candidate, genericType)
              || IsAssignableToGenericType(candidate.BaseType, genericType);
        }

        private static bool HasInterfaceThatMapsToGenericTypeDefinition(Type candidate, Type genericType)
        {
            return candidate
              .GetInterfaces()
              .Where(it => it.IsGenericType)
              .Any(it => it.GetGenericTypeDefinition() == genericType);
        }

        private static bool MapsToGenericTypeDefinition(Type candidate, Type genericType)
        {
            return genericType.IsGenericTypeDefinition
              && candidate.IsGenericType
              && candidate.GetGenericTypeDefinition() == genericType;
        }

        private XElement WriteSettingsRecursive(string sectionXmlName, object settingsSource, bool forceCrypto, bool writeType)
        {
            Type configType = settingsSource.GetType();

            //Create the section node
            var section = new XElement(sectionXmlName);

            //Only write the type attribute on the section element
            if (writeType)
            {
                section.Add(new XAttribute(ConfigElement.SectionTypeAttribute, configType.AssemblyQualifiedName));
            }

            //Create the setting nodes
            PropertyInfo[] properties = configType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                var configAttrib = property.GetAttribute<ConfigSettingAttribute>();
                if (configAttrib == null)
                {
                    continue;
                }

                //Ignore indexer properties
                if (property.GetIndexParameters().Length > 0)
                {
                    RegisterError(sectionXmlName, Msg.Text("Property '{0}' on class '{1}' is an indexer which is not supported.", property.Name, configType.FullName), ConfigErrorCode.InvalidConfigSettingAttribute);
                    continue;
                }

                //Property cannot be read
                if (!property.CanRead)
                {
                    RegisterError(sectionXmlName, Msg.Text("Property '{0}' on class '{1}' has no getter.", property.Name, configType.FullName), ConfigErrorCode.MissingGetter);
                    continue;
                }

                //Get the actual value
                object rawValue = property.GetValue(settingsSource, (BindingFlags.Public | BindingFlags.NonPublic), null, null, CultureInfo.InvariantCulture);
                if (rawValue == null)
                {
                    continue;
                }

                //Ignore self reference
                if (object.Equals(settingsSource, rawValue))
                {
                    RegisterError(sectionXmlName, Msg.Text("Property '{0}' on references its own instance.", property.Name), ConfigErrorCode.InvalidConfigValue);
                    continue;
                }

                //Get the name
                string settingName = configAttrib.SettingName;
                if (settingName == null)
                {
                    settingName = property.Name;
                }
                else
                {
                    //Ensure valid xml
                    settingName = XmlConvert.EncodeLocalName(settingName);
                }

                //Get encryption setting
                bool encrypt = (configAttrib.Encrypt || forceCrypto);

                WriteValue(section, settingName, rawValue, property.PropertyType, encrypt, configAttrib.Description);
            } //end foreach

            return section;
        }

        private void WriteValue(XElement section, string settingName, object value, Type declaredType, bool encrypt, string description = null)
        {
            Type settingType = value.GetType();
            XElement valueNode = null;

            //Get value from valid types
            if (settingType.Equals(typeof(DateTime)))
            {
                valueNode = WriteSettingValue(settingName, XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind), encrypt);
            }
            else if (typeof(IConvertible).IsAssignableFrom(settingType))
            {
                valueNode = WriteSettingValue(settingName, ((IConvertible)value).ToString(CultureInfo.InvariantCulture), encrypt);
            }
            else if (settingType.Equals(typeof(Guid)))
            {
                valueNode = WriteSettingValue(settingName, value.ToString(), encrypt);
            }
            else if (settingType.Equals(typeof(TimeSpan)))
            {
                valueNode = WriteSettingValue(settingName, ((TimeSpan)value).ToString(), encrypt);
            }
            else if (settingType.Equals(typeof(Uri)))
            {
                valueNode = WriteSettingValue(settingName, ((Uri)value).OriginalString, encrypt);
            }
            else if (IsListSetting(settingType))
            {
                RecursionGuard(section.Name.LocalName);

                //If the actual type is different (a derivative of) the property type we need to write this so it can be recreated on read, unless its the default representation which is List<> in which case there is no need to clutter the xml with type info.
                bool writeType = (declaredType.IsAbstract && settingType != typeof(List<>));
                valueNode = WriteList(settingName, (dynamic)value, writeType, encrypt);
            }
            else
            {
                RecursionGuard(section.Name.LocalName);

                //If we have a sub config section add that recursively
                var valueSectionAttrib = settingType.GetAttribute<ConfigSectionAttribute>(true);
                if (valueSectionAttrib != null)
                {
                    if (!valueSectionAttrib.HasCustomHandler)
                    {
                        //If the actual type is different (a derivative of) the property type we need to write this so it can be recreated on read
                        bool writeType = (settingType != declaredType);
                        valueNode = WriteSettingsRecursive(settingName, value, encrypt, writeType);
                    }
                    else
                    {
                        try
                        {
                            //Create custom handler
                            var customHandler = ConfigHelper.CreateHandler(valueSectionAttrib.SectionHandlerType);

                            valueNode = customHandler.WriteSection(settingName, value, _cryptoProvider, encrypt);

                            if (customHandler.Errors != null)
                            {
                                _errorList.AddRange(customHandler.Errors);
                            }
                        }
                        catch (Exception e)
                        {
                            RegisterError(
                            section.Name.LocalName,
                            Msg.Text("Setting '{0}' could not be written since its handler failed. See nested exception for details.", settingName),
                            ConfigErrorCode.SectionHandlerMismatch,
                            e);
                        }
                    }
                }
                else
                {
                    //We cannot write this type
                    RegisterError(
                        section.Name.LocalName,
                        Msg.Text("Setting '{0}' cannot be written as the handler cannot process objects of type '{1}'", settingName, settingType.FullName),
                        ConfigErrorCode.InvalidConfigType);
                }
            }

            //Add the value to the section with its description if available
            if (valueNode != null)
            {
                if (!string.IsNullOrEmpty(description))
                {
                    section.Add(new XComment(description));
                }

                section.Add(valueNode);
            }
        }

        private XElement WriteSettingValue(string settingName, string value, bool encrypt)
        {
            if (encrypt && _cryptoProvider != null)
            {
                value = _cryptoProvider.Encrypt(value);
            }

            return new XElement(
                settingName,
                new XAttribute(ConfigElement.SettingValueAttribute, value));
        }

        private XElement WriteList<T>(string settingName, IEnumerable<T> list, bool writeType, bool encrypt)
        {
            var listNode = new XElement(settingName);

            if (writeType)
            {
                listNode.Add(new XAttribute(ConfigElement.SectionTypeAttribute, list.GetType().AssemblyQualifiedName));
            }

            if (list.Any())
            {
                var itemType = typeof(T);

                //Write each item in the list
                foreach (object item in list)
                {
                    WriteValue(listNode, ConfigElement.ListElementNode, item, itemType, encrypt);
                }
            }

            return listNode;
        }

        private object CreateRecursive(XElement section, Type configType, bool forceDecrypt)
        {
            //Create the object instance
            object result;
            try
            {
                result = ConfigHelper.CreateInstance(configType, false);
            }
            catch (Exception e)
            {
                RegisterError(section.Name.LocalName, "Unable to create section, see nested exception for details.", ConfigErrorCode.ConfigTypeCreationFailed, e);
                return null;
            }

            PopulateRecursive(section, result, forceDecrypt);

            return result;
        }

        private void PopulateRecursive(XElement section, object settingInstance, bool forceDecrypt)
        {
            Type configType = settingInstance.GetType();

            PropertyInfo[] properties = configType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                //We only process properties marked as settings
                var configAttrib = property.GetAttribute<ConfigSettingAttribute>();
                if (configAttrib == null)
                {
                    continue;
                }

                //Ignore indexer properties
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                //Get the name
                string settingName = configAttrib.SettingName;
                if (settingName == null)
                {
                    settingName = property.Name;
                }
                else
                {
                    //Ensure valid xml
                    settingName = XmlConvert.EncodeLocalName(settingName);
                }

                //Get the element if present otherwise move on
                var settingNode = section.Element(settingName);
                if (settingNode == null)
                {
                    continue;
                }

                //Get decryption setting
                bool decrypt = (configAttrib.Encrypt || forceDecrypt);

                //Some properties may have a concrete type specified if the actual value is of a derived type of the property type
                Type propertyType = GetConfigType(settingNode, property.PropertyType);

                //If we can write the value we do so otherwise see if we have a reference type with a getter
                if (property.CanWrite)
                {
                    //Get the value
                    var propertyValue = CreateInstance(settingNode, propertyType, decrypt);

                    //Set the property
                    if (propertyValue != null)
                    {
                        property.SetValue(settingInstance, propertyValue, (BindingFlags.Public | BindingFlags.NonPublic), null, null, CultureInfo.InvariantCulture);
                    }
                }
                else if (property.CanRead && (propertyType.IsClass || propertyType.IsInterface) && !(propertyType.Equals(typeof(string))) && !propertyType.IsEnum)
                {
                    var propertyValue = property.GetValue(settingInstance, (BindingFlags.Public | BindingFlags.NonPublic), null, null, CultureInfo.InvariantCulture);

                    if (propertyValue != null)
                    {
                        //Instance of reference type retrieved, we just need to set its properties
                        PopulateInstance(settingNode, propertyType, propertyValue, decrypt);
                    }
                    else
                    {
                        RegisterError(
                            section.Name.LocalName,
                            Msg.Text("Property '{0}' on class '{1}' has no setter and getter returns null. Either implement a setter or lazy load in the getter.", property.Name, configType.FullName),
                            ConfigErrorCode.MissingSetter);
                    }
                }
            } // end foreach
        }

        private object CreateInstance(XElement settingNode, Type settingType, bool decrypt)
        {
            try
            {
                //Get the value from the element
                string stringValue = GetSettingValue(settingNode, decrypt);

                //Get the actual typed value based on property type
                if (settingType.Equals(typeof(string)))
                {
                    return stringValue;
                }
                else if (settingType.IsEnum)
                {
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return null;
                    }

                    return Enum.Parse(settingType, stringValue, true);
                }
                else if (settingType.Equals(typeof(DateTime)))
                {
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return null;
                    }

                    return XmlConvert.ToDateTime(stringValue, XmlDateTimeSerializationMode.RoundtripKind);
                }
                else if (typeof(IConvertible).IsAssignableFrom(settingType))
                {
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return null;
                    }

                    return Convert.ChangeType(stringValue, settingType, CultureInfo.InvariantCulture);
                }
                else if (settingType.Equals(typeof(Guid)))
                {
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return null;
                    }

                    return XmlConvert.ToGuid(stringValue);
                }
                else if (settingType.Equals(typeof(TimeSpan)))
                {
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return null;
                    }

                    return TimeSpan.Parse(stringValue, CultureInfo.InvariantCulture);
                }
                else if (settingType.Equals(typeof(Uri)))
                {
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return null;
                    }

                    return new Uri(stringValue);
                }
                else if (IsAssignableToGenericType(settingType, typeof(Nullable<>)))
                {
                    //While this could be checked as the first check and simply change the settingType, this way achieves the best overall performance for mixed config types
                    settingType = settingType.GetGenericArguments()[0];
                    return CreateInstance(settingNode, settingType, decrypt);
                }
                else if (IsListSetting(settingType))
                {
                    return CreateList(settingNode, settingType, decrypt);
                }
                else
                {
                    return CreateComplexType(settingNode, settingType, decrypt);
                }
            }
            catch (Exception e)
            {
                RegisterError(
                    settingNode.Parent.Name.LocalName,
                    Msg.Text("Setting '{0}' has an invalid value. See nested exception for details.", XmlConvert.DecodeName(settingNode.Name.LocalName)),
                    ConfigErrorCode.InvalidConfigValue,
                    e);

                return null;
            }
        }

        private object CreateComplexType(XElement settingNode, Type settingType, bool decrypt)
        {
            var valueSectionAttrib = settingType.GetAttribute<ConfigSectionAttribute>(true);
            if (valueSectionAttrib == null)
            {
                //We cannot create this type
                RegisterError(
                    settingNode.Parent.Name.LocalName,
                    Msg.Text("Setting '{0}' cannot be read as the handler cannot process objects of type '{1}'.", XmlConvert.DecodeName(settingNode.Name.LocalName), settingType.FullName),
                    ConfigErrorCode.InvalidConfigType);

                return null;
            }

            //If we cannot instantiate, throw
            if (settingType.IsAbstract)
            {
                RegisterError(
                    settingNode.Parent.Name.LocalName,
                    Msg.Text("Setting '{0}' cannot be set since type '{1}' cannot be instantiated.", XmlConvert.DecodeName(settingNode.Name.LocalName), settingType.FullName),
                    ConfigErrorCode.InvalidConfigType);

                return null;
            }

            if (!valueSectionAttrib.HasCustomHandler)
            {
                return CreateRecursive(settingNode, settingType, decrypt);
            }
            else
            {
                try
                {
                    //Create custom handler
                    var customHandler = ConfigHelper.CreateHandler(valueSectionAttrib.SectionHandlerType);

                    var val = customHandler.ReadSection(settingNode, _cryptoProvider, decrypt);

                    if (customHandler.Errors != null)
                    {
                        _errorList.AddRange(customHandler.Errors);
                    }

                    return val;
                }
                catch (Exception e)
                {
                    RegisterError(
                    settingNode.Parent.Name.LocalName,
                    Msg.Text("Setting '{0}' could not be set since its handler failed. See nested exception for details.", XmlConvert.DecodeName(settingNode.Name.LocalName)),
                    ConfigErrorCode.SectionHandlerMismatch,
                    e);

                    return null;
                }
            }
        }

        private object CreateList(XElement settingNode, Type listType, bool decrypt)
        {
            //Get the actual list type
            listType = GetConfigType(settingNode, listType);

            //If we cannot instantiate, throw
            if (listType.IsInterface)
            {
                var itemType = listType.GetGenericArguments()[0];
                listType = typeof(List<>).MakeGenericType(itemType);
            }
            else if (listType.IsAbstract)
            {
                RegisterError(
                    settingNode.Parent.Name.LocalName,
                    Msg.Text("Setting '{0}' cannot be set since type '{1}' is abstract and cannot be instantiated.", XmlConvert.DecodeName(settingNode.Name.LocalName), listType.FullName),
                    ConfigErrorCode.InvalidConfigType);

                return null;
            }

            //First create the list
            dynamic list;
            try
            {
                list = ConfigHelper.CreateInstance(listType, false);
            }
            catch (Exception e)
            {
                RegisterError(settingNode.Parent.Name.LocalName, "Unable to create list, see nested exception for details.", ConfigErrorCode.ConfigTypeCreationFailed, e);
                return null;
            }

            PopulateList(settingNode, list, decrypt);

            return list;
        }

        private void PopulateList<T>(XElement settingNode, ICollection<T> list, bool decrypt)
        {
            var itemType = typeof(T);

            //Create the actual list elements
            var items = settingNode.Elements(ConfigElement.ListElementNode);
            foreach (var item in items)
            {
                Type actualItemType = GetConfigType(item, itemType);

                T itemValue = (T)CreateInstance(item, actualItemType, decrypt);
                if (itemValue != null)
                {
                    list.Add(itemValue);
                }
            }
        }

        private void PopulateInstance(XElement settingNode, Type settingType, object settingInstance, bool decrypt)
        {
            if (IsListSetting(settingType))
            {
                PopulateList(settingNode, (dynamic)settingInstance, decrypt);
            }
            else
            {
                var valueSectionAttrib = settingType.GetAttribute<ConfigSectionAttribute>(true);
                if (valueSectionAttrib != null)
                {
                    if (!valueSectionAttrib.HasCustomHandler)
                    {
                        PopulateRecursive(settingNode, settingInstance, decrypt);
                    }
                    else
                    {
                        RegisterError(
                            settingNode.Parent.Name.LocalName,
                            Msg.Text("Setting '{0}' cannot be set as the property has no setter which is required for types like '{1}' with a custom handler ({2}).", XmlConvert.DecodeName(settingNode.Name.LocalName), settingType.FullName, valueSectionAttrib.SectionHandlerType.FullName),
                            ConfigErrorCode.MissingSetter);
                    }
                }
                else
                {
                    //We cannot create this type
                    RegisterError(
                        settingNode.Parent.Name.LocalName,
                        Msg.Text("Setting '{0}' cannot be read as the handler cannot process objects of type '{1}'.", XmlConvert.DecodeName(settingNode.Name.LocalName), settingType.FullName),
                        ConfigErrorCode.InvalidConfigType);
                }
            }
        }

        private string GetSettingValue(XElement settingNode, bool decrypt)
        {
            //Get the value if available
            var value = settingNode.GetAttributeValue(ConfigElement.SettingValueAttribute);
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (decrypt && _cryptoProvider != null)
            {
                return _cryptoProvider.Decrypt(value);
            }

            return value;
        }

        private void RegisterError(string section, string message, ConfigErrorCode code, Exception e = null)
        {
            message = string.Concat("[", section, "] ", message);
            _errorList.Add(new ConfigError(code, message, e));
        }

        private void RecursionGuard(string section)
        {
            if (_recursionGuard > MaxItemsLevel)
            {
                RegisterError(
                    section,
                    Msg.Text("Reading a config section has reached over {0} object instances, which is likely due to circular references.", MaxItemsLevel),
                    ConfigErrorCode.ConfigTypeCircularReferenceSuspected);

                //This exception must be thrown when encountered
                throw new ConfigException(_errorList);
            }

            _recursionGuard++;
        }
    }
}
