using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TroiSinhVien.Infrastructure.Storage;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Services.Dashboard;
using TroiSinhVien.Services.Implementations;
using ContractApplicationService = TroiSinhVien.Services.Implementations.ContractService;

namespace TroiSinhVien.Tests;

public sealed class CompletionTests
{
    [Fact]
    public async Task Cancelling_active_contract_closes_residence_and_releases_room()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant);
        var result = await new ContractApplicationService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock).CancelAsync(contract.Id, "Hai bên thống nhất");
        Assert.True(result.Succeeded); Assert.Equal(ContractStatus.Cancelled, contract.Status); Assert.Equal(RoomStatus.Available, core.Room.Status); Assert.NotNull((await t.Db.RoomTenants.SingleAsync()).MoveOutDate);
    }

    [Fact]
    public async Task Cancelling_draft_does_not_release_room_used_by_active_contract()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); await t.SeedActiveContractAsync(core.Room, core.Tenant);
        var other = new TenantProfile { OwnerId = t.OwnerId, FullName = "Người thứ hai", PhoneNumber = "0900000002" }; t.Db.Add(other); await t.Db.SaveChangesAsync();
        var service = new ContractApplicationService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); var draft = await service.CreateDraftAsync(Input(core.Room.Id, other.Id, "HD-DRAFT"));
        Assert.True((await service.CancelAsync(draft.Value, "Không tiếp tục")).Succeeded); Assert.Equal(RoomStatus.Occupied, core.Room.Status); Assert.Null((await t.Db.RoomTenants.SingleAsync()).MoveOutDate);
    }

    [Fact]
    public async Task New_invoice_does_not_duplicate_prior_invoice_debt()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant); var service = new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock);
        var june = await service.GenerateAsync(new() { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 6 }); await service.IssueAsync(june.Value);
        var july = await service.GenerateAsync(new() { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 7 });
        var current = await t.Db.Invoices.SingleAsync(x => x.Id == july.Value); Assert.Equal(0, current.PreviousDebtAmount); Assert.Equal(contract.MonthlyRent, current.TotalAmount);
        var details = await service.DetailsOwnerAsync(july.Value); Assert.Equal(contract.MonthlyRent, details!.PriorOutstandingDebt); Assert.Equal(contract.MonthlyRent * 2, details.AccountOutstandingDebt);
    }

    [Fact]
    public async Task Pending_tenant_payments_cannot_exceed_invoice_balance_in_aggregate()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant); var invoices = new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); var created = await invoices.GenerateAsync(new() { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 7 }); await invoices.IssueAsync(created.Value);
        var service = new PaymentService(t.Db, new FakeCurrentUser(t.TenantUserId), t.Clock, new MemoryStorage());
        Assert.True((await service.SubmitTenantAsync(Payment(created.Value, 1_500_000))).Succeeded);
        var second = await service.SubmitTenantAsync(Payment(created.Value, 600_000)); Assert.False(second.Succeeded); Assert.Equal("conflict", second.ErrorCode);
    }

    [Fact]
    public async Task Evidence_is_available_only_to_owner_or_contract_tenant()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant);
        var invoice = new Invoice { InvoiceNumber = "INV-EVIDENCE", ContractId = contract.Id, RoomId = core.Room.Id, BillingYear = 2026, BillingMonth = 7, DueDate = new(2026, 7, 10), SubtotalAmount = 100, TotalAmount = 100, RemainingAmount = 100, Status = InvoiceStatus.Issued };
        var file = new UploadedFile { StoredName = "safe.png", OriginalName = "proof.png", ContentType = "image/png", Size = 3, RelativePath = "safe.png", UploadedBy = t.TenantUserId };
        t.Db.AddRange(invoice, file); await t.Db.SaveChangesAsync(); var payment = new Payment { InvoiceId = invoice.Id, Amount = 50, Method = PaymentMethod.BankTransfer, Status = PaymentStatus.Pending, EvidenceFileId = file.Id, PaidAt = t.Clock.UtcNow, CreatedBy = t.TenantUserId }; t.Db.Add(payment); await t.Db.SaveChangesAsync();
        Assert.True((await new PaymentService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock, new MemoryStorage()).GetEvidenceAsync(payment.Id)).Succeeded);
        Assert.True((await new PaymentService(t.Db, new FakeCurrentUser(t.TenantUserId), t.Clock, new MemoryStorage()).GetEvidenceAsync(payment.Id)).Succeeded);
        Assert.False((await new PaymentService(t.Db, new FakeCurrentUser(Guid.NewGuid()), t.Clock, new MemoryStorage()).GetEvidenceAsync(payment.Id)).Succeeded);
    }

    [Fact]
    public async Task Contract_service_snapshot_is_used_by_invoice_after_catalog_price_changes()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant);
        var catalog = new PropertyService { BoardingHouseId = core.Property.Id, Name = "Internet", Unit = "phòng", CalculationType = ServiceCalculationType.FixedPerRoom, UnitPrice = 100_000 }; t.Db.Add(catalog); await t.Db.SaveChangesAsync();
        var service = new ServiceCatalogService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); Assert.True((await service.AddToContractAsync(new() { ContractId = contract.Id, PropertyServiceId = catalog.Id })).Succeeded);
        catalog.UnitPrice = 999_000; await t.Db.SaveChangesAsync();
        var invoices = new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); var created = await invoices.GenerateAsync(new() { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 7 }); var invoice = await t.Db.Invoices.Include(x => x.Details).SingleAsync(x => x.Id == created.Value);
        Assert.Contains(invoice.Details, x => x.SourceType == InvoiceDetailSourceType.Service && x.UnitPrice == 100_000);
    }

    [Fact]
    public async Task Per_unit_contract_service_uses_negotiated_quantity_and_price_snapshot()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant);
        var catalog = new PropertyService { BoardingHouseId = core.Property.Id, Name = "Chỗ để xe", Unit = "xe", CalculationType = ServiceCalculationType.PerUnit, UnitPrice = 100_000 }; t.Db.Add(catalog); await t.Db.SaveChangesAsync();
        var services = new ServiceCatalogService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); Assert.True((await services.AddToContractAsync(new() { ContractId = contract.Id, PropertyServiceId = catalog.Id, Quantity = 2.5m, UnitPriceOverride = 80_000 })).Succeeded);
        var invoices = new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); var created = await invoices.GenerateAsync(new() { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 7 }); var detail = (await t.Db.Invoices.Include(x => x.Details).SingleAsync(x => x.Id == created.Value)).Details.Single(x => x.SourceType == InvoiceDetailSourceType.Service);
        Assert.Equal(2.5m, detail.Quantity); Assert.Equal(80_000, detail.UnitPrice); Assert.Equal(200_000, detail.Amount);
    }

    [Fact]
    public async Task Meter_edit_updates_the_chain_and_is_locked_after_invoice_usage()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant); var meter = new MeterReadingService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock);
        var june = await meter.CreateAsync(new() { RoomId = core.Room.Id, MeterType = MeterType.Electricity, BillingYear = 2026, BillingMonth = 6, CurrentReading = 100 }); var july = await meter.CreateAsync(new() { RoomId = core.Room.Id, MeterType = MeterType.Electricity, BillingYear = 2026, BillingMonth = 7, CurrentReading = 145 });
        Assert.True((await meter.UpdateAsync(june.Value, new() { RoomId = core.Room.Id, MeterType = MeterType.Electricity, BillingYear = 2026, BillingMonth = 6, CurrentReading = 120 })).Succeeded); var next = await t.Db.MeterReadings.FindAsync(july.Value); Assert.Equal(120, next!.PreviousReading); Assert.Equal(25, next.Consumption);
        await new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock).GenerateAsync(new() { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 7 }); Assert.False((await meter.UpdateAsync(june.Value, new() { RoomId = core.Room.Id, MeterType = MeterType.Electricity, BillingYear = 2026, BillingMonth = 6, CurrentReading = 110 })).Succeeded);
    }

    [Fact]
    public async Task Reports_and_dashboard_are_scoped_to_current_owner()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var otherProperty = new BoardingHouse { OwnerId = t.OtherOwnerId, Name = "Khu khác", Address = "Khác" }; t.Db.Add(otherProperty); await t.Db.SaveChangesAsync(); t.Db.Add(new Room { BoardingHouseId = otherProperty.Id, RoomCode = "X1", MaximumOccupants = 1 }); await t.Db.SaveChangesAsync();
        var dashboard = await new DashboardService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock).GetDashboardDataAsync(core.Property.Id, "07/2026"); Assert.Single(dashboard.Properties); Assert.Equal(1, dashboard.Hero.TotalRoomsCount);
        var report = await new ReportService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock).OccupancyAsync(new()); Assert.Single(report.Rows); Assert.Equal(core.Property.Id, report.Rows[0].BoardingHouseId);
    }

    [Fact]
    public async Task Operational_notifications_are_idempotent_and_update_overdue_status()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant); contract.EndDate = new(2026, 7, 20);
        var invoice = new Invoice { InvoiceNumber = "INV-DUE", ContractId = contract.Id, RoomId = core.Room.Id, BillingYear = 2026, BillingMonth = 7, DueDate = new(2026, 7, 10), SubtotalAmount = 500, TotalAmount = 500, RemainingAmount = 500, Status = InvoiceStatus.Issued }; t.Db.Add(invoice); await t.Db.SaveChangesAsync();
        var tenantNotifications = new NotificationService(t.Db, new FakeCurrentUser(t.TenantUserId), t.Clock); await tenantNotifications.GenerateOperationalAsync(); await tenantNotifications.GenerateOperationalAsync();
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status); Assert.Single(t.Db.Notifications.Where(x => x.UserId == t.TenantUserId && x.Type == NotificationType.InvoiceOverdue));
        var ownerNotifications = new NotificationService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); await ownerNotifications.GenerateOperationalAsync(); await ownerNotifications.GenerateOperationalAsync();
        Assert.Single(t.Db.Notifications.Where(x => x.UserId == t.OwnerId && x.Type == NotificationType.ContractExpiring));
    }

    [Fact]
    public async Task Maintenance_comments_are_scoped_and_visible_to_both_parties()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); await t.SeedActiveContractAsync(core.Room, core.Tenant);
        var tenant = new MaintenanceService(t.Db, new FakeCurrentUser(t.TenantUserId), t.Clock); var created = await tenant.CreateTenantAsync(new() { Category = "Điện", Title = "Đèn hỏng", Description = "Không bật được" });
        var owner = new MaintenanceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); Assert.True((await owner.AddCommentAsync(new() { RequestId = created.Value, Content = "Đã tiếp nhận" })).Succeeded);
        var details = await tenant.DetailsTenantAsync(created.Value); Assert.Single(details!.Comments); Assert.Equal("Chủ trọ", details.Comments[0].AuthorLabel);
        Assert.False((await new MaintenanceService(t.Db, new FakeCurrentUser(Guid.NewGuid()), t.Clock).AddCommentAsync(new() { RequestId = created.Value, Content = "Không hợp lệ" })).Succeeded);
    }

    [Fact]
    public async Task Bulk_invoice_generation_is_idempotent_for_a_property_period()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); await t.SeedActiveContractAsync(core.Room, core.Tenant); var service = new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock);
        var first = await service.BulkGenerateAsync(new() { BoardingHouseId = core.Property.Id, BillingYear = 2026, BillingMonth = 7 }); var second = await service.BulkGenerateAsync(new() { BoardingHouseId = core.Property.Id, BillingYear = 2026, BillingMonth = 7 });
        Assert.True(first.Succeeded); Assert.Equal(1, first.Value); Assert.True(second.Succeeded); Assert.Equal(0, second.Value); Assert.Single(t.Db.Invoices);
    }

    [Fact]
    public async Task File_storage_rejects_spoofed_image_content()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }; var stream = new MemoryStream(bytes); var file = new FormFile(stream, 0, bytes.Length, "Evidence", "fake.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };
        var storage = new LocalFileStorageService(new FakeWebHostEnvironment(), Options.Create(new FileStorageOptions { RootPath = "tro-tests", MaxEvidenceBytes = 1024 }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.SaveEvidenceAsync(file));
    }

    private static ContractInputModel Input(int roomId, int tenantId, string code) => new() { ContractCode = code, RoomId = roomId, RepresentativeTenantId = tenantId, StartDate = new(2026, 7, 1), EndDate = new(2027, 6, 30), MonthlyRent = 2_000_000, DepositAmount = 2_000_000, ElectricityPrice = 4_000, WaterPrice = 25_000, PaymentDueDay = 10 };
    private static RecordPaymentInputModel Payment(int invoiceId, decimal amount) { var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]); return new() { InvoiceId = invoiceId, Amount = amount, Method = PaymentMethod.BankTransfer, Evidence = new FormFile(stream, 0, stream.Length, "Evidence", "proof.png") { Headers = new HeaderDictionary(), ContentType = "image/png" } }; }
}
