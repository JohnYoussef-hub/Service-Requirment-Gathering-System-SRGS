using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Attachments;

public static class AttachmentErrors
{
    public static Error RequestIdRequired =>
        Error.Validation("Attachment.RequestId.Required", "Request id is required.");

    public static Error UploaderRequired =>
        Error.Validation("Attachment.UploadedByUserId.Required", "Uploader id is required.");

    public static Error FileNameRequired =>
        Error.Validation("Attachment.FileName.Required", "File name is required.");

    public static Error MimeTypeRequired =>
        Error.Validation("Attachment.MimeType.Required", "Mime type is required.");

    public static Error FileDataRequired =>
        Error.Validation("Attachment.FileData.Required", "File data is required.");

    public static Error FileTooLarge =>
        Error.Validation("Attachment.FileData.TooLarge", $"File size cannot exceed {Attachment.MaxFileSizeBytes / 1024 / 1024} MB.");

    public static Error EmptyFile =>
        Error.Validation("Attachment.FileData.Empty", "The attachment cannot be empty.");

    public static Error ExtensionNotAllowed =>
        Error.Validation("Attachment.FileExtension.NotAllowed", "The file extension is not allowed.");

    public static Error NotFound(int id) => Error.NotFound(
        "Attachment.NotFound",
        $"Attachment '{id}' was not found.");

    public static Error DeleteForbidden =>
        Error.Forbidden("Attachment.Delete.Forbidden", "Only the uploader can delete this attachment.");
}
