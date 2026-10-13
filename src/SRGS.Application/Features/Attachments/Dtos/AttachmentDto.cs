namespace SRGS.Application.Features.Attachments.Dtos;

public sealed class AttachmentDto
{
    public int Id => AttachmentId;
    public string Name => FileName;
    public string Path => FileName;

    public int AttachmentId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public int UploadedById { get; init; }
    public string UploadedByName { get; init; } = string.Empty;
    public DateTimeOffset UploadedAt { get; init; }
}
