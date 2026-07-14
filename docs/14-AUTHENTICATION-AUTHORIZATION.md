# Authentication và Authorization

## Identity

Ứng dụng dùng `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`. Email là username và unique. Password development tối thiểu 8 ký tự, có hoa/thường/số; production có thể tăng qua configuration. Lockout: 5 lần sai trong 15 phút. Cookie `HttpOnly`, `SameSite=Lax`, secure bắt buộc ngoài Development, sliding expiration 8 giờ.

Role được tập trung trong `SystemRoles`: `Admin`, `Owner`, `Tenant`. Seed role idempotent. Admin development chỉ được tạo khi có `SEED_ADMIN_EMAIL` và `SEED_ADMIN_PASSWORD`; không có credential production trong source.

## Ma trận quyền

| Resource/action | Admin | Owner | Tenant |
|---|---:|---:|---:|
| Account lock/role/audit | Có | Không | Không |
| Property/room/catalog CRUD | Xem theo admin action | Chỉ của mình | Không |
| Tenant/contract | Audit/support | Chỉ property của mình | Chỉ contract tham gia |
| Invoice/payment | Audit/support | Chỉ property của mình; confirm | Chỉ invoice của mình; submit evidence |
| Maintenance | Audit/support | Chỉ property của mình | Tạo/xem request phòng đang ở |
| Notification | Của mình | Của mình | Của mình |

Role không thay ownership check. Service nhận current user từ `ICurrentUserService`; truy vấn owner bắt đầu bằng `BoardingHouses.OwnerId == currentUserId`, tenant bắt đầu bằng `TenantProfile.UserId == currentUserId` và contract membership.

## HTTP behavior

- Chưa đăng nhập: challenge -> `/Account/Login`.
- Sai role: `/Account/AccessDenied` hoặc 403.
- ID thuộc user khác: 404, tránh enumeration.
- POST/PUT state change: anti-forgery và InputModel allow-list.
- Admin action nhạy cảm luôn ghi audit; không có impersonation.
