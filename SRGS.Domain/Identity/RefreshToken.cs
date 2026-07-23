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
    public DateTimeOffset ExpiresOnUtc { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private RefreshToken()
#pragma warning restore CS8618
    { }

    private RefreshToken(Guid id, string token, int userId, DateTimeOffset expiresOnUtc)
        : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
    }

    public static Result<RefreshToken> Create(Guid id, string token, int userId, DateTimeOffset expiresOnUtc)
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

        if (expiresOnUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenErrors.ExpiryInvalid;
        }

        return new RefreshToken(id, token, userId, expiresOnUtc);
    }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresOnUtc;
}
