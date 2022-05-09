using System;

namespace AccessTokenClient;

/// <summary>
/// This class encapsulates the parameters that are
/// necessary to make a client credentials token request. 
/// </summary>
public class TokenRequest
{
    /// <summary>
    /// Gets or sets the token endpoint URL.
    /// </summary>
    public string? TokenEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string? ClientIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the requested scopes.
    /// </summary>
    public string[] Scopes { get; set; } = Array.Empty<string>();
}