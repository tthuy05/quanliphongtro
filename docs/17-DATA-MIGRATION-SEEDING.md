# Migration và seeding

## Migration

Provider là `Npgsql.EntityFrameworkCore.PostgreSQL`. Connection lấy từ `ConnectionStrings__DefaultConnection`; không commit password thật.

```powershell
dotnet restore
dotnet ef database update
dotnet ef migrations list
```

Migration hiện có: initial schema, tenant-owner scope, active lease/residence partial indexes và contract-service quantity. Trước release phải apply cả bốn migration trên PostgreSQL thật và kiểm tra `__EFMigrationsHistory`; máy audit không có PostgreSQL nên bước này chưa được xác minh.

Migration phải được review: delete behaviors, filtered/unique indexes, check constraints, enum conversion và precision. Không tự reset database ngoài Development.

## Seed

`DatabaseSeeder` idempotent, chạy sau migrate trong Development khi `SeedData:Enabled=true`. Roles luôn có thể seed an toàn; sample data chỉ seed khi chưa có marker/property code tương ứng. Production không sample-seed và không tự migrate.

Sample data là hư cấu và gồm admin từ environment, owner/tenant development, 2 properties, room statuses, tenant profiles, active contract/member/residence, service catalog, readings, issued/draft invoices, confirmed/pending payment, maintenance và notifications. Không dùng số căn cước thật.

Các biến:

- `SEED_ADMIN_EMAIL`, `SEED_ADMIN_PASSWORD`
- `SEED_OWNER_EMAIL`, `SEED_OWNER_PASSWORD`
- `SEED_TENANT_EMAIL`, `SEED_TENANT_PASSWORD`

Nếu password không được cung cấp, account tương ứng không được tạo; log chỉ nêu account bị bỏ qua, không log secret.

Production hỗ trợ bootstrap Admin một lần khi `BootstrapAdmin:Enabled=true`/`BOOTSTRAP_ADMIN=true`; cần email/password secret. Sau khi tạo thành công phải tắt cờ và xóa bootstrap password. Cơ chế này không tạo dữ liệu mẫu.
