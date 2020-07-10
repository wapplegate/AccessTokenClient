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
            TokenRequestValidator.EnsureRequestIsValid(request);

            var tokenResponse = await decoratedClient.RequestAccessToken(request, execute);

            return tokenResponse;
        }
    }
}