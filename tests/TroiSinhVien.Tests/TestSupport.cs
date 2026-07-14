using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Tests;

public sealed class FakeCurrentUser(Guid id, params string[] roles) : ICurrentUserService
{
    public Guid? UserId { get; set; } = id;
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsInRole(string role) => roles.Contains(role);
}
public sealed class FakeClock : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 7, 12, 8, 0, 0, TimeSpan.Zero);
    public DateOnly UtcToday => DateOnly.FromDateTime(UtcNow.UtcDateTime);
}
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    public ApplicationDbContext Db { get; private set; } = null!;
    public Guid OwnerId { get; } = Guid.NewGuid();
    public Guid OtherOwnerId { get; } = Guid.NewGuid();
    public Guid TenantUserId { get; } = Guid.NewGuid();
    public FakeClock Clock { get; } = new();

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync(); Db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options); await Db.Database.EnsureCreatedAsync();
        Db.Users.AddRange(User(OwnerId, "owner@test.local"), User(OtherOwnerId, "other@test.local"), User(TenantUserId, "tenant@test.local")); await Db.SaveChangesAsync();
    }
    public async Task<(BoardingHouse Property, Room Room, TenantProfile Tenant)> SeedCoreAsync(RoomStatus status = RoomStatus.Available)
    {
        var property = new BoardingHouse { OwnerId = OwnerId, Name = "Nhà trọ A", Address = "Địa chỉ test", DefaultElectricityPrice = 4000, DefaultWaterPrice = 25000 };
        var tenant = new TenantProfile { OwnerId = OwnerId, UserId = TenantUserId, FullName = "Người thuê test", PhoneNumber = "0900000000" };
        Db.AddRange(property, tenant); await Db.SaveChangesAsync(); var room = new Room { BoardingHouseId = property.Id, RoomCode = "A101", MonthlyRent = 2_000_000, DepositAmount = 2_000_000, MaximumOccupants = 2, Status = status }; Db.Add(room); await Db.SaveChangesAsync(); return (property, room, tenant);
    }
    public async Task<Contract> SeedActiveContractAsync(Room room, TenantProfile tenant)
    {
        room.Status = RoomStatus.Occupied; var c = new Contract { ContractCode = $"HD-{Guid.NewGuid():N}", RoomId = room.Id, RepresentativeTenantId = tenant.Id, StartDate = new(2026, 1, 1), EndDate = new(2026, 12, 31), MonthlyRent = room.MonthlyRent, DepositAmount = room.DepositAmount, ElectricityPrice = 4000, WaterPrice = 25000, PaymentDueDay = 10, Status = ContractStatus.Active, ActivatedAt = Clock.UtcNow }; Db.Contracts.Add(c); await Db.SaveChangesAsync(); Db.ContractMembers.Add(new ContractMember { ContractId = c.Id, TenantProfileId = tenant.Id, IsRepresentative = true, JoinedDate = c.StartDate }); Db.RoomTenants.Add(new RoomTenant { ContractId = c.Id, RoomId = room.Id, TenantProfileId = tenant.Id, MoveInDate = c.StartDate }); await Db.SaveChangesAsync(); return c;
    }
    private static ApplicationUser User(Guid id, string email) => new() { Id = id, UserName = email, NormalizedUserName = email.ToUpperInvariant(), Email = email, NormalizedEmail = email.ToUpperInvariant(), DisplayName = email, SecurityStamp = Guid.NewGuid().ToString() };
    public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
}

public sealed class NullStorage : IFileStorageService
{
    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default) => Task.FromResult<Stream?>(null);
    public Task<StoredFileResult> SaveEvidenceAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult(new StoredFileResult("safe.png", "proof.png", "image/png", file.Length, "safe.png"));
}

public sealed class MemoryStorage : IFileStorageService
{
    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default) => Task.FromResult<Stream?>(new MemoryStream([1, 2, 3]));
    public Task<StoredFileResult> SaveEvidenceAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult(new StoredFileResult($"{Guid.NewGuid():N}.png", "proof.png", "image/png", file.Length, $"{Guid.NewGuid():N}.png"));
}

public sealed class FakeWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "Tests";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = Path.GetTempPath();
    public string EnvironmentName { get; set; } = "Testing";
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
