using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests;

namespace SRGS.Application.Features.Requests.Queries.GetRequestById;

public sealed class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, Result<RequestDto>>
{
    private readonly IAppDbContext _context;

    public GetRequestByIdQueryHandler(IAppDbContext context) => _context = context;

    // Projects straight into RequestDto with a join, instead of loading the Request entity
    // and calling ToDto() — this is how the *Name fields actually get populated, since
    // Request itself carries no navigation properties. RequestMapper.ToDto() is for
    // command handlers that already have an entity in memory and don't need names;
    // this projection is for reads.
    public async Task<Result<RequestDto>> Handle(GetRequestByIdQuery query, CancellationToken ct)
    {
#pragma warning disable CS8601 // Possible null reference assignment.

        var dto = await _context.Requests
            .Where(r => r.Id == query.RequestId)
            .Select(r => new RequestDto
            {
                RequestId = r.Id,
                RequestCode = r.RequestCode,
                CreatedDate = r.CreatedDate,
                Title = r.Title,
                Description = r.Description,
                RequestTypeId = r.RequestTypeId,
                RequestTypeName = _context.RequestTypes
                    .Where(t => t.Id == r.RequestTypeId)
                    .Select(t => t.TypeName)
                    .FirstOrDefault(),
                RequestedById = r.RequestedById,
                RequestedByName = _context.Users
                    .Where(u => u.Id == r.RequestedById)
                    .Select(u => u.FirstName + " " + u.FamilyName)
                    .FirstOrDefault(),
                ImpactedModuleTypeId = r.ImpactedModuleTypeId,
                ImpactedModuleTypeName = _context.ModuleTypes
                    .Where(m => m.Id == r.ImpactedModuleTypeId)
                    .Select(m => m.ModuleName)
                    .FirstOrDefault(),
                CurrentBehavior = r.CurrentBehavior,
                ExpectedBehavior = r.ExpectedBehavior,
                BusinessJustification = r.BusinessJustification,
                Priority = r.Priority,
                Status = r.Status,
                Phase = r.Phase,
                AssignedDeveloperId = r.AssignedDeveloperId,
                AssignedDeveloperName = r.AssignedDeveloperId == null ? null :
                    _context.Users.Where(u => u.Id == r.AssignedDeveloperId)
                        .Select(u => u.FirstName + " " + u.FamilyName)
                        .FirstOrDefault(),
                AssignedBusinessAnalystId = r.AssignedBusinessAnalystId,
                AssignedBusinessAnalystName = r.AssignedBusinessAnalystId == null ? null :
                    _context.Users.Where(u => u.Id == r.AssignedBusinessAnalystId)
                        .Select(u => u.FirstName + " " + u.FamilyName)
                        .FirstOrDefault(),
                EstimatedEffort = r.EstimatedEffort,
                ActualStart = r.ActualStart,
                ActualEnd = r.ActualEnd,
                OpenTimeUtc = r.OpenTimeUtc,
                CloseTimeUtc = r.CloseTimeUtc,
                DevelopmentProgress = r.DevelopmentProgress,
                UatResult = r.UatResult
            })
            .FirstOrDefaultAsync(ct);
#pragma warning restore CS8601 // Possible null reference assignment.


        return dto is null
            ? RequestErrors.NotFound(query.RequestId)
            : dto;
    }
}
