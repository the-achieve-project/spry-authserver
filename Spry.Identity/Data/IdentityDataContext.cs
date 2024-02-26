using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spry.Identity.Models;


namespace Spry.Identity.Data
{
    public class IdentityDataContext : IdentityDbContext<User, UserRole, Guid>
    {
        public IdentityDataContext(DbContextOptions<IdentityDataContext> options) : base(options) { }

    }
}
