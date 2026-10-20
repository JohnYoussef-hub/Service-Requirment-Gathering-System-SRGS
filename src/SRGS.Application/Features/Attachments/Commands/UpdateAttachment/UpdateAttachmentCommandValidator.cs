using FluentValidation;
using SRGS.Application.Features.Attachments;

namespace SRGS.Application.Features.Attachments.Commands.UpdateAttachment;

public sealed class UpdateAttachmentCommandValidator : AbstractValidator<UpdateAttachmentCommand>
{
    public UpdateAttachmentCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0).WithMessage("Request id is required.");
        RuleFor(x => x.AttachmentId).GreaterThan(0).WithMessage("Attachment id is required.");
        RuleFor(x => x.FileName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("File name is required.")
            .Must(fileName => Path.GetFileName(fileName).Length <= AttachmentUploadOptions.MaximumFileNameLength)
            .WithMessage($"File name cannot exceed {AttachmentUploadOptions.MaximumFileNameLength} characters.");
        RuleFor(x => x.FileData)
            .NotNull().WithMessage("File data is required.")
            .Must(data => data is { Length: > 0 }).WithMessage("The attachment cannot be empty.")
            .Must(data => data is not null && data.LongLength <= AttachmentUploadOptions.MaximumFileSizeBytes)
            .WithMessage($"File size cannot exceed {AttachmentUploadOptions.MaximumFileSizeBytes / 1024 / 1024} MB.");
        RuleFor(x => x)
            .Must(command => AttachmentUploadOptions.AllowedContentTypes.ContainsKey(
                Path.GetExtension(command.FileName ?? string.Empty)))
            .WithMessage("The file extension is not allowed.");
    }
}
