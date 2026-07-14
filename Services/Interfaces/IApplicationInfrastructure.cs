using TroiSinhVien.Domain.Entities;

namespace TroiSinhVien.Services.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateOnly UtcToday { get; }
}

public interface IAuditLogService
{
    Task WriteAsync(string action, string entityType, string entityId, string summary, CancellationToken cancellationToken = default);
}

public sealed record StoredFileResult(string StoredName, string OriginalName, string ContentType, long Size, string RelativePath);
public sealed record EvidenceDownload(Stream Content, string ContentType, string FileName);

public interface IFileStorageService
{
    Task<StoredFileResult> SaveEvidenceAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
