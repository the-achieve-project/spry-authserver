using Microsoft.AspNetCore.Identity;

namespace Spry.Identity.Models
{
    public class User : IdentityUser<Guid>
    {
        public required string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? OtherNames { get; set; }
    }

    public class UserRole : IdentityRole<Guid>
    {
        public required string Description { get; set; }

    }
}
