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
        TokenRequestValidator.EnsureRequestIsValid(request);

        var concatenatedRequest = GenerateConcatenatedRequest(request);

        using var hasher = SHA256.Create();
        var textData = Encoding.UTF8.GetBytes(concatenatedRequest);
        var hash = hasher.ComputeHash(textData);

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