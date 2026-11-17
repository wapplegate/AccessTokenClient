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
    private const int MaximumErrorContentLength = 1024;

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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The token response.</returns>
    public async Task<TokenResponse> RequestAccessToken(TokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var tokenResponse = await ExecuteTokenRequest(request, cancellationToken).ConfigureAwait(false);

            return tokenResponse;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred when executing the token request to token endpoint '{TokenEndpoint}'.", request.TokenEndpoint);
            throw;
        }
    }

    private async Task<TokenResponse> ExecuteTokenRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientIdentifier);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientSecret);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.TokenEndpoint);

        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation("Executing token request to token endpoint '{TokenEndpoint}'.", request.TokenEndpoint);

        var uri = new Uri(request.TokenEndpoint);

        var parameters = new List<KeyValuePair<string, string>>
        {
            new("client_id",     request.ClientIdentifier),
            new("client_secret", request.ClientSecret),
            new("grant_type",    "client_credentials")
        };

        var scopes = request.Scopes != null ? string.Join(" ", request.Scopes) : string.Empty;

        // The scope parameter is optional, so it is only sent when scopes have been specified:
        if (!string.IsNullOrWhiteSpace(scopes))
        {
            parameters.Add(new KeyValuePair<string, string>("scope", scopes));
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

        requestMessage.Content = new FormUrlEncodedContent(parameters);

        using var responseMessage = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

        if (!responseMessage.IsSuccessStatusCode)
        {
            throw await CreateTokenRequestException(responseMessage, cancellationToken).ConfigureAwait(false);
        }

        var responseStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var tokenResponse = await JsonSerializer.DeserializeAsync(responseStream, TokenResponseJsonContext.Default.HttpTokenResponse, cancellationToken).ConfigureAwait(false);

        if (tokenResponse == null)
        {
            throw new InvalidOperationException("The token response was not deserialized successfully.");
        }

        if (!TokenResponseValid(tokenResponse))
        {
            throw new HttpRequestException($"An invalid token response was returned from token endpoint '{request.TokenEndpoint}'.");
        }

        return new TokenResponse
        {
            AccessToken = tokenResponse.AccessToken,
            ExpiresIn = tokenResponse.ExpiresIn
        };
    }

    private static async Task<HttpRequestException> CreateTokenRequestException(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        var statusCode = (int)responseMessage.StatusCode;

        var message = $"The token request failed with status code {statusCode}.";

        // Token endpoints describe the failure in the response content, so include it in the message when available:
        var responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            if (responseContent.Length > MaximumErrorContentLength)
            {
                responseContent = responseContent[..MaximumErrorContentLength];
            }

            message = $"{message} Response: {responseContent}";
        }

        return new HttpRequestException(message, null, responseMessage.StatusCode);
    }

    private static bool TokenResponseValid(HttpTokenResponse? response)
    {
        return response != null && !string.IsNullOrWhiteSpace(response.AccessToken) && response.ExpiresIn > 0;
    }
}
