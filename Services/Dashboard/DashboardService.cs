using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.ViewModels.Dashboard;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Services.Dashboard;

public sealed class DashboardService(ApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock) : IDashboardService
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public async Task<DashboardPageViewModel> GetDashboardDataAsync(int propertyId, string month)
    {
        var ownerId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var properties = await db.BoardingHouses.AsNoTracking().Where(x => x.OwnerId == ownerId && x.IsActive)
            .OrderBy(x => x.Name).Select(x => new PropertySelectorViewModel { Id = x.Id, Name = x.Name, Address = x.Address, RoomCount = x.Rooms.Count(r => !r.IsDeleted) }).ToListAsync();
        if (properties.Count == 0) return Empty(month);
        propertyId = propertyId == 0 ? properties[0].Id : propertyId;
        if (properties.All(x => x.Id != propertyId)) throw new UnauthorizedAccessException();

        var (year, billingMonth) = ParsePeriod(month);
        var periodText = $"Tháng {billingMonth}/{year}";
        var roomCounts = await db.Rooms.AsNoTracking().Where(x => x.BoardingHouseId == propertyId)
            .GroupBy(_ => 1).Select(g => new { Total = g.Count(), Occupied = g.Count(x => x.Status == RoomStatus.Occupied), Available = g.Count(x => x.Status == RoomStatus.Available), Maintenance = g.Count(x => x.Status == RoomStatus.Maintenance) }).FirstOrDefaultAsync();
        var periodStart = new DateTimeOffset(year, billingMonth, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = periodStart.AddMonths(1);
        var confirmedPayments = db.Payments.AsNoTracking()
            .Where(x => x.Invoice.Room.BoardingHouseId == propertyId && x.Status == PaymentStatus.Confirmed);
        var revenue = IsSqliteTestAdapter
            ? (await confirmedPayments.Select(x => new { x.ConfirmedAt, x.Amount }).ToListAsync())
                .Where(x => x.ConfirmedAt >= periodStart && x.ConfirmedAt < periodEnd).Sum(x => x.Amount)
            : await confirmedPayments.Where(x => x.ConfirmedAt >= periodStart && x.ConfirmedAt < periodEnd)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;
        var debt = await db.Invoices.AsNoTracking().Where(x => x.Room.BoardingHouseId == propertyId && x.Status != InvoiceStatus.Cancelled).SumAsync(x => (decimal?)x.RemainingAmount) ?? 0;
        var userName = await db.Users.AsNoTracking().Where(x => x.Id == ownerId).Select(x => x.DisplayName).FirstAsync();
        var total = roomCounts?.Total ?? 0; var occupied = roomCounts?.Occupied ?? 0; var occupancy = total == 0 ? 0 : Math.Round(occupied * 100d / total, 1);

        var model = new DashboardPageViewModel
        {
            ActivePropertyId = propertyId, ActiveMonth = periodText, Properties = properties,
            Hero = new DashboardHeroViewModel { OwnerName = userName, Greeting = Greeting(), SummaryText = "Dữ liệu vận hành được cập nhật từ hệ thống.", OccupiedCount = occupied, TotalRoomsCount = total, OccupancyPercentage = occupancy, MonthlyRevenueFormatted = Money(revenue), OutstandingDebtFormatted = Money(debt) },
            OccupancyChart = new OccupancyChartViewModel { OccupiedCount = occupied, AvailableCount = roomCounts?.Available ?? 0, MaintenanceCount = roomCounts?.Maintenance ?? 0, OccupancyPercentage = (int)Math.Round(occupancy) },
            Stats = BuildStats(total, occupied, roomCounts?.Available ?? 0, revenue, debt),
            RevenueChart = await RevenueAsync(propertyId, year, billingMonth),
            RecentInvoices = await RecentInvoicesAsync(propertyId),
            OverduePayments = await OverdueAsync(propertyId),
            ExpiringContracts = await ExpiringAsync(propertyId),
            MaintenanceRequests = await MaintenanceAsync(propertyId),
            ActivityTimeline = await ActivitiesAsync(ownerId),
            QuickActions = QuickActions(),
            Notifications = await NotificationsAsync(ownerId)
        };
        return model;
    }

    private DashboardPageViewModel Empty(string month)
    {
        var (year, m) = ParsePeriod(month);
        return new DashboardPageViewModel { ActiveMonth = $"Tháng {m}/{year}", Hero = new() { Greeting = Greeting(), SummaryText = "Chưa có khu trọ. Hãy tạo khu trọ đầu tiên." }, Stats = BuildStats(0, 0, 0, 0, 0), QuickActions = QuickActions() };
    }

    private async Task<RevenueChartViewModel> RevenueAsync(int propertyId, int year, int month)
    {
        var end = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero); var start = end.AddMonths(-5); var endExclusive = end.AddMonths(1);
        var query = db.Payments.AsNoTracking().Where(x => x.Invoice.Room.BoardingHouseId == propertyId && x.Status == PaymentStatus.Confirmed);
        var payments = IsSqliteTestAdapter
            ? (await query.Select(x => new { ConfirmedAt = x.ConfirmedAt!.Value, x.Amount }).ToListAsync())
                .Where(x => x.ConfirmedAt >= start && x.ConfirmedAt < endExclusive).ToList()
            : await query.Where(x => x.ConfirmedAt >= start && x.ConfirmedAt < endExclusive)
                .Select(x => new { ConfirmedAt = x.ConfirmedAt!.Value, x.Amount }).ToListAsync();
        var rows = payments.GroupBy(x => new { x.ConfirmedAt.Year, x.ConfirmedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(x => x.Amount) }).ToList();
        var result = new RevenueChartViewModel();
        for (var cursor = start; cursor <= end; cursor = cursor.AddMonths(1))
        {
            var amount = rows.FirstOrDefault(x => x.Year == cursor.Year && x.Month == cursor.Month)?.Amount ?? 0;
            result.Labels.Add($"Tháng {cursor.Month}"); result.DataPoints.Add(new() { Month = $"Tháng {cursor.Month}", Revenue = amount / 1_000_000m, Expense = 0 });
        }
        return result;
    }

    private async Task<List<RecentInvoiceViewModel>> RecentInvoicesAsync(int id)
    {
        var rows = await db.Invoices.AsNoTracking().Where(x => x.Room.BoardingHouseId == id)
            .OrderByDescending(x => x.Id).Take(5)
            .Select(x => new { x.Id, x.Room.RoomCode, TenantName = x.Contract.RepresentativeTenant.FullName, x.BillingMonth, x.BillingYear, x.TotalAmount, x.DueDate, x.Status })
            .ToListAsync();
        return rows.Select(x => new RecentInvoiceViewModel { Id = x.Id, RoomCode = x.RoomCode, TenantName = x.TenantName, BillingMonth = $"{x.BillingMonth:00}/{x.BillingYear}", TotalAmountFormatted = Money(x.TotalAmount), DueDate = x.DueDate.ToDateTime(TimeOnly.MinValue), Status = x.Status.ToString() }).ToList();
    }

    private async Task<List<OverduePaymentViewModel>> OverdueAsync(int id)
    {
        var today = clock.UtcToday;
        var rows = await db.Invoices.AsNoTracking().Where(x => x.Room.BoardingHouseId == id && x.RemainingAmount > 0 && x.DueDate < today && x.Status != InvoiceStatus.Cancelled)
            .OrderBy(x => x.DueDate).Take(5).Select(x => new { x.Id, x.Room.RoomCode, TenantName = x.Contract.RepresentativeTenant.FullName, x.RemainingAmount, x.DueDate }).ToListAsync();
        return rows.Select(x => new OverduePaymentViewModel { InvoiceId = x.Id, RoomCode = x.RoomCode, TenantName = x.TenantName, AmountFormatted = Money(x.RemainingAmount), OverdueDays = today.DayNumber - x.DueDate.DayNumber }).ToList();
    }

    private Task<List<ExpiringContractViewModel>> ExpiringAsync(int id)
    {
        var today = clock.UtcToday; var until = today.AddDays(30);
        return db.Contracts.AsNoTracking().Where(x => x.Room.BoardingHouseId == id && x.Status == ContractStatus.Active && x.EndDate >= today && x.EndDate <= until).OrderBy(x => x.EndDate).Take(5)
            .Select(x => new ExpiringContractViewModel { ContractId = x.Id, TenantName = x.RepresentativeTenant.FullName, RoomCode = x.Room.RoomCode, ExpiryDate = x.EndDate.ToDateTime(TimeOnly.MinValue), RemainingDays = x.EndDate.DayNumber - today.DayNumber }).ToListAsync();
    }

    private Task<List<MaintenanceRequestViewModel>> MaintenanceAsync(int id) => db.MaintenanceRequests.AsNoTracking().Where(x => x.BoardingHouseId == id && x.Status != MaintenanceStatus.Completed && x.Status != MaintenanceStatus.Cancelled).OrderByDescending(x => x.Priority).ThenByDescending(x => x.Id).Take(5)
        .Select(x => new MaintenanceRequestViewModel { Id = x.Id, Category = x.Category, RoomCode = x.Room.RoomCode, Description = x.Title, Priority = x.Priority.ToString(), Status = x.Status.ToString(), SubmittedTimeText = x.CreatedAt.ToString("dd/MM/yyyy") }).ToListAsync();

    private Task<List<ActivityItemViewModel>> ActivitiesAsync(Guid ownerId) => db.AuditLogs.AsNoTracking().Where(x => x.UserId == ownerId).OrderByDescending(x => x.Id).Take(6)
        .Select(x => new ActivityItemViewModel { Icon = "activity", Title = x.Summary, TimestampText = x.Timestamp.ToString("dd/MM/yyyy HH:mm"), Category = x.EntityType }).ToListAsync();

    private Task<List<NotificationSummaryViewModel>> NotificationsAsync(Guid ownerId) => db.Notifications.AsNoTracking().Where(x => x.UserId == ownerId).OrderByDescending(x => x.Id).Take(8)
        .Select(x => new NotificationSummaryViewModel { Id = x.Id.ToString(), Title = x.Title, Description = x.Message, TimeText = x.CreatedAt.ToString("dd/MM/yyyy HH:mm"), IsUnread = x.ReadAt == null, IconName = "bell", Type = x.Type.ToString().ToLower(), LinkUrl = x.LinkUrl }).ToListAsync();

    private static List<DashboardStatViewModel> BuildStats(int total, int occupied, int available, decimal revenue, decimal debt) =>
    [
        new() { Id = "stat_total", Title = "Tổng số phòng", Value = $"{total} phòng", IconName = "home", TrendText = "Dữ liệu hiện tại" },
        new() { Id = "stat_occupied", Title = "Phòng đang thuê", Value = $"{occupied} phòng", IconName = "user-check", TrendText = "Dữ liệu hiện tại" },
        new() { Id = "stat_vacant", Title = "Phòng còn trống", Value = $"{available} phòng", IconName = "door-open", TrendText = "Dữ liệu hiện tại" },
        new() { Id = "stat_revenue", Title = "Doanh thu tháng", Value = Money(revenue), IconName = "banknote", TrendText = "Thanh toán đã xác nhận" },
        new() { Id = "stat_debt", Title = "Công nợ chưa thu", Value = Money(debt), IconName = "alert-triangle", TrendText = "Số dư hóa đơn" }
    ];

    private static List<QuickActionViewModel> QuickActions() =>
    [
        new() { Id = "room", Label = "Thêm phòng", IconName = "key-round", ActionRoute = "/Rooms/Create", Description = "Tạo phòng mới" },
        new() { Id = "tenant", Label = "Thêm người thuê", IconName = "user-plus", ActionRoute = "/Tenants/Create", Description = "Tạo hồ sơ" },
        new() { Id = "contract", Label = "Tạo hợp đồng", IconName = "file-signature", ActionRoute = "/Contracts/Create", Description = "Lập hợp đồng" },
        new() { Id = "meter", Label = "Ghi điện nước", IconName = "zap", ActionRoute = "/MeterReadings", Description = "Nhập chỉ số" },
        new() { Id = "invoice", Label = "Tạo hóa đơn", IconName = "receipt", ActionRoute = "/Invoices/Generate", Description = "Lập hóa đơn" },
        new() { Id = "payment", Label = "Thu tiền", IconName = "credit-card", ActionRoute = "/Payments", Description = "Xác nhận thanh toán" }
    ];

    private static string Money(decimal amount) => amount.ToString("N0", Vi) + " ₫";
    private bool IsSqliteTestAdapter => db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
    private string Greeting() => clock.UtcNow.Hour switch { < 5 => "Chào buổi tối", < 12 => "Chào buổi sáng", < 18 => "Chào buổi chiều", _ => "Chào buổi tối" };
    private (int Year, int Month) ParsePeriod(string input)
    {
        var now = clock.UtcNow; if (string.IsNullOrWhiteSpace(input)) return (now.Year, now.Month);
        var normalized = input.Replace("Tháng", "", StringComparison.OrdinalIgnoreCase).Trim();
        var parts = normalized.Split('/');
        return parts.Length == 2 && int.TryParse(parts[0], out var m) && int.TryParse(parts[1], out var y) && m is >= 1 and <= 12 ? (y, m) : (now.Year, now.Month);
    }
}
