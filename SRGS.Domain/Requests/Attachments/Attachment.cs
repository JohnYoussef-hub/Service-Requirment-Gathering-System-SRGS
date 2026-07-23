using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Attachments;

public sealed class Attachment : Entity<int>
{
    public int RequestId { get; private set; }
    public int UploadedById { get; private set; }
    public string? FileType { get; private set; }
    public string FilePath { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Attachment()
#pragma warning restore CS8618
    { }

    private Attachment(int requestId, int uploadedById, string filePath, string? fileType)
    {
        RequestId = requestId;
        UploadedById = uploadedById;
        FilePath = filePath;
        FileType = fileType;
        UploadedAtUtc = DateTimeOffset.UtcNow;
    }

    public static Result<Attachment> Create(int requestId, int uploadedById, string filePath, string? fileType = null)
    {
        if (requestId <= 0)
        {
            return AttachmentErrors.RequestIdRequired;
        }

        if (uploadedById <= 0)
        {
            return AttachmentErrors.UploaderRequired;
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return AttachmentErrors.FilePathRequired;
        }

        return new Attachment(requestId, uploadedById, filePath.Trim(), string.IsNullOrWhiteSpace(fileType) ? null : fileType.Trim());
    }
}
