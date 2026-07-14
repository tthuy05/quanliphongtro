using Microsoft.AspNetCore.Identity;

namespace TroiSinhVien.Domain.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public TenantProfile? TenantProfile { get; set; }
    public ICollection<BoardingHouse> OwnedBoardingHouses { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
