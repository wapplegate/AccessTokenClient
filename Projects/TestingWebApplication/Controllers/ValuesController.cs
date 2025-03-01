using AccessTokenClient;
using Microsoft.AspNetCore.Mvc;

namespace TestingWebApplication.Controllers;

[ApiController]
[Route("values")]
public class ValuesController : ControllerBase
{
    private readonly ITokenClient client;

    public ValuesController(ITokenClient client)
    {
        this.client = client;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}

public class TestingClient : ITestingClient
{
    private readonly HttpClient client;

    public TestingClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<string> Get()
    {
        var response = await client.GetAsync(new Uri("https://google.com"));

        return await response.Content.ReadAsStringAsync();
    }
}

public interface ITestingClient
{
    Task<string> Get();
}

public class TestingClientOptions : ITokenRequestOptions
{
    public string TokenEndpoint { get; set; }

    public string ClientIdentifier { get; set; }

    public string ClientSecret { get; set; }

    public string[] Scopes { get; set; } = [];
}
