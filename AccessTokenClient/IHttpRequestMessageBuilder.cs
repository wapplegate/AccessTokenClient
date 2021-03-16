using System.Net.Http;

namespace AccessTokenClient
{
    /// <summary>
    /// Represents a builder class that returns a configured <see cref="HttpRequestMessage"/>.
    /// </summary>
    public interface IHttpRequestMessageBuilder
    {
        /// <summary>
        /// Generates the <see cref="HttpRequestMessage"/> used for the token request.
        /// </summary>
        /// <param name="request">The token request.</param>
        /// <returns>The configured <see cref="HttpRequestMessage"/>.</returns>
        HttpRequestMessage GenerateHttpRequestMessage(ITokenRequest request);
    }
}