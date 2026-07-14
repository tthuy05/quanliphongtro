using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Models.ViewModels;
using TroiSinhVien.Services.Common;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Services.Implementations;

public sealed class BoardingHouseService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IBoardingHouseService
{
    private Guid OwnerId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<IReadOnlyList<BoardingHouseListItem>> ListAsync(CancellationToken ct = default) => await db.BoardingHouses.AsNoTracking().Where(x => x.OwnerId == OwnerId).OrderBy(x => x.Name).Select(x => new BoardingHouseListItem(x.Id, x.Name, x.Address, x.ContactPhone, x.Rooms.Count(), x.IsActive)).ToListAsync(ct);
    public async Task<BoardingHouseDetailsViewModel?> DetailsAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId; var property = await db.BoardingHouses.AsNoTracking().Include(x => x.Rooms).FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == owner, ct); if (property is null) return null;
        return new() { Property = new(property.Id, property.Name, property.Address, property.ContactPhone, property.Rooms.Count, property.IsActive), DefaultElectricityPrice = property.DefaultElectricityPrice, DefaultWaterPrice = property.DefaultWaterPrice, Rooms = property.Rooms.OrderBy(x => x.RoomCode).Select(x => new RoomListItem(x.Id, property.Id, property.Name, x.RoomCode, x.RoomName, x.Floor, x.MonthlyRent, x.Status, x.MaximumOccupants)).ToList() };
    }
    public async Task<BoardingHouseInputModel?> GetForEditAsync(int id, CancellationToken ct = default) { var owner = OwnerId; return await db.BoardingHouses.AsNoTracking().Where(x => x.Id == id && x.OwnerId == owner).Select(x => new BoardingHouseInputModel { Name = x.Name, Address = x.Address, ContactPhone = x.ContactPhone, DefaultElectricityPrice = x.DefaultElectricityPrice, DefaultWaterPrice = x.DefaultWaterPrice }).FirstOrDefaultAsync(ct); }

    public async Task<ServiceResult<int>> CreateAsync(BoardingHouseInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; var name = input.Name.Trim();
        if (await db.BoardingHouses.AnyAsync(x => x.OwnerId == owner && x.Name.ToLower() == name.ToLower(), ct)) return ServiceResult<int>.Failure("conflict", "Tên khu trọ đã tồn tại.");
        var entity = new BoardingHouse { OwnerId = owner, Name = name, Address = input.Address.Trim(), ContactPhone = input.ContactPhone?.Trim(), DefaultElectricityPrice = input.DefaultElectricityPrice, DefaultWaterPrice = input.DefaultWaterPrice, CreatedBy = owner };
        db.Add(entity); await db.SaveChangesAsync(ct); Audit("Create", entity.Id, $"Tạo khu trọ {entity.Name}.", owner); await db.SaveChangesAsync(ct); return ServiceResult<int>.Success(entity.Id);
    }

    public async Task<ServiceResult> UpdateAsync(int id, BoardingHouseInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; var entity = await db.BoardingHouses.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == owner, ct); if (entity is null) return ServiceResult.Failure("not_found", "Không tìm thấy khu trọ.");
        var name = input.Name.Trim();
        if (await db.BoardingHouses.AnyAsync(x => x.Id != id && x.OwnerId == owner && x.Name.ToLower() == name.ToLower(), ct)) return ServiceResult.Failure("conflict", "Tên khu trọ đã tồn tại.");
        entity.Name = name; entity.Address = input.Address.Trim(); entity.ContactPhone = input.ContactPhone?.Trim(); entity.DefaultElectricityPrice = input.DefaultElectricityPrice; entity.DefaultWaterPrice = input.DefaultWaterPrice; entity.UpdatedBy = owner;
        Audit("Update", id, $"Cập nhật khu trọ {entity.Name}.", owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId; var entity = await db.BoardingHouses.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == owner, ct); if (entity is null) return ServiceResult.Failure("not_found", "Không tìm thấy khu trọ.");
        if (await db.Contracts.AnyAsync(x => x.Room.BoardingHouseId == id && x.Status == ContractStatus.Active, ct)) return ServiceResult.Failure("conflict", "Không thể ngừng hoạt động khu trọ còn hợp đồng hiệu lực.");
        entity.IsActive = false; entity.UpdatedBy = owner; Audit("Deactivate", id, $"Ngừng hoạt động khu trọ {entity.Name}.", owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }
    public async Task<ServiceResult> ActivateAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId; var entity = await db.BoardingHouses.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == owner, ct); if (entity is null) return ServiceResult.Failure("not_found", "Không tìm thấy khu trọ."); entity.IsActive = true; entity.UpdatedBy = owner; Audit("Activate", id, $"Kích hoạt lại khu trọ {entity.Name}.", owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }

    private void Audit(string action, int id, string summary, Guid owner) => db.AuditLogs.Add(new() { UserId = owner, Action = action, EntityType = nameof(BoardingHouse), EntityId = id.ToString(), Timestamp = clock.UtcNow, Summary = summary });
}

