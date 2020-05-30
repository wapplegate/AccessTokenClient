using System;

namespace AccessTokenClient.Caching
{
    /// <summary>
    /// This class acts as a wrapper for the cached value.
    /// </summary>
    /// <typeparam name="T">The type of the value that is cached.</typeparam>
    public class CacheItem<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItem{T}" class. />
        /// </summary>
        /// <param name="value">The value to cache.</param>
        /// <param name="expiresAfter">A timespan that indicates what time the item expires.</param>
        public CacheItem(T value, TimeSpan expiresAfter)
        {
            Value        = value;
            ExpiresAfter = expiresAfter;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value { get; }

        internal DateTimeOffset Created { get; } = DateTimeOffset.Now;

        internal TimeSpan ExpiresAfter { get; }
    }
}