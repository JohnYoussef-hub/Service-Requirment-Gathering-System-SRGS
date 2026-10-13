using FluentValidation;

namespace SRGS.Application.Features.Attachments.Commands.DeleteAttachment;

public sealed class DeleteAttachmentCommandValidator : AbstractValidator<DeleteAttachmentCommand>
{
    public DeleteAttachmentCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0).WithMessage("Request id is required.");
        RuleFor(x => x.AttachmentId).GreaterThan(0).WithMessage("Attachment id is required.");
    }
}