public sealed class RoomService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IRoomService
{
    private Guid OwnerId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<RoomDetailsViewModel?> DetailsAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId; var room = await db.Rooms.AsNoTracking().Include(x => x.BoardingHouse).Include(x => x.RoomTenants).ThenInclude(x => x.TenantProfile).Include(x => x.Contracts).ThenInclude(x => x.RepresentativeTenant).FirstOrDefaultAsync(x => x.Id == id && x.BoardingHouse.OwnerId == owner, ct); if (room is null) return null;
        return new() { Room = new(room.Id, room.BoardingHouseId, room.BoardingHouse.Name, room.RoomCode, room.RoomName, room.Floor, room.MonthlyRent, room.Status, room.MaximumOccupants), Area = room.Area, DepositAmount = room.DepositAmount, Description = room.Description, CurrentTenants = room.RoomTenants.Where(x => x.MoveOutDate == null).OrderBy(x => x.TenantProfile.FullName).Select(x => x.TenantProfile.FullName).ToList(), Contracts = room.Contracts.OrderByDescending(x => x.StartDate).Select(x => new ContractListItem(x.Id, x.ContractCode, room.RoomCode, x.RepresentativeTenant.FullName, x.StartDate, x.EndDate, x.MonthlyRent, x.Status)).ToList() };
    }
    public async Task<RoomInputModel?> GetForEditAsync(int id, CancellationToken ct = default) { var owner = OwnerId; return await db.Rooms.AsNoTracking().Where(x => x.Id == id && x.BoardingHouse.OwnerId == owner).Select(x => new RoomInputModel { BoardingHouseId = x.BoardingHouseId, RoomCode = x.RoomCode, RoomName = x.RoomName, Floor = x.Floor, Area = x.Area, MonthlyRent = x.MonthlyRent, DepositAmount = x.DepositAmount, MaximumOccupants = x.MaximumOccupants, Status = x.Status, Description = x.Description }).FirstOrDefaultAsync(ct); }
    public async Task<PagedResult<RoomListItem>> ListAsync(int? propertyId, RoomStatus? status, int? floor, decimal? minPrice, decimal? maxPrice, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100); var owner = OwnerId;
        var q = db.Rooms.AsNoTracking().Where(x => x.BoardingHouse.OwnerId == owner);
        if (propertyId.HasValue) q = q.Where(x => x.BoardingHouseId == propertyId); if (status.HasValue) q = q.Where(x => x.Status == status); if (floor.HasValue) q = q.Where(x => x.Floor == floor); if (minPrice.HasValue) q = q.Where(x => x.MonthlyRent >= minPrice); if (maxPrice.HasValue) q = q.Where(x => x.MonthlyRent <= maxPrice);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim().ToLower(); q = q.Where(x => x.RoomCode.ToLower().Contains(term) || (x.RoomName != null && x.RoomName.ToLower().Contains(term))); }
        var count = await q.CountAsync(ct); var items = await q.OrderBy(x => x.BoardingHouse.Name).ThenBy(x => x.RoomCode).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new RoomListItem(x.Id, x.BoardingHouseId, x.BoardingHouse.Name, x.RoomCode, x.RoomName, x.Floor, x.MonthlyRent, x.Status, x.MaximumOccupants)).ToListAsync(ct);
        return new() { Items = items, Page = page, PageSize = pageSize, TotalCount = count };
    }

    public async Task<ServiceResult<int>> CreateAsync(RoomInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; if (!await db.BoardingHouses.AnyAsync(x => x.Id == input.BoardingHouseId && x.OwnerId == owner && x.IsActive, ct)) return ServiceResult<int>.Failure("not_found", "Không tìm thấy khu trọ.");
        var code = input.RoomCode.Trim().ToUpperInvariant(); if (await db.Rooms.AnyAsync(x => x.BoardingHouseId == input.BoardingHouseId && x.RoomCode == code, ct)) return ServiceResult<int>.Failure("conflict", "Mã phòng đã tồn tại trong khu trọ.");
        var room = Map(new Room { CreatedBy = owner }, input, code); db.Add(room); await db.SaveChangesAsync(ct); Audit("Create", room.Id, $"Tạo phòng {code}.", owner); await db.SaveChangesAsync(ct); return ServiceResult<int>.Success(room.Id);
    }

    public async Task<ServiceResult> UpdateAsync(int id, RoomInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; var room = await db.Rooms.Include(x => x.BoardingHouse).FirstOrDefaultAsync(x => x.Id == id && x.BoardingHouse.OwnerId == owner, ct); if (room is null) return ServiceResult.Failure("not_found", "Không tìm thấy phòng.");
        if (room.BoardingHouseId != input.BoardingHouseId) return ServiceResult.Failure("validation", "Không hỗ trợ chuyển phòng sang khu trọ khác.");
        var code = input.RoomCode.Trim().ToUpperInvariant(); if (await db.Rooms.AnyAsync(x => x.Id != id && x.BoardingHouseId == room.BoardingHouseId && x.RoomCode == code, ct)) return ServiceResult.Failure("conflict", "Mã phòng đã tồn tại trong khu trọ.");
        if (room.Status == RoomStatus.Occupied && input.Status != RoomStatus.Occupied && await db.Contracts.AnyAsync(x => x.RoomId == id && x.Status == ContractStatus.Active, ct)) return ServiceResult.Failure("conflict", "Không thể đổi trạng thái phòng đang có hợp đồng hiệu lực.");
        Map(room, input, code); room.UpdatedBy = owner; Audit("Update", id, $"Cập nhật phòng {code}.", owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId; var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == id && x.BoardingHouse.OwnerId == owner, ct); if (room is null) return ServiceResult.Failure("not_found", "Không tìm thấy phòng.");
        if (await db.Contracts.AnyAsync(x => x.RoomId == id && x.Status == ContractStatus.Active, ct)) return ServiceResult.Failure("conflict", "Phòng còn hợp đồng hiệu lực."); room.Status = RoomStatus.Inactive; room.UpdatedBy = owner; Audit("Deactivate", id, $"Ngừng hoạt động phòng {room.RoomCode}.", owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }

    private static Room Map(Room room, RoomInputModel i, string code) { room.BoardingHouseId = i.BoardingHouseId; room.RoomCode = code; room.RoomName = i.RoomName?.Trim(); room.Floor = i.Floor; room.Area = i.Area; room.MonthlyRent = i.MonthlyRent; room.DepositAmount = i.DepositAmount; room.MaximumOccupants = i.MaximumOccupants; room.Status = i.Status; room.Description = i.Description?.Trim(); return room; }
    private void Audit(string action, int id, string summary, Guid owner) => db.AuditLogs.Add(new() { UserId = owner, Action = action, EntityType = nameof(Room), EntityId = id.ToString(), Timestamp = clock.UtcNow, Summary = summary });
}

