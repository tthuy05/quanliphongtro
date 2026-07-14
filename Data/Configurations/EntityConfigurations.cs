using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TroiSinhVien.Domain.Common;
using TroiSinhVien.Domain.Entities;

namespace TroiSinhVien.Data.Configurations;

internal static class ConfigurationExtensions
{
    public static void ConfigureAudit<T>(this EntityTypeBuilder<T> b) where T : AuditableEntity
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedAt).IsRequired();
        b.HasIndex(x => x.IsDeleted);
    }

    public static PropertyBuilder<TEnum> EnumText<TEnum>(this PropertyBuilder<TEnum> p) where TEnum : struct, Enum =>
        p.HasConversion<string>().HasMaxLength(32);
}

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        b.HasMany(x => x.OwnedBoardingHouses).WithOne(x => x.Owner).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Notifications).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class BoardingHouseConfiguration : IEntityTypeConfiguration<BoardingHouse>
{
    public void Configure(EntityTypeBuilder<BoardingHouse> b)
    {
        b.ToTable("BoardingHouses"); b.ConfigureAudit();
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Address).HasMaxLength(400).IsRequired();
        b.Property(x => x.ContactPhone).HasMaxLength(20);
        b.Property(x => x.DefaultElectricityPrice).HasPrecision(18, 2);
        b.Property(x => x.DefaultWaterPrice).HasPrecision(18, 2);
        b.HasIndex(x => new { x.OwnerId, x.IsActive });
        b.ToTable(t => { t.HasCheckConstraint("CK_BoardingHouse_ElectricityPrice", "\"DefaultElectricityPrice\" >= 0"); t.HasCheckConstraint("CK_BoardingHouse_WaterPrice", "\"DefaultWaterPrice\" >= 0"); });
    }
}

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.ToTable("Rooms"); b.ConfigureAudit();
        b.Property(x => x.RoomCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.RoomName).HasMaxLength(100); b.Property(x => x.Area).HasPrecision(10, 2);
        b.Property(x => x.MonthlyRent).HasPrecision(18, 2); b.Property(x => x.DepositAmount).HasPrecision(18, 2);
        b.Property(x => x.Description).HasMaxLength(1000); b.Property(x => x.Status).EnumText();
        b.HasOne(x => x.BoardingHouse).WithMany(x => x.Rooms).HasForeignKey(x => x.BoardingHouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.BoardingHouseId, x.RoomCode }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        b.HasIndex(x => new { x.BoardingHouseId, x.Status });
        b.ToTable(t => { t.HasCheckConstraint("CK_Room_MonthlyRent", "\"MonthlyRent\" >= 0"); t.HasCheckConstraint("CK_Room_Deposit", "\"DepositAmount\" >= 0"); t.HasCheckConstraint("CK_Room_MaxOccupants", "\"MaximumOccupants\" > 0"); });
    }
}

public sealed class TenantProfileConfiguration : IEntityTypeConfiguration<TenantProfile>
{
    public void Configure(EntityTypeBuilder<TenantProfile> b)
    {
        b.ToTable("TenantProfiles"); b.ConfigureAudit();
        b.Property(x => x.FullName).HasMaxLength(150).IsRequired(); b.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        b.Property(x => x.Email).HasMaxLength(256); b.Property(x => x.IdentityNumber).HasMaxLength(30);
        b.Property(x => x.PermanentAddress).HasMaxLength(400); b.Property(x => x.VehiclePlate).HasMaxLength(20);
        b.Property(x => x.EmergencyContactName).HasMaxLength(150); b.Property(x => x.EmergencyContactPhone).HasMaxLength(20);
        b.HasOne(x => x.User).WithOne(x => x.TenantProfile).HasForeignKey<TenantProfile>(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.UserId).IsUnique(); b.HasIndex(x => new { x.OwnerId, x.PhoneNumber });
    }
}

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> b)
    {
        b.ToTable("Contracts"); b.ConfigureAudit();
        b.Property(x => x.ContractCode).HasMaxLength(40).IsRequired(); b.HasIndex(x => x.ContractCode).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        b.Property(x => x.MonthlyRent).HasPrecision(18, 2); b.Property(x => x.DepositAmount).HasPrecision(18, 2);
        b.Property(x => x.ElectricityPrice).HasPrecision(18, 2); b.Property(x => x.WaterPrice).HasPrecision(18, 2);
        b.Property(x => x.Terms).HasMaxLength(4000); b.Property(x => x.CancellationReason).HasMaxLength(500); b.Property(x => x.Status).EnumText();
        b.HasOne(x => x.Room).WithMany(x => x.Contracts).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RepresentativeTenant).WithMany(x => x.RepresentedContracts).HasForeignKey(x => x.RepresentativeTenantId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.RoomId, x.Status, x.StartDate, x.EndDate });
        b.HasIndex(x => x.RoomId).IsUnique().HasFilter("\"IsDeleted\" = FALSE AND \"Status\" = 'Active'");
        b.ToTable(t => { t.HasCheckConstraint("CK_Contract_Dates", "\"EndDate\" > \"StartDate\""); t.HasCheckConstraint("CK_Contract_DueDay", "\"PaymentDueDay\" BETWEEN 1 AND 28"); t.HasCheckConstraint("CK_Contract_Money", "\"MonthlyRent\" >= 0 AND \"DepositAmount\" >= 0 AND \"ElectricityPrice\" >= 0 AND \"WaterPrice\" >= 0"); });
    }
}

