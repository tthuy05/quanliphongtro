using TroiSinhVien.Domain.Common;
using TroiSinhVien.Domain.Enums;

namespace TroiSinhVien.Domain.Entities;

public sealed class MaintenanceRequest : AuditableEntity
{
    public int BoardingHouseId { get; set; }
    public BoardingHouse BoardingHouse { get; set; } = null!;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public int TenantProfileId { get; set; }
    public TenantProfile TenantProfile { get; set; } = null!;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.New;
    public DateTimeOffset? CompletedAt { get; set; }
    public ICollection<MaintenanceComment> Comments { get; set; } = [];
}

public sealed class MaintenanceComment : AuditableEntity
{
    public int MaintenanceRequestId { get; set; }
    public MaintenanceRequest MaintenanceRequest { get; set; } = null!;
    public Guid AuthorUserId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public sealed class Notification : AuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}

public sealed class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}

public sealed class UploadedFile : AuditableEntity
{
    public string StoredName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
}
