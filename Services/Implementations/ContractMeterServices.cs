using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Models.ViewModels;
using TroiSinhVien.Services.Common;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Services.Implementations;

public sealed class ContractService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IContractService
{
    private Guid OwnerId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<IReadOnlyList<ContractListItem>> ListAsync(CancellationToken ct = default)
    {
        var owner = OwnerId; return await db.Contracts.AsNoTracking().Where(x => x.Room.BoardingHouse.OwnerId == owner).OrderByDescending(x => x.StartDate).Select(x => new ContractListItem(x.Id, x.ContractCode, x.Room.RoomCode, x.RepresentativeTenant.FullName, x.StartDate, x.EndDate, x.MonthlyRent, x.Status)).ToListAsync(ct);
    }

    public async Task<ContractDetailsViewModel?> DetailsAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId;
        var contract = await db.Contracts.AsNoTracking().Include(x => x.Room).ThenInclude(x => x.BoardingHouse).Include(x => x.RepresentativeTenant)
            .Include(x => x.Members).ThenInclude(x => x.TenantProfile).Include(x => x.Services)
            .FirstOrDefaultAsync(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner, ct);
        if (contract is null) return null;
        var memberIds = contract.Members.Select(x => x.TenantProfileId).ToArray();
        var serviceIds = contract.Services.Select(x => x.PropertyServiceId).ToArray();
        var tenants = contract.Status == ContractStatus.Draft
            ? await db.TenantProfiles.AsNoTracking().Where(x => x.OwnerId == owner && x.IsActive && !memberIds.Contains(x.Id)).OrderBy(x => x.FullName).Select(x => new SelectOption(x.Id, x.FullName + " · " + x.PhoneNumber)).ToListAsync(ct)
            : [];
        var services = contract.Status is ContractStatus.Draft or ContractStatus.Active
            ? await db.PropertyServices.AsNoTracking().Where(x => x.BoardingHouseId == contract.Room.BoardingHouseId && x.IsActive && !serviceIds.Contains(x.Id)).OrderBy(x => x.Name).Select(x => new SelectOption(x.Id, x.Name + " · " + x.UnitPrice + " / " + x.Unit)).ToListAsync(ct)
            : [];
        return new()
        {
            Id = contract.Id, ContractCode = contract.ContractCode, BoardingHouseName = contract.Room.BoardingHouse.Name, RoomCode = contract.Room.RoomCode,
            RepresentativeTenantName = contract.RepresentativeTenant.FullName, StartDate = contract.StartDate, EndDate = contract.EndDate,
            MonthlyRent = contract.MonthlyRent, DepositAmount = contract.DepositAmount, ElectricityPrice = contract.ElectricityPrice, WaterPrice = contract.WaterPrice,
            PaymentDueDay = contract.PaymentDueDay, Status = contract.Status,
            Members = contract.Members.OrderByDescending(x => x.IsRepresentative).ThenBy(x => x.TenantProfile.FullName).Select(x => new ContractMemberListItem(x.TenantProfileId, x.TenantProfile.FullName, x.IsRepresentative, x.JoinedDate, x.LeftDate)).ToList(),
            Services = contract.Services.OrderBy(x => x.ServiceNameSnapshot).Select(x => new ContractServiceListItem(x.PropertyServiceId, x.ServiceNameSnapshot, x.UnitSnapshot, x.CalculationType, x.Quantity, x.UnitPrice, x.IsActive)).ToList(),
            AvailableTenants = tenants, AvailableServices = services
        };
    }

    public async Task<ServiceResult<int>> CreateDraftAsync(ContractInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; if (input.EndDate <= input.StartDate) return ServiceResult<int>.Failure("validation", "Ngày kết thúc phải sau ngày bắt đầu.");
        var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == input.RoomId && x.BoardingHouse.OwnerId == owner, ct); if (room is null) return ServiceResult<int>.Failure("not_found", "Không tìm thấy phòng.");
        if (!await db.TenantProfiles.AnyAsync(x => x.Id == input.RepresentativeTenantId && x.OwnerId == owner, ct)) return ServiceResult<int>.Failure("not_found", "Không tìm thấy người thuê.");
        var code = input.ContractCode.Trim().ToUpperInvariant(); if (await db.Contracts.AnyAsync(x => x.ContractCode == code, ct)) return ServiceResult<int>.Failure("conflict", "Mã hợp đồng đã tồn tại.");
        var entity = new Contract { ContractCode = code, RoomId = input.RoomId, RepresentativeTenantId = input.RepresentativeTenantId, StartDate = input.StartDate, EndDate = input.EndDate, MonthlyRent = input.MonthlyRent, DepositAmount = input.DepositAmount, ElectricityPrice = input.ElectricityPrice, WaterPrice = input.WaterPrice, PaymentDueDay = input.PaymentDueDay, Terms = input.Terms?.Trim(), Status = ContractStatus.Draft, CreatedBy = owner };
        db.Add(entity); await db.SaveChangesAsync(ct); db.ContractMembers.Add(new ContractMember { ContractId = entity.Id, TenantProfileId = entity.RepresentativeTenantId, IsRepresentative = true, JoinedDate = entity.StartDate, CreatedBy = owner }); Audit("CreateDraft", entity.Id, $"Tạo nháp hợp đồng {code}.", owner); await db.SaveChangesAsync(ct); return ServiceResult<int>.Success(entity.Id);
    }

    public async Task<ServiceResult> AddMemberAsync(int contractId, int tenantId, CancellationToken ct = default)
    {
        var owner = OwnerId; var contract = await db.Contracts.FirstOrDefaultAsync(x => x.Id == contractId && x.Room.BoardingHouse.OwnerId == owner, ct); if (contract is null) return ServiceResult.Failure("not_found", "Không tìm thấy hợp đồng."); if (contract.Status != ContractStatus.Draft) return ServiceResult.Failure("conflict", "Chỉ hợp đồng nháp được thêm thành viên.");
        if (!await db.TenantProfiles.AnyAsync(x => x.Id == tenantId && x.OwnerId == owner, ct)) return ServiceResult.Failure("not_found", "Không tìm thấy người thuê."); if (await db.ContractMembers.AnyAsync(x => x.ContractId == contractId && x.TenantProfileId == tenantId, ct)) return ServiceResult.Failure("conflict", "Người thuê đã có trong hợp đồng.");
        db.ContractMembers.Add(new ContractMember { ContractId = contractId, TenantProfileId = tenantId, JoinedDate = contract.StartDate, CreatedBy = owner }); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }

    public async Task<ServiceResult> ActivateAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId; await using var tx = await db.Database.BeginTransactionAsync(ct);
        var contract = await db.Contracts.Include(x => x.Room).Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner, ct); if (contract is null) return ServiceResult.Failure("not_found", "Không tìm thấy hợp đồng.");
        if (contract.Status != ContractStatus.Draft || contract.Room.Status != RoomStatus.Available) return ServiceResult.Failure("conflict", "Hợp đồng phải ở trạng thái nháp và phòng phải còn trống.");
        if (contract.Members.Count == 0 || contract.Members.Count > contract.Room.MaximumOccupants) return ServiceResult.Failure("conflict", "Số thành viên hợp đồng không hợp lệ so với sức chứa của phòng.");
        var overlap = await db.Contracts.AnyAsync(x => x.Id != id && x.RoomId == contract.RoomId && x.Status == ContractStatus.Active && x.StartDate <= contract.EndDate && x.EndDate >= contract.StartDate, ct); if (overlap) return ServiceResult.Failure("conflict", "Phòng đã có hợp đồng giao nhau.");
        var tenantIds = contract.Members.Select(x => x.TenantProfileId).ToArray(); if (await db.RoomTenants.AnyAsync(x => tenantIds.Contains(x.TenantProfileId) && x.MoveOutDate == null, ct)) return ServiceResult.Failure("conflict", "Một thành viên đang cư trú tại phòng khác.");
        contract.Status = ContractStatus.Active; contract.ActivatedAt = clock.UtcNow; contract.UpdatedBy = owner; contract.Room.Status = RoomStatus.Occupied; contract.Room.UpdatedBy = owner;
        foreach (var member in contract.Members) db.RoomTenants.Add(new RoomTenant { RoomId = contract.RoomId, ContractId = contract.Id, TenantProfileId = member.TenantProfileId, MoveInDate = contract.StartDate, CreatedBy = owner });
        Audit("Activate", id, $"Kích hoạt hợp đồng {contract.ContractCode}.", owner); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return ServiceResult.Success();
    }

    public async Task<ServiceResult> ExtendAsync(int id, DateOnly newEndDate, CancellationToken ct = default)
    {
        var owner = OwnerId; var c = await db.Contracts.FirstOrDefaultAsync(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner, ct); if (c is null) return ServiceResult.Failure("not_found", "Không tìm thấy hợp đồng."); if (c.Status != ContractStatus.Active || newEndDate <= c.EndDate) return ServiceResult.Failure("validation", "Ngày gia hạn phải sau ngày kết thúc hiện tại của hợp đồng hiệu lực.");
        if (await db.Contracts.AnyAsync(x => x.Id != id && x.RoomId == c.RoomId && x.Status == ContractStatus.Active && x.StartDate <= newEndDate && x.EndDate >= c.StartDate, ct)) return ServiceResult.Failure("conflict", "Thời gian gia hạn giao với hợp đồng khác."); c.EndDate = newEndDate; c.UpdatedBy = owner; Audit("Extend", id, $"Gia hạn hợp đồng {c.ContractCode}.", owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }

    public async Task<ServiceResult> EndAsync(int id, bool completeMoveOut, CancellationToken ct = default)
    {
        var owner = OwnerId; await using var tx = await db.Database.BeginTransactionAsync(ct); var c = await db.Contracts.Include(x => x.Room).Include(x => x.RoomTenants).FirstOrDefaultAsync(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner, ct); if (c is null) return ServiceResult.Failure("not_found", "Không tìm thấy hợp đồng."); if (c.Status != ContractStatus.Active) return ServiceResult.Failure("conflict", "Chỉ hợp đồng hiệu lực mới có thể kết thúc.");
        if (await db.Invoices.AnyAsync(x => x.ContractId == id && x.RemainingAmount > 0 && x.Status != InvoiceStatus.Cancelled, ct)) return ServiceResult.Failure("conflict", "Hợp đồng còn hóa đơn chưa thanh toán.");
        c.Status = ContractStatus.Ended; c.EndedAt = clock.UtcNow; c.UpdatedBy = owner;
        var date = clock.UtcToday; foreach (var r in c.RoomTenants.Where(x => x.MoveOutDate == null)) { r.MoveOutDate = date; r.UpdatedBy = owner; }
        c.Room.Status = RoomStatus.Available; c.Room.UpdatedBy = owner;
        Audit("MoveOut", id, $"Kết thúc hợp đồng và trả phòng {c.ContractCode}.", owner); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return ServiceResult.Success();
    }

    public async Task<ServiceResult> CancelAsync(int id, string reason, CancellationToken ct = default)
    {
        var owner = OwnerId; if (string.IsNullOrWhiteSpace(reason)) return ServiceResult.Failure("validation", "Phải nhập lý do hủy.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var c = await db.Contracts.Include(x => x.Room).Include(x => x.RoomTenants).FirstOrDefaultAsync(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner, ct); if (c is null) return ServiceResult.Failure("not_found", "Không tìm thấy hợp đồng."); if (c.Status is ContractStatus.Ended or ContractStatus.Cancelled) return ServiceResult.Failure("conflict", "Hợp đồng không thể hủy ở trạng thái hiện tại."); if (await db.Invoices.AnyAsync(x => x.ContractId == id && x.Status != InvoiceStatus.Cancelled, ct)) return ServiceResult.Failure("conflict", "Không thể hủy hợp đồng đã có hóa đơn.");
        var wasActive = c.Status == ContractStatus.Active;
        c.Status = ContractStatus.Cancelled; c.CancellationReason = reason.Trim(); c.EndedAt = wasActive ? clock.UtcNow : c.EndedAt; c.UpdatedBy = owner;
        if (wasActive)
        {
            foreach (var residence in c.RoomTenants.Where(x => x.MoveOutDate == null)) { residence.MoveOutDate = clock.UtcToday; residence.UpdatedBy = owner; }
            c.Room.Status = RoomStatus.Available; c.Room.UpdatedBy = owner;
        }
        Audit("Cancel", id, $"Hủy hợp đồng {c.ContractCode}.", owner); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return ServiceResult.Success();
    }
    private void Audit(string action, int id, string summary, Guid owner) => db.AuditLogs.Add(new() { UserId = owner, Action = action, EntityType = nameof(Contract), EntityId = id.ToString(), Timestamp = clock.UtcNow, Summary = summary });
}

public sealed class ServiceCatalogService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IServiceCatalogService
{
    private Guid OwnerId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<PropertyServiceInputModel?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId;
        return await db.PropertyServices.AsNoTracking().Where(x => x.Id == id && x.BoardingHouse.OwnerId == owner)
            .Select(x => new PropertyServiceInputModel { BoardingHouseId = x.BoardingHouseId, Name = x.Name, Unit = x.Unit, CalculationType = x.CalculationType, UnitPrice = x.UnitPrice }).FirstOrDefaultAsync(ct);
    }
    public async Task<IReadOnlyList<PropertyServiceListItem>> ListAsync(int? propertyId, CancellationToken ct = default)
    {
        var owner = OwnerId; var query = db.PropertyServices.AsNoTracking().Where(x => x.BoardingHouse.OwnerId == owner); if (propertyId is > 0) query = query.Where(x => x.BoardingHouseId == propertyId);
        return await query.OrderBy(x => x.BoardingHouse.Name).ThenBy(x => x.Name).Select(x => new PropertyServiceListItem(x.Id, x.BoardingHouseId, x.BoardingHouse.Name, x.Name, x.Unit, x.CalculationType, x.UnitPrice, x.IsActive)).ToListAsync(ct);
    }
    public async Task<ServiceResult<int>> CreateAsync(PropertyServiceInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; if (!await db.BoardingHouses.AnyAsync(x => x.Id == input.BoardingHouseId && x.OwnerId == owner, ct)) return ServiceResult<int>.Failure("not_found", "Không tìm thấy khu trọ."); var name = input.Name.Trim(); if (await db.PropertyServices.AnyAsync(x => x.BoardingHouseId == input.BoardingHouseId && x.Name.ToLower() == name.ToLower(), ct)) return ServiceResult<int>.Failure("conflict", "Dịch vụ đã tồn tại.");
        var e = new PropertyService { BoardingHouseId = input.BoardingHouseId, Name = name, Unit = input.Unit.Trim(), CalculationType = input.CalculationType, UnitPrice = input.UnitPrice, CreatedBy = owner }; db.Add(e); await db.SaveChangesAsync(ct); return ServiceResult<int>.Success(e.Id);
    }
    public async Task<ServiceResult> UpdateAsync(int id, PropertyServiceInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; var entity = await db.PropertyServices.FirstOrDefaultAsync(x => x.Id == id && x.BoardingHouse.OwnerId == owner, ct);
        if (entity is null || !await db.BoardingHouses.AnyAsync(x => x.Id == input.BoardingHouseId && x.OwnerId == owner, ct)) return ServiceResult.Failure("not_found", "Không tìm thấy dịch vụ hoặc khu trọ.");
        if (entity.BoardingHouseId != input.BoardingHouseId && await db.ContractServices.AnyAsync(x => x.PropertyServiceId == id, ct)) return ServiceResult.Failure("conflict", "Không thể chuyển dịch vụ đã được dùng sang khu trọ khác.");
        var name = input.Name.Trim(); if (await db.PropertyServices.AnyAsync(x => x.Id != id && x.BoardingHouseId == input.BoardingHouseId && x.Name.ToLower() == name.ToLower(), ct)) return ServiceResult.Failure("conflict", "Dịch vụ đã tồn tại.");
        entity.BoardingHouseId = input.BoardingHouseId; entity.Name = name; entity.Unit = input.Unit.Trim(); entity.CalculationType = input.CalculationType; entity.UnitPrice = input.UnitPrice; entity.UpdatedBy = owner;
        db.AuditLogs.Add(new AuditLog { UserId = owner, Action = "Update", EntityType = nameof(PropertyService), EntityId = id.ToString(), Timestamp = clock.UtcNow, Summary = $"Cập nhật dịch vụ {name}." }); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }
    public async Task<ServiceResult> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var owner = OwnerId; var entity = await db.PropertyServices.FirstOrDefaultAsync(x => x.Id == id && x.BoardingHouse.OwnerId == owner, ct); if (entity is null) return ServiceResult.Failure("not_found", "Không tìm thấy dịch vụ.");
        entity.IsActive = isActive; entity.UpdatedBy = owner; db.AuditLogs.Add(new AuditLog { UserId = owner, Action = isActive ? "Activate" : "Deactivate", EntityType = nameof(PropertyService), EntityId = id.ToString(), Timestamp = clock.UtcNow, Summary = $"{(isActive ? "Kích hoạt" : "Ngừng")} dịch vụ {entity.Name}." }); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }
    public async Task<ServiceResult> AddToContractAsync(AddContractServiceInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId;
        var contract = await db.Contracts.Include(x => x.Room).FirstOrDefaultAsync(x => x.Id == input.ContractId && x.Room.BoardingHouse.OwnerId == owner, ct);
        if (contract is null) return ServiceResult.Failure("not_found", "Không tìm thấy hợp đồng.");
        if (contract.Status is not (ContractStatus.Draft or ContractStatus.Active)) return ServiceResult.Failure("conflict", "Không thể thêm dịch vụ vào hợp đồng đã kết thúc.");
        var service = await db.PropertyServices.FirstOrDefaultAsync(x => x.Id == input.PropertyServiceId && x.BoardingHouseId == contract.Room.BoardingHouseId && x.BoardingHouse.OwnerId == owner && x.IsActive, ct);
        if (service is null) return ServiceResult.Failure("not_found", "Không tìm thấy dịch vụ của khu trọ.");
        if (await db.ContractServices.AnyAsync(x => x.ContractId == contract.Id && x.PropertyServiceId == service.Id, ct)) return ServiceResult.Failure("conflict", "Dịch vụ đã có trong hợp đồng.");
        var quantity = service.CalculationType is ServiceCalculationType.PerUnit or ServiceCalculationType.Custom ? input.Quantity : 1;
        db.ContractServices.Add(new TroiSinhVien.Domain.Entities.ContractService { ContractId = contract.Id, PropertyServiceId = service.Id, ServiceNameSnapshot = service.Name, UnitSnapshot = service.Unit, CalculationType = service.CalculationType, Quantity = quantity, UnitPrice = input.UnitPriceOverride ?? service.UnitPrice, CreatedBy = owner });
        db.AuditLogs.Add(new AuditLog { UserId = owner, Action = "AddService", EntityType = nameof(Contract), EntityId = contract.Id.ToString(), Timestamp = clock.UtcNow, Summary = $"Thêm dịch vụ {service.Name} vào hợp đồng {contract.ContractCode}." });
        await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }
}

