namespace TroiSinhVien.Services.Common;

public class ServiceResult
{
    public bool Succeeded { get; init; }
    public string? ErrorCode { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static ServiceResult Success() => new() { Succeeded = true };
    public static ServiceResult Failure(string code, params string[] errors) => new() { ErrorCode = code, Errors = errors };
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Value { get; init; }

    public static ServiceResult<T> Success(T value) => new() { Succeeded = true, Value = value };
    public new static ServiceResult<T> Failure(string code, params string[] errors) => new() { ErrorCode = code, Errors = errors };
}
