namespace MechanicShop.Application.Common.Interfaces;

public interface ICurrentUser
{
    int? UserId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }
}