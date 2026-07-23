using System.ComponentModel.DataAnnotations.Schema;

namespace SRGS.Domain.Common;

/// <summary>
/// Base type for every domain entity. Generic over the primary key type since SRGS uses
/// DB-generated INT identity keys (not client-generated GUIDs like MechanicShop), except
/// where a table explicitly uses a GUID (e.g. RefreshToken).
/// </summary>
public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;

    private readonly List<DomainEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity()
    { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
