using AccessTokenClient;
using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestingThreePointOneApplication
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
            services.AddMemoryCache();

            services.AddAccessTokenClient(builder =>
            {
                builder.AddPolicyHandler((provider, _) =>
                {
                    var logger = provider.GetService<ILogger<ITokenClient>>();
                    return AccessTokenClientPolicy.GetDefaultRetryPolicy(logger);
                });
            })
            .AddAccessTokenClientCache<MemoryTokenResponseCache>();

            services.AddControllers();
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