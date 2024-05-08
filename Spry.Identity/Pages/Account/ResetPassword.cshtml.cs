using Microsoft.AspNetCore.WebUtilities;
using Spry.Identity.Models;
using Spry.Identity.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class ResetPasswordModel(UserManager<User> userManager,
        MessagingService messagingService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string ReturnUrl { get; set; }

        [TempData]
        public string Code { get; set; }

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

        public IActionResult OnGet(Guid id, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(Code))
            {
                return RedirectToPage("./Login", new { ReturnUrl });
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

            var user = await userManager.FindByIdAsync(Input.UserId.ToString());
            if (user == null)
            {
                //Don't reveal that the user does not exist
                return RedirectToPage("./Login", new { ReturnUrl });
            }

            var result = await userManager.ResetPasswordAsync(user, Input.Code, Input.Password);

            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }

}
