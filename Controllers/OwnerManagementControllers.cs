using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TroiSinhVien.Domain.Constants;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Services.Common;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Controllers;

[Authorize(Roles = SystemRoles.Owner)]
public sealed class BoardingHousesController(IBoardingHouseService service) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.ListAsync(ct));
    [HttpGet] public async Task<IActionResult> Details(int id, CancellationToken ct) { var model = await service.DetailsAsync(id, ct); return model is null ? NotFound() : View(model); }
    [HttpGet] public IActionResult Create() => View(new BoardingHouseInputModel());
    [HttpPost] public async Task<IActionResult> Create(BoardingHouseInputModel input, CancellationToken ct) { if (!ModelState.IsValid) return View(input); var r = await service.CreateAsync(input, ct); return r.Succeeded ? RedirectToAction(nameof(Index)) : Failure(r, input); }
    [HttpPost] public async Task<IActionResult> Deactivate(int id, CancellationToken ct) { var r = await service.DeactivateAsync(id, ct); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã ngừng hoạt động khu trọ." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Index)); }
    [HttpPost] public async Task<IActionResult> Activate(int id, CancellationToken ct) { var r = await service.ActivateAsync(id, ct); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã kích hoạt lại khu trọ." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Index)); }
    [HttpGet] public async Task<IActionResult> Edit(int id, CancellationToken ct) { var model = await service.GetForEditAsync(id, ct); if (model is null) return NotFound(); ViewData["FormTitle"] = "Cập nhật khu trọ"; return View("Create", model); }
    [HttpPost] public async Task<IActionResult> Edit(int id, BoardingHouseInputModel input, CancellationToken ct) { ViewData["FormTitle"] = "Cập nhật khu trọ"; if (!ModelState.IsValid) return View("Create", input); var r = await service.UpdateAsync(id, input, ct); if (r.Succeeded) { TempData["Success"] = "Đã cập nhật khu trọ."; return RedirectToAction(nameof(Index)); } foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); return View("Create", input); }
    private IActionResult Failure(ServiceResult r, BoardingHouseInputModel input) { foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); return View("Create", input); }
}

[Authorize(Roles = SystemRoles.Owner)]
public sealed class RoomsController(IRoomService service, IBoardingHouseService properties) : Controller
{
    public async Task<IActionResult> Index(int? propertyId, RoomStatus? status, int? floor, decimal? minPrice, decimal? maxPrice, string? search, int page = 1, CancellationToken ct = default) => View(await service.ListAsync(propertyId, status, floor, minPrice, maxPrice, search, page, 20, ct));
    [HttpGet] public async Task<IActionResult> Details(int id, CancellationToken ct) { var model = await service.DetailsAsync(id, ct); return model is null ? NotFound() : View(model); }
    [HttpGet] public async Task<IActionResult> Create(int propertyId = 0, CancellationToken ct = default) { await SetPropertiesAsync(ct); return View(new RoomInputModel { BoardingHouseId = propertyId }); }
    [HttpPost] public async Task<IActionResult> Create(RoomInputModel input, CancellationToken ct) { if (!ModelState.IsValid) { await SetPropertiesAsync(ct); return View(input); } var r = await service.CreateAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Index)); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetPropertiesAsync(ct); return View(input); }
    [HttpPost] public async Task<IActionResult> Deactivate(int id, CancellationToken ct) { var r = await service.DeactivateAsync(id, ct); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã ngừng hoạt động phòng." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Index)); }
    [HttpGet] public async Task<IActionResult> Edit(int id, CancellationToken ct) { var model = await service.GetForEditAsync(id, ct); if (model is null) return NotFound(); await SetPropertiesAsync(ct); ViewData["FormTitle"] = "Cập nhật phòng"; return View("Create", model); }
    [HttpPost] public async Task<IActionResult> Edit(int id, RoomInputModel input, CancellationToken ct) { ViewData["FormTitle"] = "Cập nhật phòng"; if (!ModelState.IsValid) { await SetPropertiesAsync(ct); return View("Create", input); } var r = await service.UpdateAsync(id, input, ct); if (r.Succeeded) { TempData["Success"] = "Đã cập nhật phòng."; return RedirectToAction(nameof(Index)); } foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetPropertiesAsync(ct); return View("Create", input); }
    private async Task SetPropertiesAsync(CancellationToken ct) => ViewBag.Properties = (await properties.ListAsync(ct)).Where(x => x.IsActive).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
}

