using MediatR;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Requests.Events;
using SRGS.Domain.Requests.History;

namespace SRGS.Application.Features.Requests.EventHandlers;

public sealed class RequestStatusChangedEventHandler(IAppDbContext context)
    : INotificationHandler<RequestStatusChanged>
{
    public Task Handle(RequestStatusChanged notification, CancellationToken cancellationToken)
    {
        var history = ChangeHistory.CreateStatusTransition(
            notification.RequestId,
            notification.OldStatus,
            notification.NewStatus,
            notification.ActorId,
            notification.Note);

        if (history.IsError)
        {
            throw new InvalidOperationException(history.TopError.Description);
        }

        context.ChangeHistories.Add(history.Value);
        return Task.CompletedTask;
    }
}
