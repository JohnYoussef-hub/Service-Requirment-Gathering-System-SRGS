using MediatR;
using Microsoft.EntityFrameworkCore;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Requests.Queries.GetAllRequests;

public sealed class GetAllRequestsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAllRequestsQuery, Result<List<RequestDto>>>
{
    public async Task<Result<List<RequestDto>>> Handle(GetAllRequestsQuery query, CancellationToken ct)
    {
        var requests = await context.Requests
            .AsNoTracking()
            .OrderByDescending(r => r.OpenTimeUtc)
            .Select(r => new RequestDto
            {
                RequestId = r.Id,
                RequestCode = r.RequestCode,
                CreatedDate = r.CreatedDate,
                Title = r.Title,
                Description = r.Description,
                RequestTypeId = r.RequestTypeId,
                RequestTypeName = context.RequestTypes
                    .Where(t => t.Id == r.RequestTypeId)
                    .Select(t => t.TypeName)
                    .FirstOrDefault()!,
                RequestedById = r.RequestedById,
                RequestedByName = context.Users
                    .Where(u => u.Id == r.RequestedById)
                    .Select(u => u.FirstName + " " + u.FamilyName)
                    .FirstOrDefault()!,
                ImpactedModuleTypeId = r.ImpactedModuleTypeId,
                ImpactedModuleTypeName = context.ModuleTypes
                    .Where(m => m.Id == r.ImpactedModuleTypeId)
                    .Select(m => m.ModuleName)
                    .FirstOrDefault()!,
                CurrentBehavior = r.CurrentBehavior,
                ExpectedBehavior = r.ExpectedBehavior,
                BusinessJustification = r.BusinessJustification,
                Priority = r.Priority,
                Status = r.Status,
                Phase = r.Phase,
                AssignedDeveloperId = r.AssignedDeveloperId,
                AssignedDeveloperName = r.AssignedDeveloperId == null ? null :
                    context.Users.Where(u => u.Id == r.AssignedDeveloperId)
                        .Select(u => u.FirstName + " " + u.FamilyName)
                        .FirstOrDefault(),
                AssignedBusinessAnalystId = r.AssignedBusinessAnalystId,
                AssignedBusinessAnalystName = r.AssignedBusinessAnalystId == null ? null :
                    context.Users.Where(u => u.Id == r.AssignedBusinessAnalystId)
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
            .ToListAsync(ct);

        return requests;
    }
}
