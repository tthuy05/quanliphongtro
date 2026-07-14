# Thiết kế cơ sở dữ liệu PostgreSQL

## Quy ước

- Entity nghiệp vụ dùng `int` identity để tương thích contract `propertyId`; Identity dùng `Guid`.
- Tiền: `numeric(18,2)`; chỉ số công tơ/khối lượng: `numeric(12,2)`; không dùng floating point cho tiền.
- Timestamp audit: `timestamp with time zone`, luôn UTC. Ngày hợp đồng/kỳ hạn dùng `date` (`DateOnly`).
- Enum lưu dạng chuỗi tiếng Anh ổn định, tối đa 32 ký tự; tiếng Việt chỉ là display mapping.
- Chuỗi có giới hạn cụ thể; email/phone/identity được normalized khi tìm kiếm.

## Bảng chính

| Bảng | Mục đích và khóa quan trọng |
|---|---|
| `AspNetUsers` | `ApplicationUser`; email duy nhất do Identity quản lý, `DisplayName`, account status |
| `BoardingHouses` | owner bắt buộc, tên/địa chỉ/liên hệ, giá điện/nước mặc định, active |
| `Rooms` | thuộc một boarding house; unique `(BoardingHouseId, RoomCode)` |
| `TenantProfiles` | hồ sơ người thuê; `UserId` nullable unique; dữ liệu cá nhân không đặt vào user |
| `RoomTenants` | lịch sử cư trú của tenant trong room, ngày vào/ra |
| `Contracts` | room, đại diện, snapshot rent/deposit/utility price, due day, status |
| `ContractMembers` | thành viên hợp đồng; unique `(ContractId, TenantProfileId)` |
| `PropertyServices` | catalog theo boarding house; calculation type và current price |
| `ContractServices` | snapshot/cấu hình dịch vụ áp dụng cho contract |
| `MeterReadings` | room, contract, meter type, billing year/month, previous/current/consumption |
| `Invoices` | contract+room, billing period, totals/debt/discount/due/status/cancel reason |
| `InvoiceDetails` | snapshot description, quantity, unit, unit price, amount, source |
| `Payments` | invoice, amount, method, status, evidence, confirmation metadata |
| `MaintenanceRequests` | tenant/room, category, priority/status, completed time |
| `MaintenanceComments` | comment theo request và author |
| `Notifications` | user-scoped notification và read time |
| `AuditLogs` | append-only security/business audit |
| `UploadedFiles` | safe stored name, original display name, MIME, size, uploader |

## Index và constraint

- Unique room code per property; unique contract code; unique invoice number.
- Unique active invoice tuple `(ContractId, BillingYear, BillingMonth)` được service bảo vệ và database index bảo vệ mọi invoice không bị cancel/delete.
- Unique meter tuple `(RoomId, MeterType, BillingYear, BillingMonth)`.
- Index owner/property/status, contract room/status/date, invoice status/due date/period, notification user/read, maintenance property/status.
- Check constraints: non-negative money/meter, month 1..12, due day 1..28, end date > start date, current reading >= previous reading, payment amount > 0.

## Delete behavior

Cascade chỉ dùng cho aggregate chưa phát hành an toàn như invoice -> draft details và maintenance -> comments khi xóa vật lý trong môi trường kiểm soát. Contract, invoice, payment, reading, audit và file metadata dùng restrict/no-action. Application workflow ưu tiên deactivation/cancellation/soft delete.

## Đồng thời

Entity có `xmin` concurrency token của PostgreSQL khi phù hợp cho cập nhật quan trọng. Payment confirmation và invoice generation chạy transaction; service re-read remaining amount trong transaction để ngăn overpayment/duplicate generation.
