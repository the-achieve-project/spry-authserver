using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Spry.Identity.Models;


namespace Spry.Identity.Data
{
    public class IdentityDataContext(DbContextOptions<IdentityDataContext> options, IServiceProvider serviceProvider) : IdentityDbContext<User, UserRole, Guid>(options)
    {
        public const string AIdSqlSequenceName = "AIdNumbers";
        public DbSet<UserDeviceLogin> UserDeviceLogins { get; set; }

        //properties that will trigger token revocation
        private static readonly string[] userUpdateProperties = ["PasswordHash", "Email", "UserName", "PhoneNumber"];

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

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            //await HandleAccountChangeAsync(cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }


        #region
        //should i use this or revocation where properties are changed??
        protected async Task HandleAccountChangeAsync(CancellationToken cancellationToken = default)
        {
            if (ChangeTracker.Entries<User>().Any())
            {
                var accountEntries = ChangeTracker.Entries<User>().Where(e => e.State == EntityState.Modified
                                && e.Properties.Any(p => userUpdateProperties.Contains(p.Metadata.Name) && p.CurrentValue != p.OriginalValue));

                if (accountEntries.Any())
                {
                    foreach (var accountEntry in accountEntries)
                    {
                        using var scope = serviceProvider.CreateScope();
                        var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

                        var tokens = tokenManager.FindBySubjectAsync(accountEntry.Entity.AchieveId!, cancellationToken: cancellationToken);

                        await foreach (var token in tokens)
                        {
                            _ = await tokenManager.TryRevokeAsync(token, cancellationToken);
                        }
                    }
                }
            }           
        }
        #endregion
    }
}
