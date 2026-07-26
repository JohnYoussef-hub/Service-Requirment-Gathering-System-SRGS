using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SRGS.Api.Controllers;
using SRGS.Application.Features.Requests.Commands.CreateRequest;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Application.Features.Requests.Queries.GetRequestById;

namespace srgs.api.controllers;

[Route("api/requests")]
[ApiController]
public sealed class RequestsController(ISender sender) : ApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateRequestCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match<IActionResult>(
            dto => CreatedAtAction(nameof(GetById), new { id = dto.RequestId }, dto),
            Problem);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetRequestByIdQuery(id), ct);

        return result.Match<IActionResult>(Ok, Problem);
    }

}