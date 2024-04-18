using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Api.Application.Services;
using IdentityService.Api.Application.Models;
namespace IdentityService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IIdentityServices identityService;

        public AuthController(IIdentityServices identityService)
        {
            this.identityService = identityService;
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel loginRequestModel)
        {
            var result = await identityService.Login(loginRequestModel);

            return Ok(result);
        }
    }
}
