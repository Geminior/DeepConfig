namespace DeepConfig
{
    using System;

    /// <summary>
    /// This variation of the <see cref="SingletonConfig&lt;T&gt;"/> creates a new instance and registers it with the <see cref="ConfigMaster"/> if no existing entry can be found in the provider. See remarks for details.
    /// </summary>
    /// <remarks>
    /// <see cref="Instance"/> will never throw an exception but instead create a new instance and register it with the <see cref="ConfigMaster"/> in case no entry is found in the provider.
    /// </remarks>
    /// <typeparam name="T">The actual config type, i.e. the deriving type</typeparam>
    [ConfigSection(IsSingleton = true)]
    public abstract class AutoSingletonConfig<T> where T : class, new()
    {
        private static readonly object Lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSingletonConfig&lt;T&gt;"/> class.
        /// </summary>
        protected AutoSingletonConfig()
        {
        }

        /// <summary>
        /// Gets the instance of the config type, either from the provider if it exists there, otherwise will create a new instance and register it with the <see cref="ConfigMaster"/>.
        /// </summary>
        /// <value>The instance.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This is a special case since the type parameter is the same as the deriving type, hence the type parameter is never used.")]
        public static T Instance
        {
            get
            {
                var settings = ConfigMaster.GetSettings<T>();

                if (settings == null)
                {
                    lock (Lock)
                    {
                        if (settings == null)
                        {
                            settings = new T();
                            ConfigMaster.MapInstance<T>(settings);
                        }
                    }
                }

                return settings;
            }
        }

        /// <summary>
        /// Reloads this instance. Next rerence to <see cref="Instance"/> will load the config from its provider or instantiate a new instance if no entry is found in the provider.
        /// <para>
        /// Only call this if changes to the config has been made from some outside source. Calling this after <see cref="ConfigMaster.SetSettings&lt;T&gt;"/> is a waste of time and resources.
        /// </para>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This is a special case since the type parameter is the same as the deriving type, hence the type parameter is never used.")]
        public static void Reload()
        {
            ConfigMaster.RevokeInstance<T>();
        }
    }
}
