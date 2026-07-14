# File Change Plan - Trọ Sinh Viên (Internal technical name: TroHub)

This plan records all files to be modified, created, or deleted during implementation, noting responsibilities and risk factors.

---

## 1. Summary of Planned Changes

### Backend Components
*   **[NEW]** `Services/Dashboard/IDashboardService.cs`
    *   *Purpose:* Define contract for loading dashboard data.
    *   *Risk:* Low. Simple decoupled interface.
*   **[NEW]** `Services/Dashboard/MockDashboardService.cs`
    *   *Purpose:* Provide localized mock rental property dataset.
    *   *Risk:* Low. Isolated mock data container.
*   **[NEW]** `Controllers/DashboardController.cs`
    *   *Purpose:* Manage routing request and load ViewModels.
    *   *Risk:* Low. Isolated controller wrapper.
*   **[MODIFY]** [Program.cs](file:///c:/Users/Admin/Downloads/quanliphongtro/Program.cs)
    *   *Purpose:* Register dependencies and route mapping to dashboard home page.
    *   *Risk:* Medium. Can cause boot failure if configuration lacks required syntax.

### ViewModels
*   **[NEW]** `Models/ViewModels/Dashboard/DashboardPageViewModel.cs` (and other child ViewModels)
    *   *Purpose:* Group C# model contracts.
    *   *Risk:* Low. Pure data structures.

### Razor Layout & Shared Components
*   **[MODIFY]** [Views/Shared/_Layout.cshtml](file:///c:/Users/Admin/Downloads/quanliphongtro/Views/Shared/_Layout.cshtml)
    *   *Purpose:* Update layout wrapper, add JS CDNs and include stylesheet paths.
    *   *Risk:* Medium. Affects global layout styling.
*   **[NEW]** `Views/Shared/_Sidebar.cshtml` (Sidebar menu list)
*   **[NEW]** `Views/Shared/_Topbar.cshtml` (Header status/user controls)
*   **[NEW]** `Views/Shared/_MobileNavigation.cshtml` (Mobile sliding panel)
*   **[NEW]** `Views/Shared/_PropertySelector.cshtml` (Select list overlay)
*   **[NEW]** `Views/Shared/_SearchCommand.cshtml` (Search overlay dialog)
*   **[NEW]** `Views/Shared/_NotificationPanel.cshtml` (Notification dropdown list)
*   **[NEW]** `Views/Shared/_UserMenu.cshtml` (User profile popup)
*   **[NEW]** `Views/Shared/_StatCard.cshtml` (Generic stats card rendering)
*   **[NEW]** `Views/Shared/_StatusBadge.cshtml` (Visual badges)
*   **[NEW]** `Views/Shared/_EmptyState.cshtml` (Empty panel indicators)
*   **[NEW]** `Views/Shared/_SkeletonLoader.cshtml` (Pulsing shimmer loads)
*   **[NEW]** `Views/Shared/_ConfirmModal.cshtml` (Center warning dialogue)
*   **[NEW]** `Views/Shared/_ToastContainer.cshtml` (Floating toasts stack)

### Page-Specific Views (Dashboard Home)
*   **[NEW]** `Views/Dashboard/Index.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_DashboardHero.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_RevenueOverview.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_OccupancyOverview.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_RecentInvoices.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_OverduePayments.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_ExpiringContracts.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_MaintenanceRequests.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_ActivityTimeline.cshtml`
*   **[NEW]** `Views/Dashboard/Partials/_QuickActions.cshtml`

### Styles & Asset Resources
*   **[MODIFY]** [wwwroot/css/site.css](file:///c:/Users/Admin/Downloads/quanliphongtro/wwwroot/css/site.css)
    *   *Purpose:* Clean up default boilerplate and import styling rules.
    *   *Risk:* Medium. Can cause display styling breaks.
*   **[NEW]** `wwwroot/css/theme-tokens.css` (Inject HSL and margin custom tokens)
*   **[NEW]** `wwwroot/images/dashboard/dashboard-hero-landscape.png` (Verify/Import provided asset image)

### Frontend JavaScript Modules
*   **[NEW]** `wwwroot/js/app/navigation.js`
*   **[NEW]** `wwwroot/js/app/dropdowns.js`
*   **[NEW]** `wwwroot/js/app/dialogs.js`
*   **[NEW]** `wwwroot/js/app/toasts.js`
*   **[NEW]** `wwwroot/js/dashboard/dashboard.js`
*   **[NEW]** `wwwroot/js/dashboard/hero-motion.js`
*   **[NEW]** `wwwroot/js/dashboard/counters.js`
*   **[NEW]** `wwwroot/js/dashboard/charts.js`
*   **[NEW]** `wwwroot/js/dashboard/viewport-motion.js`

---

## Backend file addendum (2026-07-12)

New changes are concentrated in `Domain/`, `Data/`, `Infrastructure/`, `Authorization/`, focused `Services`, role-scoped controllers, `Models/InputModels`, migrations and the xUnit test project. `Program.cs` and `TroiSinhVien.csproj` are high-risk integration points. Existing dashboard Views/CSS/JS are preserved; only real links, authentication-aware controls and data binding may be adjusted.
