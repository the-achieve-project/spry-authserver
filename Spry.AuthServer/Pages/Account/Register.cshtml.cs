using PhoneNumbers;
using Spry.AuthServer.Application.Attributes;
using Spry.AuthServer.Dtos.Common;
using Spry.AuthServer.Models;
using Spry.AuthServer.SeedWork;
using Spry.AuthServer.Services;
using Spry.AuthServer.Utility;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Spry.AuthServer.Pages.Account
{
#nullable disable
    public class RegisterModel : PageModel
    {
        #region fields
        readonly IDatabase _redisDb;

        readonly SignInManager<User> _signInManager;
        readonly UserManager<User> _userManager;
        readonly IUserStore<User> _userStore;
        readonly IUserEmailStore<User> _emailStore;
        readonly ILogger<RegisterModel> _logger;
        readonly MessagingService _messagingService;
        readonly IConfiguration _configuration;

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
            _redisDb = redis.GetDatabase();
        }
        #endregion

        public CountryDto country = new()
        {
            Code = "Gh",
            Flag = "https://upload.wikimedia.org/wikipedia/commons/1/19/Flag_of_Ghana.svg",
            Country = "Ghana",
        };

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
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

                        var regCodeIsSaved = await _redisDb.StringSetAsync($"2FA_Reg:{user.Id}", $"{code}+{Input.Password}",
                                          TimeSpan.FromMinutes(int.Parse(_configuration["OtpExpiryTimeInMins"])));

                        if (!regCodeIsSaved)
                        {
                            _logger.LogError("failed to generate verification code");
                            ModelState.AddModelError(string.Empty, "An error occured. Try again");
                            return Page();
                        }

                        _logger.LogInformation("Confirm account otp: {code}", code);

                        _messagingService.SendOtp(user.Email, user.FirstName, code);

                        if (!string.IsNullOrEmpty(user.PhoneNumber))
                        {
                            _messagingService.SendSMS2faNotice(user.PhoneNumber, code);
                        }

                        return RedirectToPage("./ConfirmAccount", new { ReturnUrl = returnUrl, user.Id });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(ReturnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [MaxLength(255)]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [RegularExpression("^\\+?[0-9][0-9]{7,14}$", ErrorMessage = "Invalid number")]
            [ValidatePhone]
            [Display(Name = "Phone")]
            public string PhoneNumber { get; set; }

            [Required]
            [RegularExpression("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "At least one uppercase letter, one lowercase letter, one digit, one special character and a minimum of 8 characters is required"), MaxLength(256)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }


            public static string GetNormalizedPhoneNumber(string phoneNumber, string countryCode)
            {
                try
                {
                    var util = PhoneNumberUtil.GetInstance();
                    return util.Format(util.Parse(phoneNumber, countryCode), PhoneNumberFormat.E164);
                }
                catch (Exception)
                {
                    throw new Exception("Invalid phone number");
                }
            }
        }
    }
}
