using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spry.Identity.Models;
using System.Reflection.Emit;


namespace Spry.Identity.Data
{
    public class IdentityDataContext : IdentityDbContext<User, UserRole, Guid>
    {
        public const string AIdSqlSequenceName = "AIdNumbers";
        public IdentityDataContext(DbContextOptions<IdentityDataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasSequence<long>(AIdSqlSequenceName);

            builder.ApplyConfiguration(new UserConfiguration());
            base.OnModelCreating(builder);
            //builder.UseOpenIddict();
        }
    }
}
