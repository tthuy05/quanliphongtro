# Final Project Implementation Report — Trọ Sinh Viên

Ngày hoàn tất vòng implementation: 13/07/2026  
Repository: `quanliphongtro`  
Branch được audit: `ui`  
Trạng thái tổng thể: **Functionally complete with documented limitations**

## 1. Executive summary

Repository đã được nâng từ prototype/dashboard-first thành ứng dụng ASP.NET Core MVC có workflow quản lý khu trọ dùng được ở mức source và automated integration. Các gap nghiêm trọng ban đầu về secret, mock data, dead navigation, lifecycle hợp đồng, tính tiền dịch vụ, nợ, upload evidence, authorization, reports, account/admin và deployment artifacts đã được xử lý.

Debug và Release đều build sạch; 27/27 automated tests pass. Test suite dùng relational SQLite cho business rules và `WebApplicationFactory` cho Identity login, cookie, antiforgery POST, role routing, view rendering và static assets. EF model khớp bốn migration. npm và NuGet vulnerability audit đều sạch tại thời điểm kiểm tra.

Không tuyên bố `Complete and verified`: máy audit không có PostgreSQL listener, Docker/Podman CLI hoặc graphical browser session. Vì vậy migration apply trên PostgreSQL, healthy runtime, production container và responsive/browser QA vẫn là release gates bắt buộc.

## 2. Initial repository evidence

### Initial state, build, tests and migrations

Repository ban đầu chỉ có một commit `7495b24 feat: init ui branch and project setup`; phần lớn backend hiện hữu là working-tree chưa commit và được giữ nguyên thay vì reset. Không có `AGENTS.md`, README, Dockerfile, Compose, CI hoặc browser QA artifacts. Dashboard runtime đã dùng EF service nhưng mock service, demo JavaScript, dead links và hardcoded financial value vẫn còn trong source/UI.

| Initial check | Evidence |
|---|---|
| Restore | Passed |
| Debug build | Passed, 0 warnings, 0 errors |
| Automated tests | 13 passed, 0 failed |
| EF migrations | 2 source migrations; model had no pending changes |
| Applied migrations | Not verified; PostgreSQL `127.0.0.1:5432` unavailable |
| Local host | Login 200, protected root redirected, `/health` 503 because DB unavailable |
| Docker/browser | Docker artifacts/CLI absent; graphical browser test not performed |

### Initial completion estimate by area

Đây là estimate từ audit ban đầu, không phải số đo sau implementation:

| Area | Initial estimate |
|---|---:|
| Backend foundation | 65% |
| Callable business workflows | 55% |
| Workflow UI | 35% |
| Required automated tests | 25% |
| Deployment readiness | 20% |

### Missing work discovered

- Reports, account settings, Docker/production config và nhiều detail/edit/history screens chưa tồn tại.
- Contract member/service/extend/cancel, meter edit, invoice detail/filter/bulk và payment evidence workflow chưa hoàn chỉnh.
- Notifications/dashboard navigation có dead/fake controls; mock/demo/hardcoded values còn trong production flow.
- Admin role/lock safety, one-time bootstrap, service catalog management và authenticated HTTP tests còn thiếu.

### Security issues discovered

- Tracked development connection string chứa default password; runtime phụ thuộc CDN.
- Upload tin MIME do client khai báo, chưa magic-byte inspect, thiếu authorized download và orphan cleanup.
- Admin mutation bỏ qua một số `IdentityResult`, có nguy cơ self-lock/self-demotion hoặc mất Admin cuối.
- Authorization test ban đầu chủ yếu kiểm tra attributes, chưa chạy Identity/cookie/antiforgery HTTP flow.

### Data-integrity issues discovered

- Thiếu database-enforced unique active contract/residence indexes.
- Contract cancel có thể release room nhưng không đóng residence và không atomic.
- Previous debt roll-forward có thể double-count; `PerUnit`/`Custom` thiếu contract quantity semantics.
- Meter reading thiếu protected edit policy; pending transfer chưa được tính khi kiểm tra overpayment.
- Duplicate/update/state validation không đồng nhất ở một số property/tenant workflows.

