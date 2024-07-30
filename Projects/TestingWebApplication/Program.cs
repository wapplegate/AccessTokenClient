using AccessTokenClient;
using AccessTokenClient.Caching;
using AccessTokenClient.Extensions;
using TestingWebApplication.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TestingWebApplication", Version = "v1" });
});

builder.Services.AddMemoryCache();

builder.Services.AddAccessTokenClient(httpClientBuilder =>
{
    httpClientBuilder.AddPolicyHandler((provider, _) =>
    {
        var logger = provider.GetService<ILogger<ITokenClient>>();
        return AccessTokenClientPolicy.GetDefaultRetryPolicy(logger);
    });
})
.AddAccessTokenClientCache<MemoryTokenResponseCache>(options =>
{
    options.ExpirationBuffer = 5;
    options.CacheKeyPrefix = "AccessTokenClient";
});

builder.Services.AddSingleton(new TestingClientOptions
{
    TokenEndpoint    = "https://localhost:44303/connect/token",
    ClientIdentifier = "testing_client_identifier",
    ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
    Scopes           = ["movie:read"]
});

builder.Services
    .AddHttpClient<ITestingClient, TestingClient>()
    .AddClientAccessTokenHandler<TestingClientOptions>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestingWebApplication v1"));
}

app.UseAuthorization();

app.MapControllers();

app.Run();