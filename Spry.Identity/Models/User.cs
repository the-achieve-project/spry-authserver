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

#nullable disable
    public class UserInfo
    {
        public string Sub { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string OtherNames { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
