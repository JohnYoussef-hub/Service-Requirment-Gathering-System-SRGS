using MediatR;

namespace SRGS.Domain.Common;


public interface IHasDomainEvents
{
    IReadOnlyCollection<INotification> DomainEvents { get; }
    void ClearDomainEvents();

}

public abstract class DomainEvent : INotification;
