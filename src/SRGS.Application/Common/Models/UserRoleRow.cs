namespace SRGS.Application.Common.Models;

// Maps USER_ROLE. Not a domain concept — User.RoleIds (HashSet<int>) is the domain's view
// of role membership; this is purely the persistence shape EF needs to materialize that
// many-to-many without giving User a navigation property to Role. Lives in Application
// (not Infrastructure) because IAppDbContext needs to reference it, and Application must
// never depend on Infrastructure.
public sealed class UserRoleRow
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
