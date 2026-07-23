using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Attachments;

public static class AttachmentErrors
{
    public static Error RequestIdRequired =>
        Error.Validation("Attachment.RequestId.Required", "Request id is required.");

    public static Error UploaderRequired =>
        Error.Validation("Attachment.UploadedByUserId.Required", "Uploader id is required.");

    public static Error FilePathRequired =>
        Error.Validation("Attachment.FilePath.Required", "File path is required.");
}
