using AccessTokenClient.Caching;
using AccessTokenClient.Expiration;
using AccessTokenClient.Keys;
using AccessTokenClient.Serialization;
using AccessTokenClient.Transformation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;

namespace AccessTokenClient.Extensions
{
    /// <summary>
    /// This static class contains an extension method used to add the
    /// <see cref="ITokenClient"/> and dependencies to the service collection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the token client and dependencies to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="action">An optional action to configure the token client options.</param>
        /// <param name="builderAction">
        /// An optional action used to configure the <see cref="IHttpClientBuilder"/> instance that is returned
        /// when the <see cref="ITokenClient"/> implementation is registered in the service collection via the
        /// AddHttpClient extension method. This can be used to register an <see cref="HttpMessageHandler"/> for
        /// the token client, or to configure a retry policy for the <see cref="ITokenClient"/> to handle transient
        /// errors that may be encountered. 
        /// </param>
        /// <returns>The service collection instance.</returns>
        public static IServiceCollection AddAccessTokenClient(this IServiceCollection services, Action<TokenClientOptions> action = null, Action<IHttpClientBuilder> builderAction = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var options = new TokenClientOptions();

            action?.Invoke(options);

            services.TryAddSingleton<IResponseDeserializer, ResponseDeserializer>();

            var httpClientBuilder = services.AddHttpClient<ITokenClient, TokenClient>("AccessTokenClient.TokenClient");

            builderAction?.Invoke(httpClientBuilder);

            if (options.EnableCaching)
            {
                services.TryAddSingleton<IExpirationCalculator, DefaultExpirationCalculator>();
                services.TryAddSingleton<IKeyGenerator, TokenRequestKeyGenerator>();
                services.TryAddSingleton<ITokenResponseCache, MemoryTokenResponseCache>();
                services.TryAddSingleton<IAccessTokenTransformer, DefaultAccessTokenTransformer>();
                services.TryDecorate<ITokenClient, TokenClientCachingDecorator>();
            }

            services.TryDecorate<ITokenClient, TokenClientValidationDecorator>();

            return services;
        }
    }
}