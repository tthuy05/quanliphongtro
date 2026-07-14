using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Constants;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Enums;

namespace TroiSinhVien.Tests;

public sealed class AuthenticatedFlowTests
{
    [Fact]
    public async Task Owner_tenant_and_admin_can_render_their_real_workflows()
    {
        await using var factory = new SqliteWebFactory(); await factory.SeedAsync();

        using var visitor = factory.Client();
        foreach (var assetPath in new[] { "/css/tailwind.generated.css", "/vendor/alpine.min.js", "/vendor/chart.umd.js" })
        {
            var asset = await visitor.GetAsync(assetPath);
            Assert.Equal(HttpStatusCode.OK, asset.StatusCode);
            Assert.True((await asset.Content.ReadAsByteArrayAsync()).Length > 0, $"Static asset {assetPath} was empty.");
        }

        using var owner = factory.Client(); await LoginAsync(owner, "owner@flow.local");
        foreach (var path in new[] { "/Dashboard", "/BoardingHouses/Details/1", "/Rooms", "/Rooms/Details/1", "/Tenants/Details/1", "/Contracts", "/Invoices", "/Payments", "/Maintenance", "/Reports/Revenue", "/Account/Settings" })
            Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await owner.GetAsync("/Admin")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await PostFormAsync(owner, "/Rooms/Create", new() { ["BoardingHouseId"] = "1", ["RoomCode"] = "A102", ["MaximumOccupants"] = "2", ["MonthlyRent"] = "1800000", ["DepositAmount"] = "1800000", ["Status"] = "Available" })).StatusCode);
        Assert.True(await factory.RoomExistsAsync("A102"));

        using var tenant = factory.Client(); await LoginAsync(tenant, "tenant@flow.local");
        foreach (var path in new[] { "/Tenant/Invoices", "/Tenant/Invoices/1", "/Tenant/Maintenance", "/Account/Settings" })
            Assert.Equal(HttpStatusCode.OK, (await tenant.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await tenant.GetAsync("/Rooms")).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await PostFormAsync(tenant, "/Tenant/Maintenance/Create", new() { ["Category"] = "Điện", ["Title"] = "Đèn hỏng", ["Description"] = "Không bật được", ["Priority"] = "Medium" })).StatusCode);
        Assert.True(await factory.MaintenanceExistsAsync("Đèn hỏng"));

        using var admin = factory.Client(); await LoginAsync(admin, "admin@flow.local");
        foreach (var path in new[] { "/Admin", "/Admin/Create", "/Admin/Audit", "/Account/Settings" })
            Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await PostFormAsync(admin, "/Admin/Create", new() { ["Email"] = "new-owner@flow.local", ["DisplayName"] = "New Owner", ["Role"] = SystemRoles.Owner, ["Password"] = "Valid1234", ["ConfirmPassword"] = "Valid1234" })).StatusCode);
        Assert.True(await factory.UserExistsAsync("new-owner@flow.local"));
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var page = await client.GetStringAsync("/Account/Login");
        var match = Regex.Match(page, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, "The login antiforgery token was not rendered.");
        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Password"] = "Valid1234", ["RememberMe"] = "false", ["__RequestVerificationToken"] = match.Groups[1].Value
        }));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostFormAsync(HttpClient client, string path, Dictionary<string, string> values)
    {
        var page = await client.GetStringAsync(path); var match = Regex.Match(page, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\""); Assert.True(match.Success, $"The antiforgery token was not rendered for {path}."); values["__RequestVerificationToken"] = match.Groups[1].Value; return await client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private sealed class SqliteWebFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            connection.Open(); builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused;Username=unused;Password=unused", ["SeedData:Enabled"] = "false", ["Database:ApplyMigrationsOnStartup"] = "false"
            }));
            builder.ConfigureServices(services =>
            {
                foreach (var descriptor in services.Where(x => x.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) || x.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") == true).ToList()) services.Remove(descriptor);
                services.RemoveAll<ApplicationDbContext>(); services.AddSingleton(connection); services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
            });
        }
        public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true, BaseAddress = new Uri("https://localhost") });
        public async Task<bool> RoomExistsAsync(string code) { await using var scope = Services.CreateAsyncScope(); return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Rooms.AnyAsync(x => x.RoomCode == code); }
        public async Task<bool> MaintenanceExistsAsync(string title) { await using var scope = Services.CreateAsyncScope(); return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().MaintenanceRequests.AnyAsync(x => x.Title == title); }
        public async Task<bool> UserExistsAsync(string email) { await using var scope = Services.CreateAsyncScope(); return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.AnyAsync(x => x.Email == email); }
        public async Task SeedAsync()
        {
            await using var scope = Services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); await db.Database.EnsureCreatedAsync();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>(); foreach (var role in SystemRoles.All) Assert.True((await roles.CreateAsync(new IdentityRole<Guid>(role))).Succeeded);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var owner = await UserAsync(users, "owner@flow.local", SystemRoles.Owner); var tenantUser = await UserAsync(users, "tenant@flow.local", SystemRoles.Tenant); await UserAsync(users, "admin@flow.local", SystemRoles.Admin);
            var property = new BoardingHouse { OwnerId = owner.Id, Name = "Khu kiểm thử", Address = "TP.HCM" }; var tenant = new TenantProfile { OwnerId = owner.Id, UserId = tenantUser.Id, FullName = "Người thuê kiểm thử", PhoneNumber = "0900000000" }; db.AddRange(property, tenant); await db.SaveChangesAsync();
            var room = new Room { BoardingHouseId = property.Id, RoomCode = "A101", MonthlyRent = 2_000_000, DepositAmount = 2_000_000, MaximumOccupants = 2, Status = RoomStatus.Occupied }; db.Add(room); await db.SaveChangesAsync();
            var contract = new Contract { ContractCode = "FLOW-001", RoomId = room.Id, RepresentativeTenantId = tenant.Id, StartDate = new(2026, 1, 1), EndDate = new(2026, 12, 31), MonthlyRent = room.MonthlyRent, DepositAmount = room.DepositAmount, ElectricityPrice = 4_000, WaterPrice = 25_000, PaymentDueDay = 10, Status = ContractStatus.Active }; db.Add(contract); await db.SaveChangesAsync();
            db.AddRange(new ContractMember { ContractId = contract.Id, TenantProfileId = tenant.Id, IsRepresentative = true, JoinedDate = contract.StartDate }, new RoomTenant { ContractId = contract.Id, RoomId = room.Id, TenantProfileId = tenant.Id, MoveInDate = contract.StartDate }); await db.SaveChangesAsync();
            db.Add(new Invoice { InvoiceNumber = "FLOW-INV-001", ContractId = contract.Id, RoomId = room.Id, BillingYear = 2026, BillingMonth = 7, DueDate = new(2026, 7, 10), SubtotalAmount = room.MonthlyRent, TotalAmount = room.MonthlyRent, RemainingAmount = room.MonthlyRent, Status = InvoiceStatus.Issued }); await db.SaveChangesAsync();
        }
        private static async Task<ApplicationUser> UserAsync(UserManager<ApplicationUser> manager, string email, string role) { var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = email, IsActive = true }; Assert.True((await manager.CreateAsync(user, "Valid1234")).Succeeded); Assert.True((await manager.AddToRoleAsync(user, role)).Succeeded); return user; }
        protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) connection.Dispose(); }
    }
}
