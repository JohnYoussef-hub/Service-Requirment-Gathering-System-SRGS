using FluentValidation;

namespace SRGS.Application.Features.Requests.Commands.UpdateRequest;

public sealed class UpdateRequestCommandValidator : AbstractValidator<UpdateRequestCommand>
{
    public UpdateRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0).WithMessage("Request id is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.BusinessJustification)
            .NotEmpty().WithMessage("Business justification is required.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority is invalid.");
    }
}
