using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRGS.Api.Controllers;
using SRGS.Application.Features.Requests.Commands.CreateRequest;
using SRGS.Application.Features.Requests.Commands.DeleteRequest;
using SRGS.Application.Features.Requests.Commands.UpdateRequest;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Application.Features.Requests.Queries.GetAllRequests;
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

    [HttpGet]
    [ProducesResponseType(typeof(List<RequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllRequestsQuery(), ct);

        return result.Match<IActionResult>(Ok, Problem);
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

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateRequestCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command with { RequestId = id }, ct);

        return result.Match<IActionResult>(Ok, Problem);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteRequestCommand(id), ct);

        return result.Match<IActionResult>(_ => NoContent(), Problem);
    }

}