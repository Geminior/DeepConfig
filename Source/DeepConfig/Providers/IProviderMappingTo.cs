namespace DeepConfig.Providers
{
    using System;

    /// <summary>
    /// Provides the second half of the mapping methods.
    /// </summary>
    public interface IProviderMappingTo
    {
        /// <summary>
        /// Finishes the mapping. Note that the factory is not called until an actual request for the mapping is made.
        /// </summary>
        /// <param name="factory">The factory that will provide the provider to map to.</param>
        void To(Func<Type, IConfigProvider> factory);

        /// <summary>
        /// Finishes the mapping.
        /// </summary>
        /// <param name="provider">The provider to map to.</param>
        void To(IConfigProvider provider);
    }
}
