# Service và Controller Contracts

## Quy ước service

Mọi command trả `ServiceResult`/`ServiceResult<T>` gồm `Succeeded`, `Value`, `ErrorCode`, `Errors`. Các lỗi dự kiến: `validation`, `not_found`, `conflict`, `forbidden`. Query trả ViewModel/DTO read-only và luôn scoped theo current user.

| Service | Use case chính |
|---|---|
| `IBoardingHouseService` | list/detail/create/update/deactivate của owner |
| `IRoomService` | filter/search/page/detail/create/update/status/deactivate |
| `ITenantService` | owner list/detail/create/update; tenant self-profile |
| `IContractService` | draft/member/activate/extend/end/cancel/move-out |
| `IServiceCatalogService` | catalog theo property và contract service snapshot |
| `IMeterReadingService` | list/create/edit hợp lệ, server-side previous reading |
| `IInvoiceService` | generate/review/issue/cancel/filter/detail/bulk |
| `IPaymentService` | record/submit evidence/confirm/reject/history |
| `IMaintenanceService` | tenant create/list; owner priority/status/comment |
| `IDashboardService` | aggregate owner theo property/kỳ, giữ ViewModel cũ |
| `INotificationService` | list/count/mark one/all theo user |
| `IAuditLogService` | append safe audit record |
| `IFileStorageService` | validate/save/open/delete authorized file |
| `ICurrentUserService` | authenticated user id/role |
| `IDateTimeProvider` | UTC now/today để test được |

## Controller

- `AccountController`: GET/POST login, POST logout, access denied.
- `DashboardController`: owner dashboard, `propertyId` optional; service chọn property đầu tiên thuộc owner nếu không có.
- Owner controllers: `BoardingHouses`, `Rooms`, `Tenants`, `Contracts`, `Services`, `MeterReadings`, `Invoices`, `Payments`, `Maintenance`.
- `TenantController`: room/contract/invoice/payment/maintenance/notification của user hiện tại.
- `AdminController`: users, lock/unlock, audit summary.

GET dùng ID route/query. POST dùng InputModel và `[ValidateAntiForgeryToken]`; không nhận EF entity. Controller map `not_found` -> 404, `forbidden` -> 403, validation/conflict -> ModelState hoặc 400. Route dashboard hiện tại `/Dashboard/Index?propertyId=&month=` được giữ.

## Dashboard contract

`DashboardPageViewModel` tiếp tục dùng `int ActivePropertyId`, property selector, hero, 5 stats, revenue/occupancy chart, recent invoices, overdue, expiring contracts, maintenance, activity, quick actions và notifications. Chỉ nguồn dữ liệu đổi từ mock sang projection database.
