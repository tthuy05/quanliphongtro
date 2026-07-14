using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Domain.Constants;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Enums;

namespace TroiSinhVien.Data.Seed;

public sealed class DatabaseSeeder(ApplicationDbContext db, RoleManager<IdentityRole<Guid>> roles, UserManager<ApplicationUser> users, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(bool seedSampleData, bool requireConfiguredAdmin = false, CancellationToken cancellationToken = default)
    {
        foreach (var roleName in SystemRoles.All)
            if (!await roles.RoleExistsAsync(roleName)) await roles.CreateAsync(new IdentityRole<Guid>(roleName));

        var admin = await CreateConfiguredUserAsync("SEED_ADMIN_EMAIL", "SEED_ADMIN_PASSWORD", "Quản trị hệ thống", SystemRoles.Admin);
        if (requireConfiguredAdmin && admin is null) throw new InvalidOperationException("BootstrapAdmin được bật nhưng thiếu SEED_ADMIN_EMAIL hoặc SEED_ADMIN_PASSWORD.");
        if (!seedSampleData) return;

        var owner = await CreateConfiguredUserAsync("SEED_OWNER_EMAIL", "SEED_OWNER_PASSWORD", "Nguyễn Minh Chủ", SystemRoles.Owner);
        var tenantUser = await CreateConfiguredUserAsync("SEED_TENANT_EMAIL", "SEED_TENANT_PASSWORD", "Trần An Nhiên", SystemRoles.Tenant);
        if (owner is null || tenantUser is null)
        {
            logger.LogWarning("Bỏ qua dữ liệu mẫu vì thiếu biến môi trường owner/tenant seed.");
            return;
        }

        if (await db.BoardingHouses.IgnoreQueryFilters().AnyAsync(x => x.OwnerId == owner.Id && x.Name == "Khu trọ Sinh Viên Dịch Vọng", cancellationToken)) return;
        var now = DateTimeOffset.UtcNow; var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tenant = new TenantProfile { OwnerId = owner.Id, UserId = tenantUser.Id, FullName = "Trần An Nhiên", PhoneNumber = "0900000001", Email = tenantUser.Email, PermanentAddress = "Dữ liệu hư cấu - Hà Nội", CreatedAt = now, CreatedBy = owner.Id };
        var property = new BoardingHouse { OwnerId = owner.Id, Name = "Khu trọ Sinh Viên Dịch Vọng", Address = "15 Ngõ 44 Dịch Vọng, Cầu Giấy, Hà Nội (hư cấu)", ContactPhone = "0900000000", DefaultElectricityPrice = 3800, DefaultWaterPrice = 25000, CreatedAt = now, CreatedBy = owner.Id };
        var property2 = new BoardingHouse { OwnerId = owner.Id, Name = "Khu trọ Xuân Thủy", Address = "80 Ngõ 175 Xuân Thủy, Cầu Giấy, Hà Nội (hư cấu)", ContactPhone = "0900000000", DefaultElectricityPrice = 3800, DefaultWaterPrice = 25000, CreatedAt = now, CreatedBy = owner.Id };
        db.AddRange(tenant, property, property2); await db.SaveChangesAsync(cancellationToken);

        var room = new Room { BoardingHouseId = property.Id, RoomCode = "A101", RoomName = "Phòng A101", Floor = 1, Area = 24, MonthlyRent = 2_800_000, DepositAmount = 2_800_000, MaximumOccupants = 3, Status = RoomStatus.Occupied, CreatedBy = owner.Id };
        var room2 = new Room { BoardingHouseId = property.Id, RoomCode = "A102", RoomName = "Phòng A102", Floor = 1, Area = 22, MonthlyRent = 2_600_000, DepositAmount = 2_600_000, MaximumOccupants = 2, Status = RoomStatus.Available, CreatedBy = owner.Id };
        var room3 = new Room { BoardingHouseId = property.Id, RoomCode = "A201", RoomName = "Phòng A201", Floor = 2, Area = 25, MonthlyRent = 3_000_000, DepositAmount = 3_000_000, MaximumOccupants = 3, Status = RoomStatus.Maintenance, CreatedBy = owner.Id };
        db.AddRange(room, room2, room3); await db.SaveChangesAsync(cancellationToken);

        var internet = new PropertyService { BoardingHouseId = property.Id, Name = "Internet", Unit = "phòng", CalculationType = ServiceCalculationType.FixedPerRoom, UnitPrice = 120_000, CreatedBy = owner.Id };
        var waste = new PropertyService { BoardingHouseId = property.Id, Name = "Vệ sinh", Unit = "người", CalculationType = ServiceCalculationType.PerPerson, UnitPrice = 30_000, CreatedBy = owner.Id };
        db.AddRange(internet, waste); await db.SaveChangesAsync(cancellationToken);

        var contract = new Contract { ContractCode = $"HD-{today.Year}-001", RoomId = room.Id, RepresentativeTenantId = tenant.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(10), MonthlyRent = room.MonthlyRent, DepositAmount = room.DepositAmount, ElectricityPrice = property.DefaultElectricityPrice, WaterPrice = property.DefaultWaterPrice, PaymentDueDay = 10, Status = ContractStatus.Active, ActivatedAt = now.AddMonths(-2), CreatedBy = owner.Id };
        db.Contracts.Add(contract); await db.SaveChangesAsync(cancellationToken);
        db.AddRange(
            new ContractMember { ContractId = contract.Id, TenantProfileId = tenant.Id, IsRepresentative = true, JoinedDate = contract.StartDate, CreatedBy = owner.Id },
            new RoomTenant { ContractId = contract.Id, RoomId = room.Id, TenantProfileId = tenant.Id, MoveInDate = contract.StartDate, CreatedBy = owner.Id },
            new ContractService { ContractId = contract.Id, PropertyServiceId = internet.Id, ServiceNameSnapshot = internet.Name, UnitSnapshot = internet.Unit, CalculationType = internet.CalculationType, UnitPrice = internet.UnitPrice, CreatedBy = owner.Id },
            new ContractService { ContractId = contract.Id, PropertyServiceId = waste.Id, ServiceNameSnapshot = waste.Name, UnitSnapshot = waste.Unit, CalculationType = waste.CalculationType, UnitPrice = waste.UnitPrice, CreatedBy = owner.Id });

        var previousPeriod = today.AddMonths(-1);
        db.MeterReadings.AddRange(
            new MeterReading { RoomId = room.Id, ContractId = contract.Id, MeterType = MeterType.Electricity, BillingYear = previousPeriod.Year, BillingMonth = previousPeriod.Month, PreviousReading = 100, CurrentReading = 145, Consumption = 45, ReadAt = now.AddMonths(-1), CreatedBy = owner.Id },
            new MeterReading { RoomId = room.Id, ContractId = contract.Id, MeterType = MeterType.Water, BillingYear = previousPeriod.Year, BillingMonth = previousPeriod.Month, PreviousReading = 20, CurrentReading = 24, Consumption = 4, ReadAt = now.AddMonths(-1), CreatedBy = owner.Id });
        await db.SaveChangesAsync(cancellationToken);

        var invoice = new Invoice { InvoiceNumber = $"INV-{previousPeriod:yyyyMM}-001", ContractId = contract.Id, RoomId = room.Id, BillingYear = previousPeriod.Year, BillingMonth = previousPeriod.Month, DueDate = new DateOnly(previousPeriod.Year, previousPeriod.Month, 10), SubtotalAmount = 3_191_000, TotalAmount = 3_191_000, PaidAmount = 1_500_000, RemainingAmount = 1_691_000, Status = InvoiceStatus.PartiallyPaid, IssuedAt = now.AddDays(-20), CreatedBy = owner.Id };
        db.Invoices.Add(invoice); await db.SaveChangesAsync(cancellationToken);
        db.InvoiceDetails.AddRange(
            new InvoiceDetail { InvoiceId = invoice.Id, Description = "Tiền phòng", Quantity = 1, Unit = "tháng", UnitPrice = 2_800_000, Amount = 2_800_000, SourceType = InvoiceDetailSourceType.Rent, CreatedBy = owner.Id },
            new InvoiceDetail { InvoiceId = invoice.Id, Description = "Điện", Quantity = 45, Unit = "kWh", UnitPrice = 3_800, Amount = 171_000, SourceType = InvoiceDetailSourceType.Electricity, CreatedBy = owner.Id },
            new InvoiceDetail { InvoiceId = invoice.Id, Description = "Nước", Quantity = 4, Unit = "m³", UnitPrice = 25_000, Amount = 100_000, SourceType = InvoiceDetailSourceType.Water, CreatedBy = owner.Id },
            new InvoiceDetail { InvoiceId = invoice.Id, Description = "Internet", Quantity = 1, Unit = "phòng", UnitPrice = 120_000, Amount = 120_000, SourceType = InvoiceDetailSourceType.Service, SourceReferenceId = internet.Id, CreatedBy = owner.Id });
        db.Payments.Add(new Payment { InvoiceId = invoice.Id, Amount = 1_500_000, Method = PaymentMethod.BankTransfer, Status = PaymentStatus.Confirmed, ReferenceCode = "DEV-FICTIONAL-001", PaidAt = now.AddDays(-10), ConfirmedAt = now.AddDays(-10), ConfirmedBy = owner.Id, CreatedBy = tenantUser.Id });
        db.MaintenanceRequests.Add(new MaintenanceRequest { BoardingHouseId = property.Id, RoomId = room.Id, TenantProfileId = tenant.Id, Category = "Nước", Title = "Vòi rửa bị rò nước", Description = "Dữ liệu yêu cầu sửa chữa hư cấu dùng cho môi trường phát triển.", Priority = MaintenancePriority.Medium, Status = MaintenanceStatus.New, CreatedBy = tenantUser.Id });
        db.Notifications.AddRange(
            new Notification { UserId = tenantUser.Id, Type = NotificationType.InvoiceIssued, Title = "Hóa đơn mới", Message = $"Hóa đơn {invoice.InvoiceNumber} đã được phát hành.", LinkUrl = $"/Tenant/Invoices/{invoice.Id}", CreatedBy = owner.Id },
            new Notification { UserId = owner.Id, Type = NotificationType.MaintenanceUpdated, Title = "Yêu cầu sửa chữa mới", Message = "Phòng A101 vừa gửi yêu cầu sửa chữa.", LinkUrl = "/Maintenance", CreatedBy = tenantUser.Id });
        db.AuditLogs.Add(new AuditLog { UserId = owner.Id, Action = "SeedDevelopmentData", EntityType = "BoardingHouse", EntityId = property.Id.ToString(), Timestamp = now, Summary = "Tạo dữ liệu phát triển hư cấu." });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<ApplicationUser?> CreateConfiguredUserAsync(string emailVariable, string passwordVariable, string displayName, string role)
    {
        var email = Environment.GetEnvironmentVariable(emailVariable); var password = Environment.GetEnvironmentVariable(passwordVariable);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return null;
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true, DisplayName = displayName, IsActive = true };
            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded) throw new InvalidOperationException($"Không thể tạo seed user {emailVariable}: {string.Join(", ", result.Errors.Select(x => x.Code))}");
        }
        if (!await users.IsInRoleAsync(user, role)) await users.AddToRoleAsync(user, role);
        return user;
    }
}
