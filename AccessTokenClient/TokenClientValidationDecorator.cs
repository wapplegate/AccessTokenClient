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

        public async Task<TokenResponse> GetAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null)
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

            var tokenResponse = await decoratedClient.GetAccessToken(request, execute);

            return tokenResponse;
        }
    }
}