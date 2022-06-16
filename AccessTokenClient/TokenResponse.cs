using System.Text.Json.Serialization;

namespace AccessTokenClient;

/// <summary>
/// A class that represents the response from a token endpoint.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration for the token in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}