using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Models.ViewModels;
using TroiSinhVien.Services.Common;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Services.Implementations;

public sealed class MaintenanceService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IMaintenanceService
{
    private Guid UserId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<IReadOnlyList<MaintenanceListItem>> ListOwnerAsync(CancellationToken ct = default)
    {
        var owner = UserId; return await Query().Where(x => x.BoardingHouse.OwnerId == owner).OrderByDescending(x => x.Id).Select(Map()).ToListAsync(ct);
    }
    public async Task<IReadOnlyList<MaintenanceListItem>> ListTenantAsync(CancellationToken ct = default)
    {
        var user = UserId; return await Query().Where(x => x.TenantProfile.UserId == user).OrderByDescending(x => x.Id).Select(Map()).ToListAsync(ct);
    }
    public Task<MaintenanceDetailsViewModel?> DetailsOwnerAsync(int id, CancellationToken ct = default) => DetailsAsync(id, false, ct);
    public Task<MaintenanceDetailsViewModel?> DetailsTenantAsync(int id, CancellationToken ct = default) => DetailsAsync(id, true, ct);
    private async Task<MaintenanceDetailsViewModel?> DetailsAsync(int id, bool tenantView, CancellationToken ct)
    {
        var user = UserId;
        var query = db.MaintenanceRequests.AsNoTracking().Where(x => x.Id == id);
        query = tenantView ? query.Where(x => x.TenantProfile.UserId == user) : query.Where(x => x.BoardingHouse.OwnerId == user);
        var request = await query.Include(x => x.BoardingHouse).Include(x => x.Room).Include(x => x.TenantProfile).Include(x => x.Comments).FirstOrDefaultAsync(ct);
        if (request is null) return null;
        return new MaintenanceDetailsViewModel
        {
            Id = request.Id, BoardingHouseName = request.BoardingHouse.Name, RoomCode = request.Room.RoomCode,
            TenantName = request.TenantProfile.FullName, Category = request.Category, Title = request.Title,
            Description = request.Description, Priority = request.Priority, Status = request.Status,
            CreatedAt = request.CreatedAt, CompletedAt = request.CompletedAt, IsTenantView = tenantView,
            Comments = request.Comments.OrderBy(x => x.Id).Select(x => new MaintenanceCommentListItem(
                x.AuthorUserId == request.BoardingHouse.OwnerId ? "Chủ trọ" : "Người thuê", x.Content, x.CreatedAt)).ToList()
        };
    }
    public async Task<ServiceResult<int>> CreateTenantAsync(CreateMaintenanceRequestInputModel input, CancellationToken ct = default)
    {
        var user = UserId; var residence = await db.RoomTenants.Include(x => x.Room).ThenInclude(x => x.BoardingHouse).Include(x => x.TenantProfile).FirstOrDefaultAsync(x => x.TenantProfile.UserId == user && x.MoveOutDate == null, ct); if (residence is null) return ServiceResult<int>.Failure("conflict", "Bạn không có phòng đang cư trú.");
        var request = new MaintenanceRequest { BoardingHouseId = residence.Room.BoardingHouseId, RoomId = residence.RoomId, TenantProfileId = residence.TenantProfileId, Category = input.Category.Trim(), Title = input.Title.Trim(), Description = input.Description.Trim(), Priority = input.Priority, Status = MaintenanceStatus.New, CreatedBy = user }; db.Add(request); db.Notifications.Add(new Notification { UserId = residence.Room.BoardingHouse.OwnerId, Type = NotificationType.MaintenanceUpdated, Title = "Yêu cầu sửa chữa mới", Message = $"Phòng {residence.Room.RoomCode}: {request.Title}", LinkUrl = "/Maintenance", CreatedBy = user }); await db.SaveChangesAsync(ct); return ServiceResult<int>.Success(request.Id);
    }
    public async Task<ServiceResult> UpdateOwnerAsync(UpdateMaintenanceStatusInputModel input, CancellationToken ct = default)
    {
        var owner = UserId; var request = await db.MaintenanceRequests.Include(x => x.TenantProfile).FirstOrDefaultAsync(x => x.Id == input.RequestId && x.BoardingHouse.OwnerId == owner, ct); if (request is null) return ServiceResult.Failure("not_found", "Không tìm thấy yêu cầu."); if (!Allowed(request.Status, input.Status)) return ServiceResult.Failure("conflict", "Chuyển trạng thái không hợp lệ."); request.Status = input.Status; request.Priority = input.Priority; request.UpdatedBy = owner; request.CompletedAt = input.Status == MaintenanceStatus.Completed ? clock.UtcNow : null;
        if (!string.IsNullOrWhiteSpace(input.Comment)) db.MaintenanceComments.Add(new MaintenanceComment { MaintenanceRequestId = request.Id, AuthorUserId = owner, Content = input.Comment.Trim(), CreatedBy = owner }); if (request.TenantProfile.UserId is Guid tenantUser) db.Notifications.Add(new Notification { UserId = tenantUser, Type = NotificationType.MaintenanceUpdated, Title = "Yêu cầu sửa chữa đã cập nhật", Message = $"Trạng thái mới: {input.Status}.", LinkUrl = $"/Tenant/Maintenance/{request.Id}", CreatedBy = owner }); db.AuditLogs.Add(new AuditLog { UserId = owner, Action = "UpdateStatus", EntityType = nameof(MaintenanceRequest), EntityId = request.Id.ToString(), Timestamp = clock.UtcNow, Summary = $"Cập nhật yêu cầu sửa chữa {request.Id} sang {input.Status}." }); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }
    public async Task<ServiceResult> AddCommentAsync(AddMaintenanceCommentInputModel input, CancellationToken ct = default)
    {
        var user = UserId;
        var request = await db.MaintenanceRequests.Include(x => x.BoardingHouse).Include(x => x.TenantProfile)
            .FirstOrDefaultAsync(x => x.Id == input.RequestId, ct);
        if (request is null || request.BoardingHouse.OwnerId != user && request.TenantProfile.UserId != user)
            return ServiceResult.Failure("not_found", "Không tìm thấy yêu cầu.");
        db.MaintenanceComments.Add(new MaintenanceComment { MaintenanceRequestId = request.Id, AuthorUserId = user, Content = input.Content.Trim(), CreatedBy = user });
        var isOwner = request.BoardingHouse.OwnerId == user;
        var recipient = isOwner ? request.TenantProfile.UserId : request.BoardingHouse.OwnerId;
        if (recipient is Guid recipientId)
            db.Notifications.Add(new Notification { UserId = recipientId, Type = NotificationType.MaintenanceUpdated, Title = "Yêu cầu sửa chữa có bình luận mới", Message = request.Title, LinkUrl = isOwner ? $"/Tenant/Maintenance/{request.Id}" : $"/Maintenance/Details/{request.Id}", CreatedBy = user });
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
    private IQueryable<MaintenanceRequest> Query() => db.MaintenanceRequests.AsNoTracking();
    private static System.Linq.Expressions.Expression<Func<MaintenanceRequest, MaintenanceListItem>> Map() => x => new MaintenanceListItem(x.Id, x.Room.RoomCode, x.TenantProfile.FullName, x.Category, x.Title, x.Priority, x.Status, x.CreatedAt);
    private static bool Allowed(MaintenanceStatus from, MaintenanceStatus to) => from == to || from switch { MaintenanceStatus.New => to is MaintenanceStatus.InProgress or MaintenanceStatus.Cancelled, MaintenanceStatus.InProgress => to is MaintenanceStatus.WaitingForTenant or MaintenanceStatus.Completed or MaintenanceStatus.Cancelled, MaintenanceStatus.WaitingForTenant => to is MaintenanceStatus.InProgress or MaintenanceStatus.Completed or MaintenanceStatus.Cancelled, _ => false };
}

public sealed class NotificationService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : INotificationService
{
    private Guid UserId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task GenerateOperationalAsync(CancellationToken ct = default)
    {
        var user = UserId;
        var today = clock.UtcToday;
        var existing = (await db.Notifications.AsNoTracking().Where(x => x.UserId == user && x.LinkUrl != null)
            .Select(x => new { x.Type, x.LinkUrl }).ToListAsync(ct))
            .Select(x => $"{x.Type}|{x.LinkUrl}").ToHashSet(StringComparer.Ordinal);

        var expiringContracts = await db.Contracts.AsNoTracking()
            .Where(x => x.Room.BoardingHouse.OwnerId == user && x.Status == ContractStatus.Active && x.EndDate >= today && x.EndDate <= today.AddDays(30))
            .Select(x => new { x.Id, x.ContractCode, x.Room.RoomCode, x.EndDate }).ToListAsync(ct);
        foreach (var contract in expiringContracts)
        {
            var link = $"/Contracts/Details/{contract.Id}";
            if (existing.Add($"{NotificationType.ContractExpiring}|{link}"))
                db.Notifications.Add(new Notification { UserId = user, Type = NotificationType.ContractExpiring, Title = "Hợp đồng sắp hết hạn", Message = $"{contract.ContractCode} · phòng {contract.RoomCode} · hết hạn {contract.EndDate:dd/MM/yyyy}.", LinkUrl = link });
        }

        var tenantInvoices = await db.Invoices
            .Where(x => x.Contract.Status == ContractStatus.Active && x.Contract.Members.Any(m => m.TenantProfile.UserId == user && m.LeftDate == null)
                && x.RemainingAmount > 0 && x.Status != InvoiceStatus.Draft && x.Status != InvoiceStatus.Cancelled && x.Status != InvoiceStatus.Paid && x.DueDate <= today.AddDays(3))
            .Select(x => new { Invoice = x, x.InvoiceNumber, x.Room.RoomCode, x.DueDate, x.RemainingAmount }).ToListAsync(ct);
        foreach (var row in tenantInvoices)
        {
            var overdue = row.DueDate < today;
            var type = overdue ? NotificationType.InvoiceOverdue : NotificationType.InvoiceDue;
            var link = $"/Tenant/Invoices/{row.Invoice.Id}";
            if (overdue && row.Invoice.Status != InvoiceStatus.Overdue) row.Invoice.Status = InvoiceStatus.Overdue;
            if (existing.Add($"{type}|{link}"))
                db.Notifications.Add(new Notification { UserId = user, Type = type, Title = overdue ? "Hóa đơn đã quá hạn" : "Hóa đơn sắp đến hạn", Message = $"{row.InvoiceNumber} · phòng {row.RoomCode} · còn {row.RemainingAmount:N0} ₫.", LinkUrl = link });
        }
        if (db.ChangeTracker.HasChanges()) await db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<NotificationListItem>> ListAsync(CancellationToken ct = default) { await GenerateOperationalAsync(ct); var user = UserId; return await db.Notifications.AsNoTracking().Where(x => x.UserId == user).OrderByDescending(x => x.Id).Select(x => new NotificationListItem(x.Id, x.Type, x.Title, x.Message, x.LinkUrl, x.CreatedAt, x.ReadAt)).ToListAsync(ct); }
    public Task<int> UnreadCountAsync(CancellationToken ct = default) { var user = UserId; return db.Notifications.CountAsync(x => x.UserId == user && x.ReadAt == null, ct); }
    public async Task<ServiceResult> MarkReadAsync(int id, CancellationToken ct = default) { var user = UserId; var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == user, ct); if (n is null) return ServiceResult.Failure("not_found", "Không tìm thấy thông báo."); n.ReadAt ??= clock.UtcNow; await db.SaveChangesAsync(ct); return ServiceResult.Success(); }
    public async Task MarkAllReadAsync(CancellationToken ct = default) { var user = UserId; var items = await db.Notifications.Where(x => x.UserId == user && x.ReadAt == null).ToListAsync(ct); foreach (var n in items) n.ReadAt = clock.UtcNow; await db.SaveChangesAsync(ct); }
}
