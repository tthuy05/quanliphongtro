# Continuation Status - Trọ Sinh Viên (TroHub)

> Historical checkpoint only. Its completion claims are superseded by the evidence-based audit and final report in documents 24–26 dated 13/07/2026.

Document created to checkpoint the repository status, framework, database state, completed phases, and verification results before proceeding.

## 1. Repository & Project Setup
* **Current Branch:** `ui` (tracked to `origin/ui`)
* **Target Framework:** `.NET 9.0`
* **Current Database Provider:** PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)
* **Project Structure:** Single-project modular monolith `TroiSinhVien` with views, controllers, services, and an isolated xUnit test project in `tests/TroiSinhVien.Tests`.

## 2. Completed and Partially Completed Phases
* **Completed Phases (1 - 9):**
  * **Phase 1: Foundation:** Entities, Enums, DbContext, configurations, clock, audit service.
  * **Phase 2: Authentication:** Identity cookies, login/logout actions, role seed, owner/tenant segregation.
  * **Phase 3: Boarding Houses & Rooms:** CRUD services, deactivation rules, room-code uniqueness, pagination, search.
  * **Phase 4: Tenants & Contracts:** Profiles, contract members, draft contract lifecycle (draft -> active -> extend -> ended/cancel).
  * **Phase 5: Service Catalog & Meter Readings:** Service creation, monthly meter reading ingestion, monotonic consumption validation.
  * **Phase 6: Invoices:** Automated generation based on room rent and readings, issuing, cancellation, outstanding balance handling.
  * **Phase 7: Payments:** Cash payments recorded by owners, bank transfers with uploaded receipts submitted by tenants, owner review (confirm/reject) cycle.
  * **Phase 8: Maintenance & Notifications:** Tenants lodging repairs, owners updates, automatic notification dispatch.
  * **Phase 9: Real Dashboard Data:** `DashboardService` performing real database projection aggregates for statistics, revenue, occupancy, and overdue debts.
* **Partially Completed Phase:** None. Backend logic is fully implemented.
* **Remaining Phase:** Phase 10 (Verification, Security Review, Docker/Deployment validation, and Continuation report).

## 3. Features Already Working & Verified
* Centralized account login, logout, and access denied routes.
* Full service-based CRUD operations for Owners (Properties, Rooms, Tenants, Contracts, Meter Readings, Invoices, Payments, Maintenance Status).
* Isolated Tenant endpoints for invoice tracking, payment evidence upload, and maintenance requests.
* Centralized role-based MVC route protection.
* Clean Separation of concerns via interface DI instead of hardcoded database logic in controllers.
* xUnit relational tests using SQLite in-memory: 13/13 tests passing successfully (including business invariant validation and authentication redirection challenge).

## 4. Current Database & Migration Status
* **Migrations Created:**
  1. `20260712160959_InitialBoardingHouseManagementSchema` (Initial database structure, using `Restrict` delete behaviors for financial logs).
  2. `20260712161723_AddTenantProfileOwnerScope` (Updates tenant model to scope under owners).
* **Database State:** Verified to compile and pass the xUnit relational test harness. The local PostgreSQL instance was not run due to Docker engine unavailability, but the schema has been reviewed.

## 5. Security & Isolation Status
* Checked-in code enforces:
  * HttpOnly & Lax cookies.
  * CSRF validation via `AutoValidateAntiforgeryTokenAttribute` registered globally.
  * Password strength and lockout policies.
  * Scope isolation: services retrieve the user context dynamically from `ICurrentUserService` and restrict operations strictly to resource owners or members.

## 6. Exact Continuation Plan
1. **Verify Views Configuration:** Perform a visual/code sweep of the Razor Views to check if they bind correctly to the strongly-typed InputModels and ViewModels, ensuring all state-changing forms have anti-forgery tokens.
2. **Mark Security Checklist:** Review and formally complete the security review checklist in `docs/20-SECURITY-CHECKLIST.md`.
3. **Generate Final Continuation Report:** Prepare and commit the continuation implementation report in `docs/23-CONTINUATION-IMPLEMENTATION-REPORT.md`.
