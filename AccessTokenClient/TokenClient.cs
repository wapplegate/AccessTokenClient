using AccessTokenClient.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
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

        private readonly IHttpRequestMessageBuilder builder;

        private readonly IResponseDeserializer deserializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenClient"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="client">The http client.</param>
        /// <param name="builder">The http request message builder.</param>
        /// <param name="deserializer">The response deserializer.</param>
        public TokenClient(ILogger<TokenClient> logger, HttpClient client, IHttpRequestMessageBuilder builder, IResponseDeserializer deserializer)
        {
            this.logger       = logger       ?? throw new ArgumentNullException(nameof(logger));
            this.client       = client       ?? throw new ArgumentNullException(nameof(client));
            this.builder      = builder      ?? throw new ArgumentNullException(nameof(builder));
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
        public async Task<TokenResponse> RequestAccessToken(ITokenRequest request, Func<ITokenRequest, Task<TokenResponse>> execute = null)
        {
            var tokenResponse = await ExecuteTokenRequest(request, execute);

            if (TokenResponseValid(tokenResponse))
            {
                return tokenResponse;
            }

            logger.LogError("An invalid token response was returned from token endpoint '{Endpoint}'.", request.TokenEndpoint);

            throw new InvalidTokenResponseException($"An invalid token response was returned from token endpoint '{request.TokenEndpoint}'.");
        }

        private async Task<TokenResponse> ExecuteTokenRequest(ITokenRequest request, Func<ITokenRequest, Task<TokenResponse>> execute)
        {
            if (execute != null)
            {
                return await execute(request);
            }

            var requestMessage = builder.GenerateHttpRequestMessage(request);

            var responseMessage = await client.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new UnsuccessfulTokenResponseException("The request to the token endpoint was unsuccessful.");
            }

            var content = await responseMessage.Content.ReadAsStringAsync();

            return deserializer.Deserialize(content);
        }

        private static bool TokenResponseValid(TokenResponse response)
        {
            return response != null && !string.IsNullOrWhiteSpace(response.AccessToken);
        }
    }
}