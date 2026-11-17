using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching;

/// <summary>
/// This decorator class is responsible for managing the cached access tokens that are available
/// and executing token requests using the decorated client instance to make token requests
/// when a cached value does not exist.
/// </summary>
public class TokenClientCachingDecorator : ITokenClient
{
    // The token client is registered as a transient service, so the locks are shared
    // between all instances to ensure concurrent requests for the same key are serialized.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    private readonly ILogger<TokenClientCachingDecorator> logger;

    private readonly ITokenClient decoratedClient;

    private readonly TokenClientCacheOptions options;

    private readonly ITokenResponseCache cache;

    private readonly IKeyGenerator keyGenerator;

    private readonly IAccessTokenTransformer transformer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenClientCachingDecorator"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="decoratedClient">The decorated instance.</param>
    /// <param name="options">The token client cache options.</param>
    /// <param name="cache">The token response cache.</param>
    /// <param name="keyGenerator">The key generator.</param>
    /// <param name="transformer">The access token transformer.</param>
    public TokenClientCachingDecorator(ILogger<TokenClientCachingDecorator> logger, ITokenClient decoratedClient, TokenClientCacheOptions options, ITokenResponseCache cache, IKeyGenerator keyGenerator, IAccessTokenTransformer transformer)
    {
        this.logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
        this.decoratedClient = decoratedClient ?? throw new ArgumentNullException(nameof(decoratedClient));
        this.options         = options         ?? throw new ArgumentNullException(nameof(options));
        this.cache           = cache           ?? throw new ArgumentNullException(nameof(cache));
        this.keyGenerator    = keyGenerator    ?? throw new ArgumentNullException(nameof(keyGenerator));
        this.transformer     = transformer     ?? throw new ArgumentNullException(nameof(transformer));
    }

    /// <summary>
    /// Makes a token request to the specified endpoint and returns the response.
    /// </summary>
    /// <param name="request">The token request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The token response.</returns>
    public async Task<TokenResponse> RequestAccessToken(TokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientIdentifier);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientSecret);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.TokenEndpoint);

        cancellationToken.ThrowIfCancellationRequested();

        var key = keyGenerator.GenerateTokenRequestKey(request, options.CacheKeyPrefix);

        var cachedTokenResponse = await GetCachedTokenResponse(key, cancellationToken);

        if (cachedTokenResponse != null)
        {
            return cachedTokenResponse;
        }

        // Only allow one request per key to reach the token endpoint at a time. Any
        // concurrent requests wait and then use the token response that was cached:
        var semaphore = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            cachedTokenResponse = await GetCachedTokenResponse(key, cancellationToken);

            if (cachedTokenResponse != null)
            {
                return cachedTokenResponse;
            }

            logger.LogDebug("Token response with key '{Key}' does not exist in the cache.", key);

            var tokenResponse = await decoratedClient.RequestAccessToken(request, cancellationToken);

            await CacheTokenResponse(key, tokenResponse, cancellationToken);

            return tokenResponse;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<TokenResponse?> GetCachedTokenResponse(string key, CancellationToken cancellationToken)
    {
        logger.LogDebug("Attempting to retrieve the token response with key '{Key}' from the cache.", key);

        var cachedTokenResponse = await cache.Get(key, cancellationToken);

        if (cachedTokenResponse == null)
        {
            return null;
        }

        logger.LogDebug("Successfully retrieved the token response with key '{Key}' from the cache.", key);

        // Return a copy so the instance held by the cache is never modified:
        return new TokenResponse
        {
            AccessToken = cachedTokenResponse.AccessToken != null ? transformer.Revert(cachedTokenResponse.AccessToken) : null,
            ExpiresIn   = cachedTokenResponse.ExpiresIn
        };
    }

    private async Task CacheTokenResponse(string key, TokenResponse tokenResponse, CancellationToken cancellationToken)
    {
        var expirationTimeSpan = TimeSpan.FromSeconds(tokenResponse.ExpiresIn) - TimeSpan.FromMinutes(options.ExpirationBuffer);

        if (expirationTimeSpan <= TimeSpan.Zero)
        {
            logger.LogWarning(
                "Token response with key '{Key}' expires in {ExpiresIn} seconds, which is too short to cache with the configured expiration buffer of {ExpirationBuffer} minutes.",
                key,
                tokenResponse.ExpiresIn,
                options.ExpirationBuffer);

            return;
        }

        logger.LogDebug("Attempting to store token response with key '{Key}' in the cache.", key);

        // Store a copy so the instance returned to the caller is not shared with the cache:
        var cacheEntry = new TokenResponse
        {
            AccessToken = tokenResponse.AccessToken != null ? transformer.Convert(tokenResponse.AccessToken) : null,
            ExpiresIn   = tokenResponse.ExpiresIn
        };

        var tokenStoredSuccessfully = await cache.Set(key, cacheEntry, expirationTimeSpan, cancellationToken);

        if (tokenStoredSuccessfully)
        {
            logger.LogDebug("Successfully stored token response with key '{Key}' in the cache.", key);
        }
        else
        {
            logger.LogWarning("Unable to store token response with key '{Key}' in the cache.", key);
        }
    }
}
