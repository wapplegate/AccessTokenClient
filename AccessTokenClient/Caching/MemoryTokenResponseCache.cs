using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching
{
    public class MemoryTokenResponseCache : ITokenResponseCache
    {
        private static readonly ConcurrentDictionary<string, CacheItem<TokenResponse>> Dictionary;

        static MemoryTokenResponseCache()
        {
            Dictionary = new ConcurrentDictionary<string, CacheItem<TokenResponse>>();
        }

        /// <inheritdoc />
        public Task<bool> KeyExists(string key) => Task.FromResult(Dictionary.ContainsKey(key));

        /// <inheritdoc />
        public Task<TokenGetResult<TokenResponse>> Get(string key)
        {
            var exists = Dictionary.TryGetValue(key, out var cacheItem);

            if (!exists)
            {
                return Task.FromResult(new TokenGetResult<TokenResponse>
                {
                    Successful = false,
                    Value      = null
                });
            }

            if (DateTimeOffset.Now - cacheItem.Created >= cacheItem.ExpiresAfter)
            {
                Dictionary.TryRemove(key, out var _);

                return Task.FromResult(new TokenGetResult<TokenResponse>
                {
                    Successful = false,
                    Value      = null
                });
            }

            var result = new TokenGetResult<TokenResponse>
            {
                Successful = true,
                Value      = cacheItem.Value
            };

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> Set(string key, TokenResponse response, TimeSpan expiration)
        {
            var cacheItem = new CacheItem<TokenResponse>(response, expiration);

            Dictionary[key] = cacheItem;

            return Task.FromResult(true);
        }
    }
}