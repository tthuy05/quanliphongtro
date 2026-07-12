# Project Overview - Trọ Sinh Viên (Internal technical name: TroHub)

This document provides a high-level overview of the **Trọ Sinh Viên** (internally coded as **TroHub**) boarding house and rental room management system student project, detailing its scope, objectives, users, and definition of done for the current phase.

---

## 1. Project Purpose
**Trọ Sinh Viên** (public brand) or **TroHub** (internal project identifier, historically "TroiSinhVien") is designed to modernize and simplify boarding house and rental property management in Vietnam. It targets the common pain points experienced by landlords (boarding house owners) and tenants, such as manual invoicing, scattered communication, electricity/water recording, and contract tracking. 

The application provides an exceptionally polished, premium, and highly responsive user interface built as an ASP.NET Core MVC application to deliver a commercial-grade SaaS experience for property managers.

---

## 2. Primary Users
1. **Boarding House Owner (Landlord):** The primary focus of this phase. Owners manage properties, track room occupancy, record utility readings, generate invoices, monitor overdue payments, review maintenance requests, and track financial performance.
2. **Tenant:** Residents who rent rooms, view contracts, receive monthly invoices, submit maintenance requests, and make payments.
3. **Administrator:** System admins managing system-wide accounts, platform settings, and logs.

---

## 3. Main Modules
*   **Property & Room Management:** Grouping rooms by boarding houses/locations and updating their availability/pricing.
*   **Contract Management:** Tracking lease details, security deposits, rental terms, and expiry timelines.
*   **Utility & Service Management:** Logging periodic water/electricity readings and pricing recurring service items (internet, cleaning).
*   **Billing & Invoicing:** Automatic and manual invoice generation, payment confirmation, and debt tracking.
*   **Maintenance & Tickets:** Room repair workflows, priority flags, and assignment statuses.
*   **Analytics & Reporting:** Revenue tracking, occupancy rates, and expense reporting.
*   **Notifications & Communications:** Broadcasts and critical alerts for due bills or expiring leases.

---

## 4. Current Phase Scope (Phase 1)
The primary objective of this phase is to establish the **visual foundations, shared application shell, and a premium boarding house owner dashboard** powered by strongly typed mock data. 

### In-Scope
*   **Shared Application Shell:** Collapsible desktop sidebar, top bar, responsive breadcrumb area, mobile navigation drawer, profile dropdown, notification panel, and property selector.
*   **Owner Dashboard Home:**
    *   Premium welcome hero section with landscape image zoom and light effects.
    *   Key operational statistics (total rooms, occupied, vacant, revenue, debt) with counters and sparklines.
    *   Interactive charts (revenue overview line/area chart and occupancy donut chart).
    *   Actionable tables (recent invoices, overdue payments, expiring contracts, maintenance requests).
    *   Recent activity timeline and quick action panels.
*   **UX/UI Design System:** Custom theme config via Tailwind CSS, custom fonts (Be Vietnam Pro), cohesive color scheme (indigo, slate, emerald, rose), custom transitions, focus states, and micro-interactions.
*   **Mock Services:** Strongly typed C# ViewModels and services that mock data realistically in Vietnamese locale.
*   **Motion Design:** Cinematic hero animations, page transition reveal, counter animations, hover lift, focus outlines, and skeletal loading states.
*   **Quality & Accessibility:** WCAG-compliant contrast, keyboard navigability, reduced-motion fallbacks, zero console errors, zero layout shifts, and full responsiveness (390px to 1440px+).

### Out-of-Scope (Future Phases)
*   Physical Database Integration (Entity Framework Core & SQL Server).
*   Production-ready Authentication & Authorization mechanisms (e.g., ASP.NET Core Identity).
*   Actual background workers for invoice generation or SMS/Zalo notifications.
*   Write operations persisting to a backend datastore.
*   Tenant-specific and Admin-specific portals.

---

## 5. Technical Stack
*   **Backend:** .NET 9.0 (ASP.NET Core MVC)
*   **Frontend Templating:** Razor Views (strongly typed ViewModels)
*   **CSS Framework:** Tailwind CSS (configured locally)
*   **JavaScript Scripting:** Alpine.js (lightweight UI state management), Chart.js (data visualization), Lucide Icons, and Modular Vanilla JS
*   **Assets:** Local optimized files and standard system font stacks

---

## 6. Main Quality Goals
1.  **Visual Excellence:** Clean typography, grid alignments, balanced white space, and premium dark/light contrast.
2.  **Performance:** Zero Cumulative Layout Shift (CLS), target 60 FPS transition animations, deferred script evaluation.
3.  **Accessibility:** Full support for `prefers-reduced-motion`, visible focus styling, keyboard navigation support on all dropdowns/modals.
4.  **Polish:** Smooth count-up indicators, interactive tooltips, elegant empty states, and shimmer skeleton loaders.

---

## 7. Assumptions & Constraints
*   The primary language of the user interface is Vietnamese.
*   Currency representations must conform to Vietnamese currency formatting rules (e.g., `58.400.000 ₫` or abbreviated as `58,4 triệu ₫`).
*   No database connects to the app during this phase; all dashboard actions trigger mock visual changes (e.g., displaying toasts, opening modals, toggling local states).
*   Target browsers include modern versions of Chrome, Safari, Edge, Firefox, and mobile engines (WebKit, Blink).

---

## 8. Definition of Done (DoD)
*   [ ] All 11 documentation files are created and reviewed for consistency.
*   [ ] Tailwind CSS is integrated and configured with our custom UI tokens.
*   [ ] Application shell (sidebar, top bar, mobile drawer) functions smoothly on all sizes.
*   [ ] Dashboard page is implemented using strongly typed ViewModels returned by a Mock Service.
*   [ ] Page renders all requested dashboard components without console errors.
*   [ ] Charts are fully interactive with responsive layouts.
*   [ ] Motion spec is implemented (welcome hero zoom, counters, reveals, skeletal loading).
*   [ ] UI renders beautifully across 1440px, 1280px, 1024px, 768px, and 390px viewports.
*   [ ] Verification tests pass as defined in the test plan.
*   [ ] Screenshot artifacts are generated for key resolutions.
