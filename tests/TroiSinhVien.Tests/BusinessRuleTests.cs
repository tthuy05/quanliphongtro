using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Services.Implementations;

namespace TroiSinhVien.Tests;

public sealed class BusinessRuleTests
{
    [Fact]
    public async Task Room_code_is_unique_per_property_but_allowed_in_another_property()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var service = new RoomService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock);
        var duplicate = await service.CreateAsync(new RoomInputModel { BoardingHouseId = core.Property.Id, RoomCode = "a101", MonthlyRent = 1, MaximumOccupants = 1 });
        var other = new TroiSinhVien.Domain.Entities.BoardingHouse { OwnerId = t.OwnerId, Name = "Nhà trọ B", Address = "B" }; t.Db.Add(other); await t.Db.SaveChangesAsync(); var allowed = await service.CreateAsync(new RoomInputModel { BoardingHouseId = other.Id, RoomCode = "A101", MonthlyRent = 1, MaximumOccupants = 1 });
        Assert.False(duplicate.Succeeded); Assert.True(allowed.Succeeded);
    }

    [Fact]
    public async Task Owner_cannot_create_room_in_another_owners_property()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var other = new TroiSinhVien.Domain.Entities.BoardingHouse { OwnerId = t.OtherOwnerId, Name = "Other", Address = "Other" }; t.Db.Add(other); await t.Db.SaveChangesAsync(); var service = new RoomService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock);
        var result = await service.CreateAsync(new RoomInputModel { BoardingHouseId = other.Id, RoomCode = "X1", MaximumOccupants = 1 }); Assert.False(result.Succeeded); Assert.Equal("not_found", result.ErrorCode);
    }

    [Fact]
    public async Task Activating_contract_requires_available_room_and_changes_status()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var service = new ContractService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock);
        var draft = await service.CreateDraftAsync(Input(core.Room.Id, core.Tenant.Id)); Assert.True(draft.Succeeded); var activated = await service.ActivateAsync(draft.Value); Assert.True(activated.Succeeded); Assert.Equal(RoomStatus.Occupied, (await t.Db.Rooms.FindAsync(core.Room.Id))!.Status);
        var secondTenant = new TroiSinhVien.Domain.Entities.TenantProfile { OwnerId = t.OwnerId, FullName = "B", PhoneNumber = "2" }; t.Db.Add(secondTenant); await t.Db.SaveChangesAsync(); var second = await service.CreateDraftAsync(Input(core.Room.Id, secondTenant.Id, "HD-2")); var rejected = await service.ActivateAsync(second.Value); Assert.False(rejected.Succeeded);
    }

    [Fact]
    public async Task Contract_rejects_invalid_dates_and_overlapping_active_contract()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var service = new ContractService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock);
        var invalid = Input(core.Room.Id, core.Tenant.Id); invalid.EndDate = invalid.StartDate; Assert.False((await service.CreateDraftAsync(invalid)).Succeeded);
        var active = await t.SeedActiveContractAsync(core.Room, core.Tenant); core.Room.Status = RoomStatus.Available; await t.Db.SaveChangesAsync(); var otherTenant = new TroiSinhVien.Domain.Entities.TenantProfile { OwnerId = t.OwnerId, FullName = "C", PhoneNumber = "3" }; t.Db.Add(otherTenant); await t.Db.SaveChangesAsync(); var draft = await service.CreateDraftAsync(Input(core.Room.Id, otherTenant.Id, "HD-OVERLAP")); Assert.False((await service.ActivateAsync(draft.Value)).Succeeded);
    }

    [Fact]
    public async Task Move_out_changes_room_to_available_when_no_debt()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant); var service = new ContractService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); var result = await service.EndAsync(contract.Id, true); Assert.True(result.Succeeded); Assert.Equal(RoomStatus.Available, (await t.Db.Rooms.FindAsync(core.Room.Id))!.Status); Assert.NotNull((await t.Db.RoomTenants.SingleAsync()).MoveOutDate);
    }

    [Fact]
    public async Task Meter_reading_rejects_lower_and_duplicate_and_calculates_consumption()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); await t.SeedActiveContractAsync(core.Room, core.Tenant); var service = new MeterReadingService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock);
        Assert.True((await service.CreateAsync(new() { RoomId = core.Room.Id, MeterType = MeterType.Electricity, BillingYear = 2026, BillingMonth = 6, CurrentReading = 100 })).Succeeded);
        Assert.False((await service.CreateAsync(new() { RoomId = core.Room.Id, MeterType = MeterType.Electricity, BillingYear = 2026, BillingMonth = 7, CurrentReading = 90 })).Succeeded);
        var ok = await service.CreateAsync(new() { RoomId = core.Room.Id, MeterType = MeterType.Electricity, BillingYear = 2026, BillingMonth = 7, CurrentReading = 145 }); Assert.True(ok.Succeeded); Assert.Equal(45, (await t.Db.MeterReadings.FindAsync(ok.Value))!.Consumption);
        Assert.False((await service.CreateAsync(new() { RoomId = core.Room.Id, MeterType = MeterType.Electricity, BillingYear = 2026, BillingMonth = 7, CurrentReading = 150 })).Succeeded);
    }

    [Fact]
    public async Task Invoice_preserves_prices_calculates_total_and_rejects_duplicate_period()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant); var invoiceService = new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); var input = new GenerateInvoiceInputModel { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 7, DiscountAmount = 100_000 };
        var generated = await invoiceService.GenerateAsync(input); Assert.True(generated.Succeeded); var invoice = await t.Db.Invoices.Include(x => x.Details).SingleAsync(); Assert.Equal(1_900_000, invoice.TotalAmount); Assert.Equal(2_000_000, invoice.Details.Single().UnitPrice); core.Room.MonthlyRent = 9_000_000; await t.Db.SaveChangesAsync(); Assert.Equal(2_000_000, invoice.Details.Single().UnitPrice); Assert.False((await invoiceService.GenerateAsync(input)).Succeeded);
    }

    [Fact]
    public async Task Partial_and_full_payments_update_invoice_and_overpayment_is_rejected()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant); var invoices = new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); var generated = await invoices.GenerateAsync(new() { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 7 }); await invoices.IssueAsync(generated.Value); var payments = new PaymentService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock, new NullStorage());
        Assert.False((await payments.RecordOwnerAsync(new() { InvoiceId = generated.Value, Amount = 0 })).Succeeded); Assert.False((await payments.RecordOwnerAsync(new() { InvoiceId = generated.Value, Amount = 3_000_000 })).Succeeded); Assert.True((await payments.RecordOwnerAsync(new() { InvoiceId = generated.Value, Amount = 500_000 })).Succeeded); var invoice = await t.Db.Invoices.FindAsync(generated.Value); Assert.Equal(InvoiceStatus.Overdue, invoice!.Status); Assert.True((await payments.RecordOwnerAsync(new() { InvoiceId = generated.Value, Amount = invoice.RemainingAmount })).Succeeded); Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public async Task Tenant_invoice_query_is_isolated_and_maintenance_requires_active_residence()
    {
        await using var t = new TestDatabase(); await t.InitializeAsync(); var core = await t.SeedCoreAsync(); var contract = await t.SeedActiveContractAsync(core.Room, core.Tenant); var ownerInvoices = new InvoiceService(t.Db, new FakeCurrentUser(t.OwnerId), t.Clock); await ownerInvoices.GenerateAsync(new() { ContractId = contract.Id, BillingYear = 2026, BillingMonth = 7 }); var tenantInvoices = new InvoiceService(t.Db, new FakeCurrentUser(t.TenantUserId), t.Clock); Assert.Single(await tenantInvoices.ListTenantAsync()); Assert.Empty(await new InvoiceService(t.Db, new FakeCurrentUser(Guid.NewGuid()), t.Clock).ListTenantAsync());
        var maintenance = new MaintenanceService(t.Db, new FakeCurrentUser(t.TenantUserId), t.Clock); Assert.True((await maintenance.CreateTenantAsync(new() { Category = "Điện", Title = "Hỏng đèn", Description = "Mô tả" })).Succeeded); var outsider = new MaintenanceService(t.Db, new FakeCurrentUser(Guid.NewGuid()), t.Clock); Assert.False((await outsider.CreateTenantAsync(new() { Category = "Điện", Title = "X", Description = "Y" })).Succeeded);
    }

    private static ContractInputModel Input(int roomId, int tenantId, string code = "HD-1") => new() { ContractCode = code, RoomId = roomId, RepresentativeTenantId = tenantId, StartDate = new(2026, 7, 1), EndDate = new(2027, 6, 30), MonthlyRent = 2_000_000, DepositAmount = 2_000_000, ElectricityPrice = 4000, WaterPrice = 25000, PaymentDueDay = 10 };
}
