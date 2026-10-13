using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRGS.Api.Controllers;
using SRGS.Application.Features.Attachments.Commands.DeleteAttachment;
using SRGS.Application.Features.Attachments.Commands.UploadAttachment;
using SRGS.Application.Features.Attachments.Dtos;
using SRGS.Application.Features.Attachments.Queries.DownloadAttachment;
using SRGS.Application.Features.Attachments.Queries.ListAttachments;
using SRGS.Application.Features.Requests.Commands.CreateRequest;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Application.Features.Requests.Queries.GetRequestById;

namespace srgs.api.controllers;

[Route("api/requests")]
[ApiController]
[Authorize]
public sealed class RequestsController(ISender sender) : ApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(CreateRequestCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match<IActionResult>(
            dto => CreatedAtAction(nameof(GetById), new { id = dto.RequestId }, dto),
            Problem);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetRequestByIdQuery(id), ct);

        return result.Match<IActionResult>(Ok, Problem);
    }

    [HttpPost("{requestId:int}/attachments")]
    [RequestFormLimits(MultipartBodyLengthLimit = 5_500_000)]
    [RequestSizeLimit(5_500_000)]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadAttachment(int requestId, IFormFile file, CancellationToken ct)
    {
        if (file is null)
        {
            return Problem([SRGS.Domain.Requests.Attachments.AttachmentErrors.FileDataRequired]);
        }

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);

        var result = await sender.Send(
            new UploadAttachmentCommand(requestId, file.FileName, file.ContentType, memoryStream.ToArray()), ct);

        return result.Match<IActionResult>(
            dto => CreatedAtAction(nameof(DownloadAttachment), new { requestId, attachmentId = dto.AttachmentId }, dto),
            Problem);
    }

    [HttpGet("{requestId:int}/attachments")]
    [ProducesResponseType(typeof(List<AttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListAttachments(int requestId, CancellationToken ct)
    {
        var result = await sender.Send(new ListAttachmentsQuery(requestId), ct);
        return result.Match<IActionResult>(Ok, Problem);
    }

    [HttpGet("{requestId:int}/attachments/{attachmentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAttachment(int requestId, int attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(new DownloadAttachmentQuery(requestId, attachmentId), ct);
        return result.Match<IActionResult>(
            attachment => File(attachment.Data, attachment.ContentType, attachment.FileName),
            Problem);
    }

    [HttpDelete("{requestId:int}/attachments/{attachmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachment(int requestId, int attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteAttachmentCommand(requestId, attachmentId), ct);
        return result.Match<IActionResult>(_ => NoContent(), Problem);
    }

}