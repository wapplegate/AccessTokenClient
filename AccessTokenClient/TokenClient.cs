using AccessTokenClient.Extensions;
using AccessTokenClient.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AccessTokenClient
{
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
        /// <returns>The token response.</returns>
        public async Task<TokenResponse> RequestAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null)
        {
            var tokenResponse = await ExecuteTokenRequest(request, execute);

            if (TokenResponseValid(tokenResponse))
            {
                return tokenResponse;
            }

            logger.LogError("An invalid token response was returned from token endpoint '{Endpoint}'.", request.TokenEndpoint);

            throw new Exception($"An invalid token response was returned from token endpoint '{request.TokenEndpoint}'.");
        }

        private async Task<TokenResponse> ExecuteTokenRequest(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute)
        {
            if (execute != null)
            {
                return await execute(request);
            }

            var content = await client.ExecuteClientCredentialsTokenRequest(request);

            return deserializer.Deserialize(content);
        }

        private static bool TokenResponseValid(TokenResponse response)
        {
            return response != null && !string.IsNullOrWhiteSpace(response.AccessToken);
        }
    }
}