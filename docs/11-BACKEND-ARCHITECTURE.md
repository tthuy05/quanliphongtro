# Kiến trúc backend - Trọ Sinh Viên

## Hiện trạng và mục tiêu

Repository là một ASP.NET Core MVC .NET 9 đơn project (`TroiSinhVien`) gồm Razor Views, `DashboardController`, `IDashboardService` và `MockDashboardService`. Backend mới giữ nguyên namespace, route dashboard và `DashboardPageViewModel`, đồng thời phát triển thành modular monolith:

```text
Browser -> MVC Controller -> Application Service -> ApplicationDbContext -> PostgreSQL
                              |-> authorization/validation/audit/storage
```

Không dùng generic repository. EF Core là unit-of-work và data-access abstraction. Controller chỉ bind InputModel, kiểm tra ModelState, gọi service và chuyển kết quả thành View/redirect/HTTP status.

## Cấu trúc

- `Domain/Common`, `Domain/Entities`, `Domain/Enums`, `Domain/Constants`: mô hình và invariant dùng chung.
- `Data/ApplicationDbContext.cs`, `Data/Configurations`, `Data/Seed`, `Data/Migrations`: persistence.
- `Services/Interfaces`, `Services/Implementations`: use case, quyền sở hữu, transaction và projection.
- `Models/InputModels`, `Models/ViewModels`: contract với Razor; không bind entity.
- `Authorization`: requirement/handler khi cần kiểm tra resource ngoài query service.
- `Infrastructure/Clock`, `Infrastructure/Security`, `Infrastructure/Storage`: adapter thay thế được.
- `Controllers`, `Controllers/Admin`, `Controllers/Tenant`: HTTP endpoints theo vai trò.

Dependency đi từ controller vào abstraction service; service phụ thuộc DbContext và infrastructure abstraction; Domain không phụ thuộc MVC hoặc EF Core.

## Vòng đời request

1. Cookie Identity được xác thực; endpoint yêu cầu role/policy.
2. InputModel được bind và chạy Data Annotations/custom validation.
3. Service lấy `CurrentUserId`, luôn lọc theo `OwnerId`/tenant membership trước khi đọc hoặc ghi.
4. Service áp dụng business rule, dùng transaction cho workflow nhiều bước và ghi audit.
5. Controller trả view/redirect; tài nguyên ngoài phạm vi trả 404 để tránh lộ sự tồn tại. 403 dành cho hành động role không được phép nhưng tài nguyên đã công khai trong ngữ cảnh; dữ liệu sai trả 400/ModelState.

## Authentication và authorization

ASP.NET Core Identity dùng cookie, email duy nhất, lockout và role `Admin`, `Owner`, `Tenant`. Role chỉ là lớp đầu; mọi truy vấn resource còn phải kiểm tra owner hoặc contract membership. Login redirect về dashboard phù hợp role; logout chỉ POST kèm anti-forgery.

## Validation, lỗi và logging

Data Annotations bảo vệ shape/range; service xác thực cross-field, trạng thái và tính nhất quán database. Business failure trả `ServiceResult` có lỗi tiếng Việt, không bị log như crash. Exception không dự kiến được middleware ghi structured log và hiển thị trang lỗi production; không log mật khẩu, token, căn cước, connection string hoặc nội dung chứng từ.

## Transaction, xóa mềm và audit

Transaction được dùng khi activate/end contract, generate invoice và confirm payment. Financial/history records dùng `DeleteBehavior.Restrict`/`NoAction`. Boarding house, room và catalog có `IsDeleted` query filter; dữ liệu có lịch sử được deactive/cancel, không hard delete. `CreatedAt/UpdatedAt` là `DateTimeOffset` UTC, `CreatedBy/UpdatedBy` là Identity user id. `AuditLog` lưu actor, action, entity, thời điểm và summary an toàn.

## File upload

`IFileStorageService` tạo tên ngẫu nhiên, kiểm tra MIME/extension/size, chuẩn hóa path và lưu ngoài web root theo mặc định. Metadata nằm trong `UploadedFile`; tải file phải qua endpoint kiểm tra quyền. Adapter local có thể thay bằng blob storage mà không đổi service nghiệp vụ.

## Chuyển mock sang dữ liệu thật

`MockDashboardService` được giữ trong lịch sử nhưng DI chuyển sang `DashboardService`. Service mới projection/aggregate trực tiếp trong PostgreSQL và trả đúng `DashboardPageViewModel`; database trống trả số 0 và collection rỗng.
