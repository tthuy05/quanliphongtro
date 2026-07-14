using Microsoft.Extensions.Options;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Infrastructure.Storage;

public sealed class FileStorageOptions
{
    public string RootPath { get; set; } = "App_Data/uploads";
    public long MaxEvidenceBytes { get; set; } = 5 * 1024 * 1024;
}

public sealed class LocalFileStorageService(IWebHostEnvironment environment, IOptions<FileStorageOptions> options) : IFileStorageService
{
    private static readonly IReadOnlyDictionary<string, string> AllowedTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg", ["image/png"] = ".png", ["image/webp"] = ".webp"
    };

    private readonly string _root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.RootPath));
    private readonly long _maxBytes = options.Value.MaxEvidenceBytes;

    public async Task<StoredFileResult> SaveEvidenceAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0 || file.Length > _maxBytes) throw new InvalidOperationException($"Tệp phải có dung lượng từ 1 byte đến {_maxBytes / 1024 / 1024} MB.");
        if (!AllowedTypes.TryGetValue(file.ContentType, out var extension)) throw new InvalidOperationException("Chỉ chấp nhận ảnh JPEG, PNG hoặc WebP.");
        await ValidateSignatureAsync(file, file.ContentType, cancellationToken);

        Directory.CreateDirectory(_root);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = SafePath(storedName);
        await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        await file.CopyToAsync(output, cancellationToken);
        return new(storedName, Path.GetFileName(file.FileName), file.ContentType, file.Length, storedName);
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = SafePath(relativePath);
        Stream? stream = File.Exists(fullPath) ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true) : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = SafePath(relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    private string SafePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath)) throw new InvalidOperationException("Đường dẫn tệp không hợp lệ.");
        var full = Path.GetFullPath(Path.Combine(_root, relativePath));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Đường dẫn tệp không hợp lệ.");
        return full;
    }

    private static async Task ValidateSignatureAsync(IFormFile file, string contentType, CancellationToken ct)
    {
        var header = new byte[12];
        await using var input = file.OpenReadStream();
        var read = await input.ReadAsync(header.AsMemory(0, header.Length), ct);
        var valid = contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "image/webp" => read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
        if (!valid) throw new InvalidOperationException("Nội dung tệp không khớp định dạng ảnh được khai báo.");
    }
}
