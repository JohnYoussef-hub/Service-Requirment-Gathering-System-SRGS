using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Identity;

/// <summary>
/// Maps to REFRESH_TOKEN. Unlike every other entity in this model, Id stays a GUID
/// (not an INT identity) — see the schema comment: it's handed to the client and used to
/// look up/revoke one specific token, so it must not be sequential/guessable. Also the
/// only entity in the schema that carries real audit columns, so it's the one entity
/// that actually extends AuditableEntity<TId> rather than plain Entity<TId>.
/// </summary>
public sealed class RefreshToken : AuditableEntity<Guid>
{
    public string Token { get; private set; }
    public int UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public DateTimeOffset ExpiresOnUtc { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private RefreshToken()
#pragma warning restore CS8618
    { }

    private RefreshToken(Guid id, string token, int userId, DateTimeOffset expiresOnUtc, Guid familyId)
        : base(id)
    {
        Token = token;
        UserId = userId;
        FamilyId = familyId;
        ExpiresOnUtc = expiresOnUtc;
    }

    public static Result<RefreshToken> CreateNewFamily(Guid id, string token, int userId, DateTimeOffset expiresOnUtc)
        => Create(id, token, userId, expiresOnUtc, Guid.NewGuid());

    public static Result<RefreshToken> CreateNext(
        Guid id,
        string token,
        int userId,
        DateTimeOffset expiresOnUtc,
        Guid familyId)
        => Create(id, token, userId, expiresOnUtc, familyId);

    private static Result<RefreshToken> Create(
        Guid id,
        string token,
        int userId,
        DateTimeOffset expiresOnUtc,
        Guid familyId)
    {
        if (id == Guid.Empty)
        {
            return RefreshTokenErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return RefreshTokenErrors.TokenRequired;
        }

        if (userId <= 0)
        {
            return RefreshTokenErrors.UserIdRequired;
        }

        if (familyId == Guid.Empty)
        {
            return RefreshTokenErrors.FamilyIdRequired;
        }

        if (expiresOnUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenErrors.ExpiryInvalid;
        }

        return new RefreshToken(id, token, userId, expiresOnUtc, familyId);
    }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresOnUtc;

    public Result<Updated> MarkAsUsed()
    {
        if (IsRevoked)
        {
            return RefreshTokenErrors.AlreadyRevoked;
        }

        if (IsUsed)
        {
            return RefreshTokenErrors.AlreadyUsed;
        }

        IsUsed = true;
        return Result.Updated;
    }

    public Result<Updated> Revoke()
    {
        IsRevoked = true;
        return Result.Updated;
    }
}
