using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroiSinhVien.Domain.Constants;
using TroiSinhVien.Services.Dashboard;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Controllers;

[Authorize(Roles = SystemRoles.Owner)]
public sealed class DashboardController(IDashboardService dashboardService, INotificationService notifications, ILogger<DashboardController> logger) : Controller
{
    public async Task<IActionResult> Index(int propertyId = 0, string month = "", CancellationToken ct = default)
    {
        try { await notifications.GenerateOperationalAsync(ct); return View(await dashboardService.GetDashboardDataAsync(propertyId, month)); }
        catch (UnauthorizedAccessException) { return NotFound(); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Không thể tải dashboard cho property {PropertyId}", propertyId);
            ViewData["ErrorMessage"] = "Không thể tải dữ liệu bảng điều khiển.";
            return View("Error");
        }
    }
}
