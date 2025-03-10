namespace AccessTokenClient;

/// <summary>
/// A class that represents the response from a token endpoint.
/// </summary>
public sealed class TokenResponse
{
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration for the token in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}
