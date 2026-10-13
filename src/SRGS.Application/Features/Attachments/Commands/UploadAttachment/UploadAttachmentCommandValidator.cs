using FluentValidation;
using SRGS.Application.Features.Attachments;

namespace SRGS.Application.Features.Attachments.Commands.UploadAttachment;

public sealed class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0)
            .WithMessage("Request id is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required.");

        RuleFor(x => x.FileData)
            .NotNull()
            .WithMessage("File data is required.")
            .Must(data => data is { Length: > 0 })
            .WithMessage("The attachment cannot be empty.")
            .Must(data => data is not null && data.LongLength <= AttachmentUploadOptions.MaximumFileSizeBytes)
            .WithMessage($"File size cannot exceed {AttachmentUploadOptions.MaximumFileSizeBytes / 1024 / 1024} MB.");

        RuleFor(x => x)
            .Must(command => AttachmentUploadOptions.AllowedContentTypes.ContainsKey(
                Path.GetExtension(command.FileName)))
            .WithMessage("The file extension is not allowed.");
    }
}