using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroiSinhVien.Domain.Constants;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Controllers;

[Authorize(Roles = SystemRoles.Owner)]
public sealed class ReportsController(IReportService reports) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Revenue([FromQuery] ReportFilterInputModel filter, CancellationToken ct) => View(await reports.RevenueAsync(filter, ct));

    [HttpGet]
    public async Task<IActionResult> Occupancy([FromQuery] ReportFilterInputModel filter, CancellationToken ct) => View(await reports.OccupancyAsync(filter, ct));

    [HttpGet]
    public async Task<IActionResult> Debt([FromQuery] ReportFilterInputModel filter, CancellationToken ct) => View(await reports.DebtAsync(filter, ct));

    public IActionResult Index() => RedirectToAction(nameof(Revenue));
}
