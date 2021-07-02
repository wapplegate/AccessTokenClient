using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AccessTokenClient.Extensions
{
    /// <summary>
    /// This static class provides extension methods for the http client
    /// in order to make client credentials access token requests.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Executes the token request.
        /// </summary>
        /// <param name="client">The http client.</param>
        /// <param name="request">The token request.</param>
        /// <returns>The token response.</returns>
        public static async Task<string> ExecuteClientCredentialsTokenRequest(this HttpClient client, TokenRequest request)
        {
            TokenRequestValidator.EnsureRequestIsValid(request);

            var uri = new Uri(request.TokenEndpoint);

            var scopes = request.Scopes != null ? string.Join(" ", request.Scopes) : string.Empty;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",     request.ClientIdentifier),
                new KeyValuePair<string, string>("client_secret", request.ClientSecret),
                new KeyValuePair<string, string>("grant_type",    "client_credentials"),
                new KeyValuePair<string, string>("scope",         scopes)
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };

            var responseMessage = await client.SendAsync(requestMessage);

            if (responseMessage.IsSuccessStatusCode)
            {
                return await responseMessage.Content.ReadAsStringAsync();
            }

            var statusCode = (int)responseMessage.StatusCode;
            throw new UnsuccessfulTokenResponseException($"Token request failed with status code {statusCode}.");
        }
    }
}