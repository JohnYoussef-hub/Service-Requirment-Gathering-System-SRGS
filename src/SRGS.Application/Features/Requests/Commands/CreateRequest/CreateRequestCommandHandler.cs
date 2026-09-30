using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Application.Features.Requests.Mappers;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Requests.Commands.CreateRequest;

public class CreateRequestCommandHandler(
    ILogger<CreateRequestCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    ICurrentUser currentUser)
    : IRequestHandler<CreateRequestCommand, Result<RequestDto>>
{
    private readonly ILogger<CreateRequestCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<RequestDto>> Handle(
        CreateRequestCommand command,
        CancellationToken ct)
    {
        if (_currentUser.UserId is not int requesterId)
        {
            _logger.LogError("The current user does not have a valid user id claim.");
            return ApplicationErrors.Unauthorized;
        }

        var requestTypeExists = await _context.RequestTypes.AnyAsync(x => x.Id == command.RequestTypeId, ct);
        if (!requestTypeExists)
        {
            _logger.LogWarning("Request type with id {RequestTypeId} does not exist", command.RequestTypeId);
            return ApplicationErrors.RequestTypeNotFound;
        }

        var moduleTypeExists = await _context.ModuleTypes.AnyAsync(x => x.Id == command.ImpactedModuleTypeId, ct);
        if (!moduleTypeExists)
        {
            _logger.LogWarning("Module type with id {ImpactedModuleTypeId} does not exist", command.ImpactedModuleTypeId);
            return ApplicationErrors.ModuleTypeNotFound;
        }

        var requesterExists = await _context.Users.AnyAsync(u => u.Id == requesterId, ct);

        if (!requesterExists)
        {
            _logger.LogError("User (requester) with Id '{RequesterId}' does not exist.", requesterId);

            return ApplicationErrors.UserNotFound;
        }

        var createResult = Request.Create(
            command.Title,
            command.Description,
            command.RequestTypeId,
            requesterId,
            command.ImpactedModuleTypeId,
            command.BusinessJustification,
            command.Priority,
            command.CurrentBehavior,
            command.ExpectedBehavior);

        if (createResult.IsError)
        {
            _logger.LogError("Failed to create Request: {Error}", createResult.TopError.Description);
            return createResult.Errors;
        }

        var request = createResult.Value;

        _context.Requests.Add(request);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Request with Id '{RequestId}' created successfully.", request.Id);

        await _cache.RemoveAsync("requests", ct);

        return request.ToDto();
    }
}