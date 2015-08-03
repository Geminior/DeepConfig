namespace DeepConfig
{
    using System;

    /// <summary>
    /// Convenience class that implements the singleton pattern for singleton config section. See remarks for details.
    /// </summary>
    /// <remarks>
    /// The Singleton behavior only applies to a type if it is used to define a configuration section on its own.
    /// <para>In case the type is used to declare a prperty on another config type, it will not behave as a singleton in that context</para>
    /// <para>In otehr words MySingletonConfigType.Instance and SomeParent.MySingletonConfigTypeProperty will not refer to the same instance.</para>
    /// </remarks>
    /// <typeparam name="T">The actual config type, i.e. the deriving type</typeparam>
    [ConfigSection(IsSingleton = true)]
    public abstract class SingletonConfig<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonConfig&lt;T&gt;"/> class.
        /// </summary>
        protected SingletonConfig()
        {
        }

        /// <summary>
        /// Gets the instance of the config type.
        /// </summary>
        /// <value>The instance.</value>
        /// <exception cref="ConfigException">If the section is not found</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This is a special case since the type parameter is the same as the deriving type, hence the type parameter is never used.")]
        public static T Instance
        {
            get { return GetInstance(true); }
        }

        /// <summary>
        /// Gets a value indicating whether an instance of this config type exists in its provider (storage)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This is a special case since the type parameter is the same as the deriving type, hence the type parameter is never used.")]
        public static bool Exists
        {
            get { return (GetInstance(false) != null); }
        }

        /// <summary>
        /// Reloads this instance. Next rerence to <see cref="Instance"/> will load the config from its provider (storage).
        /// <para>
        /// Only call this if changes to the config has been made from some outside source. Calling this after <see cref="ConfigMaster.SetSettings&lt;T&gt;"/> is a waste of time and resources.
        /// </para>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This is a special case since the type parameter is the same as the deriving type, hence the type parameter is never used.")]
        public static void Reload()
        {
            ConfigMaster.RevokeInstance<T>();
        }

        /// <summary>
        /// Tries to get the instance if present in its provider. Otherwise the result of <paramref name="createDefaultInstance" /> is returned.
        /// </summary>
        /// <param name="createDefaultInstance">The create default instance.</param>
        /// <param name="mapToMaster">if set to <c>true</c> and <paramref name="createDefaultInstance"/> is used to create a default, this is then mapped as the singleton instance, i.e. <see cref="Exists"/> will be true, <see cref="Instance"/> will return the instance etc.</param>
        /// <returns>
        /// The instance if it exists, otherwise the result of <paramref name="createDefaultInstance" />.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This is a special case since the type parameter is the same as the deriving type, hence the type parameter is never used.")]
        public static T TryGetInstance(Func<T> createDefaultInstance, bool mapToMaster)
        {
            var instance = GetInstance(false);

            if (instance == null)
            {
                instance = createDefaultInstance();

                if (mapToMaster)
                {
                    ConfigMaster.MapInstance<T>(instance);
                }
            }

            return instance;
        }

        private static T GetInstance(bool throwIfMissing)
        {
            var settings = ConfigMaster.GetSettings<T>();

            if (settings == null && throwIfMissing)
            {
                throw new ConfigException("No section of type " + typeof(T).Name + " could be found.", ConfigErrorCode.ConfigurationNotFound);
            }

            return settings;
        }
    }
}