[Authorize(Roles = SystemRoles.Owner)]
public sealed class TenantsController(ITenantService service) : Controller
{
    public async Task<IActionResult> Index(string? search, CancellationToken ct) => View(await service.ListAsync(search, ct));
    [HttpGet] public async Task<IActionResult> Details(int id, CancellationToken ct) { var model = await service.DetailsAsync(id, ct); return model is null ? NotFound() : View(model); }
    [HttpGet] public IActionResult Create() => View(new TenantInputModel());
    [HttpPost] public async Task<IActionResult> Create(TenantInputModel input, CancellationToken ct) { if (!ModelState.IsValid) return View(input); var r = await service.CreateAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Index)); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); return View(input); }
    [HttpGet] public async Task<IActionResult> Edit(int id, CancellationToken ct) { var model = await service.GetForEditAsync(id, ct); if (model is null) return NotFound(); ViewData["FormTitle"] = "Cập nhật người thuê"; return View("Create", model); }
    [HttpPost] public async Task<IActionResult> Edit(int id, TenantInputModel input, CancellationToken ct) { ViewData["FormTitle"] = "Cập nhật người thuê"; if (!ModelState.IsValid) return View("Create", input); var r = await service.UpdateAsync(id, input, ct); if (r.Succeeded) { TempData["Success"] = "Đã cập nhật người thuê."; return RedirectToAction(nameof(Index)); } foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); return View("Create", input); }
}

