namespace SRGS.Application.Features.Attachments.Dtos;

public sealed class AttachmentDownloadDto
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public byte[] Data { get; init; } = [];
}
