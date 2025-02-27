using AccessTokenClient;
using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

var services = new ServiceCollection();

services
    .AddLogging(configure => configure.AddSerilog())
    .AddMemoryCache()
    .AddAccessTokenClient(builder => builder.AddPolicyHandler(AccessTokenClientPolicy.GetDefaultRetryPolicy()))
    .AddAccessTokenClientCache<MemoryTokenResponseCache>();

var provider = services.BuildServiceProvider();

var client = provider.GetRequiredService<ITokenClient>();

if (client != null)
{
    while (true)
    {
        try
        {
            var tokenResponse = await client.RequestAccessToken(new TokenRequest
            {
                TokenEndpoint    = "https://localhost:44303/connect/token",
                ClientIdentifier = "testing_client_identifier",
                ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scopes           =
                [
                    "employee:read", "employee:create", "employee:edit", "employee:delete"
                ]
            });

            Console.WriteLine(tokenResponse.AccessToken);

            await Task.Delay(60000);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            Console.WriteLine("An error occurred when requesting an access token.");
        }
    }
}