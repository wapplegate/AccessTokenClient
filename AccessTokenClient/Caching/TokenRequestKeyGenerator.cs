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
        var builder = new StringBuilder();

        AppendValue(builder, request.TokenEndpoint);
        AppendValue(builder, request.ClientIdentifier);
        AppendValue(builder, request.ClientSecret);

        var scopes = request.Scopes ?? [];
        builder.Append(scopes.Length).Append(':');

        foreach (var scope in scopes)
        {
            AppendValue(builder, scope);
        }

        return builder.ToString();
    }

    private static void AppendValue(StringBuilder builder, string? value)
    {
        builder.Append(value?.Length ?? -1).Append(':').Append(value);
    }
}
