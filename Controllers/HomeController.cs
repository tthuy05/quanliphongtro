using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TroiSinhVien.Models;
using TroiSinhVien.Domain.Constants;

namespace TroiSinhVien.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }

        if (User.IsInRole(SystemRoles.Admin))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (User.IsInRole(SystemRoles.Tenant))
        {
            return RedirectToAction("Invoices", "Tenant");
        }

        if (User.IsInRole(SystemRoles.Owner))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("AccessDenied", "Account");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HttpStatus(int code)
    {
        Response.StatusCode = code;
        ViewData["StatusCode"] = code;
        return View();
    }
}
