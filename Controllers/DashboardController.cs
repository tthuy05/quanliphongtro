using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TroiSinhVien.Services.Dashboard;

namespace TroiSinhVien.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index(int propertyId = 1, string month = "")
        {
            try
            {
                var viewModel = await _dashboardService.GetDashboardDataAsync(propertyId, month);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // In actual environments, log exceptions.
                ViewData["ErrorMessage"] = "Không thể tải dữ liệu bảng điều khiển: " + ex.Message;
                return View("Error");
            }
        }
    }
}
