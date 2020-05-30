using System;
using System.Threading.Tasks;

namespace AccessTokenClient
{
    public interface IAccessTokenClient
    {
        /// <summary>
        /// Executes a token request to the specified endpoint and returns the token response.
        /// </summary>
        /// <param name="request">The token request.</param>
        /// <param name="execute">
        /// An optional function that can be passed in to override the method that executes the token request.
        /// </param>
        /// <returns>The token response.</returns>
        Task<TokenResponse> GetAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>> execute = null);
    }
}