﻿using AccessTokenClient;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestingFivePointZeroApplication.Controllers
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
                ClientIdentifier = "testing_client_identifier",
                ClientSecret     = "511536EF-F270-4058-80CA-1C89C192F69A",
                Scopes           = new[]
                {
                    "employee:read", "employee:create", "employee:edit", "employee:delete"
                },
                TokenEndpoint    = "https://localhost:44303/connect/token"
            });

            return Ok(tokenResponse);
        }
    }
}