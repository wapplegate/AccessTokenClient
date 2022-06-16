using System;

namespace AccessTokenClient;

/// <summary>
/// This static class contains methods used to validate token requests.
/// </summary>
public static class TokenRequestValidator
{
    /// <summary>
    /// Ensures the token request is valid. If a token endpoint, client
    /// identifier, or client secret have not been specified this
    /// method will throw an exception.
    /// </summary>
    /// <param name="request">The token request.</param>
    public static void EnsureRequestIsValid(TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TokenEndpoint))
        {
            throw new ArgumentException("A token endpoint must be specified.");
        }

        if (string.IsNullOrWhiteSpace(request.ClientIdentifier))
        {
            throw new ArgumentException("A client identifier must be specified.");
        }

        if (string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            throw new ArgumentException("A client secret must be specified.");
        }
    }
}