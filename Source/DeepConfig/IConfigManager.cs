namespace DeepConfig
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using DeepConfig.Cryptography;
    using DeepConfig.Providers;

    /// <summary>
    /// The inteface implemented by config managers
    /// </summary>
    public interface IConfigManager
    {
        /// <summary>
        /// Raised when a setting is changed
        /// </summary>
        event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        /// <summary>
        /// Gets the <see cref="IConfigProvider"/> used by this instance.
        /// </summary>
        IConfigProvider ConfigProvider { get; }

        /// <summary>
        /// Gets the <see cref="DeepConfig.Cryptography.ICryptoProvider"/> to use for Encrypting/Decrypting values.
        /// <para>This is set through the configuration xml itself using <see cref="SetCryptoProvider(System.Type)"/>.</para>
        /// <para>If not set this defaults to a <see cref="DeepConfig.Cryptography.AesCryptoProvider"/></para>
        /// </summary>
        ICryptoProvider CryptoProvider { get; }

        /// <summary>
        /// Gets a list with the names of the sections in the configuration xml that are supported by this configuration framework
        /// <para>
        /// The list is disconnected from the ConfigManager and thus safe to iterate over even if changes are made to the config in the meantime.
        /// </para>
        /// </summary>
        IEnumerable<string> SupportedSections { get; }

        /// <summary>
        /// Gets a list with the names of all sections in the configuration xml.
        /// <para>
        /// The list is disconnected from the ConfigManager and thus safe to iterate over even if changes are made to the config in the meantime.
        /// </para>
        /// </summary>
        IEnumerable<string> Sections { get; }

        /// <summary>
        /// Gets a value indicating whether the configuration has been altered by another source since this config instance was loaded.
        /// <para>
        /// If true, saving the current instance will overwrite any changes made by other sources.
        /// </para>
        /// </summary>
        bool ModifiedSinceLoad { get; }

        /// <summary>
        /// Gets the settings of the appSettings section
        /// </summary>
        /// <returns>A <see cref="NameValueSettings"/> containing the settings. This is never null but may be empty.
        /// <para>
        /// The collection is disconnected from the ConfigManager so changes made to it will not affect the ConfigManager.
        /// <para>To set the setting, <see cref="SetAppSettings(NameValueSettings)"/> must be called.</para>
        /// </para>
        /// </returns>
        NameValueSettings GetAppSettings();

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
        /// If the specified handler does not implement <see cref="Handlers.IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If sectionName is null.</exception>
        /// <exception cref="ArgumentException">If sectionName is empty.</exception>
        /// <exception cref="ConfigException">If the section handler fails</exception>
        object GetSettings(string sectionName);

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
        /// If the specified handler does not implement <see cref="Handlers.IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        T GetSettings<T>() where T : class;

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
        /// If the specified handler does not implement <see cref="Handlers.IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If section is null.</exception>
        T GetSettings<T>(Enum section) where T : class;

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
        /// If the specified handler does not implement <see cref="Handlers.IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If section is null or empty.</exception>
        T GetSettings<T>(string sectionName) where T : class;

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
        /// If the specified handler does not implement <see cref="Handlers.IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        List<T> GetSettingsList<T>() where T : class;

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
        bool HasSection(Enum section);

        /// <summary>
        /// Gets whether a given section is defined in this configuration or not.
        /// <para>
        /// This method can be used to determine the existence of all types of sections, i.e. both specialized and NameValue sections.
        /// </para>
        /// </summary>
        /// <param name="sectionName">The section to evaluate</param>
        /// <returns>true if the section was found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If sectionName is null.</exception>
        bool HasSection(string sectionName);

        /// <summary>
        /// Gets whether a given section is defined in this configuration or not.
        /// </summary>
        /// <typeparam name="T">The type of section to look for.</typeparam>
        /// <returns>true if the section was found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">If section is null.</exception>
        bool HasSection<T>() where T : class;

        /// <summary>
        /// Save all changes made to the configuration settings to the underlying <see cref="ConfigProvider"/>
        /// </summary>
        /// <exception cref="ConfigException">If saving fails</exception>
        void PersistChanges();

        /// <summary>
        /// Permanently changes the <see cref="ConfigProvider"/> of this instance and saves the configuration to the new provider.
        /// </summary>
        /// <param name="provider">The new provider</param>
        /// <exception cref="ConfigException">If saving fails</exception>
        void PersistTo(IConfigProvider provider);

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
        void Reload();

        /// <overloads>Removes an entire config section if it exists.</overloads>
        /// <summary>
        /// Removes an entire config section if it exists.
        /// </summary>
        /// <param name="section">The section to remove</param>
        void RemoveSection(Enum section);

        /// <summary>
        /// Removes an entire config section if it exists.
        /// </summary>
        /// <param name="sectionName">The name of the section to remove</param>
        void RemoveSection(string sectionName);

        /// <summary>
        /// Removes a section of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of section to remove.</typeparam>
        void RemoveSection<T>() where T : class;

        /// <summary>
        /// Sets the settings of the appSettings section
        /// </summary>
        /// <param name="settings">The settings to set</param>
        void SetAppSettings(NameValueSettings settings);

        /// <summary>
        /// Sets the settings of the appSettings section
        /// </summary>
        /// <param name="settings">The settings to set</param>
        void SetAppSettings(NameValueCollection settings);

        /// <summary>
        /// Sets the <see cref="DeepConfig.Cryptography.ICryptoProvider"/> to use for Encryption/Decryption.
        /// <para>
        /// The ConfigManager is then reinitialized to use the new provider.
        /// </para>
        /// </summary>
        /// <param name="cryptoProviderType">The concrete type of the <see cref="DeepConfig.Cryptography.ICryptoProvider"/> to use.</param>
        /// <exception cref="ArgumentNullException">If cryptoProviderType is null.</exception>
        /// <exception cref="DeepConfig.ConfigException">If creation of the new Cryptography provider fails.</exception>
        void SetCryptoProvider(Type cryptoProviderType);

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
        /// If the specified handler does not implement <see cref="Handlers.IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If settings is null.</exception>
        void SetSettings<T>(T settings) where T : class;

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
        /// If the specified handler does not implement <see cref="Handlers.IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If section or setings is null.</exception>
        void SetSettings(Enum section, object settings);

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
        /// If the specified handler does not implement <see cref="Handlers.IExtendedConfigurationSectionHandler"/>
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If sectionName or settings is null.</exception>
        /// <exception cref="ArgumentException">If sectionName is empty.</exception>
        void SetSettings(string sectionName, object settings);
    }
}
