using AccessTokenClient.Expiration;
using AccessTokenClient.Keys;
using AccessTokenClient.Transformation;
using Microsoft.Extensions.Logging;
using System;
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

        private readonly ITokenResponseCache cache;

        private readonly IKeyGenerator keyGenerator;

        private readonly IExpirationCalculator calculator;

        private readonly IAccessTokenTransformer transformer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenClientCachingDecorator"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="decoratedClient">The decorated instance.</param>
        /// <param name="cache">The token response cache.</param>
        /// <param name="keyGenerator">The key generator.</param>
        /// <param name="calculator">The expiration calculator.</param>
        /// <param name="transformer">The access token transformer.</param>
        public TokenClientCachingDecorator(ILogger<TokenClientCachingDecorator> logger, ITokenClient decoratedClient, ITokenResponseCache cache, IKeyGenerator keyGenerator, IExpirationCalculator calculator, IAccessTokenTransformer transformer)
        {
            this.logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
            this.decoratedClient = decoratedClient ?? throw new ArgumentNullException(nameof(decoratedClient));
            this.cache           = cache           ?? throw new ArgumentNullException(nameof(cache));
            this.keyGenerator    = keyGenerator    ?? throw new ArgumentNullException(nameof(keyGenerator));
            this.calculator      = calculator      ?? throw new ArgumentNullException(nameof(calculator));
            this.transformer     = transformer     ?? throw new ArgumentNullException(nameof(transformer));
        }

        /// <summary>
        /// Makes a token request to the specified endpoint and returns the response.
        /// </summary>
        /// <param name="request">The token request.</param>
        /// <param name="execute">An optional execute function that will override the default request implementation.</param>
        /// <returns>The token response.</returns>
        public async Task<TokenResponse> RequestAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null)
        {
            var key = keyGenerator.GenerateTokenRequestKey(request);

            logger.LogInformation("Attempting to retrieve the token response with key '{Key}' from the cache.", key);

            var cachedTokenResponse = await cache.Get(key);

            if (cachedTokenResponse != null)
            {
                logger.LogInformation("Successfully retrieved the token response with key '{Key}' from the cache successfully.", key);

                cachedTokenResponse.AccessToken = transformer.Revert(cachedTokenResponse.AccessToken);

                return cachedTokenResponse;
            }

            logger.LogInformation("Token response with key '{Key}' does not exist in the cache.", key);

            var tokenResponse = await decoratedClient.RequestAccessToken(request, execute);

            await CacheTokenResponse(key, tokenResponse);

            return tokenResponse;
        }

        private async Task CacheTokenResponse(string key, TokenResponse tokenResponse)
        {
            logger.LogInformation("Attempting to store token response with key '{Key}' in the cache.", key);

            var expirationTimeSpan = calculator.CalculateExpiration(tokenResponse);

            var accessTokenValue = tokenResponse.AccessToken;

            tokenResponse.AccessToken = transformer.Convert(tokenResponse.AccessToken);

            var tokenStoredSuccessfully = await cache.Set(key, tokenResponse, expirationTimeSpan);

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