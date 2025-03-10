using System;
using System.Linq;
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
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientIdentifier);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.ClientSecret);

        ArgumentException.ThrowIfNullOrWhiteSpace(request.TokenEndpoint);

        var concatenatedRequest = GenerateConcatenatedRequest(request);

        var textData = Encoding.UTF8.GetBytes(concatenatedRequest);

        var hash = SHA256.HashData(textData);

        var convertedHash = BitConverter.ToString(hash).Replace("-", string.Empty);

        return $"{prefix}::{convertedHash}";
    }

    private static string GenerateConcatenatedRequest(TokenRequest request)
    {
        var tokenEndpoint    = request.TokenEndpoint;
        var clientIdentifier = request.ClientIdentifier;
        var clientSecret     = request.ClientSecret;
        var scopes           = string.Join(",", request.Scopes.Select(s => s));

        var concatenated = $"{tokenEndpoint}:{clientIdentifier}:{clientSecret}:{scopes}";

        return concatenated;
    }
}
