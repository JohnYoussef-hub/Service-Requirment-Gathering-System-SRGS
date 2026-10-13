using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Identity;

public static class RefreshTokenErrors
{
    public static readonly Error IdRequired =
        Error.Validation("RefreshToken.Id.Required", "Refresh token id is required.");

    public static readonly Error TokenRequired =
        Error.Validation("RefreshToken.Token.Required", "Token value is required.");

    public static readonly Error UserIdRequired =
        Error.Validation("RefreshToken.UserId.Required", "User id is required.");

    public static readonly Error FamilyIdRequired =
        Error.Validation("RefreshToken.FamilyId.Required", "Refresh token family id is required.");

    public static readonly Error ExpiryInvalid =
        Error.Validation("RefreshToken.Expiry.Invalid", "Expiry must be in the future.");

    public static readonly Error AlreadyUsed =
        Error.Validation("RefreshToken.AlreadyUsed", "Refresh token has already been used.");

    public static readonly Error AlreadyRevoked =
        Error.Validation("RefreshToken.AlreadyRevoked", "Refresh token has been revoked.");
}
