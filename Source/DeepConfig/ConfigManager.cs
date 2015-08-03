namespace DeepConfig
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using DeepConfig.Core;
    using DeepConfig.Cryptography;
    using DeepConfig.Handlers;
    using DeepConfig.Providers;
    using DeepConfig.Utilities;

    /// <summary>
    /// Class for reading and writing configuration settings.
    /// </summary>
    /// <threadsafety static="true" instance="true">
    /// Class is safe for multithreaded operations on class as well as instance methods.
    /// </threadsafety>
    internal sealed class ConfigManager : IConfigManager
    {
        private IConfigProvider _provider;
        private ICryptoProvider _cryptoProvider;

        private object _lock = new object();

        private XDocument _cfgDoc;
        private Dictionary<string, XElement> _sections;
        private Dictionary<string, Type> _sectionHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigManager"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        private ConfigManager(IConfigProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Raised when a setting is changed
        /// </summary>
        public event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        /// <summary>
        /// Gets the <see cref="IConfigProvider"/> used by this instance.
        /// </summary>
        public IConfigProvider ConfigProvider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Gets the <see cref="DeepConfig.Cryptography.ICryptoProvider"/> to use for Encrypting/Decrypting values.
        /// <para>This is set through the configuration xml itself using <see cref="SetCryptoProvider(System.Type)"/>.</para>
        /// <para>If not set this defaults to a <see cref="DeepConfig.Cryptography.AesCryptoProvider"/></para>
        /// </summary>
        public ICryptoProvider CryptoProvider
        {
            get { return _cryptoProvider; }
        }

        /// <summary>
        /// Gets a list with the names of all sections in the configuration xml.
        /// <para>
        /// The list is disconnected from the ConfigManager and thus safe to iterate over even if changes are made to the config in the meantime.
        /// </para>
        /// </summary>
        public IEnumerable<string> Sections
        {
            get
            {
                lock (_lock)
                {
                    return _sections.Keys.ToList();
                }
            }
        }

        /// <summary>
        /// Gets a list with the names of the sections in the configuration xml that are supported by this configuration framework
        /// <para>
        /// The list is disconnected from the ConfigManager and thus safe to iterate over even if changes are made to the config in the meantime.
        /// </para>
        /// </summary>
        public IEnumerable<string> SupportedSections
        {
            get
            {
                lock (_lock)
                {
                    return (from s in _sectionHandlers
                            where typeof(IExtendedConfigurationSectionHandler).IsAssignableFrom(s.Value)
                            select s.Key).ToList();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration has been altered by another source since this config instance was loaded.
        /// <para>
        /// If true, saving the current instance will overwrite any changes made by other sources.
        /// </para>
        /// </summary>
        public bool ModifiedSinceLoad
        {
            get { return _provider.ModifiedSinceLoad; }
        }

        /// <summary>
        /// Refreshes this instance from its associated provider.
        /// <para>
        /// Useful if changes were made directly to a configuration source by other processes.
        /// </para>
        /// </summary>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the provider fails to load.
        /// <para>Or if ICryptoProvider instantiation fails.</para>
        /// </exception>
        public void Reload()
        {
            lock (_lock)
            {
                _provider.Initialize();

                //Get sections
                GetSections();

                //Init crypto provider
                InitCryptoProvider();
            }
        }

        /// <summary>
        /// Gets the settings of the appSettings section
        /// </summary>
        /// <returns>A <see cref="NameValueSettings"/> containing the settings. This is never null but may be empty.
        /// <para>
        /// The collection is disconnected from the ConfigManager so changes made to it will not affect the ConfigManager.
        /// <para>To set the setting, <see cref="SetAppSettings(NameValueSettings)"/> must be called.</para>
        /// </para>
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is not a candidate to become a property, since it creates a disconnected object")]
        public NameValueSettings GetAppSettings()
        {
            var appSettings = GetSettings<NameValueSettings>(ConfigElement.AppSettingsNode);
            if (appSettings == null)
            {
                appSettings = new NameValueSettings();
            }

            return appSettings;
        }

        /// <summary>
        /// Sets the settings of the appSettings section
        /// </summary>
        /// <param name="settings">The settings to set</param>
        public void SetAppSettings(NameValueSettings settings)
        {
            SetSettings(ConfigElement.AppSettingsNode, settings);
        }

        /// <summary>
        /// Sets the settings of the appSettings section
        /// </summary>
        /// <param name="settings">The settings to set</param>
        public void SetAppSettings(NameValueCollection settings)
        {
            SetSettings(ConfigElement.AppSettingsNode, settings);
        }

        /// <overloads>Get the settings of all sections of a given type.</overloads>
        /// <summary>
        /// Gets the settings of all sections of a particular type.
        /// </summary>
        /// <typeparam name="T">The type of section to get.</typeparam>
        /// <returns>A list containing each of the sections that match the type.</returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the handler for the Section type specified in the configuration xml could not be created (containing Assembly not loaded, missing privileges etc.).
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler for the section(s) does not match the section(s).
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler does not implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        public List<T> GetSettingsList<T>() where T : class
        {
            Type sectionType = typeof(T);

            var settings = new List<T>();

            lock (_lock)
            {
                foreach (var sectionName in _sections.Keys)
                {
                    var section = _sections[sectionName];

                    string typeName = section.GetAttributeValue(ConfigElement.SectionTypeAttribute);
                    if (typeName != null)
                    {
                        Type candidateType = ConfigHelper.ResolveType(typeName, true);
                        if ((candidateType != null) && (sectionType.Equals(candidateType)))
                        {
                            settings.Add((T)GetSettings(sectionName));
                        }
                    }
                }
            }

            return settings;
        }

        /// <overloads>Get the settings of a section.</overloads>
        /// <summary>
        /// Gets the settings of a section of a particular type, using the type name as the section name.
        /// <para>This is in other words a singleton in regards to this manager and its provider. See also <see cref="ConfigSectionAttribute.IsSingleton"/>.</para>. 
        /// </summary>
        /// <typeparam name="T">The type of section to get.</typeparam>
        /// <returns>The setting of the given type or <see langword="null" />.</returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the handler for the Section type specified in the configuration xml could not be created (containing Assembly not loaded, missing privileges etc.).
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler for the section(s) does not match the section(s).
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler does not implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        public T GetSettings<T>() where T : class
        {
            return (T)this.GetSettings(typeof(T).Name);
        }

        /// <summary>
        /// Gets the settings for a particular section.
        /// </summary>
        /// <param name="section">The section to retrieve.</param>
        /// <typeparam name="T">The type of section to get.</typeparam>
        /// <returns>A specialized object containing the configuration data of the section or null if the section was not found.
        /// <para>
        /// The data object is disconnected from the ConfigManager so changes made to it will not affect the ConfigManager.
        /// </para>
        /// </returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the handler for the Section specified in the configuration xml could not be created (containing Assembly not loaded, missing privileges etc.).
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler for the section does not match the section.
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler does not implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If section is null.</exception>
        public T GetSettings<T>(Enum section) where T : class
        {
            Ensure.ArgumentNotNull(section, "section");

            string sectionName = section.ToString();

            return (T)GetSettings(sectionName);
        }

        /// <summary>
        /// Gets the settings for a particular section.
        /// </summary>
        /// <param name="sectionName">The name of the section to retrieve.</param>
        /// <typeparam name="T">The type of section to get.</typeparam>
        /// <returns>A specialized object containing the configuration data of the section or null if the section was not found.
        /// <para>
        /// The data object is disconnected from the ConfigManager so changes made to it will not affect the ConfigManager.
        /// </para>
        /// </returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the handler for the Section specified in the configuration xml could not be created (containing Assembly not loaded, missing privileges etc.).
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler for the section does not match the section.
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler does not implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If section is null or empty.</exception>
        public T GetSettings<T>(string sectionName) where T : class
        {
            Ensure.ArgumentNotNullOrEmpty(sectionName, "sectionName");

            return (T)GetSettings(sectionName);
        }

        /// <summary>
        /// Gets the settings for a particular section.
        /// </summary>
        /// <param name="sectionName">The name of the section to retrieve. Note section names are case-sensitive!</param>
        /// <returns>A specialized object containing the configuration data of the section or null if the section was not found.
        /// <para>
        /// The data object is disconnected from the ConfigManager so changes made to it will not affect the ConfigManager.
        /// </para>
        /// </returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the handler for the Section specified in the configuration xml could not be created (containing Assembly not loaded, missing privileges etc.).
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler for the section does not match the section.
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler does not implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If sectionName is null.</exception>
        /// <exception cref="ArgumentException">If sectionName is empty.</exception>
        /// <exception cref="ConfigException">If the section handler fails</exception>
        public object GetSettings(string sectionName)
        {
            Ensure.ArgumentNotNullOrEmpty(sectionName, "sectionName");

            Type handlerType;
            XElement node;

            lock (_lock)
            {
                //If there is no handler defined we cannot create anything
                if (!_sectionHandlers.TryGetValue(sectionName, out handlerType))
                {
                    return null;
                }

                if (handlerType == null)
                {
                    throw new ConfigException(Msg.Text("The handler for section '{0}' could not be resolved.", sectionName), ConfigErrorCode.TypeResolutionFailed);
                }

                if (!_sections.TryGetValue(sectionName, out node))
                {
                    return null;
                }
            }

            //Create a handler instance
            var handler = ConfigHelper.CreateHandler(handlerType);

            //Always create a new object disconnected from the manager so changes made to the returned object does not alter the settings stored here
            object section = null;

            try
            {
                section = handler.ReadSection(node, _cryptoProvider, false);
            }
            catch (Exception e)
            {
                throw new ConfigException(Msg.Text("[{0}] Section creation failed, see nested exception for details.", sectionName), ConfigErrorCode.SectionHandlerMismatch, e);
            }

            if (handler.Errors.Any())
            {
                throw new ConfigException(handler.Errors);
            }

            return section;
        }

        /// <overloads>Sets the settings of a given section</overloads>
        /// <summary>
        /// Sets the settings of a given section of a certain type.
        /// <para>This is in other words a singleton in regards to this manager and its provider. See also <see cref="ConfigSectionAttribute.IsSingleton"/>.</para>. 
        /// </summary>
        /// <param name="settings">The settings to set.</param>
        /// <typeparam name="T">The config type</typeparam>
        /// <exception cref="DeepConfig.ConfigException">
        /// If there is no handler defined for the section
        /// <para>- or -</para>
        /// <para>
        /// If the handler for the Section specified in the configuration xml could not be created (containing Assembly not loaded, missing privileges etc.).
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler for the section does not match the section.
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler does not implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If settings is null.</exception>
        public void SetSettings<T>(T settings) where T : class
        {
            SetSettings(typeof(T).Name, settings);
        }

        /// <summary>
        /// Sets the settings of a given section.
        /// </summary>
        /// <param name="section">The section to set settings for.</param>
        /// <param name="settings">The settings to set. The type must match the type defined by the concrete handler for the section.</param>
        /// <exception cref="DeepConfig.ConfigException">
        /// If there is no handler defined for the section
        /// <para>- or -</para>
        /// <para>
        /// If the handler for the Section specified in the configuration xml could not be created (containing Assembly not loaded, missing privileges etc.).
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler for the section does not match the section.
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler does not implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If section or setings is null.</exception>
        public void SetSettings(Enum section, object settings)
        {
            Ensure.ArgumentNotNull(section, "section");

            string sectionName = section.ToString();

            SetSettings(sectionName, settings);
        }

        /// <summary>
        /// Sets the settings of a given section.
        /// </summary>
        /// <param name="sectionName">The section to set settings for. Note section names are case-sensitive! If the settings object is a <see cref="ConfigSectionAttribute.IsSingleton"/> this name will be ignored.</param>
        /// <param name="settings">The settings to set. The type must match the type defined by the concrete handler for the section.</param>
        /// <exception cref="DeepConfig.ConfigException">
        /// If there is no handler defined for the section
        /// <para>- or -</para>
        /// <para>
        /// If the handler for the Section specified in the configuration xml could not be created (containing Assembly not loaded, missing privileges etc.).
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler for the section does not match the section.
        /// </para>
        /// <para>- or -</para>
        /// <para>
        /// If the specified handler does not implement <see cref="IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If sectionName or settings is null.</exception>
        /// <exception cref="ArgumentException">If sectionName is empty.</exception>
        public void SetSettings(string sectionName, object settings)
        {
            Ensure.ArgumentNotNullOrEmpty(sectionName, "sectionName");
            Ensure.ArgumentNotNull(settings, "settings");

            //Find the handler name. The handler is either obtained from the settings object or static in case of NameValueCollections
            Type handlerType = null;
            if (settings is NameValueCollection)
            {
                handlerType = typeof(NameValueExtendedSectionHandler);
            }
            else
            {
                //See if type is marked as a config section
                Type sectionType = settings.GetType();

                var configSectionAttrib = sectionType.GetAttribute<ConfigSectionAttribute>(true);
                if (configSectionAttrib != null)
                {
                    handlerType = configSectionAttrib.SectionHandlerType;
                    if (configSectionAttrib.IsSingleton)
                    {
                        sectionName = sectionType.Name;
                    }
                }
            }

            if (handlerType == null)
            {
                throw new ConfigException(Msg.Text("[{0}] Config objects must be marked with the 'ConfigSectionAttribute' attribute.", sectionName), ConfigErrorCode.MissingConfigSectionAttribute);
            }

            //Create a handler instance
            var handler = ConfigHelper.CreateHandler(handlerType);

            //Write the section
            var sectionXmlName = XmlConvert.EncodeLocalName(sectionName);
            XElement section;

            try
            {
                section = handler.WriteSection(sectionXmlName, settings, _cryptoProvider, false);
            }
            catch (Exception e)
            {
                throw new ConfigException(Msg.Text("[{0}] Writing section failed, see nested exception for details.", sectionName), ConfigErrorCode.SectionHandlerMismatch, e);
            }

            if (handler.Errors.Any())
            {
                throw new ConfigException(handler.Errors);
            }

            if (section == null)
            {
                return;
            }

            //We must synchronize
            lock (_lock)
            {
                //write header
                ConfigHelper.WriteSectionDefinition(_cfgDoc, sectionXmlName, handlerType);

                //Write section
                _cfgDoc.Root.RemoveChildren(sectionXmlName);
                _cfgDoc.Root.Add(section);

                _sections[sectionName] = section;
                _sectionHandlers[sectionName] = handlerType;
            }

            //Notify Listeners
            OnConfigChanged(sectionName, settings.GetType(), ConfigChangedEventArgs.ChangeCause.SectionChanged);
        }

        /// <summary>
        /// Removes a section of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of section to remove.</typeparam>
        public void RemoveSection<T>() where T : class
        {
            RemoveSection(typeof(T).Name);
        }

        /// <overloads>Removes an entire config section if it exists.</overloads>
        /// <summary>
        /// Removes an entire config section if it exists.
        /// </summary>
        /// <param name="section">The section to remove</param>
        public void RemoveSection(Enum section)
        {
            if (section == null)
            {
                return;
            }

            RemoveSection(section.ToString());
        }

        /// <summary>
        /// Removes an entire config section if it exists.
        /// </summary>
        /// <param name="sectionName">The name of the section to remove</param>
        public void RemoveSection(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                return;
            }

            //Ensure valid name
            var sectionXmlName = XmlConvert.EncodeLocalName(sectionName);

            bool removed = false;
            lock (_lock)
            {
                var cfgSectionsNode = _cfgDoc.Root.Element(ConfigElement.SectionsNode);
                if (cfgSectionsNode != null)
                {
                    //Remove the section definition
                    cfgSectionsNode.RemoveChildren(ConfigElement.SectionNode, e => e.HasAttribute(ConfigElement.SectionNameAttribute, sectionXmlName));
                }

                //Remove the actual section
                _cfgDoc.Root.RemoveChildren(sectionXmlName);

                //Remove in memory
                removed = _sections.Remove(sectionName);
                _sectionHandlers.Remove(sectionName);
            }

            //Notify Listeners
            if (removed)
            {
                OnConfigChanged(sectionName, null, ConfigChangedEventArgs.ChangeCause.SectionRemoved);
            }
        }

        /// <overloads>Gets whether a given section is defined in this configuration or not.</overloads>
        /// <summary>
        /// Gets whether a given section is defined in this configuration or not.
        /// <para>
        /// This method can be used to determine the existence of all types of sections, i.e. both specialized and NameValue sections.
        /// </para>
        /// </summary>
        /// <param name="section">The section to evaluate</param>
        /// <returns>true if the section was found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If section is null.</exception>
        public bool HasSection(Enum section)
        {
            Ensure.ArgumentNotNull(section, "section");

            return HasSection(section.ToString());
        }

        /// <summary>
        /// Gets whether a given section is defined in this configuration or not.
        /// </summary>
        /// <typeparam name="T">The type of section to look for.</typeparam>
        /// <returns>true if the section was found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If section is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "This is the desired signature.")]
        public bool HasSection<T>() where T : class
        {
            return HasSection(typeof(T).Name);
        }

        /// <summary>
        /// Gets whether a given section is defined in this configuration or not.
        /// <para>
        /// This method can be used to determine the existence of all types of sections, i.e. both specialized and NameValue sections.
        /// </para>
        /// </summary>
        /// <param name="sectionName">The section to evaluate</param>
        /// <returns>true if the section was found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If sectionName is null.</exception>
        public bool HasSection(string sectionName)
        {
            Ensure.ArgumentNotNullOrEmpty(sectionName, "sectionName");

            lock (_lock)
            {
                return _sectionHandlers.ContainsKey(sectionName) && _sections.ContainsKey(sectionName);
            }
        }

        /// <summary>
        /// Sets the <see cref="DeepConfig.Cryptography.ICryptoProvider"/> to use for Encryption/Decryption.
        /// <para>
        /// The ConfigManager is then reinitialized to use the new provider.
        /// </para>
        /// </summary>
        /// <param name="cryptoProviderType">The concrete type of the <see cref="DeepConfig.Cryptography.ICryptoProvider"/> to use.</param>
        /// <exception cref="ArgumentNullException">If cryptoProviderType is null.</exception>
        /// <exception cref="DeepConfig.ConfigException">If creation of the new Cryptography provider fails.</exception>
        public void SetCryptoProvider(Type cryptoProviderType)
        {
            Ensure.ArgumentNotNull(cryptoProviderType, "cryptoProviderType");

            //No point in doing anything if its the same type as we already have assigned
            if (cryptoProviderType == _cryptoProvider.GetType())
            {
                return;
            }

            var cryptoProvider = ConfigHelper.CreateInstance(cryptoProviderType, false) as ICryptoProvider;

            if (cryptoProvider == null)
            {
                throw new ConfigException("Crypto provider creation failed, type does not implement ICryptoProvider.", ConfigErrorCode.ConfigTypeCreationFailed);
            }

            SetCryptoProvider(cryptoProvider);
        }

        /// <summary>
        /// Save all changes made to the configuration settings to the underlying <see cref="ConfigProvider"/>
        /// </summary>
        /// <exception cref="ConfigException">If saving fails</exception>
        public void PersistChanges()
        {
            try
            {
                lock (_lock)
                {
                    _provider.SaveConfig(_cfgDoc);
                }
            }
            catch (Exception e)
            {
                throw new ConfigException("Failed to save configuration.", ConfigErrorCode.FailedToSave, e);
            }
        }

        /// <summary>
        /// Permanently changes the <see cref="ConfigProvider"/> of this instance and saves the configuration to the new provider.
        /// </summary>
        /// <param name="provider">The new provider</param>
        /// <exception cref="ConfigException">If saving fails</exception>
        public void PersistTo(IConfigProvider provider)
        {
            Ensure.ArgumentNotNull(provider, "provider");

            lock (_lock)
            {
                _provider = provider;
                _provider.Initialize();

                PersistChanges();
            }
        }

        internal static ConfigManager Create(IConfigProvider provider)
        {
            var mgr = new ConfigManager(provider);
            mgr.Reload();

            return mgr;
        }

        internal void SetCryptoProvider(ICryptoProvider cryptoProvider)
        {
            lock (_lock)
            {
                _cryptoProvider = cryptoProvider;

                try
                {
                    //Create the section
                    CryptoConfigSectionHandler handler = new CryptoConfigSectionHandler();
                    var section = handler.WriteSection(_cryptoProvider.GetType());

                    //Write section definition
                    ConfigHelper.WriteSectionDefinition(_cfgDoc, ConfigElement.CryptoSettingNode, typeof(CryptoConfigSectionHandler));

                    //Add the section
                    _cfgDoc.Root.RemoveChildren(ConfigElement.CryptoSettingNode);
                    _cfgDoc.Root.Add(section);

                    //Rewrite all sections with the new crypto provider
                    var sectionsToUpdate = (from s in _sections.Keys
                                            where _sectionHandlers[s] != null
                                            select s).ToList();

                    foreach (var s in sectionsToUpdate)
                    {
                        var val = GetSettings(s);
                        SetSettings(s, val);
                    }
                }
                catch
                {
                    _cryptoProvider = null;
                    Reload();
                    throw;
                }
            }
        }

        /// <summary>
        /// Validates a config document.
        /// </summary>
        /// <param name="doc">The document to validate</param>
        private static void ValidateConfigDocument(XDocument doc)
        {
            if (doc == null)
            {
                return;
            }

            if ((doc.Root == null) || (!string.Equals(doc.Root.Name.LocalName, ConfigElement.RootNode, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ConfigException("Xml is not valid .Net configuration xml.", ConfigErrorCode.InvalidConfigurationFile);
            }
        }

        private void OnConfigChanged(string sectionName, Type sectionType, ConfigChangedEventArgs.ChangeCause cause)
        {
            var e = ConfigChanged;
            if (e != null)
            {
                ConfigChanged(
                    this,
                    new ConfigChangedEventArgs
                    {
                        SectionName = sectionName,
                        SectionType = sectionType,
                        Cause = cause
                    });
            }

            ConfigMaster.OnConfigChanged(sectionName, sectionType, cause);
        }

        /// <summary>
        /// Gets the sections from the current provider and initializes all dictionaries.
        /// </summary>
        private void GetSections()
        {
            //Create storage
            _sections = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
            _sectionHandlers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            //The appSettings handler is already known so we add it first
            _sectionHandlers.Add(ConfigElement.AppSettingsNode, typeof(NameValueExtendedSectionHandler));

            //Now load the config document(s)
            try
            {
                var docs = _provider.LoadConfig();

                if (docs == null)
                {
                    return;
                }

                //Validate and get the section of the config.
                //In the case of multi file configs, the first doc will serve as the base, and first will have precendece over second and so forth
                foreach (var doc in docs.Reverse())
                {
                    if (doc == null)
                    {
                        continue;
                    }

                    _cfgDoc = doc;
                    ValidateConfigDocument(doc);
                    GetSections(doc);
                }

                if (_cfgDoc == null)
                {
                    _cfgDoc = XmlUtil.CreateEmptyConfig();
                }
            }
            catch (ConfigException)
            {
                throw;
            }
            catch (XmlException e)
            {
                throw new ConfigException("Xml is not valid .Net configuration xml.", ConfigErrorCode.InvalidConfigurationFile, e);
            }
            catch (Exception e)
            {
                throw new ConfigException("Failed to load configuration.", ConfigErrorCode.FailedToLoad, e);
            }
        }

        /// <summary>
        /// Gets all relevant sections from a config document.
        /// </summary>
        /// <param name="sourceDoc">The source document.</param>
        private void GetSections(XDocument sourceDoc)
        {
            var root = sourceDoc.Root;

            //Get appsettings collection
            var section = root.Element(ConfigElement.AppSettingsNode);
            if (section != null)
            {
                _sections[ConfigElement.AppSettingsNode] = section;
            }

            //Get other sections
            var cfgSectionsNode = root.Element(ConfigElement.SectionsNode);
            if (cfgSectionsNode == null)
            {
                return;
            }

            var sections = from node in cfgSectionsNode.Elements(ConfigElement.SectionNode)
                           let name = node.Attribute(ConfigElement.SectionNameAttribute)
                           let typeName = node.Attribute(ConfigElement.SectionTypeAttribute)
                           where name != null && typeName != null
                           select new
                           {
                               Name = name.Value,
                               TypeName = typeName.Value
                           };

            foreach (var s in sections)
            {
                Type handlerType;

                //Redirect normal NameValue handlers to use extended handler
                if (s.TypeName.IndexOf(typeof(System.Configuration.NameValueSectionHandler).FullName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    handlerType = typeof(NameValueExtendedSectionHandler);
                }
                else
                {
                    handlerType = ConfigHelper.ResolveType(s.TypeName, true);
                }

                section = root.Element(s.Name);

                var sectionName = XmlConvert.DecodeName(s.Name);
                if (section != null)
                {
                    _sections[sectionName] = section;
                    _sectionHandlers[sectionName] = handlerType;
                }
            }
        }

        private void InitCryptoProvider()
        {
            //Check if we have explicitly set crypto provider in config xml
            XElement explicitProviderXml;
            if (_sections.TryGetValue(ConfigElement.CryptoSettingNode, out explicitProviderXml))
            {
                var configHandler = new CryptoConfigSectionHandler();
                _cryptoProvider = configHandler.ReadSection(explicitProviderXml);
            }

            if (_cryptoProvider == null)
            {
                _cryptoProvider = new AesCryptoProvider();
            }
        }
    }
}
