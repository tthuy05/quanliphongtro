# Kiểm toán hoàn thiện dự án Trọ Sinh Viên

Ngày kiểm toán ban đầu: 13/07/2026  
Nhánh: `ui` (`origin/ui`)  
Nguồn sự thật: mã nguồn hiện tại, Git, kết quả restore/build/test, mô hình EF Core, migration và kiểm tra HTTP cục bộ.

## 1. Tóm tắt điều hành

Dự án đã có nền tảng ASP.NET Core MVC, Identity, PostgreSQL/EF Core, domain model, hai migration, các service nghiệp vụ chính và giao diện dashboard có chất lượng hình ảnh tốt. Baseline mới xác nhận `dotnet restore` thành công, build không có warning/error và 13/13 test hiện có pass.

Dự án **chưa đạt definition of done**. Các báo cáo cũ đánh giá quá cao mức hoàn thiện: PostgreSQL thật chưa kết nối; `/health` trả 503; Dockerfile/Compose, báo cáo, cài đặt tài khoản và README không tồn tại; nhiều service chỉ có một phần use case; phần lớn trang quản trị dùng ID thô và thiếu detail/edit; navigation dashboard còn liên kết `href="#"`, điều khiển Mock/Demo và số doanh thu hardcode; không có endpoint tải chứng từ có phân quyền; kiểm tra upload tin vào MIME do client gửi; test authorization mới chỉ kiểm tra attribute ở ba controller và chưa kiểm tra cross-resource HTTP.

Ước lượng ban đầu theo module: nền tảng backend khoảng 65%, workflow có thể gọi bằng service khoảng 55%, UI workflow khoảng 35%, test yêu cầu khoảng 25%, deployment khoảng 20%. Trạng thái tổng thể ban đầu: **Partially complete**.

## 2. Trạng thái repository

- Repository có một commit `7495b24 feat: init ui branch and project setup`.
- Hầu hết backend, migration, test và view quản trị đang là thay đổi chưa commit. Chúng được coi là công việc của người dùng và không bị reset/restore.
- Không có `AGENTS.md`.
- Không có `README.md`, `Dockerfile`, `docker-compose.yml`, `.dockerignore`, cấu hình CI hoặc ảnh QA/browser.
- `MockDashboardService.cs` vẫn tồn tại nhưng DI runtime dùng `DashboardService`.

## 3. Baseline kỹ thuật đã chạy

| Hạng mục | Kết quả | Bằng chứng |
|---|---|---|
| Target framework | .NET 9.0 | `TroiSinhVien.csproj`, test project |
| Database provider | PostgreSQL/Npgsql 9.0.4 | `UseNpgsql`, package reference |
| Restore | Complete | `dotnet restore TroiSinhVien.slnx` thành công |
| Build Debug | Complete | 0 warnings, 0 errors |
| Automated tests | Complete cho suite hiện có | 13 passed, 0 failed |
| Migration model drift | Complete | `dotnet ef migrations has-pending-model-changes`: không có drift |
| Migration list | Complete | Hai migration được phát hiện |
| Applied migrations | Not verified | Không kết nối được `127.0.0.1:5432` |
| PostgreSQL | Broken tại máy audit | Không service/listener cổng 5432 |
| Docker | Not implemented tại máy/repo | Không có CLI và không có file Docker |
| Application host | Partially complete | Kestrel chạy ở HTTP 5020; trang login 200, `/` redirect login |
| Health check | Broken do dependency | `/health` trả 503 `Unhealthy` vì PostgreSQL không khả dụng |
| Browser click-through | Not verified | Không có DB/account seed đang hoạt động |

## 4. Ma trận module

Các trạng thái chỉ dùng: **Complete**, **Partially complete**, **Broken**, **Placeholder**, **Not implemented**, **Not verified**, **Not applicable**. Không module chức năng nào được đánh dấu Complete nếu chưa có build, test nghiệp vụ và browser workflow.

