using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Domain.Common;
using TroiSinhVien.Domain.Entities;

namespace TroiSinhVien.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<BoardingHouse> BoardingHouses => Set<BoardingHouse>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<TenantProfile> TenantProfiles => Set<TenantProfile>();
    public DbSet<RoomTenant> RoomTenants => Set<RoomTenant>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractMember> ContractMembers => Set<ContractMember>();
    public DbSet<PropertyService> PropertyServices => Set<PropertyService>();
    public DbSet<ContractService> ContractServices => Set<ContractService>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<MaintenanceComment> MaintenanceComments => Set<MaintenanceComment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        if (Database.IsSqlServer()) ApplySqlServerIndexFilters(builder);

        builder.Entity<BoardingHouse>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Room>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TenantProfile>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<RoomTenant>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Contract>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ContractMember>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<PropertyService>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ContractService>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<MeterReading>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Invoice>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<InvoiceDetail>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Payment>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<MaintenanceRequest>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<MaintenanceComment>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Notification>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<UploadedFile>().HasQueryFilter(x => !x.IsDeleted);
    }

    private static void ApplySqlServerIndexFilters(ModelBuilder builder)
    {
        foreach (var index in builder.Model.GetEntityTypes().SelectMany(x => x.GetIndexes()))
        {
            var filter = index.GetFilter();
            if (string.IsNullOrWhiteSpace(filter)) continue;

            index.SetFilter(filter
                .Replace("\"IsDeleted\" = FALSE", "[IsDeleted] = 0", StringComparison.Ordinal)
                .Replace("\"MoveOutDate\"", "[MoveOutDate]", StringComparison.Ordinal)
                .Replace("\"Status\"", "[Status]", StringComparison.Ordinal));
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = entry.Entity.CreatedAt == default ? now : entry.Entity.CreatedAt;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = entry.Entity.CreatedAt == default ? now : entry.Entity.CreatedAt;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
