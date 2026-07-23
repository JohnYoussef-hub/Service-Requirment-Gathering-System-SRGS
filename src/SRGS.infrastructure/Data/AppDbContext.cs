using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

namespace SRGS.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
: DbContext(options)
{
    // Identity & Access Control
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    // Request Management
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<ChangeHistory> ChangeHistories => Set<ChangeHistory>();
    public DbSet<Description> Descriptions => Set<Description>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Lookup Entities
    public DbSet<ModuleType> ModuleTypes => Set<ModuleType>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
}