public sealed class MeterReadingService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IMeterReadingService
{
    private Guid OwnerId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<MeterReadingInputModel?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId; return await db.MeterReadings.AsNoTracking().Where(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner).Select(x => new MeterReadingInputModel { RoomId = x.RoomId, MeterType = x.MeterType, BillingYear = x.BillingYear, BillingMonth = x.BillingMonth, CurrentReading = x.CurrentReading }).FirstOrDefaultAsync(ct);
    }
    public async Task<IReadOnlyList<MeterReadingListItem>> ListAsync(int propertyId, CancellationToken ct = default)
    {
        var owner = OwnerId; var query = db.MeterReadings.AsNoTracking().Where(x => x.Room.BoardingHouse.OwnerId == owner); if (propertyId > 0) query = query.Where(x => x.Room.BoardingHouseId == propertyId); return await query.OrderByDescending(x => x.BillingYear).ThenByDescending(x => x.BillingMonth).ThenBy(x => x.Room.RoomCode).Select(x => new MeterReadingListItem(x.Id, x.Room.RoomCode, x.MeterType, x.BillingYear, x.BillingMonth, x.PreviousReading, x.CurrentReading, x.Consumption)).ToListAsync(ct);
    }
    public async Task<ServiceResult<int>> CreateAsync(MeterReadingInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == input.RoomId && x.BoardingHouse.OwnerId == owner, ct); if (room is null) return ServiceResult<int>.Failure("not_found", "Không tìm thấy phòng."); if (await db.MeterReadings.AnyAsync(x => x.RoomId == input.RoomId && x.MeterType == input.MeterType && x.BillingYear == input.BillingYear && x.BillingMonth == input.BillingMonth, ct)) return ServiceResult<int>.Failure("conflict", "Đã có chỉ số cho phòng, loại công tơ và kỳ này.");
        var first = new DateOnly(input.BillingYear, input.BillingMonth, 1); var last = first.AddMonths(1).AddDays(-1); var contract = await db.Contracts.FirstOrDefaultAsync(x => x.RoomId == input.RoomId && x.Status == ContractStatus.Active && x.StartDate <= last && x.EndDate >= first, ct); if (contract is null) return ServiceResult<int>.Failure("conflict", "Không có hợp đồng hiệu lực trong kỳ ghi chỉ số.");
        var previous = await db.MeterReadings.Where(x => x.RoomId == input.RoomId && x.MeterType == input.MeterType && (x.BillingYear < input.BillingYear || x.BillingYear == input.BillingYear && x.BillingMonth < input.BillingMonth)).OrderByDescending(x => x.BillingYear).ThenByDescending(x => x.BillingMonth).Select(x => (decimal?)x.CurrentReading).FirstOrDefaultAsync(ct) ?? 0;
        if (input.CurrentReading < previous) return ServiceResult<int>.Failure("validation", $"Chỉ số mới không được nhỏ hơn chỉ số trước ({previous})."); var e = new MeterReading { RoomId = room.Id, ContractId = contract.Id, MeterType = input.MeterType, BillingYear = input.BillingYear, BillingMonth = input.BillingMonth, PreviousReading = previous, CurrentReading = input.CurrentReading, Consumption = input.CurrentReading - previous, ReadAt = clock.UtcNow, CreatedBy = owner }; db.Add(e); await db.SaveChangesAsync(ct); return ServiceResult<int>.Success(e.Id);
    }
    public async Task<ServiceResult> UpdateAsync(int id, MeterReadingInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; var reading = await db.MeterReadings.FirstOrDefaultAsync(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner, ct); if (reading is null) return ServiceResult.Failure("not_found", "Không tìm thấy chỉ số.");
        if (reading.RoomId != input.RoomId || reading.MeterType != input.MeterType || reading.BillingYear != input.BillingYear || reading.BillingMonth != input.BillingMonth) return ServiceResult.Failure("validation", "Không thể đổi phòng, loại công tơ hoặc kỳ của chỉ số đã ghi.");
        var previous = await db.MeterReadings.Where(x => x.Id != id && x.RoomId == reading.RoomId && x.MeterType == reading.MeterType && (x.BillingYear < reading.BillingYear || x.BillingYear == reading.BillingYear && x.BillingMonth < reading.BillingMonth)).OrderByDescending(x => x.BillingYear).ThenByDescending(x => x.BillingMonth).FirstOrDefaultAsync(ct);
        var next = await db.MeterReadings.Where(x => x.Id != id && x.RoomId == reading.RoomId && x.MeterType == reading.MeterType && (x.BillingYear > reading.BillingYear || x.BillingYear == reading.BillingYear && x.BillingMonth > reading.BillingMonth)).OrderBy(x => x.BillingYear).ThenBy(x => x.BillingMonth).FirstOrDefaultAsync(ct);
        var protectedIds = next is null ? new[] { reading.Id } : new[] { reading.Id, next.Id };
        if (await db.InvoiceDetails.AnyAsync(x => x.SourceReferenceId.HasValue && protectedIds.Contains(x.SourceReferenceId.Value) && (x.SourceType == InvoiceDetailSourceType.Electricity || x.SourceType == InvoiceDetailSourceType.Water) && x.Invoice.Status != InvoiceStatus.Cancelled, ct)) return ServiceResult.Failure("conflict", "Không thể sửa chỉ số đã được dùng trong hóa đơn.");
        var previousValue = previous?.CurrentReading ?? 0; if (input.CurrentReading < previousValue) return ServiceResult.Failure("validation", $"Chỉ số mới không được nhỏ hơn chỉ số trước ({previousValue})."); if (next is not null && input.CurrentReading > next.CurrentReading) return ServiceResult.Failure("validation", $"Chỉ số không được lớn hơn chỉ số kỳ sau ({next.CurrentReading}).");
        reading.PreviousReading = previousValue; reading.CurrentReading = input.CurrentReading; reading.Consumption = input.CurrentReading - previousValue; reading.ReadAt = clock.UtcNow; reading.UpdatedBy = owner;
        if (next is not null) { next.PreviousReading = input.CurrentReading; next.Consumption = next.CurrentReading - input.CurrentReading; next.UpdatedBy = owner; }
        db.AuditLogs.Add(new AuditLog { UserId = owner, Action = "Update", EntityType = nameof(MeterReading), EntityId = id.ToString(), Timestamp = clock.UtcNow, Summary = $"Cập nhật chỉ số {reading.MeterType} phòng {reading.RoomId}, kỳ {reading.BillingMonth}/{reading.BillingYear}." }); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }
}
