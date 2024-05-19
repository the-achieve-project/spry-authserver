using OpenIddict.Abstractions;
using Spry.Identity.Models;
using Spry.Identity.Services;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class ResetPasswordModel(SignInManager<User> signInManager,
        ILogger<ResetPasswordModel> logger,
        MessagingService messagingService,
        IConnectionMultiplexer redis, 
        IOpenIddictTokenManager tokenManager) : PageModel
    {
        readonly IDatabase redisDb = redis.GetDatabase();

        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string ReturnUrl { get; set; }

        [TempData]
        public string Code { get; set; }

        public IActionResult OnGet(Guid id, string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            //dont allow in case page was reloaded or this page was hit directly
            //because it should always be passed from previous page
            if (string.IsNullOrEmpty(Code))
            {
                return RedirectToPage("./ForgotPassword", new { ReturnUrl });
            }

            Input.UserId = id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await signInManager.UserManager.FindByIdAsync(Input.UserId.ToString());
            if (user == null)
            {
                //Don't reveal that the user does not exist
                return RedirectToPage("./Login", new { ReturnUrl });
            }

            var resetToken = await redisDb.StringGetAsync($"fp_code_{Input.UserId}");
            await redisDb.KeyDeleteAsync($"fp_code_{Input.UserId}");

            var resetResult = await signInManager.UserManager.ResetPasswordAsync(user, resetToken.ToString(), Input.Password);

            //send password reset mail
            messagingService.SendPasswordResetSuccess(user.Email, user.FirstName);

            if (resetResult.Succeeded)
            {
                //ToDo: revoke tokens
                var tokens = tokenManager.FindBySubjectAsync(user.AchieveId);

                await foreach (var token in tokens)
                {
                    await tokenManager.TryRevokeAsync(token);
                }

                SignInResult result = await signInManager.PasswordSignInAsync(user.Email, Input.Password, true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    logger.LogInformation("User logged in.");
                    return LocalRedirect(ReturnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl, RememberMe = true });
                }
                if (result.IsLockedOut)
                {
                    logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
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

            foreach (var error in resetResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }



        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public Guid UserId { get; set; }
        }
    }
}