| Module | Implementation status | Frontend status | Backend status | Database status | Authorization status | Test status | Browser verification status | Missing work | Priority |
|---|---|---|---|---|---|---|---|---|---|
| Application foundation | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Partially complete | Bỏ CDN runtime production, cấu hình secret, Docker, README; health DB chưa pass | P0/P3 |
| Authentication | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Partially complete | Redirect theo role; test invalid/lockout/logout; không dùng DB thật | P1 |
| Authorization | Partially complete | Not applicable | Partially complete | Not verified | Partially complete | Partially complete | Not verified | HTTP test cross-owner/cross-tenant/admin; file authorization; kiểm tra mọi action | P1 |
| User accounts/admin | Partially complete | Placeholder | Partially complete | Not verified | Partially complete | Not implemented | Not verified | Role hiển thị/thay đổi UI, summary, chặn tự khóa/tước Admin cuối, xử lý IdentityResult | P1/P2 |
| Boarding houses | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Not implemented | Not verified | Detail/edit, duplicate check khi update, empty state, selector dùng ViewModel | P2 |
| Rooms | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Not verified | Detail/edit/status UI, filter đầy đủ, pagination UI, validation transition | P2 |
| Tenants | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Not verified | Detail/edit, history, mask giấy tờ, duplicate check khi update, account link | P1/P2 |
| Contracts | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Not verified | Detail, add member UI, service snapshot assignment, edit draft, extend/cancel UI, transaction cancel, history | P1/P2 |
| Service catalog | Partially complete | Placeholder | Partially complete | Not verified | Partially complete | Not implemented | Not verified | List/edit/deactivate; attach to contract; PerUnit/Custom input and invoice calculation | P1/P2 |
| Meter readings | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Not verified | Selector thay ID; edit rule; bảo vệ reading nguồn paid invoice; redirect mất property | P1/P2 |
| Invoices | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Not verified | Detail/review/filter/bulk; overdue update; previous-debt semantics; PerUnit/Custom; issue transaction | P1/P2 |
| Payments | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Not verified | Reject UI; evidence metadata/download; pending-total overpayment; upload rollback; receipt/detail | P1/P2 |
| Maintenance | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Partially complete | Not verified | Detail/comments display, attachment (nếu hỗ trợ), enum validation/transition UI | P2 |
| Notifications | Partially complete | Broken | Partially complete | Not verified | Partially complete | Not implemented | Not verified | Header buttons không submit; footer dead link; due/overdue/expiry producer không có scheduler/manual job | P2 |
| Dashboard | Partially complete | Broken | Partially complete | Not verified | Partially complete | Not implemented | Not verified | Dead links/mock labels/hardcode tài chính; filter tháng; debt/period semantics; selected-property activity | P1/P2 |
| Revenue report | Not implemented | Not implemented | Not implemented | Not applicable | Not implemented | Not implemented | Not verified | Toàn bộ module | P2 |
| Occupancy report | Not implemented | Not implemented | Not implemented | Not applicable | Not implemented | Not implemented | Not verified | Toàn bộ module | P2 |
| Outstanding debt report | Not implemented | Not implemented | Not implemented | Not applicable | Not implemented | Not implemented | Not verified | Toàn bộ module | P2 |
| Administration | Partially complete | Placeholder | Partially complete | Not verified | Partially complete | Not implemented | Not verified | Dashboard summary, roles, audit filters, safety invariant admin | P1/P2 |
| Account settings | Not implemented | Not implemented | Not implemented | Not applicable | Not implemented | Not implemented | Not verified | Profile, update display name/phone, change password | P2 |
| Audit logging | Partially complete | Partially complete | Partially complete | Not verified | Partially complete | Not implemented | Not verified | Bao phủ sensitive profile/service/member/meter; transaction atomicity; safe-summary tests | P1 |
| File storage | Broken | Not implemented | Partially complete | Not verified | Not implemented | Not implemented | Not verified | Magic-byte validation, download endpoint scoped, cleanup transaction, configurable size message | P1 |
| Deployment | Not implemented | Not applicable | Partially complete | Not verified | Not applicable | Not implemented | Not verified | Dockerfile/Compose, env template, production settings, key persistence, migration/startup docs thật | P0/P3 |

## 5. Các phần đã hoàn thành theo bằng chứng hiện tại

