using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Spry.Identity.Services;
using Spry.Identity.Models;
using Spry.Identity.Workers;
using StackExchange.Redis;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    [AllowAnonymous]
    public class ResendEmailConfirmationModel(UserManager<User> userManager,
        MessagingService messagingService, IConnectionMultiplexer redis,
        ILogger<ResendEmailConfirmationModel> logger,
        IConfiguration configuration) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification code sent via email/sms.");
                return Page();
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                var code = OtpGenerator.Create();

                var dbResult = await redis.GetDatabase(0).StringSetAsync($"2FA:{user.Id}", code,
                                  TimeSpan.FromMinutes(int.Parse(configuration["OtpExpiryTimeInMins"])));

                if (!dbResult)
                {
                    logger.LogError("failed to generate verification code");
                    ModelState.AddModelError(string.Empty, "An error occured. Try again");
                    return Page();
                }

                logger.LogInformation("Confirm account otp: {0}", code);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    var mail = new MailInfo
                    {
                        RxEmail = user.Email,
                        RxName = user.FirstName,
                        EmailTemplate = configuration["EmailTemplates:2fa"],
                        EmailTemplateLocale = configuration["EmailTemplates:2fa"],
                        Content = new
                        {
                            first_name = user.FirstName,
                            code
                        }
                    };

                    messagingService.SendMail(mail);
                }

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    messagingService.SendSMS2faNotice(user.PhoneNumber, code);
                }

                return RedirectToPage("./ConfirmAccount", new { ReturnUrl = returnUrl, user.Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "account has already been confirmed.");
            }

            return Page();
        }
    }
}
