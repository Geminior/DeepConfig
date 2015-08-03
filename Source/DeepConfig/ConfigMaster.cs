namespace DeepConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DeepConfig.Providers;
    using DeepConfig.Utilities;

    /// <summary>
    /// This class is the entry point for loading configurations.
    /// </summary>
    /// <threadsafety static="true" >
    /// Class is safe for multithreaded operations.
    /// </threadsafety>
    public static class ConfigMaster
    {
        private const int CacheTime = 30000;

        private static readonly FastSmartWeakEvent<EventHandler<ConfigChangedEventArgs>> ChangeEvent = new FastSmartWeakEvent<EventHandler<ConfigChangedEventArgs>>();

        private static readonly ProviderMapping ProviderMappings = new ProviderMapping(OnMappingChange, OnDefaultMappingChange);
        private static readonly SimpleCache<string, IConfigManager> ManagerCache = new SimpleCache<string, IConfigManager>();
        private static readonly Dictionary<Type, object> SingletonsCache = new Dictionary<Type, object>();

        /// <summary>
        /// Raised when changes are made to the config either through this class or through an <see cref="ConfigManager"/>.
        /// </summary>
        public static event EventHandler<ConfigChangedEventArgs> ConfigChanged
        {
            add { ChangeEvent.Add(value); }
            remove { ChangeEvent.Remove(value); }
        }

        /// <summary>
        /// Gets the provider mapping, which allows for mapping specific config types to specific <see cref="IConfigProvider"/>s or factories. See remarks for more info.
        /// </summary>
        /// <remarks>
        /// Mapping a config type to a <see cref="IConfigProvider"/> allows for different config types to have different providers, i.e. different sources.
        /// <para>
        /// These mappings are used by the <see cref="GetConfig&lt;T&gt;"/> method, and consequently by <see cref="SingletonConfig&lt;T&gt;"/> derivatives.
        /// </para>
        /// <para>
        /// This is a simple form of dependency injection, and can be used for testing as well.
        /// </para>
        /// </remarks>
        public static IProviderMappingRoot ProviderMapping
        {
            get { return ProviderMappings; }
        }

        /// <summary>
        /// Gets the settings of a section of a particular type.
        /// <para>
        /// This is an application wide singleton that is governed by the mappings set up in <see cref="ProviderMapping"/>. See remarks for details.
        /// </para>
        /// </summary>
        /// <remarks>
        /// If a mapping for the specified type exists in <see cref="ProviderMapping"/> that will be used, otherwise the default manager <see cref="GetDefaultConfig()"/> is used to load the settings.
        /// <para>
        /// Being a singleton, the instance is cached indefinitely, with the exception of its mapping is changed.
        /// </para>
        /// <para>
        /// See <see cref="SingletonConfig&lt;T&gt;"/> for a convenient way of implementing your config type as a singleton. 
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The config type to load settings for</typeparam>
        /// <returns>The instance of the config type or null if it does not exist.</returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the underlying provider's <see cref="IConfigProvider.LoadConfig"/> fails.
        /// <para>Or if ICryptoProvider instantiation fails.</para>
        /// </exception>
        public static T GetSettings<T>() where T : class
        {
            lock (SingletonsCache)
            {
                object settingsRaw;
                if (SingletonsCache.TryGetValue(typeof(T), out settingsRaw))
                {
                    return (T)settingsRaw;
                }

                var mgr = GetConfig<T>();
                var settings = mgr.GetSettings<T>();

                SingletonsCache[typeof(T)] = settings;

                return settings;
            }
        }

        /// <summary>
        /// Stores the settings of a specific type back to its provider.
        /// <para>
        /// Since this is an application wide singleton, it also initializes / refreshes that singleton.
        /// </para>
        /// </summary>
        /// <remarks>
        /// If a mapping for the specified type exists in <see cref="ProviderMapping"/> that will be used, otherwise the default manager <see cref="GetDefaultConfig()"/> is used to save the settings.
        /// <para>
        /// Unlike <see cref="IConfigManager.SetSettings&lt;T&gt;"/>, this method stores the settings back to its provider right away.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The config type</typeparam>
        /// <param name="settings">The settings to store.</param>
        /// <exception cref="DeepConfig.ConfigException">
        /// If saving fails
        /// <para>- or -</para>
        /// <para>
        /// If there is no handler defined for the section
        /// </para>
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
        public static void SetSettings<T>(T settings) where T : class
        {
            var mgr = GetConfig<T>();

            mgr.SetSettings(settings);
            mgr.PersistChanges();

            var cfgype = typeof(T);
            lock (SingletonsCache)
            {
                SingletonsCache[cfgype] = settings;
            }
        }

        /// <summary>
        /// Gets a ConfigManager instance wrapping the default configuration data of the application.
        /// <para>
        /// If not explicitly mapped through <see cref="Providers.ProviderMapping.MapDefaultProvider(IConfigProvider)"/>, the application config file, e.g. web.config or applicationName.exe.config is used.
        /// </para>
        /// <para><see cref="ConfigurationRedirect"/>s are also be honoured.</para>
        /// <para>To manage other configurations use the <see cref="GetConfig(IConfigProvider)"/> method or overloads thereof.</para>
        /// </summary>
        /// <exception cref="DeepConfig.ConfigException">
        /// If ICryptoProvider instantiation fails.
        /// <para>- or -</para>
        /// <para>
        /// If the provider's <see cref="IConfigProvider.LoadConfig"/> fails.
        /// </para>
        /// </exception>
        /// <returns>An IConfigManager instance wrapping the default configuration.</returns>
        public static IConfigManager GetDefaultConfig()
        {
            var provider = ProviderMappings.GetDefaultProvider();

            return GetManagerForProvider(provider);
        }

        /// <summary>
        /// Gets a ConfigManager instance associated with the config file supplied as an argument.
        /// <para><see cref="ConfigurationRedirect"/>s will be honoured.</para>
        /// <para>To get an instance for the default application config file, use <see cref="GetDefaultConfig()"/> instead.</para>
        /// </summary>
        /// <param name="configFile">The full path of the config file to manage. If the file does not exist, it is created</param>
        /// <returns>An IConfigManager instance wrapping the config file.</returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the file <c>configFile</c> is not a valid config file. File can be non-existant though this will just create the file.
        /// <para>Or if ICryptoProvider instantiation fails.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If cfgFile is null or empty.</exception>
        public static IConfigManager GetConfig(string configFile)
        {
            var provider = new FileConfigProvider(configFile);
            return GetConfig(provider, false);
        }

        /// <summary>
        /// Gets a ConfigManager instance for managing the configuration settings provided by the provider. Redirects (<see cref="ConfigurationRedirect"/>) will be honored.
        /// </summary>
        /// <param name="provider">The provider of the configuration</param>
        /// <returns>An IConfigManager instance wrapping the configuration.</returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the provider's <see cref="IConfigProvider.LoadConfig"/> fails.
        /// <para>Or if ICryptoProvider instantiation fails.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If provider is null.</exception>
        public static IConfigManager GetConfig(IConfigProvider provider)
        {
            return GetConfig(provider, false);
        }

        /// <summary>
        /// Gets a ConfigManager instance for managing the configuration settings provided by the provider.
        /// </summary>
        /// <param name="provider">The provider of the configuration</param>
        /// <param name="ignoreRedirects">Whether to ignore <see cref="ConfigurationRedirect"/>s</param>
        /// <returns>An IConfigManager instance wrapping the configuration.</returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the provider's <see cref="IConfigProvider.LoadConfig"/> fails.
        /// <para>Or if ICryptoProvider instantiation fails.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If provider is null.</exception>
        public static IConfigManager GetConfig(IConfigProvider provider, bool ignoreRedirects)
        {
            Ensure.ArgumentNotNull(provider, "provider");

            var mgr = ConfigManager.Create(provider);

            //Check if the provided config contains a redirect
            if (!ignoreRedirects && ConfigurationRedirect.TryGetRedirect(mgr, out provider))
            {
                mgr = ConfigManager.Create(provider);
            }

            return mgr;
        }

        /// <summary>
        /// Provided to enable mapping a specific config type to a specific instance.
        /// </summary>
        /// <typeparam name="T">The config type</typeparam>
        /// <param name="instance">The instance that will be served as the singleton instance</param>
        internal static void MapInstance<T>(T instance) where T : class
        {
            lock (SingletonsCache)
            {
                if (instance == null)
                {
                    SingletonsCache.Remove(typeof(T));
                }
                else
                {
                    SingletonsCache[typeof(T)] = instance;
                }
            }
        }

        /// <summary>
        /// Revokes a singleton instance ensuring that it will be reloaded from storage on next access.
        /// </summary>
        /// <typeparam name="T">The config type</typeparam>
        internal static void RevokeInstance<T>() where T : class
        {
            lock (SingletonsCache)
            {
                SingletonsCache.Remove(typeof(T));

                //Remove any cached manager so this type is reloaded from scratch
                lock (ManagerCache)
                {
                    if (ManagerCache.Count == 0)
                    {
                        return;
                    }

                    var provider = ProviderMappings.ResolveProvider(typeof(T));
                    if (provider == null)
                    {
                        provider = ProviderMappings.GetDefaultProvider();
                    }

                    string key = GetProviderKey(provider);
                    ManagerCache.Remove(key);
                }
            }
        }

        internal static void OnConfigChanged(string sectionName, Type sectionType, ConfigChangedEventArgs.ChangeCause cause)
        {
            ChangeEvent.Raise(
                    null,
                    new ConfigChangedEventArgs
                    {
                        SectionName = sectionName,
                        SectionType = sectionType,
                        Cause = cause
                    });
        }

        /// <summary>
        /// Gets a ConfigManager for managing settings of the specified config type.
        /// <para>
        /// The returned IConfigManager is governed by the mappings set up in <see cref="ProviderMapping"/>. See remarks for details.
        /// </para>
        /// </summary>
        /// <remarks>
        /// If a mapping for the specified type exists in <see cref="ProviderMapping"/> that will be used, otherwise this will return the same as <see cref="GetDefaultConfig()"/>.
        /// <para>
        /// Managers retrieved in this way are cached for a short time, so that multiple mappings to the same provider <see cref="IConfigProvider.SourceIdentifier"/> does not load the configuration again if called within the cache period.
        /// </para>
        /// <para>
        /// Note that there is no guarantee that the returned IConfigManager will contain settings for the type, i.e. <see cref="IConfigManager.GetSettings&lt;T&gt;()"/> may return null.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The config type to get a manager for</typeparam>
        /// <returns>An IConfigManager instance wrapping the configuration of the IConfigProvider mapped to the specified type T.</returns>
        /// <exception cref="DeepConfig.ConfigException">
        /// If the provider's <see cref="IConfigProvider.LoadConfig"/> fails.
        /// <para>Or if ICryptoProvider instantiation fails.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException">If provider is null.</exception>
        internal static IConfigManager GetConfig<T>() where T : class
        {
            var provider = ProviderMappings.ResolveProvider(typeof(T));
            if (provider == null)
            {
                //Get default provider
                return GetDefaultConfig();
            }

            return GetManagerForProvider(provider);
        }

        private static IConfigManager GetManagerForProvider(IConfigProvider provider)
        {
            string key = GetProviderKey(provider);

            lock (ManagerCache)
            {
                IConfigManager mgr;
                if (ManagerCache.TryGet(key, out mgr))
                {
                    return mgr;
                }

                mgr = GetConfig(provider, false);
                ManagerCache.Add(key, mgr, CacheTime, true);

                return mgr;
            }
        }

        private static string GetProviderKey(IConfigProvider provider)
        {
            if (string.IsNullOrEmpty(provider.SourceIdentifier))
            {
                return provider.GetHashCode().ToString();
            }

            return string.Concat(provider.GetType().FullName, "_", provider.SourceIdentifier);
        }

        private static void OnMappingChange(Type configType)
        {
            bool instantiated = false;

            lock (SingletonsCache)
            {
                instantiated = SingletonsCache.Remove(configType);
            }

            if (instantiated)
            {
                OnConfigChanged(configType.Name, configType, ConfigChangedEventArgs.ChangeCause.MappingChanged);
            }
        }

        private static void OnDefaultMappingChange()
        {
            //look thorugh all singletons and check if they have an explicit provider mapping
            //if they dont, remove them, so they are loaded again with the new default provider on next access
            IEnumerable<Type> affectedTypes;

            lock (SingletonsCache)
            {
                affectedTypes = (from entry in SingletonsCache
                                 where !ProviderMappings.HasMapping(entry.Key)
                                 select entry.Key).ToList();

                foreach (var t in affectedTypes)
                {
                    SingletonsCache.Remove(t);
                }
            }

            foreach (var t in affectedTypes)
            {
                OnConfigChanged(t.Name, t, ConfigChangedEventArgs.ChangeCause.MappingChanged);
            }
        }
    }
}
