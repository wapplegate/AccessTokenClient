﻿using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching
{
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
        /// <param name="execute">An optional execute function that will override the default request implementation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The token response.</returns>
        public async Task<TokenResponse> RequestAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null, CancellationToken cancellationToken = default)
        {
            TokenRequestValidator.EnsureRequestIsValid(request);

            var key = keyGenerator.GenerateTokenRequestKey(request, options.CacheKeyPrefix);

            logger.LogInformation("Attempting to retrieve the token response with key '{Key}' from the cache.", key);

            var cachedTokenResponse = await cache.Get(key, cancellationToken);

            if (cachedTokenResponse != null)
            {
                logger.LogInformation("Successfully retrieved the token response with key '{Key}' from the cache successfully.", key);

                cachedTokenResponse.AccessToken = transformer.Revert(cachedTokenResponse.AccessToken);

                return cachedTokenResponse;
            }

            logger.LogInformation("Token response with key '{Key}' does not exist in the cache.", key);

            var tokenResponse = await decoratedClient.RequestAccessToken(request, execute, cancellationToken);

            await CacheTokenResponse(key, tokenResponse, cancellationToken);

            return tokenResponse;
        }

        private async Task CacheTokenResponse(string key, TokenResponse tokenResponse, CancellationToken token)
        {
            logger.LogInformation("Attempting to store token response with key '{Key}' in the cache.", key);

            var expirationTimeSpan = TimeSpan.FromMinutes(tokenResponse.ExpiresIn / 60 - options.ExpirationBuffer);

            var accessTokenValue = tokenResponse.AccessToken;

            tokenResponse.AccessToken = transformer.Convert(tokenResponse.AccessToken);

            var tokenStoredSuccessfully = await cache.Set(key, tokenResponse, expirationTimeSpan, token);

            if (tokenStoredSuccessfully)
            {
                logger.LogInformation("Successfully stored token response with key '{Key}' in the cache.", key);
            }
            else
            {
                logger.LogWarning("Unable to store token response with key '{Key}' in the cache.", key);
            }

            tokenResponse.AccessToken = accessTokenValue;
        }
    }
}