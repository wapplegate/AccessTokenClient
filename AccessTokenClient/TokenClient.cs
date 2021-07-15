using AccessTokenClient.Extensions;
using AccessTokenClient.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient
{
    /// <summary>
    /// This class contains a method that requests an access token from a
    /// specified token endpoint using the client credentials oauth flow.
    /// </summary>
    public class TokenClient : ITokenClient
    {
        private readonly ILogger<TokenClient> logger;

        private readonly HttpClient client;

        private readonly IResponseDeserializer deserializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenClient"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="client">The http client.</param>
        /// <param name="deserializer">The response deserializer.</param>
        public TokenClient(ILogger<TokenClient> logger, HttpClient client, IResponseDeserializer deserializer)
        {
            this.logger       = logger       ?? throw new ArgumentNullException(nameof(logger));
            this.client       = client       ?? throw new ArgumentNullException(nameof(client));
            this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        /// <summary>
        /// Executes a token request to the specified endpoint and returns the token response.
        /// </summary>
        /// <param name="request">The token request.</param>
        /// <param name="execute">
        /// An optional function that can be passed in to override the method that executes the token request.
        /// </param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The token response.</returns>
        public async Task<TokenResponse> RequestAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null, CancellationToken token = default)
        {
            TokenRequestValidator.EnsureRequestIsValid(request);

            try
            {
                logger.LogInformation("Executing token request to token endpoint '{TokenEndpoint}'.", request.TokenEndpoint);

                var tokenResponse = await ExecuteTokenRequest(request, execute, token);

                return tokenResponse;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred when executing the token request to token endpoint '{TokenEndpoint}'.", request.TokenEndpoint);
                throw;
            }
        }

        private async Task<TokenResponse> ExecuteTokenRequest(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute, CancellationToken token)
        {
            if (execute != null)
            {
                return await execute(request);
            }

            var content = await client.ExecuteClientCredentialsTokenRequest(request, token);

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new HttpRequestException($"A null or empty response was returned from token endpoint '{request.TokenEndpoint}'.");
            }

            var tokenResponse = deserializer.Deserialize(content);

            if (!TokenResponseValid(tokenResponse))
            {
                throw new HttpRequestException($"An invalid token response was returned from token endpoint '{request.TokenEndpoint}'.");
            }

            logger.LogInformation("Token response from token endpoint '{TokenEndpoint}' is valid.", request.TokenEndpoint);

            return tokenResponse;
        }

        private static bool TokenResponseValid(TokenResponse response)
        {
            return response != null && !string.IsNullOrWhiteSpace(response.AccessToken);
        }
    }
}