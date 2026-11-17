using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching;

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
    public Task<TokenResponse?> Get(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var exists = cache.TryGetValue<TokenResponse?>(key, out var cachedTokenResponse);

        return !exists ? Task.FromResult<TokenResponse?>(null) : Task.FromResult(cachedTokenResponse);
    }

    /// <inheritdoc />
    public Task<bool> Set(string key, TokenResponse response, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // A memory cache with a size limit rejects entries that do not specify a size, so each token response counts as one unit:
        cache.Set(key, response, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            Size                            = 1
        });

        return Task.FromResult(true);
    }
}
