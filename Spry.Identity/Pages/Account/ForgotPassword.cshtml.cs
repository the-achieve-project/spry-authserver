using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Spry.Identity.Models;
using Spry.Identity.Services;
using Spry.Identity.Infrastructure.IntegrationEvents;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class ForgotPasswordModel(UserManager<User> userManager, 
        ILogger<ForgotPasswordModel> logger, IConfiguration configuration,
        MessagingService messagingService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page("/Account/ResetPassword", pageHandler: null, values: new { code }, protocol: Request.Scheme);

                logger.LogInformation("Reset link {callbackUrl}", HtmlEncoder.Default.Encode(callbackUrl!));

                var mail = new MailInfo
                {
                    RxEmail = user.Email,
                    RxName = user.FirstName,
                    EmailTemplate = configuration["EmailTemplates:ResetPassword"],
                    EmailTemplateLocale = configuration["EmailTemplates:ResetPassword"],
                    Content = new
                    {
                        first_name = user.FirstName,
                        reset_url = callbackUrl
                    }
                };

                messagingService.SendMail(mail);

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var message = $"Hello {user.FirstName}, \nClick the link to reset your password" +
                    $"\n\n{callbackUrl}";

                    messagingService.SendSms(new Sms_Task { Text = message, To = user.UserName });
                }


                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
