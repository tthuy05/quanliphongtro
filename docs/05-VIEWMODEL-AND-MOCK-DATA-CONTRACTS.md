# ViewModel and Mock Data Contracts - Trọ Sinh Viên (Internal technical name: TroHub)

This document details the strongly typed C# ViewModels and data structures representing model contracts between controllers, mock services, and Razor views.

---

## 1. C# ViewModels

All ViewModels are defined inside the `TroiSinhVien.Models.ViewModels.Dashboard` namespace.

### 1.1. `DashboardPageViewModel`
Main container containing all segments of the dashboard page.
*   `ActivePropertyId` | `int` | Current selected boarding house property ID.
*   `ActiveMonth` | `string` | Selected month (e.g. "07/2026").
*   `Properties` | `List<PropertySelectorViewModel>` | List of property selector dropdown items.
*   `Hero` | `DashboardHeroViewModel` | Header greeting & summary section data.
*   `Stats` | `List<DashboardStatViewModel>` | The 5 key metric card items.
*   `RevenueChart` | `RevenueChartViewModel` | Monthly revenue & expense chart data.
*   `OccupancyChart` | `OccupancyChartViewModel` | Rooms occupancy doughnut data.
*   `RecentInvoices` | `List<RecentInvoiceViewModel>` | Table list of invoices.
*   `OverduePayments` | `List<OverduePaymentViewModel>` | Table list of overdue accounts.
*   `ExpiringContracts` | `List<ExpiringContractViewModel>` | Leases ending soon list.
*   `MaintenanceRequests` | `List<MaintenanceRequestViewModel>` | Ongoing work orders.
*   `ActivityTimeline` | `List<ActivityItemViewModel>` | Event timeline feed.
*   `QuickActions` | `List<QuickActionViewModel>` | Dashboard shortcut action items.
*   `Notifications` | `List<NotificationSummaryViewModel>` | Top bar alert list.

### 1.2. `DashboardHeroViewModel`
*   `OwnerName` | `string` | Landlord greeting name (e.g. "Anh Thủy").
*   `Greeting` | `string` | Good morning/afternoon localized string.
*   `SummaryText` | `string` | Operational summary overview sentence.
*   `OccupiedCount` | `int` | Count of occupied rooms.
*   `TotalRoomsCount` | `int` | Total count of owned rooms.
*   `OccupancyPercentage` | `double` | Percentage of rooms occupied (e.g. 79.2).
*   `MonthlyRevenueFormatted` | `string` | Formatted revenue string (e.g. "58,4M ₫").
*   `OutstandingDebtFormatted` | `string` | Formatted uncollected debts (e.g. "7,8M ₫").

### 1.3. `DashboardStatViewModel`
*   `Id` | `string` | Unique identifier (e.g. "stat_vacant").
*   `Title` | `string` | Display label (e.g. "Phòng còn trống").
*   `Value` | `string` | The formatted display value (e.g. "5 phòng").
*   `IconName` | `string` | Lucide icon identifier (e.g. "door-open").
*   `TrendPercentage` | `double` | Trend change percentage (e.g. -12.5).
*   `TrendDirection` | `string` | direction of change: "up" | "down" | "neutral".
*   `TrendText` | `string` | Subtitle description (e.g. "So với tháng trước").
*   `SparklineData` | `List<double>?` | Optional history data (e.g. `[4, 5, 5, 6, 4, 5]`).

### 1.4. `RevenueChartViewModel` & `RevenueDataPointViewModel`
*   `RevenueChartViewModel`:
    *   `Labels` | `List<string>` | Month labels (e.g. `["T2", "T3", "T4", "T5", "T6", "T7"]`).
    *   `DataPoints` | `List<RevenueDataPointViewModel>` | List of revenue/expense coordinates.
*   `RevenueDataPointViewModel`:
    *   `Month` | `string` | Short name representation.
    *   `Revenue` | `decimal` | Decimal currency value.
    *   `Expense` | `decimal` | Decimal cost value.

### 1.5. `OccupancyChartViewModel`
*   `OccupiedCount` | `int` | Slices.
*   `AvailableCount` | `int` | Slices.
*   `MaintenanceCount` | `int` | Slices.
*   `OccupancyPercentage` | `int` | Centered text value (e.g., 79).

### 1.6. `RecentInvoiceViewModel`
*   `RoomCode` | `string` | (e.g. "A101").
*   `TenantName` | `string` | (e.g. "Nguyễn Văn A").
*   `BillingMonth` | `string` | Month indicator.
*   `TotalAmountFormatted` | `string` | Currency string.
*   `DueDate` | `DateTime` | Expiration date.
*   `Status` | `string` | "Paid" | "Pending" | "Partial" | "Overdue".

### 1.7. `OverduePaymentViewModel`
*   `RoomCode` | `string` | (e.g. "B201").
*   `TenantName` | `string` | (e.g. "Trần Thị B").
*   `AmountFormatted` | `string` | (e.g. "3.500.000 ₫").
*   `OverdueDays` | `int` | Total elapsed days (e.g. 15).

### 1.8. `ExpiringContractViewModel`
*   `TenantName` | `string` | (e.g. "Phạm Minh C").
*   `RoomCode` | `string` | (e.g. "A203").
*   `ExpiryDate` | `DateTime` | Expiration date.
*   `RemainingDays` | `int` | Days left (e.g. 12).
*   `AvatarUrl` | `string?` | Optional profile image path.

### 1.9. `MaintenanceRequestViewModel`
*   `Category` | `string` | Category: "Điện", "Nước", "Internet", "Thiết bị", "An ninh", "Khác".
*   `RoomCode` | `string` | Room origin.
*   `Description` | `string` | Details (e.g. "Hỏng vòi sen tắm").
*   `Priority` | `string` | Priority Level: "High" | "Medium" | "Low".
*   `Status` | `string` | Status: "New" | "In-Progress" | "Resolved".
*   `SubmittedTimeText` | `string` | (e.g. "2 giờ trước").

### 1.10. `ActivityItemViewModel`
*   `Icon` | `string` | Lucide icon string.
*   `Title` | `string` | Details of event.
*   `TimestampText` | `string` | Elapsed time text.
*   `Category` | `string` | Event classification context.

---

## 2. Service Interfaces

The C# business layer loads contracts using the following interface:

```csharp
namespace TroiSinhVien.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardPageViewModel> GetDashboardDataAsync(int propertyId, string month);
    }
}
```

The implementation `MockDashboardService.cs` instantiates mock arrays inside C# using localized fields before returning the models to the controller.

---

## Backend contract addendum (2026-07-12)

The class shapes above remain the Razor contract, but `DashboardService` projects owner-scoped EF Core aggregates into them. Monetary calculations remain `decimal`; formatting occurs at the presentation boundary. `ActivePropertyId` remains `int` because business entities use integer identities. A submitted property ID is never trusted without `OwnerId` filtering. Form posts use dedicated types under `Models/InputModels`; EF entities are not MVC input contracts.
