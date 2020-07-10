using System;
using System.Threading.Tasks;

namespace AccessTokenClient
{
    public class TokenClientValidationDecorator : ITokenClient
    {
        private readonly ITokenClient decoratedClient;

        public TokenClientValidationDecorator(ITokenClient decoratedClient)
        {
            this.decoratedClient = decoratedClient;
        }

        public async Task<TokenResponse> RequestAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null)
        {
            if (string.IsNullOrWhiteSpace(request.TokenEndpoint))
            {
                throw new ArgumentNullException(nameof(request.TokenEndpoint));
            }

            if (string.IsNullOrWhiteSpace(request.ClientIdentifier))
            {
                throw new ArgumentNullException(nameof(request.ClientIdentifier));
            }

            if (string.IsNullOrWhiteSpace(request.ClientSecret))
            {
                throw new ArgumentNullException(nameof(request.ClientSecret));
            }

            var tokenResponse = await decoratedClient.RequestAccessToken(request, execute);

            return tokenResponse;
        }
    }
}