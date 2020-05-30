using AccessTokenClient.Extensions;
using AccessTokenClient.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AccessTokenClient
{
    public class AccessTokenClient : IAccessTokenClient
    {
        private readonly ILogger<AccessTokenClient> logger;

        private readonly HttpClient client;

        private readonly IResponseDeserializer deserializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenClient"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="client">The http client.</param>
        /// <param name="deserializer">The response deserializer.</param>
        public AccessTokenClient(ILogger<AccessTokenClient> logger, HttpClient client, IResponseDeserializer deserializer)
        {
            this.logger       = logger       ?? throw new ArgumentNullException(nameof(logger));
            this.client       = client       ?? throw new ArgumentNullException(nameof(client));
            this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        /// <summary>
        /// Makes a token request to the specified endpoint and returns the response.
        /// </summary>
        /// <param name="request">The token request.</param>
        /// <param name="execute">An optional execute function that will override the default request implementation.</param>
        /// <returns>The token response.</returns>
        public async Task<TokenResponse> GetAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null)
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