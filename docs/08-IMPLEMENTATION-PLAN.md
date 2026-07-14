# Implementation Plan - Trọ Sinh Viên (Internal technical name: TroHub)

This plan outlines the implementation steps, dependencies, and validation checkpoints for Phase 1 of the Trọ Sinh Viên dashboard.

---

## 1. Phased Roadmap

### Phase 1: Foundation & Global Styling
*   **Files to Create:**
    *   `wwwroot/css/theme-tokens.css` (CSS variables for custom design tokens)
    *   `Views/Shared/_StatusBadge.cshtml` (Reusable badge view)
*   **Files to Modify:**
    *   `wwwroot/css/site.css` (Apply Tailwind directives & import tokens)
    *   `Views/Shared/_Layout.cshtml` (Wire layout metadata, CSS, and JS CDNs)
*   **Dependencies:** Tailwind CSS CDN setup (or local compiler), Lucide Icons, Alpine.js.
*   **Validation Steps:** Inspect page to ensure font sizes, colors, and layout margins match token variables. Verify Tailwind builds correctly.
*   **Expected Result:** Standardized, blank skeleton page layout utilizing Be Vietnam Pro font stack.

---

### Phase 2: Application Navigation Shell
*   **Files to Create:**
    *   `Views/Shared/_Sidebar.cshtml`
    *   `Views/Shared/_Topbar.cshtml`
    *   `Views/Shared/_MobileNavigation.cshtml`
    *   `Views/Shared/_PropertySelector.cshtml`
    *   `Views/Shared/_SearchCommand.cshtml`
    *   `Views/Shared/_NotificationPanel.cshtml`
    *   `Views/Shared/_UserMenu.cshtml`
    *   `wwwroot/js/app/navigation.js`
    *   `wwwroot/js/app/dropdowns.js`
*   **Files to Modify:**
    *   `Views/Shared/_Layout.cshtml`
*   **Dependencies:** Alpine.js, Lucide Icons.
*   **Validation Steps:** Toggle sidebar collapse on desktop. Toggle mobile drawer on mobile size. Test click-out-of-bounds to close notifications and property selectors. Verify Escape key behavior.
*   **Expected Result:** Fully functional navigation shell with responsive drawer and active state indicators.

---

### Phase 3: Dashboard Welcome Hero
*   **Files to Create:**
    *   `Views/Dashboard/Partials/_DashboardHero.cshtml`
    *   `wwwroot/js/dashboard/hero-motion.js`
*   **Files to Modify:**
    *   `Views/Dashboard/Index.cshtml`
*   **Dependencies:** Hero landscape image, `IntersectionObserver` module in JS.
*   **Validation Steps:** Check that landscape image fits the right column. Verify image zoom on page load. Test parallax offsets by moving mouse on desktop. Verify animation pausing when scrolled offscreen.
*   **Expected Result:** Visual welcome banner featuring cinematic image zooms and smooth text fade-in.

---

### Phase 4: Statistic Cards & Sparks
*   **Files to Create:**
    *   `Views/Shared/_StatCard.cshtml`
    *   `wwwroot/js/dashboard/counters.js`
*   **Files to Modify:**
    *   `Views/Dashboard/Index.cshtml`
*   **Dependencies:** Lucide Icons, IntersectionObserver.
*   **Validation Steps:** Scroll down to statistics grid. Check if numbers increment smoothly from 0 to target. Verify hover translation highlights.
*   **Expected Result:** 5 statistics cards displaying operational indicators, including counters and trend labels.

---

### Phase 5: Interactive Charts (Chart.js)
*   **Files to Create:**
    *   `Views/Dashboard/Partials/_RevenueOverview.cshtml`
    *   `Views/Dashboard/Partials/_OccupancyOverview.cshtml`
    *   `wwwroot/js/dashboard/charts.js`
*   **Files to Modify:**
    *   `Views/Dashboard/Index.cshtml`
*   **Dependencies:** Chart.js library loaded in Layout.
*   **Validation Steps:** Check revenue line/area chart rendering. Check occupancy donut segments. Test hovering over elements to trigger custom tooltips. Verify aspect-ratio preservation.
*   **Expected Result:** Responsive data visualizations indicating financial trends and occupancy ratios.

---

### Phase 6: Operational Tables & Feed Panels
*   **Files to Create:**
    *   `Views/Dashboard/Partials/_RecentInvoices.cshtml`
    *   `Views/Dashboard/Partials/_OverduePayments.cshtml`
    *   `Views/Dashboard/Partials/_ExpiringContracts.cshtml`
    *   `Views/Dashboard/Partials/_MaintenanceRequests.cshtml`
    *   `Views/Dashboard/Partials/_ActivityTimeline.cshtml`
    *   `Views/Dashboard/Partials/_QuickActions.cshtml`
*   **Files to Modify:**
    *   `Views/Dashboard/Index.cshtml`
*   **Dependencies:** Reusable status badges, standard table layout.
*   **Validation Steps:** Verify row hover highlight states. Test mobile response (tables overflow/collapse gracefully). Click quick actions to test feedback triggers.
*   **Expected Result:** Rich operational grid filled with invoices, expiring leases, maintenance tickets, activity feeds, and quick buttons.

---

### Phase 7: Dynamic State Loaders & Edge States
*   **Files to Create:**
    *   `Views/Shared/_EmptyState.cshtml`
    *   `Views/Shared/_SkeletonLoader.cshtml`
    *   `Views/Shared/_ConfirmModal.cshtml`
    *   `Views/Shared/_ToastContainer.cshtml`
    *   `wwwroot/js/app/dialogs.js`
    *   `wwwroot/js/app/toasts.js`
*   **Files to Modify:**
    *   `Views/Dashboard/Index.cshtml`
*   **Dependencies:** Alpine.js client states.
*   **Validation Steps:** Toggle "Loading" switch in dashboard header context. Skeletons should render. Toggle "Empty State" mock. Empty components should show standard warning boxes. Trigger test toasts and modals.
*   **Expected Result:** Polished transition views representing empty database queries, slow connections, or network failure overrides.

---

### Phase 8: Verification & QA Build
*   **Files to Modify:** None.
*   **Dependencies:** Dotnet SDK.
*   **Validation Steps:** Execute build commands (`dotnet build`). Clean up temporary script markers. Inspect in browser. Run keyboard-only navigations. Validate zero console errors. Capture final screenshots.
*   **Expected Result:** Highly stable, premium, accessible, and fast dashboard.

---

## 2. Documentation Completion Checklist

Before writing implementation code, verify that all files exist and have consistent naming conventions.

- [x] 01-PROJECT-OVERVIEW.md created.
- [x] 02-FRONTEND-ARCHITECTURE.md created.
- [x] 03-UI-UX-DESIGN-SYSTEM.md created.
- [x] 04-DASHBOARD-TECHNICAL-SPECIFICATION.md created.
- [x] 05-VIEWMODEL-AND-MOCK-DATA-CONTRACTS.md created.
- [x] 06-MOTION-AND-ANIMATION-SPECIFICATION.md created.
- [x] 07-RESPONSIVE-ACCESSIBILITY-PERFORMANCE.md created.
- [x] 08-IMPLEMENTATION-PLAN.md created.
- [x] 09-TEST-PLAN.md created.
- [x] 10-FILE-CHANGE-PLAN.md created.
- [x] DECISION-LOG.md created.

---

## Backend plan status

This file remains the completed frontend plan. The active backend roadmap is `18-BACKEND-IMPLEMENTATION-PLAN.md`. Backend work must preserve the UI, route and ViewModel decisions recorded here and execute build/test checkpoints after each major phase.
