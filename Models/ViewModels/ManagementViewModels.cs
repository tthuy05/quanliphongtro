using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;

namespace TroiSinhVien.Models.ViewModels;

public sealed record BoardingHouseListItem(int Id, string Name, string Address, string? ContactPhone, int RoomCount, bool IsActive);
public sealed class BoardingHouseDetailsViewModel
{
    public BoardingHouseListItem Property { get; init; } = null!;
    public decimal DefaultElectricityPrice { get; init; }
    public decimal DefaultWaterPrice { get; init; }
    public IReadOnlyList<RoomListItem> Rooms { get; init; } = [];
}
public sealed record RoomListItem(int Id, int BoardingHouseId, string BoardingHouseName, string RoomCode, string? RoomName, int? Floor, decimal MonthlyRent, RoomStatus Status, int MaximumOccupants);
public sealed class RoomDetailsViewModel
{
    public RoomListItem Room { get; init; } = null!;
    public decimal? Area { get; init; }
    public decimal DepositAmount { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string> CurrentTenants { get; init; } = [];
    public IReadOnlyList<ContractListItem> Contracts { get; init; } = [];
}
public sealed record TenantListItem(int Id, string FullName, string PhoneNumber, string? Email, string? CurrentRoomCode);
public sealed record ResidenceHistoryListItem(string BoardingHouseName, string RoomCode, DateOnly MoveInDate, DateOnly? MoveOutDate);
public sealed class TenantDetailsViewModel
{
    public TenantListItem Tenant { get; init; } = null!;
    public DateOnly? DateOfBirth { get; init; }
    public string? IdentityNumber { get; init; }
    public DateOnly? IdentityIssueDate { get; init; }
    public string? PermanentAddress { get; init; }
    public string? VehiclePlate { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public IReadOnlyList<ResidenceHistoryListItem> Residences { get; init; } = [];
    public IReadOnlyList<ContractListItem> Contracts { get; init; } = [];
}
public sealed record ContractListItem(int Id, string ContractCode, string RoomCode, string TenantName, DateOnly StartDate, DateOnly EndDate, decimal MonthlyRent, ContractStatus Status);
public sealed record PropertyServiceListItem(int Id, int BoardingHouseId, string BoardingHouseName, string Name, string Unit, ServiceCalculationType CalculationType, decimal UnitPrice, bool IsActive);
public sealed record ContractMemberListItem(int TenantId, string FullName, bool IsRepresentative, DateOnly JoinedDate, DateOnly? LeftDate);
public sealed record ContractServiceListItem(int PropertyServiceId, string Name, string Unit, ServiceCalculationType CalculationType, decimal Quantity, decimal UnitPrice, bool IsActive);
public sealed class ContractDetailsViewModel
{
    public int Id { get; init; }
    public string ContractCode { get; init; } = string.Empty;
    public string BoardingHouseName { get; init; } = string.Empty;
    public string RoomCode { get; init; } = string.Empty;
    public string RepresentativeTenantName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal MonthlyRent { get; init; }
    public decimal DepositAmount { get; init; }
    public decimal ElectricityPrice { get; init; }
    public decimal WaterPrice { get; init; }
    public int PaymentDueDay { get; init; }
    public ContractStatus Status { get; init; }
    public IReadOnlyList<ContractMemberListItem> Members { get; init; } = [];
    public IReadOnlyList<ContractServiceListItem> Services { get; init; } = [];
    public IReadOnlyList<SelectOption> AvailableTenants { get; init; } = [];
    public IReadOnlyList<SelectOption> AvailableServices { get; init; } = [];
}
public sealed record MeterReadingListItem(int Id, string RoomCode, MeterType MeterType, int BillingYear, int BillingMonth, decimal PreviousReading, decimal CurrentReading, decimal Consumption);
public sealed record InvoiceListItem(int Id, string InvoiceNumber, string RoomCode, string TenantName, int BillingYear, int BillingMonth, decimal TotalAmount, decimal PaidAmount, decimal RemainingAmount, InvoiceStatus Status, DateOnly DueDate);
public sealed record InvoiceDetailListItem(string Description, decimal Quantity, string Unit, decimal UnitPrice, decimal Amount, InvoiceDetailSourceType SourceType);
public sealed record PaymentListItem(int Id, string InvoiceNumber, decimal Amount, PaymentMethod Method, PaymentStatus Status, DateTimeOffset PaidAt, string? ReferenceCode, int? EvidenceFileId, string? RejectionReason);
public sealed class InvoiceDetailsViewModel
{
    public InvoiceListItem Invoice { get; init; } = null!;
    public IReadOnlyList<InvoiceDetailListItem> Details { get; init; } = [];
    public IReadOnlyList<PaymentListItem> Payments { get; init; } = [];
    public decimal PriorOutstandingDebt { get; init; }
    public decimal AccountOutstandingDebt { get; init; }
    public bool IsTenantView { get; init; }
}
public sealed record MaintenanceListItem(int Id, string RoomCode, string TenantName, string Category, string Title, MaintenancePriority Priority, MaintenanceStatus Status, DateTimeOffset CreatedAt);
public sealed record MaintenanceCommentListItem(string AuthorLabel, string Content, DateTimeOffset CreatedAt);
public sealed class MaintenanceDetailsViewModel
{
    public int Id { get; init; }
    public string BoardingHouseName { get; init; } = string.Empty;
    public string RoomCode { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public MaintenancePriority Priority { get; init; }
    public MaintenanceStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public bool IsTenantView { get; init; }
    public IReadOnlyList<MaintenanceCommentListItem> Comments { get; init; } = [];
}
public sealed record NotificationListItem(int Id, NotificationType Type, string Title, string Message, string? LinkUrl, DateTimeOffset CreatedAt, DateTimeOffset? ReadAt);
public sealed record AdminUserListItem(Guid Id, string Email, string DisplayName, string Role, bool IsActive, DateTimeOffset? LockoutEnd);
public sealed record AuditLogListItem(long Id, string Action, string EntityType, string EntityId, DateTimeOffset Timestamp, string Summary);
public sealed record SelectOption(int Id, string Label);
public sealed record RevenueReportRow(int BoardingHouseId, string BoardingHouseName, int Year, int Month, decimal Amount);
public sealed record OccupancyReportRow(int BoardingHouseId, string BoardingHouseName, int TotalRooms, int OccupiedRooms, int AvailableRooms, int MaintenanceRooms)
{
    public decimal OccupancyRate => TotalRooms == 0 ? 0 : Math.Round(OccupiedRooms * 100m / TotalRooms, 1);
}
public sealed record DebtReportRow(int InvoiceId, string BoardingHouseName, string RoomCode, string TenantName, string InvoiceNumber, DateOnly DueDate, decimal RemainingAmount, int OverdueDays);
public sealed class RevenueReportViewModel
{
    public ReportFilterInputModel Filter { get; init; } = new();
    public IReadOnlyList<SelectOption> Properties { get; init; } = [];
    public IReadOnlyList<RevenueReportRow> Rows { get; init; } = [];
    public decimal Total => Rows.Sum(x => x.Amount);
}
public sealed class OccupancyReportViewModel
{
    public ReportFilterInputModel Filter { get; init; } = new();
    public IReadOnlyList<SelectOption> Properties { get; init; } = [];
    public IReadOnlyList<OccupancyReportRow> Rows { get; init; } = [];
}
public sealed class DebtReportViewModel
{
    public ReportFilterInputModel Filter { get; init; } = new();
    public IReadOnlyList<SelectOption> Properties { get; init; } = [];
    public IReadOnlyList<DebtReportRow> Rows { get; init; } = [];
    public decimal Total => Rows.Sum(x => x.RemainingAmount);
}
public sealed class AccountSettingsViewModel
{
    public string Email { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = [];
    public UpdateProfileInputModel Profile { get; init; } = new();
    public ChangePasswordInputModel ChangePassword { get; init; } = new();
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