- Solution/project structure hợp lệ; .NET 9 build sạch.
- DI có implementation cho các interface hiện có.
- Global antiforgery filter, Identity cookie, lockout, HTTPS/HSTS production, rate limiting login và ba security headers đã cấu hình.
- EF model khớp snapshot migration; decimal precision, enum text, nhiều check constraint và restrict delete đã có.
- Dashboard runtime đã chuyển từ mock service sang EF projection.
- 13 relational SQLite test hiện có pass, gồm một số invariant phòng/hợp đồng/công tơ/hóa đơn/thanh toán.
- Login page render 200; protected root redirect đến login; health check báo đúng DB không khả dụng.

## 6. Phần partial, broken, placeholder và mock

### Partially complete

CRUD service có `UpdateAsync` cho property/room/tenant nhưng controller/view không cung cấp edit. Contract service có `AddMemberAsync` và `ExtendAsync` nhưng không có action/view. Danh mục dịch vụ chỉ create, chưa có list hoặc gắn snapshot vào hợp đồng. Invoice/payment/maintenance chỉ có list tối giản, không có detail đầy đủ.

### Broken

- Sidebar/mobile/user menu/dashboard card có nhiều `href="#"`; logout ở premium user menu không hoạt động.
- Notification header “mark all read” chỉ là button không form/handler; “xem tất cả” là dead link.
- Dashboard revenue panel hiển thị tổng thu/chi/lợi nhuận hardcode `58,4M/17,2M/41,2M` dù chart dùng DB.
- Meter create redirect về `propertyId=0`, tạo trang trống dù dữ liệu vừa lưu.
- Link notification invoice (`/Tenant/Invoices/{id}`) không có route tương ứng.
- `/health` hiện 503 vì database thực không tồn tại.

### Placeholder

- Các form nhập trực tiếp ID property/room/tenant/contract/invoice, không có selector scoped.
- `_ManagementLayout` là shell tối giản tách rời premium dashboard.
- Demo state switch vẫn ghi “Mock”; revenue dropdown không làm thay đổi query.
- Admin UI không hiển thị/chọn role dù controller có ChangeRole.

### Mock/hardcoded còn dùng

- `MockDashboardService.cs`: obsolete, không đăng ký DI, nên không nằm trong production flow.
- Dashboard view: mô tả “Mock Data”, demo switch và tổng tài chính hardcode.
- Hero/user initials, active property Alpine mặc định và một số text năm 2026 hardcode.
- Seed data là development-only có điều kiện, được coi là hợp lệ chứ không phải production mock.

## 7. Chức năng chưa triển khai

- Ba báo cáo bắt buộc.
- Account settings/profile/password change.
- Detail pages có dữ liệu lịch sử cho property, room, tenant, contract, invoice, payment, maintenance.
- Download payment evidence có kiểm tra owner/tenant.
- Bulk invoice generation.
- Meter edit có rule paid-source protection.
- List/edit/deactivate service catalog và attach service vào contract.
- Notification due-soon/overdue/contract-expiry production workflow.
- Docker/deployment artifacts và README setup.

## 8. Authentication và authorization

Login dùng generic error, lockout on failure, inactive check và local return URL. Cookie production dùng Secure/HttpOnly/Lax. Logout là POST. Thiếu redirect theo Admin/Tenant; Admin/Tenant login hiện bị chuyển tới Owner dashboard và nhận access denied. Authorization chủ yếu nằm trong query service và khá nhất quán ở các đường hiện có, nhưng chưa có test HTTP đủ sâu. Evidence file có metadata nhưng chưa có authorized read endpoint. Admin mutation chưa kiểm tra kết quả Identity, chưa ngăn tự khóa/tự đổi role hay tước Admin cuối.

## 9. Security

### Concern bắt buộc sửa

