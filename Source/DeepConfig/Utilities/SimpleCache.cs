namespace DeepConfig.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Simple cache for caching objects. This class is thread safe.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    internal class SimpleCache<TKey, TItem>
    {
        private Dictionary<TKey, CacheItem> _cache;

        private Timer _autoCleanupTimer;
        private int _cleanUpInterval;
        private object _timerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCache&lt;TKey, TItem&gt;"/> class.
        /// </summary>
        internal SimpleCache()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCache&lt;TKey, TItem&gt;"/> class.
        /// </summary>
        /// <param name="cleanUpInterval">The interval in miliseconds between auto clean up of expired cache items. Default is 1 minute.</param>
        internal SimpleCache(int cleanUpInterval)
            : this(null, cleanUpInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCache&lt;TKey, TItem&gt;"/> class.
        /// </summary>
        /// <param name="keyComparer">The comparer to use for key comparisons</param>
        internal SimpleCache(IEqualityComparer<TKey> keyComparer)
            : this(keyComparer, 60000)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCache&lt;TKey, TItem&gt;"/> class.
        /// </summary>
        /// <param name="keyComparer">The comparer to use for key comparisons</param>
        /// <param name="cleanUpInterval">The interval in miliseconds between auto clean up of expired cache items. Default is 1 minute.</param>
        internal SimpleCache(IEqualityComparer<TKey> keyComparer, int cleanUpInterval)
        {
            _cache = new Dictionary<TKey, CacheItem>(keyComparer);
            _cleanUpInterval = cleanUpInterval;
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                lock (_cache)
                {
                    return _cache.Count;
                }
            }
        }

        /// <summary>
        /// Cache indexer. Gets an object from the cache by key.
        /// </summary>
        /// <param name="key">The key for which to retrieve an item</param>
        /// <returns>The item matching the key, or null if not found.</returns>
        public TItem this[TKey key]
        {
            get
            {
                TItem value;
                TryGet(key, out value);

                return value;
            }
        }

        /// <summary>
        /// Gets the value from the cache if it exists
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <param name="value">The value</param>
        /// <returns><see langword="true"/> if found otherwise <see langword="false"/></returns>
        public bool TryGet(TKey key, out TItem value)
        {
            lock (_cache)
            {
                CacheItem item;
                if (_cache.TryGetValue(key, out item) && !item.HasExpired)
                {
                    value = item.Item;
                    item.UpdateExpiration();
                    return true;
                }
            }

            value = default(TItem);
            return false;
        }

        /// <summary>
        /// Adds or updates an item in the cache.
        /// </summary>
        /// <param name="key">The unique key identifying the object in the cache.</param>
        /// <param name="item">The item to store in the cache.</param>
        /// <param name="cacheForInMilliseconds">The number of miliseconds the object will remain valid for, i.e. after this time it will expire from the cache.</param>
        /// <param name="slidingExpiration">Whether to renew the expiration time on an item if it is accessed before it expires.</param>
        public void Add(TKey key, TItem item, double cacheForInMilliseconds, bool slidingExpiration)
        {
            lock (_cache)
            {
                _cache[key] = new CacheItem(item, cacheForInMilliseconds, slidingExpiration);

                EnsureCleanupTimer();
            }
        }

        /// <summary>
        /// Removes an item from the cache if it exists.
        /// <para>
        /// If the item specified does not exist, nothing occurs.
        /// </para>
        /// </summary>
        /// <param name="key">The unique key identifying the object in the cache.</param>
        /// <returns>The object that was removed or null if the object was not found or had expired.</returns>
        public TItem Remove(TKey key)
        {
            lock (_cache)
            {
                TItem val = this[key];

                _cache.Remove(key);

                if (_cache.Count == 0)
                {
                    DisposeCleanupTimer();
                }

                return val;
            }
        }

        /// <summary>
        /// Determines if the particular key exists in the cache
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns><see langword="true"/> if found otherwise <see langword="false"/></returns>
        public bool Contains(TKey key)
        {
            lock (_cache)
            {
                CacheItem item;
                if (_cache.TryGetValue(key, out item))
                {
                    return !item.HasExpired;
                }
            }

            return false;
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public void Clear()
        {
            lock (_cache)
            {
                _cache.Clear();
                DisposeCleanupTimer();
            }
        }

        private void CleanupTimeReached(object timerId)
        {
            List<TKey> keysToDelete = new List<TKey>();

            lock (_cache)
            {
                //If the callback is called after the timer is disposed (which may well happen), this ensures that it does not run.
                if (timerId != _timerId)
                {
                    return;
                }

                foreach (KeyValuePair<TKey, CacheItem> entry in _cache)
                {
                    if (entry.Value.HasExpired)
                    {
                        keysToDelete.Add(entry.Key);
                    }
                }

                foreach (TKey key in keysToDelete)
                {
                    _cache.Remove(key);
                }

                if (_cache.Count == 0)
                {
                    DisposeCleanupTimer();
                }
            }
        }

        private void EnsureCleanupTimer()
        {
            if (_autoCleanupTimer == null)
            {
                _timerId = new object();
                _autoCleanupTimer = new Timer(CleanupTimeReached, _timerId, _cleanUpInterval, _cleanUpInterval);
            }
        }

        private void DisposeCleanupTimer()
        {
            if (_autoCleanupTimer != null)
            {
                _timerId = null;
                _autoCleanupTimer.Dispose();
                _autoCleanupTimer = null;
            }
        }

        private class CacheItem
        {
            private DateTime _expires;
            private double _sliding;

            internal CacheItem(TItem item, double cacheForInMilliseconds, bool slidingExpiration)
            {
                this.Item = item;
                _expires = DateTime.UtcNow.AddMilliseconds(cacheForInMilliseconds);
                _sliding = slidingExpiration ? cacheForInMilliseconds : 0;
            }

            internal TItem Item { get; set; }

            internal bool HasExpired
            {
                get
                {
                    return (_expires <= DateTime.UtcNow);
                }
            }

            internal void UpdateExpiration()
            {
                if (_sliding > 0)
                {
                    _expires = DateTime.UtcNow.AddMilliseconds(_sliding);
                }
            }
        }
    }
}
