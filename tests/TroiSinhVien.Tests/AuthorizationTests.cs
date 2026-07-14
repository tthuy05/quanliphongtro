using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TroiSinhVien.Controllers;
using TroiSinhVien.Domain.Constants;

namespace TroiSinhVien.Tests;

public sealed class AuthorizationTests
{
    [Fact]
    public async Task Unauthenticated_user_is_challenged_on_protected_page()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused;Username=unused;Password=unused", ["SeedData:Enabled"] = "false", ["Database:ApplyMigrationsOnStartup"] = "false" })));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }); var response = await client.GetAsync("/Rooms"); Assert.Equal(HttpStatusCode.Redirect, response.StatusCode); Assert.Equal("/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Theory]
    [InlineData(typeof(AdminController), SystemRoles.Admin)]
    [InlineData(typeof(TenantController), SystemRoles.Tenant)]
    [InlineData(typeof(RoomsController), SystemRoles.Owner)]
    public void Role_scoped_controllers_declare_required_role(Type controller, string role)
    {
        var attribute = controller.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single(); Assert.Contains(role, attribute.Roles ?? string.Empty);
    }
}
