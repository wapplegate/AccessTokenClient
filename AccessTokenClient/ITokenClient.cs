using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient
{
    /// <summary>
    /// Represents a token client that requests access tokens.
    /// </summary>
    public interface ITokenClient
    {
        /// <summary>
        /// Executes a token request to the specified endpoint and returns the token response.
        /// </summary>
        /// <param name="request">The token request.</param>
        /// <param name="execute">
        /// An optional function that can be passed in to override the method that executes the token request.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The token response.</returns>
        Task<TokenResponse> RequestAccessToken(TokenRequest request, Func<TokenRequest, Task<TokenResponse>>? execute = null, CancellationToken cancellationToken = default);
    }
}