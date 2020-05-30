using AccessTokenClient;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestingTwoPointOneApplication.Controllers
{
    [ApiController]
    [Route("service")]
    public class ServiceController : ControllerBase
    {
        private readonly IAccessTokenClient client;

        public ServiceController(IAccessTokenClient client)
        {
            this.client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await client.GetAccessToken(new TokenRequest());

            return Ok(result);
        }
    }
}