using System.Security.Claims;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Infrastructure.Security;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public Guid? UserId => Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public bool IsInRole(string role) => User?.IsInRole(role) == true;
}
