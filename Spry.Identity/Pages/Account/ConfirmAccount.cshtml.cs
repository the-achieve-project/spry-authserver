using Newtonsoft.Json;
using Spry.Identity.Models;
using Spry.Identity.Utility;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;

namespace Spry.Identity.Pages.Account
{
#nullable disable
    public class ConfirmAccountModel(IConnectionMultiplexer redis, UserManager<User> userManager) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string ReturnUrl { get; set; }

        public void OnGet(Guid id, string returnUrl = null)
        {
            Input.UserId = id;

            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var record = await redis.GetDatabase().StringGetAsync($"2FA_Reg:{Input.UserId}");

                if (!record.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Code has expired");
                    return Page();
                }

                if (record.ToString() == Input.Code)
                {
                    var user = await userManager.FindByIdAsync(Input.UserId.ToString());
                    user.EmailConfirmed = true;
                    user.AchieveId = $"AI{new Hasher(user.SequenceId).Hash()}";

                    var result = await userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "confirmation failed. try again");
                        return Page();
                    }
                }

                return LocalRedirect(ReturnUrl);
            }

            return Page();
        }

        public class InputModel
        {
            [Required]
            public Guid UserId { get; set; }

            [Required]
            public string Code { get; set; }
        }
    }
}
