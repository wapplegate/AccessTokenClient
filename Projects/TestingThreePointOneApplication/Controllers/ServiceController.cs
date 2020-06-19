using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestingThreePointOneApplication.Controllers
{
    [ApiController]
    [Route("service")]
    public class ServiceController : ControllerBase
    {
        private readonly ITestingClient client;

        public ServiceController(ITestingClient client)
        {
            this.client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await client.Get();

            return Ok(result);
        }
    }
}