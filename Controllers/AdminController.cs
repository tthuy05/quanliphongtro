using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Constants;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Models.ViewModels;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Controllers;

[Authorize(Roles = SystemRoles.Admin)]
public sealed class AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users, IAuditLogService audit) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var accounts = await db.Users.AsNoTracking().OrderBy(x => x.Email).ToListAsync(ct);
        var items = new List<AdminUserListItem>(accounts.Count);
        foreach (var account in accounts)
        {
            var roles = await users.GetRolesAsync(account);
            items.Add(new(account.Id, account.Email ?? "", account.DisplayName, roles.FirstOrDefault() ?? "Chưa gán", account.IsActive, account.LockoutEnd));
        }
        return View(items);
    }
    [HttpGet] public async Task<IActionResult> Create(CancellationToken ct) { await SetTenantProfilesAsync(ct); return View(new CreateSystemUserInputModel { Role = SystemRoles.Owner }); }
    [HttpPost] public async Task<IActionResult> Create(CreateSystemUserInputModel input, CancellationToken ct)
    {
        if (!SystemRoles.All.Contains(input.Role)) ModelState.AddModelError(nameof(input.Role), "Vai trò không hợp lệ.");
        var tenant = input.TenantProfileId.HasValue ? await db.TenantProfiles.FirstOrDefaultAsync(x => x.Id == input.TenantProfileId && x.UserId == null, ct) : null;
        if (input.Role == SystemRoles.Tenant && tenant is null) ModelState.AddModelError(nameof(input.TenantProfileId), "Tài khoản người thuê phải liên kết với một hồ sơ chưa có tài khoản.");
        if (!ModelState.IsValid) { await SetTenantProfilesAsync(ct); return View(input); }
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var email = input.Email.Trim(); var user = new ApplicationUser { UserName = email, Email = email, DisplayName = input.DisplayName.Trim(), PhoneNumber = input.PhoneNumber?.Trim(), IsActive = true, EmailConfirmed = true };
        var created = await users.CreateAsync(user, input.Password); if (!created.Succeeded) { await tx.RollbackAsync(ct); foreach (var error in created.Errors) ModelState.AddModelError(string.Empty, error.Description); await SetTenantProfilesAsync(ct); return View(input); }
        var roleResult = await users.AddToRoleAsync(user, input.Role); if (!roleResult.Succeeded) { await tx.RollbackAsync(ct); foreach (var error in roleResult.Errors) ModelState.AddModelError(string.Empty, error.Description); await SetTenantProfilesAsync(ct); return View(input); }
        if (input.Role == SystemRoles.Tenant && tenant is not null) tenant.UserId = user.Id;
        await db.SaveChangesAsync(ct); await audit.WriteAsync("CreateAccount", nameof(ApplicationUser), user.Id.ToString(), $"Tạo tài khoản {email} với vai trò {input.Role}.", ct); await tx.CommitAsync(ct);
        TempData["Success"] = "Đã tạo tài khoản."; return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Audit(CancellationToken ct)
    {
        var items = await db.AuditLogs.AsNoTracking().OrderByDescending(x => x.Id).Take(200).Select(x => new AuditLogListItem(x.Id, x.Action, x.EntityType, x.EntityId, x.Timestamp, x.Summary)).ToListAsync(ct);
        return View(items);
    }
    [HttpPost] public async Task<IActionResult> Lock(Guid id)
    {
        if (IsCurrentUser(id)) return Failure("Không thể tự khóa tài khoản đang đăng nhập.");
        var user = await users.FindByIdAsync(id.ToString()); if (user is null) return NotFound();
        if (await users.IsInRoleAsync(user, SystemRoles.Admin) && await ActiveAdminCountAsync() <= 1) return Failure("Không thể khóa quản trị viên hoạt động cuối cùng.");
        user.IsActive = false; var result = await users.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue); if (!result.Succeeded) return IdentityFailure(result);
        await audit.WriteAsync("LockAccount", nameof(ApplicationUser), id.ToString(), "Khóa tài khoản người dùng."); return Success("Đã khóa tài khoản.");
    }
    [HttpPost] public async Task<IActionResult> Unlock(Guid id)
    {
        var user = await users.FindByIdAsync(id.ToString()); if (user is null) return NotFound(); user.IsActive = true;
        var unlock = await users.SetLockoutEndDateAsync(user, null); if (!unlock.Succeeded) return IdentityFailure(unlock);
        var reset = await users.ResetAccessFailedCountAsync(user); if (!reset.Succeeded) return IdentityFailure(reset);
        await audit.WriteAsync("UnlockAccount", nameof(ApplicationUser), id.ToString(), "Mở khóa tài khoản người dùng."); return Success("Đã mở khóa tài khoản.");
    }
    [HttpPost] public async Task<IActionResult> ChangeRole(Guid id, string role)
    {
        if (!SystemRoles.All.Contains(role)) return BadRequest();
        if (IsCurrentUser(id)) return Failure("Không thể tự thay đổi vai trò của tài khoản đang đăng nhập.");
        var user = await users.FindByIdAsync(id.ToString()); if (user is null) return NotFound(); var existing = await users.GetRolesAsync(user);
        if (existing.Contains(SystemRoles.Admin) && role != SystemRoles.Admin && await ActiveAdminCountAsync() <= 1) return Failure("Hệ thống phải còn ít nhất một quản trị viên hoạt động.");
        var linkedTenant = await db.TenantProfiles.FirstOrDefaultAsync(x => x.UserId == id);
        if (role == SystemRoles.Tenant && linkedTenant is null) return Failure("Không thể chuyển sang vai trò Tenant khi chưa liên kết hồ sơ người thuê; hãy tạo tài khoản người thuê mới từ hồ sơ.");
        if (!existing.Contains(role)) { var add = await users.AddToRoleAsync(user, role); if (!add.Succeeded) return IdentityFailure(add); }
        var obsolete = existing.Where(x => x != role).ToArray(); if (obsolete.Length > 0) { var remove = await users.RemoveFromRolesAsync(user, obsolete); if (!remove.Succeeded) return IdentityFailure(remove); }
        if (role != SystemRoles.Tenant && linkedTenant is not null) { linkedTenant.UserId = null; await db.SaveChangesAsync(); }
        await audit.WriteAsync("ChangeRole", nameof(ApplicationUser), id.ToString(), $"Đổi vai trò tài khoản thành {role}."); return Success("Đã cập nhật vai trò.");
    }

    private bool IsCurrentUser(Guid id) => Guid.TryParse(users.GetUserId(User), out var currentId) && currentId == id;
    private async Task<int> ActiveAdminCountAsync() => (await users.GetUsersInRoleAsync(SystemRoles.Admin)).Count(x => x.IsActive && (!x.LockoutEnd.HasValue || x.LockoutEnd <= DateTimeOffset.UtcNow));
    private async Task SetTenantProfilesAsync(CancellationToken ct) => ViewBag.TenantProfiles = await db.TenantProfiles.AsNoTracking().Where(x => x.UserId == null).OrderBy(x => x.OwnerId).ThenBy(x => x.FullName).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem($"{x.FullName} · {x.PhoneNumber}", x.Id.ToString())).ToListAsync(ct);
    private IActionResult IdentityFailure(IdentityResult result) => Failure(string.Join(" ", result.Errors.Select(x => x.Description)));
    private IActionResult Failure(string message) { TempData["Error"] = message; return RedirectToAction(nameof(Index)); }
    private IActionResult Success(string message) { TempData["Success"] = message; return RedirectToAction(nameof(Index)); }
}
