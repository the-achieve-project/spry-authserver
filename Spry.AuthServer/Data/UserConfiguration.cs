using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Spry.AuthServer.Models;

namespace Spry.AuthServer.Data
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.Property(s => s.SequenceId)
                  .HasDefaultValueSql($"nextval('\"{IdentityDataContext.AIdSqlSequenceName}\"')");
        }
    }
}