1. `appsettings.json` commit credential `postgres/postgres`; trái với checklist và production-safe configuration.
2. Upload tin `IFormFile.ContentType` nhưng không kiểm tra signature; file giả mạo MIME có thể lưu.
3. Không có endpoint download scoped; nếu thêm naïve endpoint sẽ rò chứng từ, nên phải thiết kế trước.
4. Tenant có thể gửi nhiều payment pending mà tổng mỗi payment riêng lẻ không vượt remaining; owner xác nhận tuần tự chặn overpay nhưng UX/race cần aggregate pending và transaction isolation/constraint phù hợp.
5. Admin action bỏ qua `IdentityResult`; role change remove rồi add không transaction và có thể để user không role.
6. `AuditLogService` cắt chuỗi bằng range nhưng không guard null; các caller hiện non-null, cần contract rõ.

### Đã có

Antiforgery toàn cục, InputModels, EF parameterization, Razor encoding, production exception handler, HSTS, headers, rate limit, random file name, outside-web-root storage và path containment.

## 10. Data integrity và migration

Hai migration hợp lệ về mặt model drift. Chưa biết migration nào đã apply. Schema có unique room code/property, contract code, invoice number/period, meter period; money/meter/date/due-day check; restrict cho lịch sử tài chính chính.

Các thiếu sót:

- Không có DB-enforced unique partial index “một active contract mỗi room”.
- Không có DB-enforced unique partial index “một active residence mỗi tenant”.
- Document nói có PostgreSQL `xmin` cho entity nghiệp vụ nhưng model chỉ có concurrency token mặc định của Identity; không có business concurrency token.
- Invoice previous debt được cộng vào hóa đơn mới trong khi hóa đơn cũ vẫn giữ remaining; dashboard sum mọi remaining sẽ double-count và payment vào hóa đơn mới không tất toán record cũ.
- `PreviousDebtAmount` trong input không được dùng, nhưng vẫn exposed ở InputModel.
- PerUnit service bị bỏ qua hoàn toàn; Custom bị tính quantity 1 không có contract-specific value.
- Cancel active contract có thể đổi room Available nhưng không đóng `RoomTenant`; thao tác này không chạy transaction.
- Update tenant không chống duplicate phone; update boarding house không chống duplicate name.
- Không có protected edit semantics cho meter reading đã được dùng trong paid invoice vì edit chưa triển khai.

## 11. Performance

Dashboard dùng projection/aggregate, không thấy N+1 rõ ở query read. Tuy nhiên chạy nhiều query tuần tự; phù hợp quy mô đồ án nhưng cần cancellation token và index coverage. Notification mark-all tải toàn bộ rows thay vì `ExecuteUpdateAsync`. Admin audit cố định 200 không pagination. Tenant/property list không pagination. `ToLower().Contains` giảm khả năng dùng index PostgreSQL; nên dùng normalized value/ILIKE cho search khi cần.

## 12. Accessibility và frontend

Premium dashboard có reduced-motion, focus styles, responsive layout và chart fallback. Chưa có browser verification ở 1440/1024/768/390. Management views là table/form tối giản, nhiều label không có `for`, validation field-level thiếu, empty states thiếu, raw ID gây lỗi thao tác. Dead buttons và anchors vi phạm keyboard/UX. CDN phụ thuộc mạng và Tailwind CDN không phù hợp production.

## 13. Deployment

Deployment status **Not implemented**. Config runtime có health check và production HSTS/secure cookie, nhưng repository thiếu Docker artifacts mà docs khẳng định có. Không có production appsettings, env example, data-protection key persistence, forwarded headers, upload volume declaration, README hoặc verified migration step. Connection string sample chứa password thật dạng mặc định.

## 14. Documentation

Docs 01-23 và decision log đã được đọc. Nhiều tài liệu frontend còn mô tả mock phase; addendum có giải thích. Docs 20, 21, 22, 23 có claim không đúng code/repo (sample credential placeholder, Docker Compose tồn tại, tất cả backend complete, browser host verified đầy đủ). Chúng phải được cập nhật theo kết quả cuối.

## 15. Ma trận test coverage