public sealed class ContractMemberConfiguration : IEntityTypeConfiguration<ContractMember>
{
    public void Configure(EntityTypeBuilder<ContractMember> b)
    {
        b.ToTable("ContractMembers"); b.ConfigureAudit();
        b.HasOne(x => x.Contract).WithMany(x => x.Members).HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TenantProfile).WithMany(x => x.ContractMemberships).HasForeignKey(x => x.TenantProfileId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ContractId, x.TenantProfileId }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
    }
}

public sealed class RoomTenantConfiguration : IEntityTypeConfiguration<RoomTenant>
{
    public void Configure(EntityTypeBuilder<RoomTenant> b)
    {
        b.ToTable("RoomTenants"); b.ConfigureAudit();
        b.HasOne(x => x.Room).WithMany(x => x.RoomTenants).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TenantProfile).WithMany(x => x.RoomTenancies).HasForeignKey(x => x.TenantProfileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Contract).WithMany(x => x.RoomTenants).HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TenantProfileId, x.MoveOutDate });
        b.HasIndex(x => x.TenantProfileId).IsUnique().HasFilter("\"IsDeleted\" = FALSE AND \"MoveOutDate\" IS NULL");
    }
}

public sealed class PropertyServiceConfiguration : IEntityTypeConfiguration<PropertyService>
{
    public void Configure(EntityTypeBuilder<PropertyService> b)
    {
        b.ToTable("PropertyServices"); b.ConfigureAudit();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired(); b.Property(x => x.Unit).HasMaxLength(30).IsRequired(); b.Property(x => x.CalculationType).EnumText(); b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.HasOne(x => x.BoardingHouse).WithMany(x => x.Services).HasForeignKey(x => x.BoardingHouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.BoardingHouseId, x.Name }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        b.ToTable(t => t.HasCheckConstraint("CK_PropertyService_Price", "\"UnitPrice\" >= 0"));
    }
}

public sealed class ContractServiceConfiguration : IEntityTypeConfiguration<ContractService>
{
    public void Configure(EntityTypeBuilder<ContractService> b)
    {
        b.ToTable("ContractServices"); b.ConfigureAudit();
        b.Property(x => x.ServiceNameSnapshot).HasMaxLength(100).IsRequired(); b.Property(x => x.UnitSnapshot).HasMaxLength(30).IsRequired(); b.Property(x => x.CalculationType).EnumText(); b.Property(x => x.Quantity).HasPrecision(12, 2); b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.HasOne(x => x.Contract).WithMany(x => x.Services).HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PropertyService).WithMany(x => x.ContractServices).HasForeignKey(x => x.PropertyServiceId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ContractId, x.PropertyServiceId }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        b.ToTable(t => t.HasCheckConstraint("CK_ContractService_Quantity", "\"Quantity\" > 0"));
    }
}

public sealed class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> b)
    {
        b.ToTable("MeterReadings"); b.ConfigureAudit(); b.Property(x => x.MeterType).EnumText();
        b.Property(x => x.PreviousReading).HasPrecision(12, 2); b.Property(x => x.CurrentReading).HasPrecision(12, 2); b.Property(x => x.Consumption).HasPrecision(12, 2);
        b.HasOne(x => x.Room).WithMany(x => x.MeterReadings).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Contract).WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.RoomId, x.MeterType, x.BillingYear, x.BillingMonth }).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        b.ToTable(t => { t.HasCheckConstraint("CK_Meter_Period", "\"BillingMonth\" BETWEEN 1 AND 12"); t.HasCheckConstraint("CK_Meter_Values", "\"PreviousReading\" >= 0 AND \"CurrentReading\" >= \"PreviousReading\" AND \"Consumption\" = \"CurrentReading\" - \"PreviousReading\""); });
    }
}

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.ToTable("Invoices"); b.ConfigureAudit();
        b.Property(x => x.InvoiceNumber).HasMaxLength(40).IsRequired(); b.HasIndex(x => x.InvoiceNumber).IsUnique().HasFilter("\"IsDeleted\" = FALSE"); b.Property(x => x.Status).EnumText();
        b.Property(x => x.SubtotalAmount).HasPrecision(18, 2); b.Property(x => x.PreviousDebtAmount).HasPrecision(18, 2); b.Property(x => x.DiscountAmount).HasPrecision(18, 2); b.Property(x => x.TotalAmount).HasPrecision(18, 2); b.Property(x => x.PaidAmount).HasPrecision(18, 2); b.Property(x => x.RemainingAmount).HasPrecision(18, 2);
        b.Property(x => x.CancellationReason).HasMaxLength(500);
        b.HasOne(x => x.Contract).WithMany(x => x.Invoices).HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ContractId, x.BillingYear, x.BillingMonth }).IsUnique().HasFilter("\"IsDeleted\" = FALSE AND \"Status\" <> 'Cancelled'");
        b.HasIndex(x => new { x.RoomId, x.Status, x.DueDate });
        b.ToTable(t => { t.HasCheckConstraint("CK_Invoice_Period", "\"BillingMonth\" BETWEEN 1 AND 12"); t.HasCheckConstraint("CK_Invoice_Amounts", "\"SubtotalAmount\" >= 0 AND \"PreviousDebtAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"TotalAmount\" >= 0 AND \"PaidAmount\" >= 0 AND \"RemainingAmount\" >= 0"); t.HasCheckConstraint("CK_Invoice_Total", "\"TotalAmount\" = \"SubtotalAmount\" + \"PreviousDebtAmount\" - \"DiscountAmount\""); });
    }
}

