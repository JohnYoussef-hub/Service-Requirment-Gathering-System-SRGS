using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Lookups;
using SRGS.Domain.Roles;
using SRGS.Infrastructure.Data;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace SRGS.Infrastructure.Tests.Integration;

public sealed class SrgsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestUsername = "test.user";
    public const string TestPassword = "fake-password";
    public const string TestIssuer = "srgs-integration-tests";
    public const string TestAudience = "srgs-api-integration-tests";
    public const string TestSecret = "integration-test-secret-that-is-long-enough-for-hs256";

    private readonly string _databaseName = $"SRGS_IntegrationTests_{Guid.NewGuid():N}";
    private readonly string _connectionString;

    public SrgsApiFactory()
    {
        _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;";
    }

    public int RequestTypeId { get; private set; }
    public int ModuleTypeId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("JwtSettings:Secret", TestSecret);
        builder.UseSetting("JwtSettings:Issuer", TestIssuer);
        builder.UseSetting("JwtSettings:Audience", TestAudience);
        builder.UseSetting("JwtSettings:TokenExpirationInMinutes", "15");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(_connectionString));

            services.RemoveAll<ILdapService>();
            services.AddScoped<ILdapService, FakeLdapService>();
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync(CancellationToken.None);

        var role = Role.Create("Requester").Value;
        var requestType = RequestType.Create("Question").Value;
        var moduleType = ModuleType.Create("TestModule").Value;

        context.Roles.Add(role);
        context.RequestTypes.Add(requestType);
        context.ModuleTypes.Add(moduleType);
        await context.SaveChangesAsync(CancellationToken.None);

        RequestTypeId = requestType.Id;
        ModuleTypeId = moduleType.Id;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync(CancellationToken.None);
        await base.DisposeAsync();
    }

    public IServiceScope CreateDatabaseScope() => Services.CreateScope();

    public static string CreateExpiredToken(int userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
            ]),
            Issuer = TestIssuer,
            Audience = TestAudience,
            Expires = DateTime.UtcNow.AddMinutes(-5),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private sealed class FakeLdapService : ILdapService
    {
        public Task<Result<LdapUserDto>> AuthenticateAsync(string username, string password)
        {
            if (username == TestUsername && password == TestPassword)
            {
                return Task.FromResult<Result<LdapUserDto>>(new LdapUserDto(
                    TestUsername,
                    "Test",
                    null,
                    "User",
                    "test.user@example.test",
                    null,
                    null,
                    null,
                    null,
                    "00000"));
            }

            return Task.FromResult<Result<LdapUserDto>>(Error.Unauthorized(
                "Test.InvalidCredentials",
                "The username or password is invalid."));
        }
    }

}