## 3. Architecture delivered

- ASP.NET Core MVC .NET 9, Razor views, built-in Identity với `Guid` key.
- EF Core 9 + Npgsql/PostgreSQL cho runtime; SQLite chỉ là relational test adapter.
- Service-oriented application layer cho dashboard, property, room, tenant, contract, service catalog, meter, invoice, payment, maintenance, notification và reports.
- Owner resource scoping tại query/service boundary; Tenant được nối với `TenantProfile` và active residence; Admin dùng role policy.
- Global antiforgery, production exception handling/HSTS, login rate limiting, security headers, forwarded headers và persistent data-protection keys.
- Private evidence storage ngoài `wwwroot`; file chỉ trả qua authorized controller.
- Tailwind/Alpine/Chart.js/Lucide pin version và self-host; runtime không cần CDN.
- Production container tách frontend asset build, .NET build, migration tool và web runtime.

## 4. Change and file inventory

### Files created

- Solution/deployment/frontend: `TroiSinhVien.slnx`, `.config/dotnet-tools.json`, `README.md`, `Dockerfile`, `docker-compose.yml`, `.dockerignore`, `.env.example`, `appsettings.Production.json`, `package.json`, `package-lock.json`, `tailwind.config.js`, `Styles/tailwind.css`, `scripts/copy-vendor.mjs`, generated CSS và local vendor assets.
- Backend: Account/Admin/Owner-management/Tenant/Reports/PaymentEvidence controllers; `Data/**`, `Domain/**`, `Infrastructure/**`, management Input/ViewModels, service interfaces/implementations và real `DashboardService`.
- UI: management layout và Account, Admin, BoardingHouses, Rooms, Tenants, Contracts, PropertyServices, MeterReadings, Invoices, Payments, Maintenance, Notifications, Reports và Tenant views.
- Tests: `tests/TroiSinhVien.Tests/**`, gồm business rules, completion/security, authorization và authenticated flow suites.
- Evidence/docs: documents 11–26, trong đó 24 là audit, 25 là execution plan và 26 là báo cáo này.

### Files modified

- Startup/config: `Program.cs`, `TroiSinhVien.csproj`, `.gitignore`, `appsettings.json`, `appsettings.Development.json`.
- Existing dashboard shell: Dashboard/Home controllers, dashboard ViewModel/views/partials, shared layout/navigation/property/notification/user components và toast script.
- Existing specifications: documents 01, 02, 05, 08, 09, 10, 15, 17, 20–23 và decision log được sửa hoặc bổ sung reconciliation note.

### Files removed or replaced

- `MockDashboardService.cs` được thay bằng EF-backed `DashboardService`.
- Demo dashboard JavaScript và fake confirm/search/empty/skeleton partials bị xóa khi không còn consumer thật.
- Required `href="#"`, fake interactions, hardcoded financial totals và runtime CDN references được loại bỏ.

### Modules completed and repaired

- Mới hoàn thiện: reports, account settings, service catalog UI, protected evidence download, production deployment artifacts và one-time admin bootstrap.
- Được sửa/hoàn chỉnh: auth/authorization, admin, property/room/tenant, contract/member/service, meter, invoice/debt, payment, maintenance, notifications và dashboard.

## 5. Implemented workflows

### Owner operations

- Boarding house: list, create, edit, detail, activate/deactivate và danh sách phòng.
- Room: scoped selector/filter, create, edit, detail, current tenants, contract history và state validation.
- Tenant: create/edit/detail, owner-only sensitive profile, residence/contract history và account-link support.
- Contract: create draft, attach member/service, negotiated quantity/price, activate, extend, end và cancel transactionally.
- Meter: electricity/water create/edit; immutable room/type/period; previous/next chain update; edit blocked khi đã được invoice dùng.
- Invoice: detail/line items, filters, issue/cancel state, overdue projection và idempotent bulk generation theo property/kỳ.
- Payment: cash/transfer, partial/full, evidence review, confirm/reject, payment history và prior/account debt display.
- Maintenance: detail, status transitions và threaded comments giữa tenant/owner.
- Dashboard và reports: month/property filter, real aggregates, revenue, occupancy và outstanding debt.

