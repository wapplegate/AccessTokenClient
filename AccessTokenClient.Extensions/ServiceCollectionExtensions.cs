using AccessTokenClient.Caching;
using AccessTokenClient.Encryption;
using AccessTokenClient.Expiration;
using AccessTokenClient.Keys;
using AccessTokenClient.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

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
        /// <returns>The service collection instance.</returns>
        public static IServiceCollection AddAccessTokenClient(this IServiceCollection services, Action<TokenClientOptions> action = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var options = new TokenClientOptions();

            action?.Invoke(options);

            services.TryAddTransient<IExpirationCalculator, DefaultExpirationCalculator>();
            services.TryAddTransient<IKeyGenerator, TokenRequestKeyGenerator>();
            services.TryAddTransient<ITokenResponseCache, MemoryTokenResponseCache>();
            services.TryAddTransient<IEncryptionService, DefaultEncryptionService>();
            services.TryAddTransient<IResponseDeserializer, ResponseDeserializer>();

            services.AddHttpClient<ITokenClient, TokenClient>("AccessTokenClient.TokenClient");

            if (options.EnableCaching)
            {
                services.TryDecorate<ITokenClient, TokenClientCachingDecorator>();
            }

            services.TryDecorate<ITokenClient, TokenClientValidationDecorator>();

            return services;
        }
    }
}