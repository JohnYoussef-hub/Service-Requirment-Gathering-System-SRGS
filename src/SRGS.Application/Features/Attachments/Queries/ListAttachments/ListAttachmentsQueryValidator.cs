using FluentValidation;

namespace SRGS.Application.Features.Attachments.Queries.ListAttachments;

public sealed class ListAttachmentsQueryValidator : AbstractValidator<ListAttachmentsQuery>
{
    public ListAttachmentsQueryValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0).WithMessage("Request id is required.");
    }
}
