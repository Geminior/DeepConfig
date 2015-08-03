namespace DeepConfig.Providers
{
    using System;

    /// <summary>
    /// Provides the first half of the mapping methods.
    /// </summary>
    public interface IProviderMappingRoot
    {
        /// <summary>
        /// Maps a config type to a provider. Follow this call with a call to <see cref="IProviderMappingTo.To(IConfigProvider)"/>.
        /// </summary>
        /// <typeparam name="T">The config type to map a provider to</typeparam>
        /// <returns>A <see cref="IProviderMappingTo"/> on which the mapping can be completed, i.e. Map&lt;T&gt;().To(...)</returns>
        IProviderMappingTo Map<T>() where T : class;

        /// <summary>
        ///  Maps a config type to a provider. Follow this call with a call to <see cref="IProviderMappingTo.To(IConfigProvider)"/>.
        /// </summary>
        /// <param name="configType">The config type to map a provider to.</param>
        /// <returns>A <see cref="IProviderMappingTo"/> on which the mapping can be completed, i.e. Map&lt;T&gt;().To(...)</returns>
        IProviderMappingTo Map(Type configType);

        /// <summary>
        /// Unmaps a config type from its provider, so that it will instead use the default provider. Calling this on an already unmapped type is safe.
        /// </summary>
        /// <typeparam name="T">The config type to remove mapping from.</typeparam>
        void Unmap<T>() where T : class;

        /// <summary>
        /// Unmaps a config type from its provider, so that it will instead use the default provider. Calling this on an already unmapped type is safe.
        /// </summary>
        /// <param name="configType">The config type to remove mapping from.</param>
        void Unmap(Type configType);

        /// <summary>
        /// Maps the default <see cref="IConfigProvider"/> to use, in the form of a factory function.
        /// </summary>
        /// <param name="factory">The factory. If null will behave the same as calling <see cref="UnmapDefaultProvider"/></param>
        void MapDefaultProvider(Func<IConfigProvider> factory);

        /// <summary>
        /// Maps the default <see cref="IConfigProvider"/> to use.
        /// </summary>
        /// <param name="provider">The provider. If null will behave the same as calling <see cref="UnmapDefaultProvider"/></param>
        void MapDefaultProvider(IConfigProvider provider);

        /// <summary>
        /// Reverts the default provider to a <see cref="DefaultFileConfigProvider"/>.
        /// </summary>
        void UnmapDefaultProvider();
    }
}
