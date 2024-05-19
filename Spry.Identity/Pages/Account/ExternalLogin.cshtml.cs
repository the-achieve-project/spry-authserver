using Spry.Identity.Models;
using Spry.Identity.Utility;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    [AllowAnonymous]
    public class ExternalLoginModel(
        SignInManager<User> signInManager,
        ILogger<ExternalLoginModel> logger) : PageModel
    {
        readonly UserManager<User> userManager = signInManager.UserManager;

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl });
            }

            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl });
            }

            //LogUserInfo(info);

            // Sign in the user with this external login provider if the user already has a login.
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl });
            }

            if (ModelState.IsValid)
            {
                //ToDo: what if the email has a local account already
                var user = new User
                {
                    FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                    LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                    UserName = Input.Email,
                    Email = Input.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    user.AchieveId = $"AI{new Hasher(user.SequenceId).Hash()}";

                    result = await userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(ReturnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            return Page();
        }

        private void LogUserInfo(ExternalLoginInfo info)
        {
            logger.LogInformation($"User info sub: {info.Principal.FindFirstValue(ClaimTypes.NameIdentifier)}");
            logger.LogInformation($"User info name: {info.Principal.FindFirstValue(ClaimTypes.Name)}");
            logger.LogInformation($"User info givenname: {info.Principal.FindFirstValue(ClaimTypes.GivenName)}");
            logger.LogInformation($"User info surname: {info.Principal.FindFirstValue(ClaimTypes.Surname)}");
        }
    }
}
