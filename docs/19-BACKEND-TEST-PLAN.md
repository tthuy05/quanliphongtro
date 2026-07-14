# Backend test plan

## Automated

Test project xUnit dùng SQLite in-memory connection mở xuyên suốt test (relational, không dùng EF InMemory) cho business/service tests; PostgreSQL integration suite dùng connection riêng khi `TEST_POSTGRES_CONNECTION` tồn tại.

- Auth: protected endpoint challenge; role rejection; owner/tenant isolation; admin-only action.
- Room/contract: room-code scope; available-only activation; date/overlap; status transitions/move-out.
- Meter: monotonic, duplicate kỳ, consumption.
- Invoice: total, duplicate kỳ, price snapshot, paid immutable.
- Payment: positive, no overpayment, partial/full status, owner-only confirm.
- Maintenance: active room only; cross-property rejection.
- Dashboard: owner-only aggregate và empty state.

Commands:

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

## Integration/browser

Chạy PostgreSQL development, apply migration, seed account từ environment. Với từng role: login; kiểm tra route unauthorized; thực hiện workflow property -> room -> tenant -> contract -> readings -> invoice -> payment -> dashboard -> maintenance. Kiểm tra console/network, validation/toast/empty state và viewport 1440/1024/768/390.

Không đánh dấu browser/PostgreSQL pass nếu thiếu runtime/container hoặc chưa mở trang thực.