### Tenant operations

- Role-aware login redirect và account settings/password change.
- Xem invoice/detail/payment history và debt đúng contract của mình.
- Gửi transfer payment kèm evidence; pending total ngăn overpayment.
- Tạo maintenance request từ active residence, xem detail và comment.
- Xem/đánh dấu notification; due/overdue/contract-expiry notifications được tạo idempotently khi dashboard/notification pages được truy cập.

### Admin operations

- List user/role/status; create Owner/Admin hoặc Tenant gắn với unlinked profile.
- Change role, lock/unlock với xử lý `IdentityResult`.
- Chặn self-lock, self-demotion và mất active Admin cuối cùng.
- Audit-log list và mutation audit.
- Optional one-time production bootstrap admin; mặc định tắt, không tạo sample data.

## 6. Database and financial integrity

Các migration hiện có:

1. `20260712160959_InitialBoardingHouseManagementSchema`
2. `20260712161723_AddTenantProfileOwnerScope`
3. `20260713060259_EnforceActiveLeaseInvariants`
4. `20260713063655_AddContractServiceQuantity`

### Database and migration changes

- Migration 3 thêm PostgreSQL partial unique indexes để chặn nhiều active contract trên một room và nhiều active residence trên một tenant.
- Migration 4 thêm `ContractService.Quantity`, default/backfill an toàn là 1 và positive check constraint.
- EF snapshot/configuration được cập nhật; model drift check pass nhưng apply status trên PostgreSQL chưa được xác minh.

### Business rules implemented

- Unique active contract per room và unique active residence per tenant bằng PostgreSQL partial unique index.
- Unique invoice per contract/billing period và meter reading per room/type/period.
- Check constraints cho money, contract dates, due day, meter consumption và service quantity.
- `ContractService` snapshot tên/đơn vị/calculation type/unit price/quantity; catalog change không làm sai invoice lịch sử.
- `FixedPerRoom` dùng quantity 1; `PerPerson` dùng active member count; `PerUnit`/`Custom` dùng negotiated quantity.
- Nợ invoice cũ giữ ở ledger riêng. Invoice mới không roll-forward số nợ cũ vào total, tránh double-count trong dashboard và account debt.
- Contract cancel/end đóng active residence và chỉ release room thích hợp trong cùng transaction.

`dotnet ef migrations has-pending-model-changes` xác nhận không có model drift. Applied migration status chưa xác minh vì PostgreSQL không khả dụng.

## 7. Security controls

### Authentication work

- Identity login/logout, generic failure, lockout policy, role-aware landing, profile update và password change.
- Admin user creation và Tenant-profile association dùng `UserManager`/`RoleManager` result handling.
- Development sample accounts chỉ tạo khi bật seed và cung cấp secret; Production bootstrap Admin là opt-in một lần.

### Authorization work

- `[Authorize]`/role policies trên management, tenant và admin routes; global antiforgery cho state changes.
- Owner-scoped queries/mutations cho property resources; tenant invoice/payment/maintenance dựa trên linked profile và contract/residence.
- Payment evidence download kiểm tra property owner hoặc tenant membership thay vì phục vụ file tĩnh.

### Security implementation

