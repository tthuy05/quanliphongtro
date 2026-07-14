namespace TroiSinhVien.Domain.Constants;

public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string Owner = "Owner";
    public const string Tenant = "Tenant";

    public static readonly string[] All = [Admin, Owner, Tenant];
}
