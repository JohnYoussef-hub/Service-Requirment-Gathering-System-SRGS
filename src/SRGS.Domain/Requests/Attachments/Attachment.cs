using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Attachments;

public sealed class Attachment : Entity<int>
{
    public int RequestId { get; private set; }
    public int UploadedByUserId { get; private set; }
    public string FileName { get; private set; }
    public string? FileExtension { get; private set; }
    public string MimeType { get; private set; }
    public long FileSize { get; private set; }
    public byte[] FileData { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }

    // Matches CHK_ATTACHMENT_FILE_SIZE (5 MiB). Kept as a named constant instead of a
    // magic number since the validator/handler will need this exact figure too — one
    // source of truth for "what's the max upload size" instead of the number appearing
    // in three places and drifting if it ever changes.
    public const long MaxFileSizeBytes = 5_242_880;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Attachment()
#pragma warning restore CS8618
    { }

    private Attachment(
        int requestId,
        int uploadedByUserId,
        string fileName,
        string? fileExtension,
        string mimeType,
        long fileSize,
        byte[] fileData)
    {
        RequestId = requestId;
        UploadedByUserId = uploadedByUserId;
        FileName = fileName;
        FileExtension = fileExtension;
        MimeType = mimeType;
        FileSize = fileSize;
        FileData = fileData;
        UploadedAtUtc = DateTimeOffset.UtcNow;
    }

    public static Result<Attachment> Create(
        int requestId,
        int uploadedByUserId,
        string fileName,
        string mimeType,
        byte[] fileData,
        string? fileExtension = null)
    {
        if (requestId <= 0)
        {
            return AttachmentErrors.RequestIdRequired;
        }

        if (uploadedByUserId <= 0)
        {
            return AttachmentErrors.UploaderRequired;
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return AttachmentErrors.FileNameRequired;
        }

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            return AttachmentErrors.MimeTypeRequired;
        }

        if (fileData is null || fileData.Length == 0)
        {
            return AttachmentErrors.FileDataRequired;
        }

        // CHK_ATTACHMENT_FILE_SIZE checks file_size > 0 AND <= 5242880 at the DB level —
        // this repeats it in the domain so a violation surfaces as a clean Result<T>
        // error before the insert, instead of a raw SqlException bubbling up from a
        // constraint the caller never saw coming.
        if (fileData.Length > MaxFileSizeBytes)
        {
            return AttachmentErrors.FileTooLarge;
        }

        return new Attachment(
            requestId,
            uploadedByUserId,
            fileName.Trim(),
            string.IsNullOrWhiteSpace(fileExtension) ? null : fileExtension.Trim(),
            mimeType.Trim(),
            fileData.Length,
            fileData);
    }
}