- Không có tracked database/admin password; production secrets phải đến từ environment/secret store.
- Cookie Identity, secure cookie ở Production, HSTS/HTTPS, global antiforgery và login rate limit.
- Owner/Tenant/Admin route authorization và resource ownership checks trong services/controllers.
- Evidence upload chỉ chấp nhận JPEG/PNG/WebP sau magic-byte inspection, giới hạn kích thước cấu hình được, tên/path do server kiểm soát và cleanup orphan khi DB mutation lỗi.
- Evidence download chỉ cho property owner hoặc tenant thuộc contract của invoice.
- Confirmed payment và lịch sử tài chính không bị hard delete; audit log tránh ghi identity document/evidence bytes.
- Production sample seed tắt; bootstrap admin phải bật rõ ràng một lần rồi tắt/xóa secret.
- NuGet audit: không có vulnerable package. Native SQLite test dependency đã được nâng lên bản không bị advisory hiện tại đánh dấu.

Giới hạn còn lại: malware scanning, MFA, distributed rate limiting, audit export/retention và full penetration test chưa nằm trong scope repository này.

## 8. Automated tests and verification evidence

| Verification | Result |
|---|---|
| `npm ci --ignore-scripts` | Complete |
| `npm run build:assets` | Complete |
| npm audit during clean install | 0 vulnerabilities |
| `dotnet restore TroiSinhVien.slnx` | Complete |
| Debug build | 0 warnings, 0 errors |
| Release build | 0 warnings, 0 errors |
| Debug automated tests | 27 passed, 0 failed, 0 skipped |
| Release automated tests | 27 passed, 0 failed, 0 skipped |
| NuGet transitive vulnerability audit | No vulnerable packages |
| EF pending model changes | None |
| Authenticated HTTP integration | Owner/Tenant/Admin login, role denial, antiforgery POST and core page render pass |
| Static asset HTTP integration | Tailwind/Alpine/Chart.js HTTP 200 and non-empty |
| Dead `href="#"` scan | No matches in `Views`/`wwwroot` |
| Runtime CDN scan | No external runtime CDN references; source package filenames containing `cdn.min.js` are copied locally only |
| `git diff --check` | Pass; only Windows LF→CRLF notices |

Test coverage tập trung vào contract lifecycle, service snapshot/calculation, debt semantics, pending overpayment, payment evidence, meter edit/lock, dashboard/report scope, notification idempotency, maintenance comments, bulk invoice generation, upload spoof rejection, controller authorization và authenticated role flows.

14 test cases được bổ sung so với baseline 13-test suite, đưa tổng số lên 27. Các test mới bao phủ contract cancel/residence, ledger debt, pending overpayment, evidence storage/signature, service snapshot/quantity, meter edit/invoice lock, owner report/dashboard scope, notification idempotency, maintenance comments, bulk invoice, authenticated Identity login, real antiforgery POST và static assets.

## 9. Browser and end-to-end results

- Kestrel Development host đã start thành công trên `http://127.0.0.1:5021` trong smoke test rồi được dừng sạch.
- Login page, Tailwind CSS và Alpine local lần lượt trả HTTP 200; protected-route redirect và Chart.js asset được kiểm tra thêm qua integration test.
- `/health` trả 503 `Unhealthy` vì `127.0.0.1:5432` không có listener. Đây là kết quả đúng thiết kế, không phải lỗi được che giấu.
- Không có graphical browser executable/session để kiểm tra screenshot, console, network, keyboard/focus và các viewport 1440/1024/768/390.
- Không có Docker/Podman CLI để build image hoặc chạy Compose.

Do đó `browser verified`, `PostgreSQL verified` và `production container verified` đều có trạng thái **Not verified**.

End-to-end result: **automated HTTP integration passed for the covered workflows, but full graphical-browser E2E was not performed**. Responsive result: **Not verified**. Empty-data and server validation behavior có automated/service coverage và Razor views tương ứng, nhưng chưa được visual-inspect ở browser.

## 10. Deployment readiness and procedure

1. Cài Docker Engine/Compose hoặc chuẩn bị PostgreSQL 15+.
2. Sao chép `.env.example` thành `.env`; thay mọi placeholder bằng secret thật, không commit file này.
3. Build và khởi động database:

   ```powershell
   docker compose up -d postgres
   docker compose --profile tools run --rm migrate
   ```

