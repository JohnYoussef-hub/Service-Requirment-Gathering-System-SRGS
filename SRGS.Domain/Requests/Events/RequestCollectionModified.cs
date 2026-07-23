using SRGS.Domain.Common;

namespace SRGS.Domain.Requests.Events;

// Generic "something in the request list changed" signal — mirrors
// WorkOrderCollectionModified in MechanicShop. Typically raised from the Application
// layer command handlers (not the aggregate itself) to trigger a SignalR/dashboard
// refresh without callers caring about which specific field changed.
public sealed class RequestCollectionModified : DomainEvent;
