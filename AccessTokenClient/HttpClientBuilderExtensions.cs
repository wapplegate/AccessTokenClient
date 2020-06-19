using Microsoft.Extensions.DependencyInjection;

namespace AccessTokenClient
{
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="AccessTokenDelegatingHandler"/> as a message handler for the named client.
        /// </summary>
        /// <typeparam name="T">The type of options to inject into the handler.</typeparam>
        /// <param name="httpClientBuilder">The http client builder.</param>
        /// <returns>The http client builder instance.</returns>
        public static IHttpClientBuilder AddClientAccessTokenHandler<T>(this IHttpClientBuilder httpClientBuilder) where T : ITokenEndpointOptions
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var options = provider.GetRequiredService<T>();

                var tokenClient = provider.GetRequiredService<IAccessTokenClient>();

                return new AccessTokenDelegatingHandler(options, tokenClient);
            });
        }
    }
}