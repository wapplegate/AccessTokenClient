using AccessTokenClient;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestingTwoPointOneApplication.Controllers
{
    [ApiController]
    [Route("service")]
    public class ServiceController : ControllerBase
    {
        private readonly ITokenClient client;

        public ServiceController(ITokenClient client)
        {
            this.client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tokenResponse = await client.RequestAccessToken(new TokenRequest
            {
                ClientIdentifier = "client",
                ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scopes           = new[] { "api1" },
                TokenEndpoint    = "https://localhost:44303/connect/token"
            });

            return Ok(tokenResponse);
        }
    }
}