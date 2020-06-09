using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching
{
    public class MemoryTokenResponseCache : ITokenResponseCache
    {
        private readonly IMemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryTokenResponseCache"/> class.
        /// </summary>
        /// <param name="cache">The memory cache.</param>
        public MemoryTokenResponseCache(IMemoryCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <inheritdoc />
        public Task<bool> KeyExists(string key) => Task.FromResult(cache.TryGetValue(key, out var _));

        /// <inheritdoc />
        public Task<TokenGetResult<TokenResponse>> Get(string key)
        {
            var exists = cache.TryGetValue<TokenResponse>(key, out var cacheItem);

            if (!exists)
            {
                return Task.FromResult(new TokenGetResult<TokenResponse>
                {
                    Successful = false,
                    Value      = null
                });
            }

            var result = new TokenGetResult<TokenResponse>
            {
                Successful = true,
                Value      = cacheItem
            };

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> Set(string key, TokenResponse response, TimeSpan expiration)
        {
            cache.Set(key, response, expiration);

            return Task.FromResult(true);
        }
    }
}