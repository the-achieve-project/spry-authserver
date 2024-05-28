using Microsoft.EntityFrameworkCore;
using Spry.Identity.Data;
using Spry.Identity.Models;
using Spry.Identity.Services;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web;
using UAParser;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class LoginModel(SignInManager<User> signInManager,
            ILogger<LoginModel> logger, IConfiguration configuration,
            IdentityDataContext dbContext, MessagingService messagingService) : PageModel
    {    
        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        private static readonly string[] payrollClients = ["spry.admin", "spry.ess"];

        //ToDo: redirect to "home page" if request is not openIdConnect
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }

            var uri = new Uri($"http://localhost{WebUtility.UrlDecode(returnUrl)}");

            string acrValues = HttpUtility.ParseQueryString(uri.Query).Get("acr_values");
            string clientId = HttpUtility.ParseQueryString(uri.Query).Get("client_id");

            if (string.IsNullOrEmpty(acrValues) && (payrollClients).Contains(clientId))
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
            ReturnUrl = returnUrl ?? Url.Content("~/");

            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var user = await signInManager.UserManager.FindByNameAsync(Input.Email);

                if (user is not null)
                {
                    var hasPassword = await signInManager.UserManager.CheckPasswordAsync(user, Input.Password);

                    if (hasPassword)
                    {
                        if (!await signInManager.UserManager.IsEmailConfirmedAsync(user))
                        {
                            return RedirectToPage("./ConfirmAccount", new { ReturnUrl, user.Id });
                        }

                        SignInResult result = await signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            logger.LogInformation("User logged in.");

                            await LoginNotificationAsync(user);
                            return LocalRedirect(ReturnUrl);
                        }
                        if (result.RequiresTwoFactor)
                        {
                            return RedirectToPage("./LoginWith2fa", new { ReturnUrl, Input.RememberMe });
                        }
                        if (result.IsLockedOut)
                        {
                            logger.LogWarning("User account locked out.");
                            return RedirectToPage("./Lockout");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "invalid password.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "invalid email.");
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }


        async Task LoginNotificationAsync(User user)
        {
            if (configuration.GetValue<bool>("EnableSecurityAlerts"))
            {
                var ua = HttpContext.Request.Headers.UserAgent;
                var uaParser = Parser.GetDefault();
                ClientInfo clientInfo = uaParser.Parse(ua);

                string device;

                if (clientInfo.Device.Family == "Other" && clientInfo.OS.Family.Equals("Windows", StringComparison.CurrentCultureIgnoreCase))
                {
                    //for windows take note of the OS version because usually the Device.Family is "Other"
                    device = clientInfo.OS.ToString();
                }
                else
                {
                    device = clientInfo.Device.Family;
                }

                //dont send notice for first-time login
                if (!await dbContext.UserDeviceLogins.AnyAsync(u => u.UserId == user.Id))
                {
                    await dbContext.UserDeviceLogins.AddAsync(new UserDeviceLogin
                    {
                        UserId = user.Id,
                        Device = device,
                        Request = ua
                    });

                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    if (!await dbContext.UserDeviceLogins.AnyAsync(u => u.UserId == user.Id && u.Device == device ))
                    {
                        await dbContext.UserDeviceLogins.AddAsync(new UserDeviceLogin
                        {
                            UserId = user.Id,
                            Device = device,
                            Request = ua
                        });

                        await dbContext.SaveChangesAsync();

                        string userAgent = device.Contains("Windows", StringComparison.CurrentCultureIgnoreCase) ? device.Split(' ')[0] : device;

                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            messagingService.SendNewLoginNotice(user.Email, userAgent);
                        }

                        if (!string.IsNullOrEmpty(user.PhoneNumber))
                        {
                            messagingService.SendSMSNewLoginNotice(user.PhoneNumber, userAgent);
                        }
                    }
                }
            }
        }
    }


    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Stay signed in")]
        public bool RememberMe { get; set; } = true;
    }
}
