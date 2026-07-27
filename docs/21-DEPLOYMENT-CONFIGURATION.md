# Deployment và cấu hình

## Local PostgreSQL

`docker-compose.yml` cung cấp PostgreSQL và named volume. Tạo `.env` local (không commit) từ `.env.example` hoặc đặt:

```powershell
$env:ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=tro_sinh_vien;Username=postgres;Password=<local-secret>'
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet ef database update
dotnet run
```

Health endpoint `/health` kiểm tra app và database; không công khai chi tiết exception.

## Production

- Bắt buộc HTTPS reverse proxy, `ASPNETCORE_ENVIRONMENT=Production`, connection string/seed secret trong secret manager.
- Không bật sample seed. Chạy migration như deployment step có backup, không migrate/reset âm thầm bởi request đầu tiên.
- Mount persistent private upload directory; giới hạn request body; malware scanning là khuyến nghị trước production thực tế.
- Persist data-protection keys để cookie đăng nhập không mất hiệu lực khi thay container.
- Giới hạn quyền DB user theo database ứng dụng; bật backup/restore test, log aggregation và health monitoring.
- CSS/JavaScript frontend được pin version, build bằng npm và self-host trong `wwwroot`; runtime không phụ thuộc CDN.

## Docker Compose

```powershell
docker compose up -d postgres
docker compose ps
docker compose --profile tools run --rm migrate
docker compose up --build web
```

Stop container không xóa volume. Chỉ `docker compose down -v` khi người vận hành chủ động muốn xóa dữ liệu development.

Lần triển khai đầu có thể đặt `BOOTSTRAP_ADMIN=true` cùng `SEED_ADMIN_EMAIL` và `SEED_ADMIN_PASSWORD`, khởi động `web` đúng một lần, sau đó tắt cờ và xóa mật khẩu bootstrap khỏi secret store. Production không tự động migrate, không tạo dữ liệu mẫu và không có tài khoản mặc định.

## Free deployment cho demo

Nếu cần deploy miễn phí, cấu hình khuyến nghị là Render Free Web Service + Neon Free PostgreSQL:

- Render chạy app bằng `Dockerfile`; app tự đọc biến `PORT` của platform khi biến này tồn tại.
- Neon giữ PostgreSQL lâu hơn cho demo; dùng connection string Npgsql với `SSL Mode=Require;Trust Server Certificate=true`.
- Chạy migration từ máy local hoặc CI trước khi deploy app:

```powershell
dotnet tool restore
$env:ConnectionStrings__DefaultConnection='Host=<host>;Database=<db>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true'
dotnet ef database update --project TroiSinhVien.csproj
```

Render environment variables tối thiểu:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ConnectionStrings__DefaultConnection=<neon-npgsql-connection-string>
SeedData__Enabled=false
Database__ApplyMigrationsOnStartup=false
BootstrapAdmin__Enabled=true
SEED_ADMIN_EMAIL=<email-admin>
SEED_ADMIN_PASSWORD=<mat-khau-admin>
```

Sau lần deploy đầu và đăng nhập được admin, đổi `BootstrapAdmin__Enabled=false` rồi redeploy. Render free web service có cold start khi ngủ; filesystem của web service free là tạm thời, nên upload/chứng từ không phải lưu trữ bền vững. Muốn giữ file thật cần object storage hoặc hosting có persistent disk.

Các artifact Docker đã được code review nhưng chưa build/chạy tại máy audit ngày 13/07/2026 vì Docker/Podman CLI không có sẵn. PostgreSQL thật và `/health` healthy là release gate bắt buộc ở môi trường triển khai.
