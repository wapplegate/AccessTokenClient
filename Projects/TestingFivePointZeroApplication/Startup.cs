using AccessTokenClient;
using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestingFivePointZeroApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMemoryCache();

            services.AddAccessTokenClient(builder =>
            {
                builder.AddPolicyHandler((provider, _) =>
                {
                    var logger = provider.GetService<ILogger<ITokenClient>>();
                    return AccessTokenClientPolicy.GetDefaultRetryPolicy(logger);
                });
            })
            .AddAccessTokenClientCaching<MemoryTokenResponseCache>();
            
            services.AddSingleton(new TestingClientOptions
            {
                TokenEndpoint    = "https://localhost:44342/connect/token",
                ClientIdentifier = "client",
                ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scopes           = new[] { "api1" }
            });

            services
                .AddHttpClient<ITestingClient, TestingClient>()
                .AddClientAccessTokenHandler<TestingClientOptions>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}