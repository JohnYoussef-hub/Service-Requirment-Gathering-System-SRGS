namespace SRGS.Domain.Common;

/// <summary>
/// Adds Created/LastModified audit columns. Only used by entities whose table actually
/// carries those columns (currently just REFRESH_TOKEN) — do not apply this to every
/// entity just because MechanicShop does; most SRGS tables don't have audit columns.
/// </summary>
public abstract class AuditableEntity<TId> : Entity<TId>
{
    protected AuditableEntity()
    { }

    protected AuditableEntity(TId id)
        : base(id)
    {
    }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModifiedUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
