using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SRGS.Domain.Users;

namespace SRGS.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("USER");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("user_id").ValueGeneratedOnAdd();

        builder.Property(u => u.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
        builder.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
        builder.Property(u => u.MiddleName).HasColumnName("middle_name").HasMaxLength(50);
        builder.Property(u => u.FamilyName).HasColumnName("family_name").HasMaxLength(50).IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
        builder.Property(u => u.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
        builder.Property(u => u.Department).HasColumnName("department").HasMaxLength(100);
        builder.Property(u => u.SubDepartment).HasColumnName("sub_department").HasMaxLength(100);
        builder.Property(u => u.Title).HasColumnName("title").HasMaxLength(100);
        builder.Property(u => u.EmployeeNumber).HasColumnName("employee_number").HasMaxLength(20).IsRequired();

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.EmployeeNumber).IsUnique();

        // RoleIds is a private HashSet<int> backing field, exposed as IReadOnlyCollection —
        // EF needs the backing field name to map it as an owned collection of scalars.
        // This maps ONLY the in-memory RoleIds set for domain reads; USER_ROLE itself is
        // written/read through UserRoleRow (see UserRoleRowConfiguration), not through this.
        builder.Ignore(u => u.RoleIds);
    }
}