[Authorize(Roles = SystemRoles.Owner)]
public sealed class ContractsController(IContractService service, IRoomService rooms, ITenantService tenants) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.ListAsync(ct));
    [HttpGet] public async Task<IActionResult> Details(int id, CancellationToken ct) { var model = await service.DetailsAsync(id, ct); return model is null ? NotFound() : View(model); }
    [HttpGet] public async Task<IActionResult> Create(CancellationToken ct) { await SetOptionsAsync(ct); return View(new ContractInputModel { StartDate = DateOnly.FromDateTime(DateTime.Today), EndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)) }); }
    [HttpPost] public async Task<IActionResult> Create(ContractInputModel input, CancellationToken ct) { if (!ModelState.IsValid) { await SetOptionsAsync(ct); return View(input); } var r = await service.CreateDraftAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Details), new { id = r.Value }); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetOptionsAsync(ct); return View(input); }
    [HttpPost] public Task<IActionResult> Activate(int id, CancellationToken ct) => Execute(() => service.ActivateAsync(id, ct));
    [HttpPost] public Task<IActionResult> End(int id, bool completeMoveOut, CancellationToken ct) => Execute(() => service.EndAsync(id, completeMoveOut, ct));
    [HttpPost] public Task<IActionResult> Cancel(int id, string reason, CancellationToken ct) => Execute(() => service.CancelAsync(id, reason, ct));
    [HttpPost] public async Task<IActionResult> AddMember(AddContractMemberInputModel input, CancellationToken ct) { var r = ModelState.IsValid ? await service.AddMemberAsync(input.ContractId, input.TenantId, ct) : ServiceResult.Failure("validation", "Dữ liệu không hợp lệ."); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã thêm thành viên hợp đồng." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Details), new { id = input.ContractId }); }
    [HttpPost] public async Task<IActionResult> Extend(int id, DateOnly newEndDate, CancellationToken ct) { var r = await service.ExtendAsync(id, newEndDate, ct); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã gia hạn hợp đồng." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Details), new { id }); }
    private async Task<IActionResult> Execute(Func<Task<ServiceResult>> action) { var r = await action(); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Thao tác hợp đồng thành công." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Index)); }
    private async Task SetOptionsAsync(CancellationToken ct)
    {
        var roomItems = await rooms.ListAsync(null, RoomStatus.Available, null, null, null, null, 1, 100, ct);
        ViewBag.Rooms = roomItems.Items.Select(x => new SelectListItem($"{x.BoardingHouseName} · {x.RoomCode} · {x.MonthlyRent:N0} ₫", x.Id.ToString())).ToList();
        ViewBag.Tenants = (await tenants.ListAsync(null, ct)).Select(x => new SelectListItem($"{x.FullName} · {x.PhoneNumber}", x.Id.ToString())).ToList();
    }
}

[Authorize(Roles = SystemRoles.Owner)]
public sealed class PropertyServicesController(IServiceCatalogService service, IBoardingHouseService properties) : Controller
{
    [HttpGet] public async Task<IActionResult> Index(int? propertyId, CancellationToken ct) => View(await service.ListAsync(propertyId, ct));
    [HttpGet] public async Task<IActionResult> Create(int propertyId = 0, CancellationToken ct = default) { await SetPropertiesAsync(ct); return View(new PropertyServiceInputModel { BoardingHouseId = propertyId }); }
    [HttpPost] public async Task<IActionResult> Create(PropertyServiceInputModel input, CancellationToken ct) { if (!ModelState.IsValid) { await SetPropertiesAsync(ct); return View(input); } var r = await service.CreateAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Index), new { propertyId = input.BoardingHouseId }); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetPropertiesAsync(ct); return View(input); }
    [HttpGet] public async Task<IActionResult> Edit(int id, CancellationToken ct) { var model = await service.GetForEditAsync(id, ct); if (model is null) return NotFound(); await SetPropertiesAsync(ct); ViewData["FormTitle"] = "Cập nhật dịch vụ"; return View("Create", model); }
    [HttpPost] public async Task<IActionResult> Edit(int id, PropertyServiceInputModel input, CancellationToken ct) { ViewData["FormTitle"] = "Cập nhật dịch vụ"; if (!ModelState.IsValid) { await SetPropertiesAsync(ct); return View("Create", input); } var r = await service.UpdateAsync(id, input, ct); if (r.Succeeded) { TempData["Success"] = "Đã cập nhật dịch vụ."; return RedirectToAction(nameof(Index), new { propertyId = input.BoardingHouseId }); } foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetPropertiesAsync(ct); return View("Create", input); }
    [HttpPost] public async Task<IActionResult> SetActive(int id, bool isActive, CancellationToken ct) { var r = await service.SetActiveAsync(id, isActive, ct); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã cập nhật trạng thái dịch vụ." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Index)); }
    [HttpPost] public async Task<IActionResult> AddToContract(AddContractServiceInputModel input, CancellationToken ct) { var r = ModelState.IsValid ? await service.AddToContractAsync(input, ct) : ServiceResult.Failure("validation", "Dữ liệu không hợp lệ."); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã thêm dịch vụ vào hợp đồng." : string.Join(' ', r.Errors); return RedirectToAction("Details", "Contracts", new { id = input.ContractId }); }
    private async Task SetPropertiesAsync(CancellationToken ct) => ViewBag.Properties = (await properties.ListAsync(ct)).Where(x => x.IsActive).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
}

[Authorize(Roles = SystemRoles.Owner)]
public sealed class MeterReadingsController(IMeterReadingService service, IRoomService rooms) : Controller
{
    public async Task<IActionResult> Index(int propertyId, CancellationToken ct) => View(await service.ListAsync(propertyId, ct));
    [HttpGet] public async Task<IActionResult> Create(int roomId = 0, CancellationToken ct = default) { var now = DateTime.Today; await SetRoomsAsync(ct); return View(new MeterReadingInputModel { RoomId = roomId, BillingYear = now.Year, BillingMonth = now.Month }); }
    [HttpPost] public async Task<IActionResult> Create(MeterReadingInputModel input, CancellationToken ct) { if (!ModelState.IsValid) { await SetRoomsAsync(ct); return View(input); } var r = await service.CreateAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Index), new { propertyId = 0 }); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetRoomsAsync(ct); return View(input); }
    [HttpGet] public async Task<IActionResult> Edit(int id, CancellationToken ct) { var model = await service.GetForEditAsync(id, ct); if (model is null) return NotFound(); ViewData["FormTitle"] = "Sửa chỉ số"; ViewData["IsEdit"] = true; return View("Create", model); }
    [HttpPost] public async Task<IActionResult> Edit(int id, MeterReadingInputModel input, CancellationToken ct) { ViewData["FormTitle"] = "Sửa chỉ số"; ViewData["IsEdit"] = true; if (!ModelState.IsValid) return View("Create", input); var r = await service.UpdateAsync(id, input, ct); if (r.Succeeded) { TempData["Success"] = "Đã cập nhật chỉ số."; return RedirectToAction(nameof(Index), new { propertyId = 0 }); } foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); return View("Create", input); }
    private async Task SetRoomsAsync(CancellationToken ct) { var result = await rooms.ListAsync(null, RoomStatus.Occupied, null, null, null, null, 1, 100, ct); ViewBag.Rooms = result.Items.Select(x => new SelectListItem($"{x.BoardingHouseName} · {x.RoomCode}", x.Id.ToString())).ToList(); }
}

