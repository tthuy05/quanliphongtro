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

## Deploy free bằng Render + Neon

Phương án free dễ nhất cho demo là Render Free Web Service chạy Dockerfile của repo và Neon Free PostgreSQL làm database. Không dùng Render Free Postgres nếu cần giữ dữ liệu lâu vì gói free của Render Postgres có hạn hết hạn; Neon free phù hợp hơn cho demo dài ngày.

1. Tạo database PostgreSQL free trên Neon, lấy connection string dạng ADO.NET/Npgsql:

   ```text
   Host=<host>;Database=<db>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true
   ```

2. Apply migration từ máy local vào Neon:

   ```powershell
   dotnet tool restore
   $env:ConnectionStrings__DefaultConnection='Host=<host>;Database=<db>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true'
   dotnet ef database update --project TroiSinhVien.csproj
   ```

3. Trên Render, tạo Web Service từ GitHub repo, chọn branch `ui`, chọn runtime `Docker`, Dockerfile path là `Dockerfile`.

4. Thêm environment variables trên Render:

   ```text
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
   ConnectionStrings__DefaultConnection=<connection-string-neon>
   SeedData__Enabled=false
   Database__ApplyMigrationsOnStartup=false
   BootstrapAdmin__Enabled=true
   SEED_ADMIN_EMAIL=<email-admin>
   SEED_ADMIN_PASSWORD=<mat-khau-admin>
   ```

5. Deploy lần đầu, đăng nhập bằng admin vừa bootstrap, sau đó đổi `BootstrapAdmin__Enabled=false` và redeploy.

Lưu ý free tier: Render web service sẽ sleep khi không có truy cập, lần mở đầu có thể chậm. File upload/chứng từ lưu trên filesystem của web service free không bền sau redeploy/sleep; nếu cần dùng thật lâu dài thì phải thêm object storage hoặc chuyển sang gói có persistent disk.

Chi tiết kiến trúc, bảo mật và deployment nằm trong thư mục `docs/`.
