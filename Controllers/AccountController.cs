using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Constants;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Models.ViewModels;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Controllers;

public sealed class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IAuditLogService audit) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginInputModel { ReturnUrl = returnUrl });

    [AllowAnonymous, HttpPost, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginInputModel input)
    {
        if (!ModelState.IsValid) return View(input);
        var user = await userManager.FindByEmailAsync(input.Email.Trim());
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(input);
        }

        var result = await signInManager.PasswordSignInAsync(user, input.Password, input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded && Url.IsLocalUrl(input.ReturnUrl)) return LocalRedirect(input.ReturnUrl!);
        if (result.Succeeded)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains(SystemRoles.Admin)) return RedirectToAction("Index", "Admin");
            if (roles.Contains(SystemRoles.Tenant)) return RedirectToAction("Invoices", "Tenant");
            return RedirectToAction("Index", "Dashboard");
        }
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Tài khoản tạm khóa do đăng nhập sai nhiều lần." : "Email hoặc mật khẩu không đúng.");
        return View(input);
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout() { await signInManager.SignOutAsync(); return RedirectToAction(nameof(Login)); }

    [AllowAnonymous, HttpGet]
    public IActionResult AccessDenied() => View();

    [Authorize, HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user = await userManager.GetUserAsync(User); if (user is null) return Challenge();
        return View(await SettingsModelAsync(user));
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> UpdateProfile(UpdateProfileInputModel input)
    {
        var user = await userManager.GetUserAsync(User); if (user is null) return Challenge();
        if (!ModelState.IsValid) return View("Settings", await SettingsModelAsync(user, input));
        user.DisplayName = input.DisplayName.Trim(); user.PhoneNumber = input.PhoneNumber?.Trim();
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) { AddIdentityErrors(result); return View("Settings", await SettingsModelAsync(user, input)); }
        await audit.WriteAsync("UpdateProfile", nameof(ApplicationUser), user.Id.ToString(), "Cập nhật hồ sơ tài khoản.");
        TempData["Success"] = "Đã cập nhật hồ sơ."; return RedirectToAction(nameof(Settings));
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordInputModel input)
    {
        var user = await userManager.GetUserAsync(User); if (user is null) return Challenge();
        if (!ModelState.IsValid) return View("Settings", await SettingsModelAsync(user, changePassword: input));
        var result = await userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
        if (!result.Succeeded) { AddIdentityErrors(result); return View("Settings", await SettingsModelAsync(user, changePassword: input)); }
        await signInManager.RefreshSignInAsync(user);
        await audit.WriteAsync("ChangePassword", nameof(ApplicationUser), user.Id.ToString(), "Đổi mật khẩu tài khoản.");
        TempData["Success"] = "Đã đổi mật khẩu."; return RedirectToAction(nameof(Settings));
    }

    private async Task<AccountSettingsViewModel> SettingsModelAsync(ApplicationUser user, UpdateProfileInputModel? profile = null, ChangePasswordInputModel? changePassword = null) => new()
    {
        Email = user.Email ?? string.Empty,
        Roles = (await userManager.GetRolesAsync(user)).ToArray(),
        Profile = profile ?? new() { DisplayName = user.DisplayName, PhoneNumber = user.PhoneNumber },
        ChangePassword = changePassword ?? new()
    };

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }
}
