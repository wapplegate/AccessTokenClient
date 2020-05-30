using AccessTokenClient;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestingThreePointOneApplication.Controllers
{
    [ApiController]
    [Route("service")]
    public class ServiceController : ControllerBase
    {
        private readonly IAccessTokenClient tokenClient;

        public ServiceController(IAccessTokenClient tokenClient)
        {
            this.tokenClient = tokenClient;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tokenResponse = await tokenClient.GetAccessToken(new TokenRequest
            {
                ClientIdentifier = "client",
                ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scopes           = new[] {"api1"},
                TokenEndpoint    = "https://localhost:5001/connect/token"
            });

            return Ok(tokenResponse);
        }
    }
}