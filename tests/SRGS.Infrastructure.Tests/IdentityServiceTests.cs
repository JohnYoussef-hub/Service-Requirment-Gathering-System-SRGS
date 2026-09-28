using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Common.Models;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Roles;
using SRGS.Domain.Users;
using SRGS.Infrastructure.Data;
using SRGS.infrastructure.Identity;
using Xunit;

namespace SRGS.Infrastructure.Tests;

public sealed class IdentityServiceTests
{
    private const string DefaultRoleName = "Requester";

    [Fact]
    public async Task AuthenticateAsync_CreatesNewUserWithDefaultRole()
    {
        await using var context = CreateContext();
        var role = await AddDefaultRoleAsync(context);
        var service = CreateService(context, CreateLdapUser());

        var result = await service.AuthenticateAsync("test.user", "test-password");

        Assert.True(result.IsSuccess);
        var user = await context.Users.SingleAsync();
        Assert.Equal("test.user", user.Username);
        Assert.Contains(context.UserRoleRows, row => row.UserId == user.Id && row.RoleId == role.Id);
    }

    [Fact]
    public async Task AuthenticateAsync_RefreshesExistingUserProfile()
    {
        await using var context = CreateContext();
        var existingUser = await AddUserAsync(context, CreateLdapUser(
            email: "stored.user@example.com",
            phoneNumber: "00001",
            department: "StoredDepartment"));
        var service = CreateService(context, CreateLdapUser(
            email: "test.user@example.com",
            phoneNumber: "00002",
            department: "UpdatedDepartment",
            subDepartment: "UpdatedSubDepartment",
            title: "UpdatedTitle"));

        var result = await service.AuthenticateAsync("test.user", "test-password");

        Assert.True(result.IsSuccess);
        var refreshedUser = await context.Users.SingleAsync(user => user.Id == existingUser.Id);
        Assert.Equal("test.user@example.com", refreshedUser.Email);
        Assert.Equal("00002", refreshedUser.PhoneNumber);
        Assert.Equal("UpdatedDepartment", refreshedUser.Department);
        Assert.Equal("UpdatedSubDepartment", refreshedUser.SubDepartment);
        Assert.Equal("UpdatedTitle", refreshedUser.Title);
    }

    [Fact]
    public async Task AuthenticateAsync_ExistingUserKeepsRoles()
    {
        await using var context = CreateContext();
        var role = await AddDefaultRoleAsync(context);
        var user = await AddUserAsync(context, CreateLdapUser());
        context.UserRoleRows.Add(new UserRoleRow { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync(CancellationToken.None);
        var service = CreateService(context, CreateLdapUser(email: "updated.user@example.com"));

        var result = await service.AuthenticateAsync("test.user", "test-password");

        Assert.True(result.IsSuccess);
        Assert.Equal([DefaultRoleName], result.Value.Roles);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidLdapDataOnFirstLoginReturnsErrorAndCreatesNothing()
    {
        await using var context = CreateContext();
        await AddDefaultRoleAsync(context);
        var service = CreateService(context, CreateLdapUser(email: "invalid-email"));

        var result = await service.AuthenticateAsync("test.user", "test-password");

        Assert.True(result.IsError);
        Assert.Empty(context.Users);
        Assert.Empty(context.UserRoleRows);
    }

    [Fact]
    public async Task AuthenticateAsync_MissingDefaultRoleReturnsErrorAndCreatesNothing()
    {
        await using var context = CreateContext();
        var service = CreateService(context, CreateLdapUser());

        var result = await service.AuthenticateAsync("test.user", "test-password");

        Assert.Equal(ApplicationErrors.DefaultRoleNotFound.Code, result.TopError.Code);
        Assert.Empty(context.Users);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidEmailOnRefreshDoesNotBlockLogin()
    {
        await using var context = CreateContext();
        var existingUser = await AddUserAsync(context, CreateLdapUser(email: "stored.user@example.com"));
        var service = CreateService(context, CreateLdapUser(
            email: "invalid-email",
            department: "UpdatedDepartment"));

        var result = await service.AuthenticateAsync("test.user", "test-password");

        Assert.True(result.IsSuccess);
        var refreshedUser = await context.Users.SingleAsync(user => user.Id == existingUser.Id);
        Assert.Equal("stored.user@example.com", refreshedUser.Email);
        Assert.Equal("UpdatedDepartment", refreshedUser.Department);
    }

    private static IdentityService CreateService(AppDbContext context, LdapUserDto ldapUser)
    {
        return new IdentityService(context, new FakeLdapService(ldapUser));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, new Mock<IMediator>().Object);
    }

    private static async Task<Role> AddDefaultRoleAsync(AppDbContext context)
    {
        var role = Role.Create(DefaultRoleName).Value;
        context.Roles.Add(role);
        await context.SaveChangesAsync(CancellationToken.None);
        return role;
    }

    private static async Task<User> AddUserAsync(AppDbContext context, LdapUserDto ldapUser)
    {
        var user = User.Create(
            ldapUser.Username,
            ldapUser.FirstName,
            ldapUser.FamilyName,
            ldapUser.Email,
            ldapUser.EmployeeNumber,
            ldapUser.MiddleName,
            ldapUser.PhoneNumber,
            ldapUser.Department,
            ldapUser.SubDepartment,
            ldapUser.Title).Value;
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);
        return user;
    }

    private static LdapUserDto CreateLdapUser(
        string email = "test.user@example.com",
        string phoneNumber = "00000",
        string department = "TestDepartment",
        string subDepartment = "TestSubDepartment",
        string title = "TestTitle")
    {
        return new LdapUserDto(
            "test.user",
            "Test",
            null,
            "User",
            email,
            phoneNumber,
            department,
            subDepartment,
            title,
            "00000");
    }

    private sealed class FakeLdapService(LdapUserDto ldapUser) : ILdapService
    {
        public Task<Result<LdapUserDto>> AuthenticateAsync(string username, string password)
            => Task.FromResult<Result<LdapUserDto>>(ldapUser);
    }
}
