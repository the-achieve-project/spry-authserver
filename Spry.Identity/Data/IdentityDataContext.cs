using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spry.Identity.Models;
using System.Reflection.Emit;


namespace Spry.Identity.Data
{
    public class IdentityDataContext(DbContextOptions<IdentityDataContext> options) : IdentityDbContext<User, UserRole, Guid>(options)
    {
        public const string AIdSqlSequenceName = "AIdNumbers";
        public DbSet<UserDeviceLogin> UserDeviceLogins { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                builder.HasSequence<long>(AIdSqlSequenceName);
            }

            builder.ApplyConfiguration(new UserConfiguration());
            base.OnModelCreating(builder);
            //builder.UseOpenIddict();
        }
    }
}
