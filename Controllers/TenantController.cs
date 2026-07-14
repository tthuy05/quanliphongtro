using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TroiSinhVien.Domain.Constants;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Controllers;

[Authorize(Roles = SystemRoles.Tenant)]
[Route("Tenant")]
public sealed class TenantController(IInvoiceService invoices, IPaymentService payments, IMaintenanceService maintenance) : Controller
{
    [HttpGet("")] public IActionResult Index() => RedirectToAction(nameof(Invoices));
    [HttpGet("Invoices")] public async Task<IActionResult> Invoices(CancellationToken ct) => View(await invoices.ListTenantAsync(ct));
    [HttpGet("Invoices/{id:int}")] public async Task<IActionResult> InvoiceDetails(int id, CancellationToken ct) { var model = await invoices.DetailsTenantAsync(id, ct); return model is null ? NotFound() : View("~/Views/Invoices/Details.cshtml", model); }
    [HttpGet("Payment/{invoiceId:int}")] public IActionResult Payment(int invoiceId) => View(new RecordPaymentInputModel { InvoiceId = invoiceId, Method = PaymentMethod.BankTransfer });
    [HttpPost("Payment")] public async Task<IActionResult> Payment(RecordPaymentInputModel input, CancellationToken ct) { if (!ModelState.IsValid) return View(input); var r = await payments.SubmitTenantAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Invoices)); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); return View(input); }
    [HttpGet("Maintenance")] public async Task<IActionResult> Maintenance(CancellationToken ct) => View(await maintenance.ListTenantAsync(ct));
    [HttpGet("Maintenance/{id:int}")] public async Task<IActionResult> MaintenanceDetails(int id, CancellationToken ct) { var model = await maintenance.DetailsTenantAsync(id, ct); return model is null ? NotFound() : View("~/Views/Maintenance/Details.cshtml", model); }
    [HttpGet("Maintenance/Create")] public IActionResult CreateMaintenance() => View(new CreateMaintenanceRequestInputModel());
    [HttpPost("Maintenance/Create")] public async Task<IActionResult> CreateMaintenance(CreateMaintenanceRequestInputModel input, CancellationToken ct) { if (!ModelState.IsValid) return View(input); var r = await maintenance.CreateTenantAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Maintenance)); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); return View(input); }
    [HttpPost("Maintenance/Comment")] public async Task<IActionResult> AddMaintenanceComment(AddMaintenanceCommentInputModel input, CancellationToken ct) { var r = ModelState.IsValid ? await maintenance.AddCommentAsync(input, ct) : Services.Common.ServiceResult.Failure("validation", "Nội dung bình luận không hợp lệ."); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã thêm bình luận." : string.Join(' ', r.Errors); return RedirectToAction(nameof(MaintenanceDetails), new { id = input.RequestId }); }
}
