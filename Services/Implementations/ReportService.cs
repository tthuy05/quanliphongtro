using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Models.ViewModels;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Services.Implementations;

public sealed class ReportService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IReportService
{
    private Guid OwnerId => current.UserId ?? throw new UnauthorizedAccessException();

    public async Task<RevenueReportViewModel> RevenueAsync(ReportFilterInputModel filter, CancellationToken ct = default)
    {
        var owner = OwnerId; var normalized = Normalize(filter);
        var from = new DateTimeOffset(normalized.FromDate!.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toExclusive = new DateTimeOffset(normalized.ToDate!.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var query = db.Payments.AsNoTracking().Where(x => x.Invoice.Room.BoardingHouse.OwnerId == owner && x.Status == PaymentStatus.Confirmed);
        if (normalized.PropertyId.HasValue) query = query.Where(x => x.Invoice.Room.BoardingHouseId == normalized.PropertyId);
        List<RevenueReportRow> rows;
        if (db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            var payments = await query.Select(x => new { x.Invoice.Room.BoardingHouseId, BoardingHouseName = x.Invoice.Room.BoardingHouse.Name, ConfirmedAt = x.ConfirmedAt!.Value, x.Amount }).ToListAsync(ct);
            rows = payments.Where(x => x.ConfirmedAt >= from && x.ConfirmedAt < toExclusive).GroupBy(x => new { x.BoardingHouseId, x.BoardingHouseName, x.ConfirmedAt.Year, x.ConfirmedAt.Month })
                .Select(g => new RevenueReportRow(g.Key.BoardingHouseId, g.Key.BoardingHouseName, g.Key.Year, g.Key.Month, g.Sum(x => x.Amount))).OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.BoardingHouseName).ToList();
        }
        else
        {
            rows = await query.Where(x => x.ConfirmedAt >= from && x.ConfirmedAt < toExclusive).GroupBy(x => new { x.Invoice.Room.BoardingHouseId, x.Invoice.Room.BoardingHouse.Name, x.ConfirmedAt!.Value.Year, x.ConfirmedAt.Value.Month })
                .Select(g => new RevenueReportRow(g.Key.BoardingHouseId, g.Key.Name, g.Key.Year, g.Key.Month, g.Sum(x => x.Amount))).OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.BoardingHouseName).ToListAsync(ct);
        }
        return new() { Filter = normalized, Properties = await PropertiesAsync(owner, ct), Rows = rows };
    }

    public async Task<OccupancyReportViewModel> OccupancyAsync(ReportFilterInputModel filter, CancellationToken ct = default)
    {
        var owner = OwnerId; var normalized = Normalize(filter); var query = db.BoardingHouses.AsNoTracking().Where(x => x.OwnerId == owner && x.IsActive);
        if (normalized.PropertyId.HasValue) query = query.Where(x => x.Id == normalized.PropertyId);
        var rows = await query.OrderBy(x => x.Name).Select(x => new OccupancyReportRow(x.Id, x.Name, x.Rooms.Count(), x.Rooms.Count(r => r.Status == RoomStatus.Occupied), x.Rooms.Count(r => r.Status == RoomStatus.Available), x.Rooms.Count(r => r.Status == RoomStatus.Maintenance))).ToListAsync(ct);
        return new() { Filter = normalized, Properties = await PropertiesAsync(owner, ct), Rows = rows };
    }

    public async Task<DebtReportViewModel> DebtAsync(ReportFilterInputModel filter, CancellationToken ct = default)
    {
        var owner = OwnerId; var normalized = Normalize(filter); var asOf = normalized.ToDate!.Value;
        var query = db.Invoices.AsNoTracking().Where(x => x.Room.BoardingHouse.OwnerId == owner && x.Status != InvoiceStatus.Cancelled && x.RemainingAmount > 0 && x.DueDate <= asOf);
        if (normalized.PropertyId.HasValue) query = query.Where(x => x.Room.BoardingHouseId == normalized.PropertyId);
        var rows = await query.OrderBy(x => x.DueDate).Select(x => new DebtReportRow(x.Id, x.Room.BoardingHouse.Name, x.Room.RoomCode, x.Contract.RepresentativeTenant.FullName, x.InvoiceNumber, x.DueDate, x.RemainingAmount, asOf.DayNumber - x.DueDate.DayNumber)).ToListAsync(ct);
        return new() { Filter = normalized, Properties = await PropertiesAsync(owner, ct), Rows = rows };
    }

    private ReportFilterInputModel Normalize(ReportFilterInputModel filter)
    {
        var to = filter.ToDate ?? clock.UtcToday;
        return new() { PropertyId = filter.PropertyId is > 0 ? filter.PropertyId : null, FromDate = filter.FromDate ?? new DateOnly(to.Year, 1, 1), ToDate = to };
    }

    private Task<List<SelectOption>> PropertiesAsync(Guid owner, CancellationToken ct) => db.BoardingHouses.AsNoTracking().Where(x => x.OwnerId == owner && x.IsActive).OrderBy(x => x.Name).Select(x => new SelectOption(x.Id, x.Name)).ToListAsync(ct);
}
