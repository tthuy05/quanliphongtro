using TroiSinhVien.Data;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Services.Implementations;

public sealed class AuditLogService(ApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock, IHttpContextAccessor http) : IAuditLogService
{
    public async Task WriteAsync(string action, string entityType, string entityId, string summary, CancellationToken cancellationToken = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUser.UserId,
            Action = action[..Math.Min(action.Length, 100)],
            EntityType = entityType[..Math.Min(entityType.Length, 100)],
            EntityId = entityId[..Math.Min(entityId.Length, 100)],
            Summary = summary[..Math.Min(summary.Length, 1000)],
            Timestamp = clock.UtcNow,
            IpAddress = http.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