public sealed class TenantService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : ITenantService
{
    private Guid OwnerId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<TenantDetailsViewModel?> DetailsAsync(int id, CancellationToken ct = default)
    {
        var owner = OwnerId; var tenant = await db.TenantProfiles.AsNoTracking().Include(x => x.RoomTenancies).ThenInclude(x => x.Room).ThenInclude(x => x.BoardingHouse).Include(x => x.ContractMemberships).ThenInclude(x => x.Contract).ThenInclude(x => x.Room).FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == owner, ct); if (tenant is null) return null;
        var currentRoom = tenant.RoomTenancies.Where(x => x.MoveOutDate == null).Select(x => x.Room.RoomCode).FirstOrDefault();
        return new() { Tenant = new(tenant.Id, tenant.FullName, tenant.PhoneNumber, tenant.Email, currentRoom), DateOfBirth = tenant.DateOfBirth, IdentityNumber = tenant.IdentityNumber, IdentityIssueDate = tenant.IdentityIssueDate, PermanentAddress = tenant.PermanentAddress, VehiclePlate = tenant.VehiclePlate, EmergencyContactName = tenant.EmergencyContactName, EmergencyContactPhone = tenant.EmergencyContactPhone, Residences = tenant.RoomTenancies.OrderByDescending(x => x.MoveInDate).Select(x => new ResidenceHistoryListItem(x.Room.BoardingHouse.Name, x.Room.RoomCode, x.MoveInDate, x.MoveOutDate)).ToList(), Contracts = tenant.ContractMemberships.OrderByDescending(x => x.Contract.StartDate).Select(x => new ContractListItem(x.Contract.Id, x.Contract.ContractCode, x.Contract.Room.RoomCode, tenant.FullName, x.Contract.StartDate, x.Contract.EndDate, x.Contract.MonthlyRent, x.Contract.Status)).ToList() };
    }
    public async Task<TenantInputModel?> GetForEditAsync(int id, CancellationToken ct = default) { var owner = OwnerId; return await db.TenantProfiles.AsNoTracking().Where(x => x.Id == id && x.OwnerId == owner).Select(x => new TenantInputModel { FullName = x.FullName, DateOfBirth = x.DateOfBirth, PhoneNumber = x.PhoneNumber, Email = x.Email, IdentityNumber = x.IdentityNumber, IdentityIssueDate = x.IdentityIssueDate, PermanentAddress = x.PermanentAddress, VehiclePlate = x.VehiclePlate, EmergencyContactName = x.EmergencyContactName, EmergencyContactPhone = x.EmergencyContactPhone }).FirstOrDefaultAsync(ct); }
    public async Task<IReadOnlyList<TenantListItem>> ListAsync(string? search, CancellationToken ct = default)
    {
        var owner = OwnerId; var q = db.TenantProfiles.AsNoTracking().Where(x => x.OwnerId == owner); if (!string.IsNullOrWhiteSpace(search)) { var t = search.Trim().ToLower(); q = q.Where(x => x.FullName.ToLower().Contains(t) || x.PhoneNumber.Contains(t)); }
        return await q.OrderBy(x => x.FullName).Select(x => new TenantListItem(x.Id, x.FullName, x.PhoneNumber, x.Email, x.RoomTenancies.Where(r => r.MoveOutDate == null).Select(r => r.Room.RoomCode).FirstOrDefault())).ToListAsync(ct);
    }
    public async Task<ServiceResult<int>> CreateAsync(TenantInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; if (await db.TenantProfiles.AnyAsync(x => x.OwnerId == owner && x.PhoneNumber == input.PhoneNumber.Trim(), ct)) return ServiceResult<int>.Failure("conflict", "Số điện thoại đã tồn tại.");
        var e = Map(new TenantProfile { OwnerId = owner, CreatedBy = owner }, input); db.Add(e); await db.SaveChangesAsync(ct); Audit("Create", e.Id, $"Tạo hồ sơ người thuê {e.FullName}.", owner); await db.SaveChangesAsync(ct); return ServiceResult<int>.Success(e.Id);
    }
    public async Task<ServiceResult> UpdateAsync(int id, TenantInputModel input, CancellationToken ct = default)
    {
        var owner = OwnerId; var e = await db.TenantProfiles.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == owner, ct); if (e is null) return ServiceResult.Failure("not_found", "Không tìm thấy người thuê.");
        var phone = input.PhoneNumber.Trim(); if (await db.TenantProfiles.AnyAsync(x => x.Id != id && x.OwnerId == owner && x.PhoneNumber == phone, ct)) return ServiceResult.Failure("conflict", "Số điện thoại đã tồn tại.");
        Map(e, input); e.UpdatedBy = owner; Audit("Update", id, $"Cập nhật hồ sơ người thuê {e.FullName}.", owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }
    private static TenantProfile Map(TenantProfile e, TenantInputModel i) { e.FullName = i.FullName.Trim(); e.DateOfBirth = i.DateOfBirth; e.PhoneNumber = i.PhoneNumber.Trim(); e.Email = i.Email?.Trim(); e.IdentityNumber = i.IdentityNumber?.Trim(); e.IdentityIssueDate = i.IdentityIssueDate; e.PermanentAddress = i.PermanentAddress?.Trim(); e.VehiclePlate = i.VehiclePlate?.Trim(); e.EmergencyContactName = i.EmergencyContactName?.Trim(); e.EmergencyContactPhone = i.EmergencyContactPhone?.Trim(); return e; }
    private void Audit(string action, int id, string summary, Guid owner) => db.AuditLogs.Add(new() { UserId = owner, Action = action, EntityType = nameof(TenantProfile), EntityId = id.ToString(), Timestamp = clock.UtcNow, Summary = summary });
}
