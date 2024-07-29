using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Spry.AuthServer.Services;

namespace Spry.AuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        [HttpDelete("iddict-client")]
        public async Task<IActionResult> DeleteClientApplication(string clientId, [FromServices] IOpenIddictApplicationManager manager)
        {
            try
            {
                var app = await manager.FindByClientIdAsync(clientId);

                await manager.DeleteAsync(app!);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
