using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace Spry.Identity.Models
{
    public class User : IdentityUser<Guid>
    {
        public long SequenceId { get; set; }
        public string? AchieveId { get; set; }
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
