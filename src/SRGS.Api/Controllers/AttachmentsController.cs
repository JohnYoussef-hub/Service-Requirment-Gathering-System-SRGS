using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRGS.Api.Controllers;
using SRGS.Application.Features.Attachments.Commands.DeleteAttachment;
using SRGS.Application.Features.Attachments.Commands.UpdateAttachment;
using SRGS.Application.Features.Attachments.Commands.UploadAttachment;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Application.Features.Attachments.Queries.DownloadAttachment;
using SRGS.Application.Features.Attachments.Queries.ListAttachments;
using SRGS.Domain.Requests.Attachments;

namespace srgs.api.controllers;

[Route("api/requests/{requestId:int}/attachments")]
[ApiController]
[Authorize]
public sealed class AttachmentsController(ISender sender) : ApiController
{
    [HttpPost]
    [RequestFormLimits(MultipartBodyLengthLimit = 5_500_000)]
    [RequestSizeLimit(5_500_000)]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(int requestId, IFormFile file, CancellationToken ct)
    {
        if (file is null)
        {
            return Problem([AttachmentErrors.FileDataRequired]);
        }

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);

        var result = await sender.Send(
            new UploadAttachmentCommand(requestId, file.FileName, file.ContentType, memoryStream.ToArray()), ct);

        return result.Match<IActionResult>(
            dto => CreatedAtAction(nameof(Download), new { requestId, attachmentId = dto.AttachmentId }, dto),
            Problem);
    }

    [HttpPut("{attachmentId:int}")]
    [RequestFormLimits(MultipartBodyLengthLimit = 5_500_000)]
    [RequestSizeLimit(5_500_000)]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int requestId,
        int attachmentId,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null)
        {
            return Problem([AttachmentErrors.FileDataRequired]);
        }

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);

        var result = await sender.Send(
            new UpdateAttachmentCommand(
                requestId,
                attachmentId,
                file.FileName,
                file.ContentType,
                memoryStream.ToArray()), ct);

        return result.Match<IActionResult>(Ok, Problem);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(int requestId, CancellationToken ct)
    {
        var result = await sender.Send(new ListAttachmentsQuery(requestId), ct);
        return result.Match<IActionResult>(Ok, Problem);
    }

    [HttpGet("{attachmentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int requestId, int attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(new DownloadAttachmentQuery(requestId, attachmentId), ct);
        return result.Match<IActionResult>(
            attachment => File(attachment.Data, attachment.ContentType, attachment.FileName),
            Problem);
    }

    [HttpDelete("{attachmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int requestId, int attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteAttachmentCommand(requestId, attachmentId), ct);
        return result.Match<IActionResult>(_ => NoContent(), Problem);
    }
}
