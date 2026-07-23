using Microsoft.EntityFrameworkCore;
using SRGS.Domain.Identity;
using SRGS.Domain.Requests;
using SRGS.Domain.Requests.Approvals;
using SRGS.Domain.Requests.Attachments;
using SRGS.Domain.Requests.History;
using SRGS.Domain.Requests.Lookups;
using SRGS.Domain.Requests.Notes;
using SRGS.Domain.Requests.Notifications;
using SRGS.Domain.Roles;
using SRGS.Domain.Users;

namespace Application.Common.Interfaces;

public interface IAppDbContext
{
    // Identity & Access Control
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }

    // Request Management
    DbSet<Request> Requests { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<Approval> Approvals { get; }
    DbSet<ChangeHistory> ChangeHistories { get; }
    DbSet<Description> Descriptions { get; }
    DbSet<Notification> Notifications { get; }

    // Lookup Entities
    DbSet<ModuleType> ModuleTypes { get; }
    DbSet<RequestType> RequestTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}