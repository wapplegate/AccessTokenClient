using System;
using System.Threading.Tasks;

namespace AccessTokenClient
{
    /// <summary>
    /// Decorator that ensures the <see cref="TokenRequest"/> is valid.
    /// </summary>
    public class TokenClientValidationDecorator : ITokenClient
    {
        private readonly ITokenClient decoratedClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenClientValidationDecorator"/> class.
        /// </summary>
        /// <param name="decoratedClient">The decorated token client.</param>
        public TokenClientValidationDecorator(ITokenClient decoratedClient)
        {
            this.decoratedClient = decoratedClient;
        }

        /// <summary>
        /// Validates the token request is valid.
        /// </summary>
        /// <param name="request">The token request.</param>
        /// <param name="execute">A function to override the access token request process.</param>
        /// <returns>The <see cref="TokenResponse"/>.</returns>
        public async Task<TokenResponse> RequestAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null)
        {
            TokenRequestValidator.EnsureRequestIsValid(request);

            var tokenResponse = await decoratedClient.RequestAccessToken(request, execute);

            return tokenResponse;
        }
    }
}