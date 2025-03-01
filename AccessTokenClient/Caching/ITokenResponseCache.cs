using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching;

/// <summary>
/// Represents a token response cache that allows for the storage and retrieval of the <see cref="TokenResponse"/>.
/// </summary>
public interface ITokenResponseCache
{
    /// <summary>
    /// Gets the token response associated to the given key, if it exists.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the token response.</returns>
    Task<TokenResponse?> Get(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a token response in the cache with the specified expiration.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="response">The token response to store in the cache.</param>
    /// <param name="expiration">The expiration time span.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value indicating if the set operation was successful.</returns>
    Task<bool> Set(string key, TokenResponse response, TimeSpan expiration, CancellationToken cancellationToken = default);
}
