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
    private readonly ILogger<TokenClientCachingDecorator> logger;

    private readonly ITokenClient decoratedClient;

    private readonly TokenClientCacheOptions options;

    private readonly ITokenResponseCache cache;

    private readonly IKeyGenerator keyGenerator;

    private readonly IAccessTokenTransformer transformer;

    private readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new();

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
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientSecret);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TokenEndpoint);

        cancellationToken.ThrowIfCancellationRequested();

        var key = keyGenerator.GenerateTokenRequestKey(request, options.CacheKeyPrefix);

        logger.LogDebug("Attempting to retrieve the token response with key '{Key}' from the cache.", key);

        var cachedTokenResponse = await cache.Get(key, cancellationToken);

        if (cachedTokenResponse != null)
        {
            logger.LogDebug("Successfully retrieved the token response with key '{Key}' from the cache.", key);

            return new TokenResponse
            {
                AccessToken = transformer.Revert(cachedTokenResponse.AccessToken),
                ExpiresIn = cachedTokenResponse.ExpiresIn
            };
        }

        var semaphore = locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            // Double-check the cache after acquiring the lock:
            cachedTokenResponse = await cache.Get(key, cancellationToken);

            if (cachedTokenResponse != null)
            {
                logger.LogDebug("Successfully retrieved the token response with key '{Key}' from the cache.", key);

                return new TokenResponse
                {
                    AccessToken = transformer.Revert(cachedTokenResponse.AccessToken),
                    ExpiresIn = cachedTokenResponse.ExpiresIn
                };
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

    private async Task CacheTokenResponse(string key, TokenResponse tokenResponse, CancellationToken cancellationToken)
    {
        var expirationMinutes = (double)tokenResponse.ExpiresIn / 60 - options.ExpirationBuffer;

        if (expirationMinutes <= 0)
        {
            logger.LogWarning(
                "Token response with key '{Key}' has a lifetime of {ExpiresIn} seconds which is too short to cache with the configured expiration buffer of {Buffer} minutes.",
                key, tokenResponse.ExpiresIn, options.ExpirationBuffer);

            return;
        }

        logger.LogDebug("Attempting to store token response with key '{Key}' in the cache.", key);

        var expirationTimeSpan = TimeSpan.FromMinutes(expirationMinutes);

        var cacheEntry = new TokenResponse
        {
            AccessToken = transformer.Convert(tokenResponse.AccessToken),
            ExpiresIn = tokenResponse.ExpiresIn
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
