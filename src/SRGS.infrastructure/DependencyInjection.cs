using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SRGS.Application.Common.Interfaces;
using SRGS.Infrastructure.Data;
using SRGS.infrastructure.Identity;

namespace SRGS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Registered as scoped explicitly (AddDbContext already registers AppDbContext as
        // scoped) so handlers depending on IAppDbContext get the same per-request instance
        // as anything depending on AppDbContext directly — no double-context surprises.
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ILdapService, LdapService>();
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddHybridCache();

        return services;
    }
}
