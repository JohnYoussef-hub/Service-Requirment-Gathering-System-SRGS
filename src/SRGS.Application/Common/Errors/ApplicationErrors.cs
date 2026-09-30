using SRGS.Domain.Common.Results;

namespace SRGS.Application.Common.Errors;

// Cross-entity/lookup errors that don't belong to any single aggregate's own *Errors class
// (e.g. Domain.Requests.RequestErrors) — same split MechanicShop uses.
public static class ApplicationErrors
{
    public static Error InvalidCredentials =>
        Error.Unauthorized("Auth.InvalidCredentials", "The username or password is invalid.");

    public static readonly Error Unauthorized = Error.Unauthorized(
        "Auth.Unauthorized",
        "Authentication is required.");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "Auth.InvalidRefreshToken",
        "The refresh token is invalid.");

    public static Error UserNotFound =>
        Error.NotFound("ApplicationErrors.User.NotFound", "User does not exist.");

    public static Error DefaultRoleNotFound =>
        Error.NotFound("ApplicationErrors.DefaultRole.NotFound", "The default role does not exist.");

    public static Error RequestTypeNotFound =>
        Error.NotFound("ApplicationErrors.RequestType.NotFound", "Request type does not exist.");

    public static Error ModuleTypeNotFound =>
        Error.NotFound("ApplicationErrors.ModuleType.NotFound", "Module type does not exist.");

    public static readonly Error ExpiredAccessTokenInvalid = Error.Conflict(
         code: "Auth.ExpiredAccessToken.Invalid",
         description: "Expired access token is not valid.");

    public static readonly Error UserIdClaimInvalid = Error.Conflict(
        code: "Auth.UserIdClaim.Invalid",
        description: "Invalid userId claim.");

    public static readonly Error RefreshTokenExpired = Error.Conflict(
        code: "Auth.RefreshToken.Expired",
        description: "Refresh token is invalid or has expired.");

    public static readonly Error TokenGenerationFailed = Error.Failure(
        code: "Auth.TokenGeneration.Failed",
        description: "Failed to generate new JWT token.");
}
