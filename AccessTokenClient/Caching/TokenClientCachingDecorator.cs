using AccessTokenClient.Encryption;
using AccessTokenClient.Expiration;
using AccessTokenClient.Keys;
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
        private readonly ITokenClient decoratedClient;

        private readonly ITokenResponseCache cache;

        private readonly IKeyGenerator keyGenerator;

        private readonly IExpirationCalculator calculator;

        private readonly IEncryptionService encryptionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenClientCachingDecorator"/> class.
        /// </summary>
        /// <param name="decoratedClient">The decorated instance.</param>
        /// <param name="cache">The token response cache.</param>
        /// <param name="keyGenerator">The key generator.</param>
        /// <param name="calculator">The expiration calculator.</param>
        /// <param name="encryptionService">The encryption service.</param>
        public TokenClientCachingDecorator(ITokenClient decoratedClient, ITokenResponseCache cache, IKeyGenerator keyGenerator, IExpirationCalculator calculator, IEncryptionService encryptionService)
        {
            this.decoratedClient   = decoratedClient   ?? throw new ArgumentNullException(nameof(decoratedClient));
            this.cache             = cache             ?? throw new ArgumentNullException(nameof(cache));
            this.keyGenerator      = keyGenerator      ?? throw new ArgumentNullException(nameof(keyGenerator));
            this.calculator        = calculator        ?? throw new ArgumentNullException(nameof(calculator));
            this.encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
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

            if (await cache.KeyExists(key))
            {
                var getResult = await cache.Get(key);

                if (getResult.Successful && getResult.Value != null)
                {
                    getResult.Value.AccessToken = encryptionService.Decrypt(getResult.Value.AccessToken);

                    return getResult.Value;
                }
            }

            var tokenResponse = await decoratedClient.RequestAccessToken(request, execute);

            var expiration = calculator.CalculateExpiration(tokenResponse);

            tokenResponse.AccessToken = encryptionService.Encrypt(tokenResponse.AccessToken);

            await cache.Set(key, tokenResponse, TimeSpan.FromMinutes(expiration));

            return tokenResponse;
        }
    }
}