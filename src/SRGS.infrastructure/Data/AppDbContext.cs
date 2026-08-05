using System.Security.Cryptography.X509Certificates;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Common.Models;
using SRGS.Domain.Common;
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

public class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
: DbContext(options), IAppDbContext
{
    // Identity & Access Control
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRoleRow> UserRoleRows => Set<UserRoleRow>();


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


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entitiesWithEvents = ChangeTracker.Entries()
            .Where(e => e.Entity is IHasDomainEvents hasEvents && hasEvents.DomainEvents.Count != 0)
            .Select(e => (IHasDomainEvents)e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }
    }
}