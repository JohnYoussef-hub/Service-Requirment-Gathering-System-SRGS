using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Attachments;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Attachments;

namespace SRGS.Application.Features.Attachments.Commands.UploadAttachment;

public sealed class UploadAttachmentCommandHandler(
    ILogger<UploadAttachmentCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    ICurrentUser currentUser)
    : IRequestHandler<UploadAttachmentCommand, Result<AttachmentDto>>
{
    public async Task<Result<AttachmentDto>> Handle(UploadAttachmentCommand command, CancellationToken ct)
    {
        if (currentUser.UserId is not int uploaderId)
        {
            logger.LogError("The current user does not have a valid user id claim.");
            return ApplicationErrors.Unauthorized;
        }

        if (!await context.Requests.AnyAsync(r => r.Id == command.RequestId, ct))
        {
            return RequestErrors.NotFound(command.RequestId);
        }

        var fileName = SanitizeFileName(command.FileName);
        var extension = Path.GetExtension(fileName);
        if (!AttachmentUploadOptions.AllowedContentTypes.TryGetValue(extension, out var contentType))
        {
            return AttachmentErrors.ExtensionNotAllowed;
        }

        if (!HasExpectedSignature(extension, command.FileData))
        {
            return AttachmentErrors.ExtensionNotAllowed;
        }

        var createResult = Attachment.Create(
            command.RequestId,
            uploaderId,
            fileName,
            contentType,
            command.FileData,
            extension);

        if (createResult.IsError)
        {
            logger.LogWarning("Failed to create attachment: {Error}", createResult.TopError.Description);
            return createResult.Errors;
        }

        var attachment = createResult.Value;
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync(ct);
        await cache.RemoveAsync("requests", ct);

        var dto = await context.Attachments
            .Where(a => a.Id == attachment.Id)
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

        return dto;
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName).Trim();
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(name
            .Where(character => !invalidCharacters.Contains(character) && !char.IsControl(character))
            .ToArray());

        if (sanitized.Length <= AttachmentUploadOptions.MaximumFileNameLength)
        {
            return sanitized;
        }

        var extension = Path.GetExtension(sanitized);
        var baseLength = Math.Max(1, AttachmentUploadOptions.MaximumFileNameLength - extension.Length);
        return sanitized[..baseLength] + extension;
    }

    private static bool HasExpectedSignature(string extension, byte[] data)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => data.All(byteValue => byteValue is >= 9 and <= 13 or >= 32),
            ".pdf" => StartsWith(data, [0x25, 0x50, 0x44, 0x46]),
            ".png" => StartsWith(data, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
            ".jpg" or ".jpeg" => StartsWith(data, [0xFF, 0xD8, 0xFF]),
            _ => false
        };
    }

    private static bool StartsWith(byte[] data, byte[] signature)
        => data.Length >= signature.Length && data.AsSpan(0, signature.Length).SequenceEqual(signature);
}