[Authorize(Roles = SystemRoles.Owner)]
public sealed class InvoicesController(IInvoiceService service, IContractService contracts, IBoardingHouseService properties) : Controller
{
    public async Task<IActionResult> Index(int? propertyId, InvoiceStatus? status, int? year, int? month, CancellationToken ct) { await SetPropertiesAsync(ct); return View(await service.ListOwnerFilteredAsync(propertyId, status, year, month, ct)); }
    [HttpGet] public async Task<IActionResult> Details(int id, CancellationToken ct) { var model = await service.DetailsOwnerAsync(id, ct); return model is null ? NotFound() : View(model); }
    [HttpGet] public async Task<IActionResult> Generate(int contractId = 0, CancellationToken ct = default) { var now = DateTime.Today; await SetContractsAsync(ct); return View(new GenerateInvoiceInputModel { ContractId = contractId, BillingYear = now.Year, BillingMonth = now.Month }); }
    [HttpPost] public async Task<IActionResult> Generate(GenerateInvoiceInputModel input, CancellationToken ct) { if (!ModelState.IsValid) { await SetContractsAsync(ct); return View(input); } var r = await service.GenerateAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Details), new { id = r.Value }); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetContractsAsync(ct); return View(input); }
    [HttpGet] public async Task<IActionResult> BulkGenerate(CancellationToken ct) { var now = DateTime.Today; await SetPropertiesAsync(ct); return View(new BulkGenerateInvoiceInputModel { BillingYear = now.Year, BillingMonth = now.Month }); }
    [HttpPost] public async Task<IActionResult> BulkGenerate(BulkGenerateInvoiceInputModel input, CancellationToken ct) { if (!ModelState.IsValid) { await SetPropertiesAsync(ct); return View(input); } var r = await service.BulkGenerateAsync(input, ct); if (r.Succeeded) { TempData["Success"] = $"Đã tạo {r.Value} hóa đơn mới; các hóa đơn đã tồn tại được bỏ qua."; return RedirectToAction(nameof(Index), new { propertyId = input.BoardingHouseId, year = input.BillingYear, month = input.BillingMonth }); } foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetPropertiesAsync(ct); return View(input); }
    [HttpPost] public Task<IActionResult> Issue(int id, CancellationToken ct) => Execute(() => service.IssueAsync(id, ct));
    [HttpPost] public Task<IActionResult> Cancel(int id, string reason, CancellationToken ct) => Execute(() => service.CancelAsync(id, reason, ct));
    private async Task<IActionResult> Execute(Func<Task<ServiceResult>> action) { var r = await action(); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Thao tác hóa đơn thành công." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Index)); }
    private async Task SetContractsAsync(CancellationToken ct) => ViewBag.Contracts = (await contracts.ListAsync(ct)).Where(x => x.Status == ContractStatus.Active).Select(x => new SelectListItem($"{x.ContractCode} · {x.RoomCode} · {x.TenantName}", x.Id.ToString())).ToList();
    private async Task SetPropertiesAsync(CancellationToken ct) => ViewBag.Properties = (await properties.ListAsync(ct)).Where(x => x.IsActive).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
}

