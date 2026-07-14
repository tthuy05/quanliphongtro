# Trọ Sinh Viên

Ứng dụng ASP.NET Core MVC .NET 9 quản lý khu trọ, phòng, người thuê, hợp đồng, chỉ số điện nước, hóa đơn, thanh toán và bảo trì. Tên kỹ thuật của project/assembly vẫn là `TroiSinhVien`.

## Chạy local

Yêu cầu: .NET SDK 9, Node.js 22+ và PostgreSQL 15+.

```powershell
dotnet tool restore
$env:ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=tro_sinh_vien;Username=<user>;Password=<secret>'
dotnet ef database update
dotnet run
```

Sau khi thay đổi class giao diện hoặc thư viện frontend, chạy `npm ci` rồi `npm run build:assets`. CSS Tailwind, Alpine, Chart.js và Lucide được đóng gói trong `wwwroot`; runtime không phụ thuộc CDN.

Không commit connection string hoặc mật khẩu. Có thể dùng .NET user-secrets thay cho biến môi trường.

## Seed dữ liệu phát triển

Seed chỉ chạy trong `Development`. Đặt `SeedData__Enabled=true`, `Database__ApplyMigrationsOnStartup=true` và các biến `SEED_*` được mô tả trong `.env.example`. Không bật sample seed trong Production.

## Build và test

```powershell
dotnet restore TroiSinhVien.slnx
dotnet build TroiSinhVien.slnx --no-restore
dotnet test TroiSinhVien.slnx --no-build
```

## Docker Compose

Sao chép `.env.example` thành `.env`, thay toàn bộ secret, sau đó:

```powershell
docker compose up -d postgres
docker compose --profile tools run --rm migrate
docker compose up --build web
```

Migration là deployment step có chủ đích; container Production không tự migrate hoặc seed. Upload và data-protection keys dùng named volume. Endpoint `/health` trả healthy chỉ khi kết nối database thành công.

Lần triển khai đầu tiên có thể đặt `BOOTSTRAP_ADMIN=true` cùng `SEED_ADMIN_EMAIL` và `SEED_ADMIN_PASSWORD`, khởi động `web` một lần để tạo tài khoản quản trị, rồi lập tức chuyển `BOOTSTRAP_ADMIN=false` và xóa mật khẩu bootstrap khỏi secret store. Tính năng này không tạo dữ liệu mẫu.

Chi tiết kiến trúc, bảo mật và deployment nằm trong thư mục `docs/`.
