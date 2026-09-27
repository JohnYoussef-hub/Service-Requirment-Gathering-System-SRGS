namespace SRGS.infrastructure.Identity;

public sealed class AppUser
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public IList<string> Roles { get; init; } = [];
}