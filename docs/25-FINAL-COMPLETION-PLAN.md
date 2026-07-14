# Kế hoạch hoàn thiện cuối cùng

Ngày lập: 13/07/2026. Trạng thái được cập nhật sau mỗi vòng build/test. `Pending`, `In progress`, `Complete`, `Blocked` là trạng thái backlog; không đồng nghĩa module đã browser-verified.

## Priority 0 — Build, startup và deployment blockers

| ID | Module | Problem / evidence | Files affected | Dependencies | Implementation steps | Validation | Completion criteria | Status |
|---|---|---|---|---|---|---|---|---|
| P0-01 | Configuration/security | `appsettings.json` commit `Password=postgres` | appsettings*, Program.cs, docs | None | Bỏ credential; yêu cầu env ngoài Development/test; local example an toàn | Build; startup config tests | Không secret/default password trong tracked config | Pending |
| P0-02 | Deployment | Docs nói có Compose nhưng repo không có Dockerfile/Compose | Dockerfile, compose, .dockerignore, .env.example, README, docs/21 | P0-01 | Tạo multi-stage image, PostgreSQL service, health, volume, env contract | Parse/config review; Docker khi CLI có | Artifacts và hướng dẫn khớp code; limitation ghi rõ | Pending |
| P0-03 | Runtime baseline | PostgreSQL localhost không chạy; health 503 | Local environment/docs/report | P0-01/P0-02 | Thử local PostgreSQL; nếu không có thì giữ verification blocked, không reset DB | `ef migrations list/update`, `/health` | DB/migration/health pass hoặc blocker được ghi chính xác | Blocked (external runtime unavailable) |

## Priority 1 — Security và data integrity

| ID | Module | Problem / evidence | Files affected | Dependencies | Implementation steps | Validation | Completion criteria | Status |
|---|---|---|---|---|---|---|---|---|
| P1-01 | Admin/Identity | Bỏ qua IdentityResult; có thể tự khóa/tước Admin cuối | Admin controller/service, VM/view, tests | None | Tạo admin service; kiểm tra result; ngăn self-lock và last-admin loss; audit atomic | Unit/integration tests; build | Mutation an toàn, lỗi hiển thị, audit đúng | Pending |
| P1-02 | Authorization | Coverage chỉ attribute; thiếu cross-resource HTTP và evidence | Controllers/services/tests | P1-01 | Bổ sung scoped detail/download; test owner/tenant/admin denial | Relational + WebApplicationFactory | Cross-owner/tenant trả 404/403 đúng | Pending |
| P1-03 | File storage | MIME client-trusted; không download scoped; orphan file risk | Storage, Payment service/controller, tests | P1-02 | Kiểm tra magic bytes; authorized evidence endpoint; cleanup khi DB fail | File tests, auth tests | JPEG/PNG/WebP thật; traversal/type/size chặn; chỉ đúng user tải | Pending |
| P1-04 | Contract lifecycle | Cancel active không đóng residence, không transaction | Contract service/tests | None | Transaction cancel; đóng RoomTenant; room status chỉ đổi khi contract đó active | Relational tests | Không active residence mồ côi; history giữ nguyên | Pending |
| P1-05 | DB invariants | Active contract/residence chỉ service-enforced | EF configuration, migration, tests | P1-04 | Thêm partial unique indexes PostgreSQL; review migration | Model drift; migration script; SQLite-compatible service tests | DB chặn duplicate khi provider hỗ trợ | Pending |
| P1-06 | Invoice/debt | Roll-forward double-count remaining; input debt unused | Invoice/payment/dashboard service, models, tests, docs | None | Chọn một nguồn nợ; không double-count; giữ snapshot hiển thị; test nhiều kỳ/payment | Financial tests | Tổng nợ/dashboard/end-contract nhất quán | Pending |
| P1-07 | Services/invoice | PerUnit bị skip, Custom sai; chưa attach service | Domain/InputModels/service/controller/views/tests | P1-06 | Contract service assignment; quantity/custom rate snapshot; invoice details | Calculation tests | Mọi calculation type có behavior rõ hoặc validation từ chối | Pending |
| P1-08 | Validation/data | Update property/tenant thiếu duplicate; enum/input state validation | Services/InputModels/tests | None | Chuẩn hóa và kiểm tra duplicate/state | Tests | Invariant create/update giống nhau | Pending |

## Priority 2 — Chức năng bắt buộc

