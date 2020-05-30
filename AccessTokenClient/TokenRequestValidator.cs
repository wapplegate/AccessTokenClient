using System;

namespace AccessTokenClient
{
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
                throw new Exception("A token endpoint has not been specified.");
            }

            if (string.IsNullOrWhiteSpace(request.ClientIdentifier))
            {
                throw new Exception("A client identifier has not been specified.");
            }

            if (string.IsNullOrWhiteSpace(request.ClientSecret))
            {
                throw new Exception("A client secret has not been specified.");
            }
        }
    }
}