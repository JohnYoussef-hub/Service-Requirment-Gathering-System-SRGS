namespace SRGS.Application.Features.Attachments;

public static class AttachmentUploadOptions
{
    // TODO: confirm with BRD
    public const long MaximumFileSizeBytes = 5_242_880;

    // TODO: confirm with BRD
    public static readonly IReadOnlyDictionary<string, string> AllowedContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".txt"] = "text/plain",
            [".pdf"] = "application/pdf",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg"
        };

    public const int MaximumFileNameLength = 255;
}
