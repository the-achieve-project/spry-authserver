using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata;
using Spry.Identity.Models;
using Spry.Identity.Services;
using Spry.Identity.Workers;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class RegisterModel : PageModel
    {
        #region fields
        readonly SignInManager<User> _signInManager;
        readonly UserManager<User> _userManager;
        readonly IUserStore<User> _userStore;
        readonly IUserEmailStore<User> _emailStore;
        readonly ILogger<RegisterModel> _logger;
        readonly MessagingService _messagingService;
        readonly IConfiguration _configuration;
        readonly IConnectionMultiplexer _redis;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            MessagingService messagingService,
            IConfiguration configuration,
            IConnectionMultiplexer redis)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _messagingService = messagingService;
            _configuration = configuration;
            _redis = redis;
        }
        #endregion

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last name")]
            public string LastName { get; set; }
            
            //[Required]
            [Display(Name = "Phone")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                };

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        var code = OtpGenerator.Create();

                        var dbResult = await _redis.GetDatabase(0).StringSetAsync($"2FA:{user.Id}", code,
                                          TimeSpan.FromMinutes(int.Parse(_configuration["OtpExpiryTimeInMins"])));

                        if (!dbResult)
                        {
                            _logger.LogError("failed to generate verification code");
                            ModelState.AddModelError(string.Empty, "An error occured. Try again");
                            ReturnUrl = returnUrl;
                            return Page();
                        }

                        _logger.LogInformation("Confirm account otp: {0}", code);

                        var mail = new MailInfo
                        {
                            RxEmail = user.Email,
                            RxName = user.FirstName,
                            EmailTemplate = _configuration["EmailTemplates:2fa"],
                            EmailTemplateLocale = _configuration["EmailTemplates:2fa"],
                            Content = new
                            {
                                first_name = user.FirstName,
                                code
                            }
                        };

                        _messagingService.SendMail(mail);

                        return RedirectToPage("./ConfirmAccount", new { ReturnUrl = returnUrl, user.Id });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            ReturnUrl = returnUrl;
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }

}