[Authorize(Roles = SystemRoles.Owner)]
public sealed class PaymentsController(IPaymentService service, IInvoiceService invoices) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.ListOwnerAsync(ct));
    [HttpGet] public async Task<IActionResult> Record(int invoiceId = 0, CancellationToken ct = default) { await SetInvoicesAsync(ct); return View(new RecordPaymentInputModel { InvoiceId = invoiceId, Method = PaymentMethod.Cash }); }
    [HttpPost] public async Task<IActionResult> Record(RecordPaymentInputModel input, CancellationToken ct) { if (!ModelState.IsValid) { await SetInvoicesAsync(ct); return View(input); } var r = await service.RecordOwnerAsync(input, ct); if (r.Succeeded) return RedirectToAction(nameof(Index)); foreach (var e in r.Errors) ModelState.AddModelError(string.Empty, e); await SetInvoicesAsync(ct); return View(input); }
    [HttpPost] public async Task<IActionResult> Confirm(ConfirmPaymentInputModel input, CancellationToken ct) { var r = await service.ConfirmAsync(input.PaymentId, ct); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã xác nhận thanh toán." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Index)); }
    [HttpPost] public async Task<IActionResult> Reject(RejectPaymentInputModel input, CancellationToken ct) { var r = await service.RejectAsync(input.PaymentId, input.Reason, ct); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã từ chối thanh toán." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Index)); }
    private async Task SetInvoicesAsync(CancellationToken ct) => ViewBag.Invoices = (await invoices.ListOwnerAsync(ct)).Where(x => x.RemainingAmount > 0 && x.Status is not (InvoiceStatus.Draft or InvoiceStatus.Cancelled or InvoiceStatus.Paid)).Select(x => new SelectListItem($"{x.InvoiceNumber} · {x.RoomCode} · còn {x.RemainingAmount:N0} ₫", x.Id.ToString())).ToList();
}

[Authorize(Roles = SystemRoles.Owner)]
public sealed class MaintenanceController(IMaintenanceService service) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.ListOwnerAsync(ct));
    public async Task<IActionResult> Details(int id, CancellationToken ct) { var model = await service.DetailsOwnerAsync(id, ct); return model is null ? NotFound() : View(model); }
    [HttpPost] public async Task<IActionResult> Update(UpdateMaintenanceStatusInputModel input, CancellationToken ct) { var r = ModelState.IsValid ? await service.UpdateOwnerAsync(input, ct) : ServiceResult.Failure("validation", "Dữ liệu không hợp lệ."); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã cập nhật yêu cầu." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Details), new { id = input.RequestId }); }
    [HttpPost] public async Task<IActionResult> AddComment(AddMaintenanceCommentInputModel input, CancellationToken ct) { var r = ModelState.IsValid ? await service.AddCommentAsync(input, ct) : ServiceResult.Failure("validation", "Nội dung bình luận không hợp lệ."); TempData[r.Succeeded ? "Success" : "Error"] = r.Succeeded ? "Đã thêm bình luận." : string.Join(' ', r.Errors); return RedirectToAction(nameof(Details), new { id = input.RequestId }); }
}

[Authorize]
public sealed class NotificationsController(INotificationService service) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await service.ListAsync(ct));
    [HttpPost] public async Task<IActionResult> MarkRead(int id, string? returnUrl, CancellationToken ct) { await service.MarkReadAsync(id, ct); return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index)); }
    [HttpPost] public async Task<IActionResult> MarkAllRead(CancellationToken ct) { await service.MarkAllReadAsync(ct); return RedirectToAction(nameof(Index)); }
}
