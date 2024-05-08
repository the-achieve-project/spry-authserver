using Spry.Identity.Models;
using Spry.Identity.Utility;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    [AllowAnonymous]
    public class ForgotPasswordConfirmationModel(IConnectionMultiplexer redis, UserManager<User> userManager) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; }

        [TempData]
        public string Code { get; set; }

        public class InputModel
        {
            public Guid UserId { get; set; }

            [Required]
            public string Code { get; set; }
        }

        public void OnGet(Guid id, string returnUrl = null)
        {
            Input.UserId = id;
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var record = await redis.GetDatabase().StringGetAsync($"2FA_FP:{Input.UserId}");

                if (!record.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Code has expired");
                    return Page();
                }

                if (record.ToString() == Input.Code)
                {
                    Code = record.ToString();   
                    return RedirectToPage("./ResetPassword", new { ReturnUrl, id = Input.UserId });
                }
            }

            return Page();
        }
    }
}
