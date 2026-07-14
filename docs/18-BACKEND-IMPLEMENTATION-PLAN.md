# Kế hoạch triển khai backend

1. **Foundation:** package EF/Npgsql/Identity; domain, configurations, DbContext, clock/current-user, error handling, audit; build.
2. **Auth:** cookie login/logout/access denied, role seed, owner/tenant scoping; auth tests; build/run.
3. **Property/room:** service, InputModel, controller, views, search/filter/page/deactivate; tests.
4. **Tenant/contract:** profile, members, activate/end/move-out transaction; overlap tests.
5. **Catalog/meter:** snapshot service and monotonic readings; duplicate tests.
6. **Invoice:** generation/details/issue/cancel and period uniqueness; calculation tests.
7. **Payment:** partial payment, evidence, owner confirm/reject; transaction/security tests.
8. **Maintenance/notification:** tenant/owner workflows and user-scoped notifications.
9. **Dashboard/report:** replace mock DI with efficient aggregate projection while preserving ViewModel/Razor.
10. **Quality/deploy:** migration, seed, xUnit relational tests, health check, Docker Compose, browser workflows, docs/report.

Mỗi phase chạy restore/build/test; phase có UI phải run và kiểm tra trang thật. Nếu PostgreSQL/Docker không khả dụng, ghi rõ migration/apply/browser phần nào chưa verify thay vì tuyên bố hoàn tất.
