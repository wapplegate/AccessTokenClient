using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AccessTokenClient.Keys
{
    /// <summary>
    /// A key generator that returns a hash of the <see cref="TokenRequest"/>.
    /// </summary>
    public class TokenRequestKeyGenerator : IKeyGenerator
    {
        /// <inheritdoc />
        public string GenerateTokenRequestKey(TokenRequest request)
        {
            TokenRequestValidator.EnsureRequestIsValid(request);

            var concatenated = GenerateConcatenatedRequest(request);

            using (var hasher = new SHA256Managed())
            {
                var textData = Encoding.UTF8.GetBytes(concatenated);
                var hash     = hasher.ComputeHash(textData);

                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
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
}