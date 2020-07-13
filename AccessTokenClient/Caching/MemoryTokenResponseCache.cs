using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching
{
    /// <summary>
    /// This class is used to store token responses in a memory
    /// cache where they can be retrieved and reused quickly.
    /// </summary>
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
            var exists = cache.TryGetValue<TokenResponse>(key, out var cachedTokenResponse);

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
                Value      = cachedTokenResponse
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