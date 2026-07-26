using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Application.Common.Models;
using SRGS.Domain.Roles;
using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class UserRoleRowConfiguration : IEntityTypeConfiguration<UserRoleRow>
{
    public void Configure(EntityTypeBuilder<UserRoleRow> builder)
    {
        builder.ToTable("USER_ROLE");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.Property(ur => ur.UserId).HasColumnName("user_id");
        builder.Property(ur => ur.RoleId).HasColumnName("role_id");

        builder.HasOne<User>().WithMany().HasForeignKey(ur => ur.UserId);
        builder.HasOne<Role>().WithMany().HasForeignKey(ur => ur.RoleId);

        // NOTE: User.RoleIds does not auto-populate from this table on load — it's
        // Ignore()'d in UserConfiguration. Whoever needs a User's roles loaded queries
        // UserRoleRows separately and calls user.AssignRole(id) per row (or writes a
        // small "LoadRolesAsync" helper on the repository) — same manual-sync pattern
        // we settled on when USER_ROLE first came up, kept deliberately simple rather
        // than fighting EF into populating a private HashSet<int> automatically.
    }
}