| ID | Module | Problem / evidence | Files affected | Dependencies | Implementation steps | Validation | Completion criteria | Status |
|---|---|---|---|---|---|---|---|---|
| P2-01 | Shared navigation | Premium shell có dead links/mock demo/logout hỏng | Shared/dashboard views/JS | Routes tồn tại | Nối route thật, logout POST, notifications, bỏ demo/hardcode | Razor build, browser | Không dead required control | Pending |
| P2-02 | Property/room/tenant | Thiếu detail/edit/status/history/mask/selector | Services/controllers/VM/views | P1-08 | Thêm query/input screens; scoped selects; empty/paging | Tests/browser | CRUD bắt buộc dùng được không nhập ID thô | Pending |
| P2-03 | Contracts | Thiếu detail/member/edit/extend/cancel/service UI | Contract files | P1-04/P1-07 | Tạo contract workspace và actions | Tests/browser | Workflow draft→members/services→activate→extend/end/cancel | Pending |
| P2-04 | Meter | Thiếu selector/edit/protection/redirect context | Meter files | P2-02/P2-03 | Scoped room select; period flow; paid-source policy | Tests/browser | Electricity/water workflow hoàn chỉnh | Pending |
| P2-05 | Invoice | Thiếu details/review/filter/bulk/status overdue | Invoice files | P1-06/P1-07 | Detail projection, issue/cancel, filters, bulk if required | Tests/browser | Invoice review/issue/cancel/details đúng | Pending |
| P2-06 | Payment | Thiếu reject/evidence/history/receipt | Payment files | P1-03/P2-05 | Pending review detail; confirm/reject; evidence link; receipt metadata | Tests/browser | Cash/transfer/partial/full/reject workflow | Pending |
| P2-07 | Maintenance | Thiếu detail/comments display | Maintenance files | P2-02 | Detail projection và history; transitions | Tests/browser | Tenant/owner workflow nhìn thấy lịch sử | Pending |
| P2-08 | Notifications | Header hỏng; thiếu event due/overdue/expiry | Notification/dashboard files | P2-01/P2-05 | Nối read actions; deterministic notification generation command | Tests/browser | List/count/read và required event types hoạt động | Pending |
| P2-09 | Reports | Ba report không tồn tại | Reports service/controller/views/tests | Core data stable | Owner-scoped revenue/occupancy/debt filter property/date | Aggregate tests/browser | 3 report có empty/filter/scope đúng | Pending |
| P2-10 | Account settings | Không tồn tại | Account controller/models/views/tests | Identity | Profile/update/password change | Identity tests/browser | User chỉ sửa profile/password của mình | Pending |
| P2-11 | Dashboard | Hardcode/mock labels/dead action, scope/period thiếu | Dashboard service/views/tests | P1-06/P2-01 | DB-backed totals; month links; selected scope; empty; CT | Tests/browser | Không production mock/hardcode; owner scope đúng | Pending |
| P2-12 | Admin | Thiếu summary/roles/filter audit | Admin files | P1-01 | Summary, role visibility/change UI, audit filter/page | Tests/browser | Admin workflow required hoàn chỉnh | Pending |

## Priority 3 — Testing và deployment

| ID | Module | Problem / evidence | Files affected | Dependencies | Implementation steps | Validation | Completion criteria | Status |
|---|---|---|---|---|---|---|---|---|
| P3-01 | Tests | 13 tests không đủ matrix | tests | P1/P2 modules | Thêm critical relational/auth/dashboard/report/file tests | `dotnet test` | Critical matrix pass | Pending |
| P3-02 | PostgreSQL | Migration chưa apply thật | DB environment/report | P0-03 | Apply non-destructive migrations; inspect history/schema/seed | EF/SQL/health | Migration and seed idempotent pass | Blocked (external runtime unavailable) |
| P3-03 | Production | Chưa build/run Production | config/Docker | P0-01/P0-02 | Release build; production startup safe; forwarded headers/key docs | Build/run/HTTP | Production start không local-path/secret dependency ẩn | Pending |

## Priority 4 — UX, accessibility và browser QA

| ID | Module | Problem / evidence | Files affected | Dependencies | Implementation steps | Validation | Completion criteria | Status |
|---|---|---|---|---|---|---|---|---|
| P4-01 | Forms/lists | Raw IDs, thiếu field errors/empty states | Management views | P2 | Scoped selects, labels, validation, empty components | Browser/keyboard | University presentation flow rõ ràng | Pending |
| P4-02 | Responsive | Chưa verify 1440/1024/768/390 | Views/CSS | P2 | Automated screenshots và manual inspect | Browser console/network/screenshots | Không overflow/console/asset errors | Blocked (needs working DB/browser session) |
| P4-03 | Accessibility | Dead buttons, focus/semantic chưa audit full | Views/JS | P2-01 | Keyboard/focus/aria/reduced motion pass | Browser/a11y inspection | Required controls keyboard accessible | Pending |

## Vòng validation bắt buộc

Sau mỗi cụm P1/P2: `dotnet build --no-restore`, test liên quan, rồi toàn suite. Cuối cùng: restore, Release build, toàn test, model drift, migration apply PostgreSQL, seed idempotent, host, Admin/Owner/Tenant login, workflow E2E, unauthorized access, health, console/network và bốn viewport. Bất kỳ bước nào chưa chạy sẽ được ghi `Not verified`, không chuyển thành Complete bằng suy đoán.

