using Spry.AuthServer.Dtos.Account;
using Spry.AuthServer.Services;

namespace Spry.AuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        readonly AccountService _accountService;
        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _accountService.RegisterAsync(request);

            if (result.HasErrors)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPost("reset")]
        public async Task<IActionResult> CreateUser(PasswordChange request)
        {
            var result = await _accountService.ResetPasswordAsync(request);

            if (result.HasErrors)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }
    }
}
