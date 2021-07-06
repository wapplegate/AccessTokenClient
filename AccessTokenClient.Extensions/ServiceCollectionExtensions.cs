﻿using AccessTokenClient.Caching;
using AccessTokenClient.Serialization;
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
        /// Adds the token client to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="builderAction">
        /// An optional action used to configure the <see cref="IHttpClientBuilder"/> instance that is returned
        /// when the <see cref="ITokenClient"/> implementation is registered in the service collection via the
        /// AddHttpClient extension method. This can be used to register an <see cref="HttpMessageHandler"/> for
        /// the token client, or to configure a retry policy for the <see cref="ITokenClient"/> to handle transient
        /// errors that may be encountered. 
        /// </param>
        /// <returns>The service collection instance.</returns>
        public static IServiceCollection AddAccessTokenClient(this IServiceCollection services, Action<IHttpClientBuilder> builderAction = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IResponseDeserializer, ResponseDeserializer>();

            var httpClientBuilder = services.AddHttpClient<ITokenClient, TokenClient>("AccessTokenClient.TokenClient");

            builderAction?.Invoke(httpClientBuilder);

            return services;
        }

        /// <summary>
        /// Enables caching for the token client.
        /// </summary>
        /// <typeparam name="T">
        /// An implementation of the <see cref="ITokenResponseCache"/> interface to register.
        /// </typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection instance.</returns>
        public static IServiceCollection AddAccessTokenClientCaching<T>(this IServiceCollection services) where T : ITokenResponseCache
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IExpirationCalculator, DefaultExpirationCalculator>();
            services.TryAddSingleton<IKeyGenerator, TokenRequestKeyGenerator>();
            services.TryAddSingleton(typeof(ITokenResponseCache), typeof(T));
            services.TryAddSingleton<IAccessTokenTransformer, DefaultAccessTokenTransformer>();

            services.TryDecorate<ITokenClient, TokenClientCachingDecorator>();

            return services;
        }
    }
}