| Module | Business/authorization rule | Existing test | Result | Missing test | Required action |
|---|---|---|---|---|---|
| Auth | Unauthenticated protected route | Có | Pass | Invalid login, lockout, logout CSRF, role redirect | Thêm integration tests |
| Authorization | Controller role attribute | Có, chỉ 3 controller | Pass | Tất cả controller/action, cross-owner/tenant HTTP, admin reject | Thêm authenticated integration tests |
| Room | Unique code/property; other owner create | Có | Pass | Update duplicate, deactivate active, status transition | Thêm service tests |
| Contract | Date/overlap/activate/move-out | Có | Pass | member isolation, extend overlap, cancel closes residence, debt block | Thêm tests và fix cancel |
| Meter | Lower/duplicate/consumption | Có | Pass | no active contract, owner isolation list, paid-source edit | Thêm tests; edit nếu triển khai |
| Invoice | Total/snapshot/duplicate | Có | Pass | owner/tenant detail, issue/cancel audit, debt roll-forward, services, paid immutable | Thêm tests, sửa debt/services |
| Payment | zero/overpay/partial/full | Có | Pass | negative, tenant cross-invoice submit, confirm other owner, evidence validation/download, pending aggregate | Thêm tests |
| Maintenance | active residence create | Có một phần | Pass | tenant another room, owner another property update, transition/completion | Thêm tests |
| Dashboard | Owner aggregate/empty | Không | Not implemented | Toàn bộ | Thêm tests |
| Reports | aggregation/scope/filter | Không | Not implemented | Toàn bộ | Triển khai và test |
| Admin | admin-only/lock/role/audit | Không | Not implemented | Toàn bộ | Thêm test và safety rules |
| Account settings | ownership/password | Không | Not implemented | Toàn bộ | Triển khai và test |
| Notifications | user scope/read | Không | Not implemented | Toàn bộ | Thêm tests |

## 16. Critical bugs, blockers và thứ tự khuyến nghị

1. P0: bỏ committed DB password, tạo production-safe configuration/deployment artifacts; giữ local override qua env/user-secrets.
2. P1: sửa authorization/admin safety, upload signature/download scope, contract cancel/residence, financial debt semantics và service calculation.
3. P2: hoàn thiện UI/use cases bắt buộc, reports, account settings, detail/edit/member/service/evidence workflows; nối lại premium navigation.
4. P3: mở rộng relational/integration tests, Docker build, PostgreSQL migration/apply, health.
5. P4: browser responsive/accessibility/console/network và UX empty/validation.

## 17. Độ phức tạp ước lượng phần thiếu

| Area | Complexity | Lý do |
|---|---|---|
| Security/config/deployment baseline | Medium | Ít file nhưng ảnh hưởng startup/production |
| Contract/member/service lifecycle | High | Transaction, ownership, snapshot, UI nhiều bước |
| Invoice/debt/payment correctness | High | Financial history, concurrency, migration/test |
| Reports | Medium | Projection/filter/view, không cần schema mới |
| Account settings/admin safety | Medium | Identity workflows và authorization |
| File evidence | Medium | Binary validation, atomic cleanup, scoped download |
| CRUD detail/edit UI | Medium | Nhiều module nhưng pattern lặp lại |
| Notifications scheduled events | Medium/High | Không có background infrastructure; cần giải pháp phù hợp scope |
| Browser/E2E verification | High | Cần PostgreSQL/account data và automation browser |

## 18. Trạng thái kiểm toán

Audit ban đầu hoàn tất. Kết luận: **Partially complete**. Không có blocker build; blocker verification hiện tại là PostgreSQL/Docker không khả dụng. Việc implementation có thể tiếp tục cục bộ bằng relational SQLite tests và code review, nhưng không được tuyên bố database/browser/E2E complete trước khi chạy PostgreSQL thật.

## 19. Đối soát sau implementation (13/07/2026)

Phần 1–18 là ảnh chụp trung thực trước khi hoàn thiện. Bảng dưới đây là trạng thái mới nhất và thay thế các trạng thái ban đầu; không xóa lịch sử audit để tránh che giấu khoảng cách đã phát hiện.

