using System;
using System.Security.Cryptography;
using System.Text;

namespace AccessTokenClient.Caching;

/// <summary>
/// A key generator that returns a hash of the <see cref="TokenRequest"/>.
/// </summary>
public class TokenRequestKeyGenerator : IKeyGenerator
{
    /// <inheritdoc />
    public string GenerateTokenRequestKey(TokenRequest request, string prefix)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientIdentifier);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientSecret);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.TokenEndpoint);

        var concatenatedRequest = GenerateConcatenatedRequest(request);

        var textData = Encoding.UTF8.GetBytes(concatenatedRequest);

        var hash = SHA256.HashData(textData);

        var convertedHash = Convert.ToHexString(hash);

        return $"{prefix}::{convertedHash}";
    }

    private static string GenerateConcatenatedRequest(TokenRequest request)
    {
        var tokenEndpoint    = request.TokenEndpoint;
        var clientIdentifier = request.ClientIdentifier;
        var clientSecret     = request.ClientSecret;
        var scopes           = request.Scopes != null ? string.Join(",", request.Scopes) : string.Empty;

        var concatenated = $"{tokenEndpoint}:{clientIdentifier}:{clientSecret}:{scopes}";

        return concatenated;
    }
}
