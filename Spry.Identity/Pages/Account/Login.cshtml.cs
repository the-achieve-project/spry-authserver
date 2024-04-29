using Microsoft.AspNetCore;
using OpenIddict.Abstractions;
using Spry.Identity.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Encodings.Web;
using System.Web;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class LoginModel(SignInManager<User> signInManager, 
        ILogger<LoginModel> logger, IConfiguration configuration) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        private static readonly string[] payrollClients = ["spry.admin", "spry.ess"];

        //ToDo: if request doesnt contain tenant in acr_values redirect to user-organization page selection
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            var uri = new Uri($"http://localhost{WebUtility.UrlDecode(returnUrl)}");

            string acrValues = HttpUtility.ParseQueryString(uri.Query).Get("acr_values");
            string clientId = HttpUtility.ParseQueryString(uri.Query).Get("client_id");

            if (string.IsNullOrEmpty(acrValues) && !(payrollClients).Contains(clientId))
            {
                return Redirect(configuration["Idsrv4Urls:AccountSelection"]);
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true

                SignInResult result = await signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    var user = await signInManager.UserManager.FindByEmailAsync(Input.Email);

                    if (user != null)
                    {
                        if (!await signInManager.UserManager.IsEmailConfirmedAsync(user))
                        {
                            ModelState.AddModelError(string.Empty, "email not confirmed.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }


    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
