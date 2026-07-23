using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace SRGS.Application.Features.Requests.Commands.CreateRequest;

public sealed class CreateRequestCommandValidator : AbstractValidator<CreateRequestCommand>
{
    private readonly IAppDbContext _context;

    public CreateRequestCommandValidator(IAppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.BusinessJustification)
            .NotEmpty().WithMessage("Business justification is required.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority is invalid.");

        RuleFor(x => x.RequestTypeId)
            .GreaterThan(0).WithMessage("Request type is required.")
            .MustAsync(RequestTypeExistsAsync).WithMessage("Request type does not exist.");

        RuleFor(x => x.RequestedById)
            .GreaterThan(0).WithMessage("Requester is required.")
            .MustAsync(UserExistsAsync).WithMessage("Requester does not exist.");

        RuleFor(x => x.ImpactedModuleTypeId)
            .GreaterThan(0).WithMessage("Impacted module is required.")
            .MustAsync(ModuleTypeExistsAsync).WithMessage("Impacted module does not exist.");

        // Cross-field rule: CurrentBehavior/ExpectedBehavior only for Issue/Change request types.
        // This has to run on the whole command object, not a single property — FluentValidation's
        // When() scopes to one RuleFor chain, but "does field A's requiredness depend on field B's
        // value" needs both at once.
        RuleFor(x => x)
            .MustAsync(BehaviorFieldsMatchRequestTypeAsync)
            .WithMessage("Current behavior and expected behavior are required for Issue/Change requests, and must be left empty for every other request type.")
            .WithName("CurrentBehavior");
    }

    private async Task<bool> RequestTypeExistsAsync(int requestTypeId, CancellationToken ct)
        => await _context.RequestTypes.AnyAsync(t => t.Id == requestTypeId, ct);

    private async Task<bool> UserExistsAsync(int userId, CancellationToken ct)
        => await _context.Users.AnyAsync(u => u.Id == userId, ct);

    private async Task<bool> ModuleTypeExistsAsync(int moduleTypeId, CancellationToken ct)
        => await _context.ModuleTypes.AnyAsync(m => m.Id == moduleTypeId, ct);

    private async Task<bool> BehaviorFieldsMatchRequestTypeAsync(CreateRequestCommand command, CancellationToken ct)
    {
        var typeName = await _context.RequestTypes
            .Where(t => t.Id == command.RequestTypeId)
            .Select(t => t.TypeName)
            .FirstOrDefaultAsync(ct);

        var requiresBehaviorFields = typeName is "Issue" or "Change";

        if (requiresBehaviorFields)
        {
            return !string.IsNullOrWhiteSpace(command.CurrentBehavior)
                && !string.IsNullOrWhiteSpace(command.ExpectedBehavior);
        }

        return string.IsNullOrWhiteSpace(command.CurrentBehavior)
            && string.IsNullOrWhiteSpace(command.ExpectedBehavior);
    }
}