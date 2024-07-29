using System.ComponentModel.DataAnnotations;
using Spry.AuthServer.Models;
using Spry.AuthServer.Services;
using Spry.AuthServer.Utility;
using StackExchange.Redis;

namespace Spry.AuthServer.Pages.Account
{
#nullable disable
    public class ForgotPasswordModel(UserManager<User> userManager,
        IConnectionMultiplexer redis,
        ILogger<ForgotPasswordModel> logger, IConfiguration configuration,
        MessagingService messagingService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation", new { ReturnUrl });
                }

                var code = OtpGenerator.Create();

                var dbResult = await redis.GetDatabase(0).StringSetAsync($"2FA_FP:{user.Id}", code,
                                  TimeSpan.FromMinutes(int.Parse(configuration["OtpExpiryTimeInMins"])));

                if (!dbResult)
                {
                    logger.LogError("failed to generate verification code");
                    ModelState.AddModelError(string.Empty, "An error occured. Try again");
                    return Page();
                }

                logger.LogInformation("Confirm account otp: {code}", code);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    messagingService.SendOtp(user.Email, user.FirstName, code);
                }

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    messagingService.SendSMS2faNotice(user.PhoneNumber, code);
                }

                return RedirectToPage("./ForgotPasswordConfirmation", new { ReturnUrl, user.Id });
            }

            return Page();
        }
    }
}
