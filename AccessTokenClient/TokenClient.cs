using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient;

/// <summary>
/// This class contains a method that requests an access token from a
/// specified token endpoint using the client credentials oauth flow.
/// </summary>
public class TokenClient : ITokenClient
{
    private readonly ILogger<TokenClient> logger;

    private readonly HttpClient client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenClient"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="client">The http client.</param>
    public TokenClient(ILogger<TokenClient> logger, HttpClient client)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Executes a token request to the specified endpoint and returns the token response.
    /// </summary>
    /// <param name="request">The token request.</param>
    /// <param name="execute">
    /// An optional function that can be passed in to override the method that executes the token request.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The token response.</returns>
    public async Task<TokenResponse> RequestAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>>? execute = null, CancellationToken cancellationToken = default)
    {
        TokenRequestValidator.EnsureRequestIsValid(request);

        try
        {
            logger.LogInformation("Executing token request to token endpoint '{TokenEndpoint}'.", request.TokenEndpoint);

            var tokenResponse = await ExecuteTokenRequest(request, execute, cancellationToken);

            return tokenResponse;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred when executing the token request to token endpoint '{TokenEndpoint}'.", request.TokenEndpoint);
            throw;
        }
    }

    private async Task<TokenResponse> ExecuteTokenRequest(TokenRequest request, Func<TokenRequest, Task<TokenResponse>>? execute, CancellationToken cancellationToken)
    {
        if (execute != null)
        {
            return await execute(request);
        }

        TokenRequestValidator.EnsureRequestIsValid(request);

        cancellationToken.ThrowIfCancellationRequested();

        var uri = new Uri(request.TokenEndpoint);

        var scopes = request.Scopes != null ? string.Join(" ", request.Scopes) : string.Empty;

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id",     request.ClientIdentifier),
            new KeyValuePair<string, string>("client_secret", request.ClientSecret),
            new KeyValuePair<string, string>("grant_type",    "client_credentials"),
            new KeyValuePair<string, string>("scope",         scopes)
        ]);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };

        var responseMessage = await client.SendAsync(requestMessage, cancellationToken);

        if (!responseMessage.IsSuccessStatusCode)
        {
            var statusCode = (int)responseMessage.StatusCode;

            throw new HttpRequestException($"The token request failed with status code {statusCode}.");
        }
            
        var responseStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

        var tokenResponse = await JsonSerializer.DeserializeAsync(responseStream, TokenResponseJsonContext.Default.TokenResponse, cancellationToken);

        if (tokenResponse == null)
        {
            throw new InvalidOperationException("The token response was not deserialized successfully.");
        }

        if (!TokenResponseValid(tokenResponse))
        {
            throw new HttpRequestException($"An invalid token response was returned from token endpoint '{request.TokenEndpoint}'.");
        }

        logger.LogDebug("Token response from token endpoint '{TokenEndpoint}' is valid.", request.TokenEndpoint);

        return tokenResponse;
    }

    private static bool TokenResponseValid(TokenResponse? response)
    {
        return response != null && !string.IsNullOrWhiteSpace(response.AccessToken);
    }
}