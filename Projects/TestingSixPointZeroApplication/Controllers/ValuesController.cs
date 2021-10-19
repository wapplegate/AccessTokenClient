using AccessTokenClient;
using Microsoft.AspNetCore.Mvc;

namespace TestingSixPointZeroApplication.Controllers;

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