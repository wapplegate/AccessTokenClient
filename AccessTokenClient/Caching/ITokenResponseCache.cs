using System;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching
{
    public interface ITokenResponseCache
    {
        /// <summary>
        /// Determines if the specified key exists in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>A value indicating whether the key exists in the cache.</returns>
        Task<bool> KeyExists(string key);

        /// <summary>
        /// Gets the token response associated to the given key, if it exists.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>A result containing the token response.</returns>
        Task<TokenGetResult<TokenResponse>> Get(string key);

        /// <summary>
        /// Sets a token response in the cache with the specified expiration.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="response">The token response to store in the cache.</param>
        /// <param name="expiration">The expiration time span.</param>
        /// <returns>A value indicating if the set operation was successful.</returns>
        Task<bool> Set(string key, TokenResponse response, TimeSpan expiration);
    }
}