using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages
{
    public class IndexModel(ILogger<IndexModel> logger) : PageModel
    {
        public IActionResult OnGet()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToPage("Account/Login");
            }

            return Page();
        }
    }
}
