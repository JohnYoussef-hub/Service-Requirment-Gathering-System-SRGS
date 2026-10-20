using Microsoft.EntityFrameworkCore;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Attachments;

namespace SRGS.Application.Features.Attachments;

internal sealed record AttachmentFile(string FileName, string Extension, string ContentType)
{
    public static Result<AttachmentFile> Create(string fileName, byte[] fileData)
    {
        var sanitizedName = Path.GetFileName(fileName).Trim();
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(sanitizedName
            .Where(character => !invalidCharacters.Contains(character) && !char.IsControl(character))
            .ToArray());

        if (sanitized.Length > AttachmentUploadOptions.MaximumFileNameLength)
        {
            var extension = Path.GetExtension(sanitized);
            var baseLength = Math.Max(1, AttachmentUploadOptions.MaximumFileNameLength - extension.Length);
            sanitized = sanitized[..baseLength] + extension;
        }

        var fileExtension = Path.GetExtension(sanitized);
        if (!AttachmentUploadOptions.AllowedContentTypes.TryGetValue(fileExtension, out var contentType))
        {
            return AttachmentErrors.ExtensionNotAllowed;
        }

        if (!HasExpectedSignature(fileExtension, fileData))
        {
            return AttachmentErrors.ExtensionNotAllowed;
        }

        return new AttachmentFile(sanitized, fileExtension, contentType);
    }

    public static async Task<AttachmentDto> ToDtoAsync(
        IAppDbContext context,
        int attachmentId,
        CancellationToken ct)
        => await context.Attachments
            .Where(a => a.Id == attachmentId)
            .Select(a => new AttachmentDto
            {
                AttachmentId = a.Id,
                FileName = a.FileName,
                ContentType = a.MimeType,
                Size = a.FileSize,
                UploadedById = a.UploadedByUserId,
                UploadedByName = context.Users
                    .Where(u => u.Id == a.UploadedByUserId)
                    .Select(u => u.FirstName + " " + u.FamilyName)
                    .First(),
                UploadedAt = a.UploadedAtUtc
            })
            .SingleAsync(ct);

    private static bool HasExpectedSignature(string extension, byte[] data)
        => extension.ToLowerInvariant() switch
        {
            ".txt" => data.All(byteValue => byteValue is >= 9 and <= 13 or >= 32),
            ".pdf" => StartsWith(data, [0x25, 0x50, 0x44, 0x46]),
            ".png" => StartsWith(data, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
            ".jpg" or ".jpeg" => StartsWith(data, [0xFF, 0xD8, 0xFF]),
            _ => false
        };

    private static bool StartsWith(byte[] data, byte[] signature)
        => data.Length >= signature.Length && data.AsSpan(0, signature.Length).SequenceEqual(signature);
}