| Module | Implementation | Automated/integration test | PostgreSQL thật | Browser đồ họa | Trạng thái cuối |
|---|---|---|---|---|---|
| Foundation/config/frontend assets | Complete | Complete | Not verified | Not verified | Partially complete |
| Authentication/account settings | Complete | Complete | Not verified | Not verified | Partially complete |
| Authorization/resource scope | Complete cho workflow đã triển khai | Complete cho role route và service scope trọng yếu | Not verified | Not verified | Partially complete |
| Admin/accounts/audit | Complete cho create, role, lock/unlock, safety và audit list | Complete cho create/login/route | Not verified | Not verified | Partially complete |
| Boarding houses/rooms/tenants | Complete | Complete cho scope/rule chính và HTTP render | Not verified | Not verified | Partially complete |
| Contracts/members/services | Complete | Complete | Not verified | Not verified | Partially complete |
| Meter readings | Complete | Complete, gồm edit chain và khóa khi đã lập invoice | Not verified | Not verified | Partially complete |
| Invoices/debt/payments/evidence | Complete | Complete | Not verified | Not verified | Partially complete |
| Maintenance/notifications | Complete; notification định kỳ sinh theo page access, không có background scheduler | Complete | Not verified | Not verified | Partially complete |
| Dashboard/reports | Complete | Complete cho aggregate, filter và owner scope | Not verified | Not verified | Partially complete |
| Deployment artifacts | Complete ở mức source/artifact | Release build complete; Docker run not verified | Not verified | Not applicable | Partially complete |

### Các thay đổi đã đóng gap ban đầu

- Xóa credential database khỏi tracked settings; bổ sung production settings, forwarded headers, data-protection key persistence, `.env.example`, `.dockerignore`, multi-stage `Dockerfile`, Compose PostgreSQL/migration/web và hướng dẫn vận hành.
- Thay phụ thuộc CDN bằng Tailwind/Alpine/Chart.js/Lucide pin version và self-host; asset build bằng npm và được kiểm tra qua HTTP integration test.
- Bổ sung hai migration cho unique partial indexes active lease/residence và số lượng dịch vụ hợp đồng; EF model không còn drift.
- Hoàn thiện lifecycle hợp đồng, thành viên, dịch vụ snapshot/quantity/price override, công tơ edit-chain, invoice detail/bulk/filter, ledger nợ không double-count, thanh toán/pending-overpay, evidence download có scope và magic-byte validation.
- Hoàn thiện detail/history/edit/status cho property/room/tenant, maintenance comments/transitions, notifications, account settings, dashboard dữ liệu thật và ba báo cáo owner-scoped.
- Hoàn thiện admin create/role/lock/unlock với self/last-admin protection; production bootstrap admin là opt-in một lần và không tạo sample data.
- Xóa mock dashboard, số liệu tài chính hardcode, dead `href="#"`, fake control và runtime CDN.

### Bằng chứng xác minh cuối

| Check | Kết quả |
|---|---|
| npm clean install/build/audit | Complete; 0 vulnerability |
| NuGet vulnerability audit | Complete; không có package vulnerable ở app hoặc test |
| Debug build | Complete; 0 warning, 0 error |
| Release build | Complete; 0 warning, 0 error |
| Debug tests | 27/27 passed |
| Release tests | 27/27 passed |
| Authenticated HTTP flows | Owner, Tenant, Admin login bằng Identity thật; GET/antiforgery POST và role denial pass trên relational SQLite |
| Static assets | Tailwind, Alpine và Chart.js trả HTTP 200 và non-empty trong integration test |
| EF migration/model | 4 migration được phát hiện; không có pending model changes |
| PostgreSQL apply/health | Blocked: không có listener `127.0.0.1:5432`; `/health` trả 503 đúng thiết kế |
| Docker image/Compose run | Not verified: máy audit không có Docker/Podman CLI |
| Browser responsive/console/network | Not verified: không có graphical browser session và PostgreSQL runtime |

## 20. Kết luận audit cuối

Implementation và automated critical workflow suite đã hoàn thành ở mức repository. Final overall classification: **Functionally complete with documented limitations**; production verification gates vẫn mở do dependency môi trường. Không được đổi sang `Complete and verified` trước khi apply migration trên PostgreSQL thật, health check healthy, chạy container Production và hoàn tất browser QA ở các viewport yêu cầu.
