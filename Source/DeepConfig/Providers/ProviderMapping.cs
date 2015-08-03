namespace DeepConfig.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DeepConfig.Utilities;

    internal sealed class ProviderMapping : IProviderMappingRoot
    {
        private Dictionary<Type, object> _mappings = new Dictionary<Type, object>();
        private object _defaultProvider;

        private Action<Type> _onMappingChange;
        private Action _onDefaultMappingChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderMapping"/> class.
        /// </summary>
        /// <param name="onMappingChange">Handler to be invoked when a mapping is changed.</param>
        /// <param name="onDefaultMappingChange">Handler to be invoked when the default mapping is changed.</param>
        public ProviderMapping(Action<Type> onMappingChange, Action onDefaultMappingChange)
        {
            _onMappingChange = onMappingChange;
            _onDefaultMappingChange = onDefaultMappingChange;
        }

        /// <summary>
        /// Maps a config type to a provider. Follow this call with a call to <see cref="IProviderMappingTo.To(IConfigProvider)"/>.
        /// </summary>
        /// <typeparam name="T">The config type to map a provider to</typeparam>
        /// <returns>A <see cref="IProviderMappingTo"/> on which the mapping can be completed, i.e. Map&lt;T&gt;().To(...)</returns>
        public IProviderMappingTo Map<T>() where T : class
        {
            return Map(typeof(T));
        }

        /// <summary>
        ///  Maps a config type to a provider. Follow this call with a call to <see cref="IProviderMappingTo.To(IConfigProvider)"/>.
        /// </summary>
        /// <param name="configType">The config type to map a provider to.</param>
        /// <returns>A <see cref="IProviderMappingTo"/> on which the mapping can be completed, i.e. Map&lt;T&gt;().To(...)</returns>
        public IProviderMappingTo Map(Type configType)
        {
            Ensure.ArgumentNotNull(configType, "configType");

            return new ToMapper(
                o =>
                {
                    lock (_mappings)
                    {
                        _mappings[configType] = o;
                    }

                    _onMappingChange(configType);
                });
        }

        /// <summary>
        /// Unmaps a config type from its provider, so that it will instead use the default provider. Calling this on an already unmapped type is safe.
        /// </summary>
        /// <typeparam name="T">The config type to remove mapping from.</typeparam>
        public void Unmap<T>() where T : class
        {
            Unmap(typeof(T));
        }

        /// <summary>
        /// Unmaps a config type from its provider, so that it will instead use the default provider. Calling this on an already unmapped type is safe.
        /// </summary>
        /// <param name="configType">The config type to remove mapping from.</param>
        public void Unmap(Type configType)
        {
            lock (_mappings)
            {
                _mappings.Remove(configType);
            }

            _onMappingChange(configType);
        }

        /// <summary>
        /// Maps the default <see cref="IConfigProvider"/> to use, in the form of a factory function.
        /// </summary>
        /// <param name="factory">The factory. If null will behave the same as calling <see cref="UnmapDefaultProvider"/></param>
        public void MapDefaultProvider(Func<IConfigProvider> factory)
        {
            _defaultProvider = factory;
            _onDefaultMappingChange();
        }

        /// <summary>
        /// Maps the default <see cref="IConfigProvider"/> to use.
        /// </summary>
        /// <param name="provider">The provider. If null will behave the same as calling <see cref="UnmapDefaultProvider"/></param>
        public void MapDefaultProvider(IConfigProvider provider)
        {
            _defaultProvider = provider;
            _onDefaultMappingChange();
        }

        /// <summary>
        /// Reverts the default provider to a <see cref="DefaultFileConfigProvider"/>.
        /// </summary>
        public void UnmapDefaultProvider()
        {
            _defaultProvider = null;
            _onDefaultMappingChange();
        }

        internal void UnmapAll()
        {
            UnmapDefaultProvider();

            var keys = _mappings.Keys.ToList();
            foreach (var k in keys)
            {
                Unmap(k);
            }
        }

        internal IConfigProvider GetDefaultProvider()
        {
            if (_defaultProvider == null)
            {
                return new DefaultFileConfigProvider();
            }

            var p = _defaultProvider as IConfigProvider;
            if (p != null)
            {
                return p;
            }

            var factory = _defaultProvider as Func<IConfigProvider>;
            return factory();
        }

        internal IConfigProvider ResolveProvider(Type configType)
        {
            Ensure.ArgumentNotNull(configType, "configType");

            object o;
            lock (_mappings)
            {
                if (!_mappings.TryGetValue(configType, out o))
                {
                    return null;
                }
            }

            var p = o as IConfigProvider;
            if (p != null)
            {
                return p;
            }

            var factory = o as Func<Type, IConfigProvider>;
            return factory(configType);
        }

        internal bool HasMapping(Type configType)
        {
            lock (_mappings)
            {
                return _mappings.ContainsKey(configType);
            }
        }

        private class ToMapper : IProviderMappingTo
        {
            private Action<object> _mapper;

            internal ToMapper(Action<object> mapper)
            {
                _mapper = mapper;
            }

            /// <summary>
            /// Finishes the mapping. Note that the factory is not called until an actual request for the mapping is made.
            /// </summary>
            /// <param name="factory">The factory that will provide the provider to map to.</param>
            void IProviderMappingTo.To(Func<Type, IConfigProvider> factory)
            {
                Ensure.ArgumentNotNull(factory, "factory");

                _mapper(factory);
            }

            /// <summary>
            /// Finishes the mapping.
            /// </summary>
            /// <param name="provider">The provider to map to.</param>
            void IProviderMappingTo.To(IConfigProvider provider)
            {
                Ensure.ArgumentNotNull(provider, "provider");

                _mapper(provider);
            }
        }
    }
}
