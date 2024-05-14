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
        SignInManager<User> signInManager, IConfiguration configuration,
        ILogger<ConfirmAccountModel> logger, MessagingService messagingService) : PageModel
    {
        readonly UserManager<User> _userManager = signInManager.UserManager;

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
                var codeRecord = await redis.GetDatabase().StringGetAsync($"2FA_Reg:{Input.UserId}");

                if (!codeRecord.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Code has expired");
                    return Page();
                }

                if (codeRecord.ToString().Split("+")[0] == Input.Code)
                {
                    var user = await _userManager.FindByIdAsync(Input.UserId.ToString());
                    user.EmailConfirmed = true;
                    user.AchieveId = $"AI{new Hasher(user.SequenceId).Hash()}";

                    var updateResult = await _userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "confirmation failed. try again");
                        return Page();
                    }

                    var signInResult = await signInManager.PasswordSignInAsync(user.Email, codeRecord.ToString().Split("+")[1], true, lockoutOnFailure: false);

                    if (signInResult.Succeeded)
                    {
                        logger.LogInformation("User logged in.");
                        return LocalRedirect(ReturnUrl);
                    }
                    if (signInResult.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl, RememberMe = true });
                    }
                    if (signInResult.IsLockedOut)
                    {
                        logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout", new { ReturnUrl });
                    }
                    else
                    {
                        if (!await signInManager.UserManager.IsEmailConfirmedAsync(user))
                        {
                            ModelState.AddModelError(string.Empty, "account not confirmed.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        }

                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                        return Page();
                    }
                }
            }

            return Page();
        }

        //ToDo: use JS to contact handler
        public async Task<IActionResult> OnPostResend(Guid userId, string returnUrl = null)
        {
            var code = OtpGenerator.Create();
            var user = await _userManager.FindByIdAsync(Input.UserId.ToString());

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