using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Spry.Identity.Services;
using Spry.Identity.Models;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    [AllowAnonymous]
    public class ResendEmailConfirmationModel(UserManager<User> userManager,
        MessagingService messagingService, 
        IConfiguration configuration) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                var userId = await userManager.GetUserIdAsync(user);
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail", pageHandler: null,
                    values: new { userId = userId, code = code },
                    protocol: Request.Scheme);

                var mail = new MailInfo
                {
                    RxEmail = user.Email,
                    RxName = user.FirstName,
                    EmailTemplate = configuration["EmailTemplates:ConfirmAccount"],
                    EmailTemplateLocale = configuration["EmailTemplates:ConfirmAccount"],
                    Content = new
                    {
                        first_name = user.FirstName,
                        reset_url = callbackUrl
                    }
                };

                messagingService.SendMail(mail);

                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "account has already been confirmed.");
            }

            return Page();
        }
    }
}
