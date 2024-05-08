using Spry.Identity.Models;
using Spry.Identity.Utility;
using Spry.Identity.Workers;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    [AllowAnonymous]
    public class ForgotPasswordConfirmationModel(
        IConnectionMultiplexer redis,
        UserManager<User> userManager,
        ILogger<ForgotPasswordConfirmationModel> logger,
        IConfiguration configuration) : PageModel
    {
        readonly IDatabase redisDb = redis.GetDatabase();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; }

        [TempData]
        public string Code { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

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

        public async Task<IActionResult> OnPostResend(Guid userId, string returnUrl = null)
        {
            var code = OtpGenerator.Create();
            var user = await userManager.FindByIdAsync(Input.UserId.ToString());

            var dbResult = await redis.GetDatabase(0).StringSetAsync($"2FA_FP:{user.Id}", code,
                              TimeSpan.FromMinutes(int.Parse(configuration["OtpExpiryTimeInMins"])));

            if (dbResult)
            {
                StatusMessage = "verification code resent.";
                logger.LogInformation("Confirm account otp: {0}", code);
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");
            Input.UserId = userId;
            return Page();
        }
    }
}