public sealed class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> b)
    {
        b.ToTable("InvoiceDetails"); b.ConfigureAudit();
        b.Property(x => x.Description).HasMaxLength(250).IsRequired(); b.Property(x => x.Unit).HasMaxLength(30).IsRequired(); b.Property(x => x.SourceType).EnumText();
        b.Property(x => x.Quantity).HasPrecision(12, 2); b.Property(x => x.UnitPrice).HasPrecision(18, 2); b.Property(x => x.Amount).HasPrecision(18, 2);
        b.HasOne(x => x.Invoice).WithMany(x => x.Details).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        b.ToTable(t => t.HasCheckConstraint("CK_InvoiceDetail_Amounts", "\"Quantity\" >= 0 AND \"UnitPrice\" >= 0 AND \"Amount\" >= 0"));
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments"); b.ConfigureAudit(); b.Property(x => x.Amount).HasPrecision(18, 2); b.Property(x => x.Method).EnumText(); b.Property(x => x.Status).EnumText();
        b.Property(x => x.ReferenceCode).HasMaxLength(100); b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.HasOne(x => x.Invoice).WithMany(x => x.Payments).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EvidenceFile).WithMany().HasForeignKey(x => x.EvidenceFileId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.InvoiceId, x.Status }); b.ToTable(t => t.HasCheckConstraint("CK_Payment_Amount", "\"Amount\" > 0"));
    }
}

public sealed class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> b)
    {
        b.ToTable("MaintenanceRequests"); b.ConfigureAudit();
        b.Property(x => x.Category).HasMaxLength(50).IsRequired(); b.Property(x => x.Title).HasMaxLength(150).IsRequired(); b.Property(x => x.Description).HasMaxLength(2000).IsRequired(); b.Property(x => x.Priority).EnumText(); b.Property(x => x.Status).EnumText();
        b.HasOne(x => x.BoardingHouse).WithMany().HasForeignKey(x => x.BoardingHouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TenantProfile).WithMany().HasForeignKey(x => x.TenantProfileId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.BoardingHouseId, x.Status, x.Priority });
    }
}

public sealed class MaintenanceCommentConfiguration : IEntityTypeConfiguration<MaintenanceComment>
{
    public void Configure(EntityTypeBuilder<MaintenanceComment> b)
    {
        b.ToTable("MaintenanceComments"); b.ConfigureAudit(); b.Property(x => x.Content).HasMaxLength(2000).IsRequired();
        b.HasOne(x => x.MaintenanceRequest).WithMany(x => x.Comments).HasForeignKey(x => x.MaintenanceRequestId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications"); b.ConfigureAudit(); b.Property(x => x.Type).EnumText(); b.Property(x => x.Title).HasMaxLength(150).IsRequired(); b.Property(x => x.Message).HasMaxLength(1000).IsRequired(); b.Property(x => x.LinkUrl).HasMaxLength(500);
        b.HasIndex(x => new { x.UserId, x.ReadAt, x.CreatedAt });
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs"); b.HasKey(x => x.Id); b.Property(x => x.Action).HasMaxLength(100).IsRequired(); b.Property(x => x.EntityType).HasMaxLength(100).IsRequired(); b.Property(x => x.EntityId).HasMaxLength(100).IsRequired(); b.Property(x => x.Summary).HasMaxLength(1000).IsRequired(); b.Property(x => x.IpAddress).HasMaxLength(64); b.HasIndex(x => new { x.EntityType, x.EntityId, x.Timestamp }); b.HasIndex(x => new { x.UserId, x.Timestamp });
    }
}

public sealed class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    public void Configure(EntityTypeBuilder<UploadedFile> b)
    {
        b.ToTable("UploadedFiles"); b.ConfigureAudit(); b.Property(x => x.StoredName).HasMaxLength(100).IsRequired(); b.Property(x => x.OriginalName).HasMaxLength(255).IsRequired(); b.Property(x => x.ContentType).HasMaxLength(100).IsRequired(); b.Property(x => x.RelativePath).HasMaxLength(500).IsRequired(); b.HasIndex(x => x.StoredName).IsUnique(); b.ToTable(t => t.HasCheckConstraint("CK_UploadedFile_Size", "\"Size\" > 0"));
    }
}