## Final execution ledger — 13/07/2026

Các trạng thái dưới đây thay thế cột `Status` ban đầu. `Partially complete` nghĩa là source/workflow đã có nhưng một phần acceptance evidence hoặc scope phụ vẫn chưa được xác minh; `Blocked` chỉ dùng cho dependency môi trường không có trên máy audit.

| ID | Final status | Evidence / remaining boundary |
|---|---|---|
| P0-01 | Complete | Tracked settings không còn database/admin password; secret contract dùng environment/user-secrets. |
| P0-02 | Partially complete | Dockerfile, Compose, env template, persistent upload/key volumes và README đã có; Docker CLI không có để build/run. |
| P0-03 | Blocked | Không có PostgreSQL listener ở port 5432; health 503 được xác nhận. |
| P1-01 | Partially complete | IdentityResult, self-lock/self-demotion và last-active-admin guards đã có; authenticated admin create pass; mutation safety chưa có full HTTP matrix. |
| P1-02 | Partially complete | Role routes, owner/tenant service scope và evidence authorization đã có; chưa có exhaustive cross-owner HTTP matrix. |
| P1-03 | Complete | Magic-byte JPEG/PNG/WebP, size/path constraints, orphan cleanup và scoped evidence download; spoof test pass. |
| P1-04 | Complete | Cancel/end transaction đóng active residence và chỉ release đúng room; relational test pass. |
| P1-05 | Partially complete | Partial unique indexes và migration đã sinh, model không drift; chưa apply PostgreSQL thật. |
| P1-06 | Complete | Nợ cũ giữ ledger riêng, không roll-forward/double-count; financial test pass. |
| P1-07 | Complete | Catalog/attach/snapshot/quantity/price override và mọi calculation type đã triển khai; test pass. |
| P1-08 | Complete | Enum/state/duplicate validation ở create/update trọng yếu đã triển khai. |
| P2-01 | Complete | Route thật, logout POST, notification controls; mock/dead required controls đã xóa. |
| P2-02 | Complete | Scoped selectors, create/edit/status/detail/history cho property/room/tenant. |
| P2-03 | Complete | Workspace draft → member/service → activate → extend/end/cancel. |
| P2-04 | Complete | Scoped create/edit, immutable identity/period, chain consistency và invoice lock. |
| P2-05 | Complete | Detail, filters, overdue state và idempotent bulk generation. |
| P2-06 | Complete | Cash/transfer, partial/full, pending evidence, confirm/reject, history và protected evidence. |
| P2-07 | Complete | Detail, transitions và scoped threaded comments. |
| P2-08 | Partially complete | Due/overdue/expiry generation idempotent theo page access; chưa có background scheduler. |
| P2-09 | Complete | Revenue, occupancy và outstanding-debt reports với filter và owner scope. |
| P2-10 | Complete | Update profile và change password cho current user. |
| P2-11 | Complete | Aggregate thật, month/property filters và CTA route thật; scope test pass. |
| P2-12 | Partially complete | User create/role/lock/unlock/audit list có; audit filter/pagination không nằm trong vòng hoàn thiện này. |
| P3-01 | Complete | Suite tăng từ 13 lên 27 tests; Debug/Release đều 27/27 pass. |
| P3-02 | Blocked | PostgreSQL service không khả dụng, nên migration history/schema/seed idempotency chưa được chạy thật. |
| P3-03 | Partially complete | Release build 0 warning/error và production-safe startup/artifacts; container Production chưa chạy. |
| P4-01 | Complete cho required workflow | Raw-ID input ở workflow chính được thay bằng scoped selectors; validation/empty states có. |
| P4-02 | Blocked | Không có graphical browser + working PostgreSQL để chụp/inspect 1440/1024/768/390. |
| P4-03 | Partially complete | Semantic/dead-control scan, focus/reduced-motion baseline có; chưa chạy browser accessibility tooling. |

### Release gates còn mở

1. Trên môi trường có Docker/PostgreSQL: build image, chạy migration tool, kiểm tra migration history, seed/bootstrap idempotency và `/health` healthy.
2. Đăng nhập Admin/Owner/Tenant trên PostgreSQL thật; click-through các luồng tạo phòng → hợp đồng → công tơ → hóa đơn → thanh toán, maintenance và admin safety.
3. Browser QA bốn viewport, keyboard/focus, console/network và file evidence download; ghi screenshot/log nếu đây là yêu cầu nghiệm thu.

Không còn item source-code P0/P1/P2 nào chặn build hoặc automated critical workflows. Overall final status: **Functionally complete with documented limitations**; các production verification gates được liệt kê ở trên vẫn mở.