4. Nếu là lần triển khai đầu, đặt `BOOTSTRAP_ADMIN=true` và admin email/password secret, start `web` một lần, sau đó ngay lập tức đặt lại `false` và xóa bootstrap password.
5. Khởi động application:

   ```powershell
   docker compose up --build -d web
   docker compose ps
   ```

6. Xác nhận `/health` healthy, migration history đủ bốn migration, upload và data-protection key volumes writable/persistent.
7. Chạy smoke/E2E với Admin, Owner, Tenant. Không dùng `docker compose down -v` trên dữ liệu cần giữ.

Production web container không tự migrate và không sample-seed. Migration là deployment step có chủ đích, nên backup/rollback phải được chuẩn bị trước khi chạy trên database có dữ liệu.

Deployment readiness: source artifacts và Release build đã sẵn sàng; actual deployment vẫn bị chặn bởi PostgreSQL/Docker/runtime QA chưa chạy.

## 11. Known limitations, future enhancements and blockers

### Blocked by environment

- Apply/rollback/idempotency test bốn migration trên PostgreSQL thật.
- Healthy database runtime và production container smoke test.
- Admin/Owner/Tenant click-through trên PostgreSQL.
- Browser responsive, accessibility, console/network và evidence-download QA.

### Non-blocking enhancements

- Đưa notification generation sang durable background scheduler nếu cần gửi sự kiện khi không có page access.
- Thêm exhaustive cross-owner/cross-tenant HTTP authorization matrix.
- Thêm audit log filter/pagination, MFA, malware scanning và observability/backup restore drill.
- Thêm receipt document/export nếu nghiệp vụ yêu cầu chứng từ in được.

### Unfinished-item impact matrix

| Unfinished item | Why unfinished | What is needed | Required for university presentation | Blocks deployment |
|---|---|---|---|---|
| PostgreSQL migration/apply/health | Audit machine has no PostgreSQL service/listener | PostgreSQL 15+; apply four migrations; inspect history; run seed/bootstrap idempotency and health | Yes, for a live data presentation | Yes |
| Docker image/Compose runtime | Docker/Podman CLI is not installed | Build all targets, run Compose, inspect volumes/health/restart behavior | No; local `dotnet run` can be used | Yes, for the documented container deployment |
| Graphical E2E/responsive/a11y QA | No graphical browser session and no working DB runtime | Chrome/Edge automation or manual session at 1440/1024/768/390; console/network/keyboard checks | Yes | Yes for verified release; not for source build |
| Exhaustive cross-resource HTTP matrix | Critical scopes are covered, but every owner/tenant ID permutation was not automated | Add multi-owner/multi-tenant WebApplicationFactory fixtures and assert 403/404 for every detail/mutation route | No | No for academic/local use; recommended before public exposure |
| Background notification scheduler | Deliberate on-page deterministic generation was chosen | Add durable scheduler/outbox if notifications must appear without page access | No | No under current documented behavior |
| Audit filters/pagination and receipt export | Non-critical workflow enhancements | Add query filters/paging and optional printable/export artifact | No | No |
| MFA, malware scanning, distributed rate limiting and observability drills | Operational security features exceed repository completion scope | Select production providers/policies and run security/backup/restore exercises | No | Policy-dependent for public production; no for university demo |

## 12. Final status

| Area | Status |
|---|---|
| Source implementation | Complete cho required repository workflows |
| Build and automated critical workflows | Complete |
| Security/config baseline | Complete; external security operations remain deployment concerns |
| Database model and migrations | Complete at source/model level; PostgreSQL apply Not verified |
| Deployment artifacts | Complete at source level; container execution Not verified |
| Graphical browser QA | Not verified |
| Overall project | **Functionally complete with documented limitations** |

Điều kiện để nâng trạng thái thành `Complete and verified`: PostgreSQL migration/health pass, Docker Production run pass, authenticated E2E pass trên database thật và browser QA pass ở các viewport yêu cầu. Không có bằng chứng đó thì không được coi dự án là production-verified.
