using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Spry.Identity.Models;
using Spry.Identity.Utility;
using System.Text;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class ConfirmEmailModel(UserManager<User> userManager) : PageModel
    {
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                user.AchieveId = $"AI{new Hasher(user.SequenceId).Hash()}";

                await userManager.UpdateAsync(user);
                StatusMessage = "Thank you for confirming your email.";
            }
            else
            {
                StatusMessage = "Error confirming your email.";
            }
            return Page();
        }
    }

}
