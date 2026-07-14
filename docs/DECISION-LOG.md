# Decision Log - Trọ Sinh Viên (Internal technical name: TroHub)

This log records major architecture, design, library selection, and implementation decisions made during the project.

---

## [2026-07-12] Decision 1: Use ASP.NET Core MVC Razor Views instead of SPA (React/Vue)
*   **Context:** The student project requires a highly polished visual interface, but must be built inside the existing ASP.NET Core MVC environment.
*   **Alternatives Considered:** Creating a separate React frontend using Vite, or building with Blazor.
*   **Reason:** Integrating single page frameworks (SPA) increases deployment complexity, asset bundling size, and disrupts standard serverside rendering benefits. Using Razor Views keeps architecture monolithic, fast, and SEO-friendly.
*   **Consequences:** Frontend state management must be solved using lightweight vanilla scripts and Alpine.js rather than virtual DOM models.

---

## [2026-07-12] Decision 2: Styling via Tailwind CSS Utility System
*   **Context:** Standard Bootstrap styles feel generic and do not match modern premium SaaS aesthetics (e.g. Stripe, Linear).
*   **Alternatives Considered:** Vanilla CSS variables, Bootstrap, or custom Sass stylesheets.
*   **Reason:** Tailwind enables rapid layout building and simplifies design token integration.
*   **Consequences:** Markup contains long utility strings unless repetitive components are extracted into Razor Partials or CSS utility classes.

---

## [2026-07-12] Decision 3: Decoupled Mock Service Implementation
*   **Context:** Phase 1 requires mock data, but future phases will require database storage (Entity Framework Core & SQL Server).
*   **Alternatives Considered:** Hardcoding lists directly inside Razor views.
*   **Reason:** Hardcoding data prevents future integrations and creates code clutter. Defining `IDashboardService` enables replacing the mock provider with an EF Core database service later with zero changes to Razor views.
*   **Consequences:** Controller references the interface instead of concrete mock classes.

---

## [2026-07-12] Decision 4: Use CSS + IntersectionObserver for Motion
*   **Context:** Motion design is required, but adding multiple complex animation libraries (e.g. Framer Motion, GSAP) is forbidden.
*   **Alternatives Considered:** GSAP, Anime.js, or raw intervals in JS.
*   **Reason:** Combining CSS animations, Alpine transitions, and an IntersectionObserver maximizes GPU-accelerated performance while keeping bundle size minimal. Parallax is supplemented with a simple requestAnimationFrame scroll hook.
*   **Consequences:** Complex sequence paths require manual CSS timing declarations.

---

## [2026-07-12] Decision 5: Chart.js for Visual Data representation
*   **Context:** Dynamic revenue line charts and occupancy donuts are required.
*   **Alternatives Considered:** ApexCharts, D3.js.
*   **Reason:** Chart.js is lightweight, highly configurable, responsive, and easy to link to C# viewmodel structures.
*   **Consequences:** Canvas element sizing rules must be strictly configured to prevent Cumulative Layout Shift (CLS).

---

## [2026-07-12] Decision 6: Keep supplied hero image watermark intact
*   **Context:** The project requires using the provided landscape image as the welcome hero visual.
*   **Alternatives Considered:** Removing the watermark or cropping the image.
*   **Reason:** Respect copyright and developer boundaries. The watermark is treated as a temporary asset indicator.
*   **Consequences:** A note must specify replacing this asset with a licensed image prior to commercial production.

---

## [2026-07-12] Decision 7: Rename public brand from TroHub to Trọ Sinh Viên
*   **Context:** The user requested updating the public branding of the application from "TroHub" to "Trọ Sinh Viên" (ShortName: "TSV", Tagline: "Quản lý phòng trọ đơn giản và hiệu quả").
*   **Alternatives Considered:** Renaming all namespaces, project filenames, folders, assemblies, and database entities to match the public brand name.
*   **Reason:** Keeping internal technical names as "TroHub" (and "TroiSinhVien" namespaces) avoids introducing build-breaking changes, assembly misalignments, routing failures, or build warnings. Only user-facing views, titles, alt texts, and documentation are updated. A complete refactoring rename can be performed later as a separate task.
*   **Consequences:** Centralized branding configuration constants `BrandConstants` are added in C# to clean up Razor templates. Developers should reference `BrandConstants.ProductName` instead of hardcoding the brand.

---

## [2026-07-12] Decision 8: PostgreSQL, EF Core and ASP.NET Core Identity
* **Context:** The repository has no persistence or authentication and targets .NET 9.
* **Decision:** Use Npgsql EF Core, `IdentityDbContext` with Guid user keys, and integer business keys to retain the existing dashboard `propertyId` contract.
* **Consequences:** PostgreSQL is required for deployment; SQLite is allowed only as an isolated relational test adapter where PostgreSQL is unavailable.

## [2026-07-12] Decision 9: Application services instead of generic repositories
* **Context:** Ownership and financial workflows require use-case-specific queries and transactions.
* **Decision:** Controllers call focused services; services use `ApplicationDbContext` directly and enforce owner/member scope.
* **Consequences:** Authorization cannot be bypassed by a generic `GetById`; tests target scoped use cases.

## [2026-07-12] Decision 10: Preserve financial and contract history
* **Context:** Catalog prices and profiles change while invoices/payments must remain auditable.
* **Decision:** Store price snapshots, UTC audit fields, cancellation reasons and use soft deletion/deactivation plus restrictive foreign keys.
* **Consequences:** Paid financial corrections require an explicit audited workflow, not direct CRUD edits.

## [2026-07-12] Decision 11: Regenerate the unapplied initial migration after review
* **Context:** Review of the first generated migration found cascade deletes from `Contracts` to `ContractMembers` and `ContractServices`.
* **Decision:** Because the migration had not been applied to any database, remove and recreate it once after changing both relationships to `Restrict`.
* **Consequences:** The checked-in initial migration directly represents the reviewed historical-data policy; there is no misleading corrective migration.
