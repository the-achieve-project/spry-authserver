using Spry.Identity.Models;
using Spry.Identity.Utility;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    [AllowAnonymous]
    public class ForgotPasswordConfirmationModel(
        IConnectionMultiplexer redis,
        UserManager<User> userManager) : PageModel
    {
        readonly IDatabase redisDb = redis.GetDatabase();

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
                var record = await redisDb.StringGetAsync($"2FA_FP:{Input.UserId}");
                await redisDb.KeyDeleteAsync($"2FA_FP:{Input.UserId}");

                if (!record.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Code has expired");
                    return Page();
                }

                if (record.ToString() == Input.Code)
                {
                    Code = record.ToString();

                    var user = await userManager.FindByIdAsync(Input.UserId.ToString());
                    var code = await userManager.GeneratePasswordResetTokenAsync(user);

                    var codeIsSaved = await redis.GetDatabase().StringSetAsync($"fp_code_{Input.UserId}", code, TimeSpan.FromMinutes(5));

                    if (!codeIsSaved)
                    {
                        return RedirectToPage("./ForgotPassword", new { ReturnUrl });
                    }

                    return RedirectToPage("./ResetPassword", new { ReturnUrl, id = Input.UserId });
                }
            }

            return Page();
        }
    }
}
