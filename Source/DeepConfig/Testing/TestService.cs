namespace DeepConfig.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Provides methods to help in testing when using the config framework
    /// </summary>
    public static class TestService
    {
        /// <summary>
        /// Map a specific instance, thus enabling proper testing while using the Singleton implementation on a config type, i.e. in calls to <see cref="ConfigMaster.GetSettings&lt;T&gt;"/> or using the <see cref="SingletonConfig&lt;T&gt;"/>
        /// </summary>
        /// <typeparam name="T">The config type</typeparam>
        /// <param name="instance">The instance that will be served as the singleton instance.</param>.
        public static void MapSingletonInstance<T>(this T instance) where T : class
        {
            ConfigMaster.MapInstance(instance);
        }

        /// <summary>
        /// Unmaps an instance previously mapped using <see cref="MapSingletonInstance&lt;T&gt;" />
        /// </summary>
        /// <typeparam name="T">The config type</typeparam>
        /// <param name="instance">The instance that wants to unmap itself.</param>
        public static void UnmapSingletonInstance<T>(this T instance) where T : class
        {
            ConfigMaster.MapInstance<T>(null);
        }

        /// <summary>
        /// Unmaps an instance previously mapped using <see cref="MapSingletonInstance&lt;T&gt;"/>
        /// </summary>
        /// <typeparam name="T">The config type</typeparam>
        public static void UnmapSingletonInstance<T>() where T : class
        {
            ConfigMaster.MapInstance<T>(null);
        }
    }
}
