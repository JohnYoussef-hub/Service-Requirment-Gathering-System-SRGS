using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Enums;

namespace SRGS.Domain.Requests;

public static class RequestErrors
{
    public static Error TitleRequired =>
        Error.Validation("Request.Title.Required", "Title is required.");

    public static Error DescriptionRequired =>
        Error.Validation("Request.Description.Required", "Description is required.");

    public static Error RequestTypeRequired =>
        Error.Validation("Request.RequestTypeId.Required", "Request type is required.");

    public static Error RequesterRequired =>
        Error.Validation("Request.RequestedById.Required", "The requester is required.");

    public static Error ModuleTypeRequired =>
        Error.Validation("Request.ImpactedModuleTypeId.Required", "Impacted module is required.");

    public static Error BusinessJustificationRequired =>
        Error.Validation("Request.BusinessJustification.Required", "Business justification is required.");

    public static Error PriorityInvalid =>
        Error.Validation("Request.Priority.Invalid", "The provided priority is invalid.");

    public static Error PhaseInvalid =>
        Error.Validation("Request.Phase.Invalid", "The provided phase is invalid.");

    public static Error UatResultInvalid =>
        Error.Validation("Request.UatResult.Invalid", "The provided UAT result is invalid.");

    public static Error DeveloperRequired =>
        Error.Validation("Request.AssignedDeveloperId.Required", "A valid developer id is required.");

    public static Error BusinessAnalystRequired =>
        Error.Validation("Request.AssignedBusinessAnalystId.Required", "A valid business analyst id is required.");

    public static Error EstimatedEffortInvalid =>
        Error.Validation("Request.EstimatedEffort.Invalid", "Estimated effort cannot be negative.");

    public static Error DevelopmentProgressInvalid =>
        Error.Validation("Request.DevelopmentProgress.Invalid", "Development progress must be between 0 and 100.");

    public static Error InvalidTiming =>
        Error.Conflict("Request.InvalidTiming", "Actual end cannot be before actual start.");

    public static Error ReadOnly(int id) => Error.Conflict(
        code: "Request.ReadOnly",
        description: $"Request '{id}' cannot be modified in its current status/phase.");

    public static Error InvalidStatusTransition(RequestStatus current, RequestStatus next) => Error.Conflict(
        code: "Request.InvalidStatusTransition",
        description: $"Request cannot transition from '{current}' to '{next}'.");
}
