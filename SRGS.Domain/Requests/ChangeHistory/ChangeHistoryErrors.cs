using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.History;

public static class ChangeHistoryErrors
{
    public static Error RequestIdRequired =>
        Error.Validation("ChangeHistory.RequestId.Required", "Request id is required.");

    public static Error OperationInvalid =>
        Error.Validation("ChangeHistory.Operation.Invalid", "The provided change operation is invalid.");

    public static Error ChangedByRequired =>
        Error.Validation("ChangeHistory.ChangedByUserId.Required", "ChangedBy user id is required.");
}
