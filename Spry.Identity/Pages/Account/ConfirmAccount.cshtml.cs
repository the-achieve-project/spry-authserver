using Newtonsoft.Json;
using Spry.Identity.Models;
using Spry.Identity.Services;
using Spry.Identity.Utility;
using Spry.Identity.Workers;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class ConfirmAccountModel(IConnectionMultiplexer redis, 
        UserManager<User> userManager, IConfiguration configuration,
        ILogger<ConfirmAccountModel> logger, MessagingService messagingService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string ReturnUrl { get; set; }
        public string StatusMessage { get; set; }
        public void OnGet(Guid id, string returnUrl = null)
        {
            Input.UserId = id;

            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var record = await redis.GetDatabase().StringGetAsync($"2FA_Reg:{Input.UserId}");

                if (!record.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Code has expired");
                    return Page();
                }

                if (record.ToString() == Input.Code)
                {
                    var user = await userManager.FindByIdAsync(Input.UserId.ToString());
                    user.EmailConfirmed = true;
                    user.AchieveId = $"AI{new Hasher(user.SequenceId).Hash()}";

                    var result = await userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "confirmation failed. try again");
                        return Page();
                    }
                }

                return LocalRedirect(ReturnUrl);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResend(Guid userId, string returnUrl = null)
        {
            var code = OtpGenerator.Create();
            var user = await userManager.FindByIdAsync(Input.UserId.ToString());

            var dbResult = await redis.GetDatabase(0).StringSetAsync($"2FA_Reg:{user.Id}", code,
                              TimeSpan.FromMinutes(int.Parse(configuration["OtpExpiryTimeInMins"])));

            if (dbResult)
            {
                StatusMessage = "verification code resent.";
                logger.LogInformation("Confirm account otp: {code}", code);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    messagingService.SendOtp(user.Email, user.FirstName, code);
                }

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    messagingService.SendSMS2faNotice(user.PhoneNumber, code);
                }
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");
            Input.UserId = userId;
            return Page();
        }

        public class InputModel
        {
            [Required]
            public Guid UserId { get; set; }

            [Required]
            public string Code { get; set; }
        }
    }
}
