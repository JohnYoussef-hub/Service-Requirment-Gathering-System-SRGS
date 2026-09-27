using FluentValidation;

namespace SRGS.Application.Features.Identity.Queries.GenerateTokens;

public sealed class GenerateTokenQueryValidator : AbstractValidator<GenerateTokenQuery>
{
    public GenerateTokenQueryValidator()
    {
        RuleFor(request => request.Username)
            .NotNull().NotEmpty()
            .WithErrorCode("Username_Null_Or_Empty")
            .WithMessage("Username cannot be null or empty");

        RuleFor(request => request.Username)
            .MaximumLength(50)
            .WithErrorCode("Username_Too_Long")
            .WithMessage("Username must not exceed 50 characters");

        RuleFor(request => request.Password)
            .NotNull().NotEmpty()
            .WithErrorCode("Password_Null_Or_Empty")
            .WithMessage("Password cannot be null or empty.");
    }
}