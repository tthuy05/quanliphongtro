using System;
using System.Collections.Generic;

namespace TroiSinhVien.Models.ViewModels.Dashboard
{
    public class DashboardPageViewModel
    {
        public int ActivePropertyId { get; set; }
        public string ActiveMonth { get; set; } = string.Empty;
        public List<PropertySelectorViewModel> Properties { get; set; } = new();
        public DashboardHeroViewModel Hero { get; set; } = new();
        public List<DashboardStatViewModel> Stats { get; set; } = new();
        public RevenueChartViewModel RevenueChart { get; set; } = new();
        public OccupancyChartViewModel OccupancyChart { get; set; } = new();
        public List<RecentInvoiceViewModel> RecentInvoices { get; set; } = new();
        public List<OverduePaymentViewModel> OverduePayments { get; set; } = new();
        public List<ExpiringContractViewModel> ExpiringContracts { get; set; } = new();
        public List<MaintenanceRequestViewModel> MaintenanceRequests { get; set; } = new();
        public List<ActivityItemViewModel> ActivityTimeline { get; set; } = new();
        public List<QuickActionViewModel> QuickActions { get; set; } = new();
        public List<NotificationSummaryViewModel> Notifications { get; set; } = new();
    }

    public class PropertySelectorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int RoomCount { get; set; }
    }

    public class DashboardHeroViewModel
    {
        public string OwnerName { get; set; } = string.Empty;
        public string Greeting { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public int OccupiedCount { get; set; }
        public int TotalRoomsCount { get; set; }
        public double OccupancyPercentage { get; set; }
        public string MonthlyRevenueFormatted { get; set; } = string.Empty;
        public string OutstandingDebtFormatted { get; set; } = string.Empty;
    }

    public class DashboardStatViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public double TrendPercentage { get; set; }
        public string TrendDirection { get; set; } = "neutral"; // "up", "down", "neutral"
        public string TrendText { get; set; } = string.Empty;
        public List<double>? SparklineData { get; set; }
    }

    public class RevenueChartViewModel
    {
        public List<string> Labels { get; set; } = new();
        public List<RevenueDataPointViewModel> DataPoints { get; set; } = new();
    }

    public class RevenueDataPointViewModel
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Expense { get; set; }
    }

    public class OccupancyChartViewModel
    {
        public int OccupiedCount { get; set; }
        public int AvailableCount { get; set; }
        public int MaintenanceCount { get; set; }
        public int OccupancyPercentage { get; set; }
    }

    public class RecentInvoiceViewModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string BillingMonth { get; set; } = string.Empty;
        public string TotalAmountFormatted { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending"; // "Paid", "Pending", "Partial", "Overdue"
    }

    public class OverduePaymentViewModel
    {
        public string RoomCode { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string AmountFormatted { get; set; } = string.Empty;
        public int OverdueDays { get; set; }
    }

    public class ExpiringContractViewModel
    {
        public string TenantName { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int RemainingDays { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class MaintenanceRequestViewModel
    {
        public string Category { get; set; } = string.Empty; // "Điện", "Nước", "Internet", "Thiết bị", "An ninh", "Khác"
        public string RoomCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // "High", "Medium", "Low"
        public string Status { get; set; } = "New"; // "New", "In-Progress", "Resolved"
        public string SubmittedTimeText { get; set; } = string.Empty;
    }

    public class ActivityItemViewModel
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string TimestampText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class QuickActionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public string ActionRoute { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class NotificationSummaryViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TimeText { get; set; } = string.Empty;
        public bool IsUnread { get; set; }
        public string IconName { get; set; } = "bell";
        public string Type { get; set; } = "info"; // "info", "warning", "danger", "success"
    }
}
