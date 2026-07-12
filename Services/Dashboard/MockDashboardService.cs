using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TroiSinhVien.Models.ViewModels.Dashboard;

namespace TroiSinhVien.Services.Dashboard
{
    public class MockDashboardService : IDashboardService
    {
        public Task<DashboardPageViewModel> GetDashboardDataAsync(int propertyId, string month)
        {
            // Fallback default values
            if (string.IsNullOrEmpty(month))
            {
                month = "Tháng 7/2026";
            }

            var model = new DashboardPageViewModel
            {
                ActivePropertyId = propertyId,
                ActiveMonth = month,
                Properties = GetMockProperties(),
                Hero = GetMockHero(propertyId),
                Stats = GetMockStats(propertyId),
                RevenueChart = GetMockRevenueChart(propertyId),
                OccupancyChart = GetMockOccupancyChart(propertyId),
                RecentInvoices = GetMockInvoices(propertyId),
                OverduePayments = GetMockOverduePayments(propertyId),
                ExpiringContracts = GetMockExpiringContracts(propertyId),
                MaintenanceRequests = GetMockMaintenanceRequests(propertyId),
                ActivityTimeline = GetMockActivities(propertyId),
                QuickActions = GetMockQuickActions(),
                Notifications = GetMockNotifications()
            };

            return Task.FromResult(model);
        }

        private List<PropertySelectorViewModel> GetMockProperties()
        {
            return new List<PropertySelectorViewModel>
            {
                new() { Id = 1, Name = "Khu Trọ A (Phố Dịch Vọng)", Address = "Số 15 Ngõ 44 Dịch Vọng, Cầu Giấy, Hà Nội", RoomCount = 24 },
                new() { Id = 2, Name = "Khu Trọ B (Đường Xuân Thủy)", Address = "Số 80 Ngõ 175 Xuân Thủy, Cầu Giấy, Hà Nội", RoomCount = 18 },
                new() { Id = 3, Name = "Khu Trọ C (Phố Chùa Láng)", Address = "Số 102 Ngõ 119 Chùa Láng, Đống Đa, Hà Nội", RoomCount = 30 }
            };
        }

        private DashboardHeroViewModel GetMockHero(int propertyId)
        {
            string summaryText = "Mọi hoạt động tại khu trọ của bạn đang ổn định.";
            int occupied = 19;
            int total = 24;
            string rev = "58,4M ₫";
            string debt = "7,8M ₫";

            if (propertyId == 2)
            {
                summaryText = "Có 2 yêu cầu sửa chữa điện cần xử lý gấp hôm nay.";
                occupied = 14;
                total = 18;
                rev = "42,0M ₫";
                debt = "3,2M ₫";
            }
            else if (propertyId == 3)
            {
                summaryText = "Công nợ thu hồi đang ở mức an toàn. Tỷ lệ trống còn khá cao.";
                occupied = 22;
                total = 30;
                rev = "78,5M ₫";
                debt = "12,4M ₫";
            }

            return new DashboardHeroViewModel
            {
                OwnerName = "Anh Thủy",
                Greeting = "Chào buổi sáng",
                SummaryText = summaryText,
                OccupiedCount = occupied,
                TotalRoomsCount = total,
                OccupancyPercentage = Math.Round((double)occupied / total * 100, 1),
                MonthlyRevenueFormatted = rev,
                OutstandingDebtFormatted = debt
            };
        }

        private List<DashboardStatViewModel> GetMockStats(int propertyId)
        {
            if (propertyId == 2)
            {
                return new List<DashboardStatViewModel>
                {
                    new() { Id = "stat_total", Title = "Tổng số phòng", Value = "18 phòng", IconName = "home", TrendPercentage = 0, TrendDirection = "neutral", TrendText = "Không đổi so với tháng trước" },
                    new() { Id = "stat_occupied", Title = "Phòng đang thuê", Value = "14 phòng", IconName = "user-check", TrendPercentage = 7.7, TrendDirection = "up", TrendText = "Tăng thêm 1 phòng mới" },
                    new() { Id = "stat_vacant", Title = "Phòng còn trống", Value = "4 phòng", IconName = "door-open", TrendPercentage = -20.0, TrendDirection = "down", TrendText = "Giảm số lượng phòng trống" },
                    new() { Id = "stat_revenue", Title = "Doanh thu tháng này", Value = "42,0 triệu ₫", IconName = "banknote", TrendPercentage = 5.2, TrendDirection = "up", TrendText = "So với tháng trước", SparklineData = new List<double> { 38.0, 39.5, 40.0, 39.8, 41.2, 42.0 } },
                    new() { Id = "stat_debt", Title = "Công nợ chưa thu", Value = "3,2 triệu ₫", IconName = "alert-triangle", TrendPercentage = -15.4, TrendDirection = "down", TrendText = "Giảm nợ đọng thành công", SparklineData = new List<double> { 5.1, 4.8, 4.0, 3.8, 3.5, 3.2 } }
                };
            }

            if (propertyId == 3)
            {
                return new List<DashboardStatViewModel>
                {
                    new() { Id = "stat_total", Title = "Tổng số phòng", Value = "30 phòng", IconName = "home", TrendPercentage = 0, TrendDirection = "neutral", TrendText = "Không đổi so với tháng trước" },
                    new() { Id = "stat_occupied", Title = "Phòng đang thuê", Value = "22 phòng", IconName = "user-check", TrendPercentage = 10.0, TrendDirection = "up", TrendText = "Thuê thêm 2 phòng mới" },
                    new() { Id = "stat_vacant", Title = "Phòng còn trống", Value = "8 phòng", IconName = "door-open", TrendPercentage = -20.0, TrendDirection = "down", TrendText = "Giảm số lượng phòng trống" },
                    new() { Id = "stat_revenue", Title = "Doanh thu tháng này", Value = "78,5 triệu ₫", IconName = "banknote", TrendPercentage = 12.0, TrendDirection = "up", TrendText = "So với tháng trước", SparklineData = new List<double> { 70.0, 71.2, 73.0, 75.5, 76.0, 78.5 } },
                    new() { Id = "stat_debt", Title = "Công nợ chưa thu", Value = "12,4 triệu ₫", IconName = "alert-triangle", TrendPercentage = 8.5, TrendDirection = "up", TrendText = "Tăng do hóa đơn mới chưa đóng", SparklineData = new List<double> { 8.5, 9.0, 10.2, 9.8, 11.5, 12.4 } }
                };
            }

            // Default: PropertyId = 1
            return new List<DashboardStatViewModel>
            {
                new() { Id = "stat_total", Title = "Tổng số phòng", Value = "24 phòng", IconName = "home", TrendPercentage = 0, TrendDirection = "neutral", TrendText = "Không thay đổi cấu trúc" },
                new() { Id = "stat_occupied", Title = "Phòng đang thuê", Value = "19 phòng", IconName = "user-check", TrendPercentage = 5.6, TrendDirection = "up", TrendText = "Tăng thêm 1 hợp đồng mới" },
                new() { Id = "stat_vacant", Title = "Phòng còn trống", Value = "5 phòng", IconName = "door-open", TrendPercentage = -16.7, TrendDirection = "down", TrendText = "Giảm số lượng phòng trống" },
                new() { Id = "stat_revenue", Title = "Doanh thu tháng", Value = "58,4 triệu ₫", IconName = "banknote", TrendPercentage = 8.4, TrendDirection = "up", TrendText = "Tăng trưởng doanh thu hợp đồng", SparklineData = new List<double> { 52.0, 53.5, 54.0, 55.8, 56.5, 58.4 } },
                new() { Id = "stat_debt", Title = "Công nợ chưa thu", Value = "7,8 triệu ₫", IconName = "alert-triangle", TrendPercentage = -12.3, TrendDirection = "down", TrendText = "Giảm tỷ lệ nợ quá hạn", SparklineData = new List<double> { 9.2, 8.8, 8.5, 8.0, 8.1, 7.8 } }
            };
        }

        private RevenueChartViewModel GetMockRevenueChart(int propertyId)
        {
            var data = new List<RevenueDataPointViewModel>();
            if (propertyId == 2)
            {
                data.Add(new() { Month = "Tháng 2", Revenue = 38.0m, Expense = 12.0m });
                data.Add(new() { Month = "Tháng 3", Revenue = 39.5m, Expense = 12.5m });
                data.Add(new() { Month = "Tháng 4", Revenue = 40.0m, Expense = 13.0m });
                data.Add(new() { Month = "Tháng 5", Revenue = 39.8m, Expense = 11.8m });
                data.Add(new() { Month = "Tháng 6", Revenue = 41.2m, Expense = 12.2m });
                data.Add(new() { Month = "Tháng 7", Revenue = 42.0m, Expense = 12.5m });
            }
            else if (propertyId == 3)
            {
                data.Add(new() { Month = "Tháng 2", Revenue = 70.0m, Expense = 24.0m });
                data.Add(new() { Month = "Tháng 3", Revenue = 71.2m, Expense = 23.5m });
                data.Add(new() { Month = "Tháng 4", Revenue = 73.0m, Expense = 25.0m });
                data.Add(new() { Month = "Tháng 5", Revenue = 75.5m, Expense = 24.8m });
                data.Add(new() { Month = "Tháng 6", Revenue = 76.0m, Expense = 26.2m });
                data.Add(new() { Month = "Tháng 7", Revenue = 78.5m, Expense = 25.5m });
            }
            else
            {
                data.Add(new() { Month = "Tháng 2", Revenue = 52.0m, Expense = 15.0m });
                data.Add(new() { Month = "Tháng 3", Revenue = 53.5m, Expense = 16.2m });
                data.Add(new() { Month = "Tháng 4", Revenue = 54.0m, Expense = 15.8m });
                data.Add(new() { Month = "Tháng 5", Revenue = 55.8m, Expense = 17.0m });
                data.Add(new() { Month = "Tháng 6", Revenue = 56.5m, Expense = 16.5m });
                data.Add(new() { Month = "Tháng 7", Revenue = 58.4m, Expense = 17.2m });
            }

            var labels = new List<string>();
            foreach (var d in data)
            {
                labels.Add(d.Month);
            }

            return new RevenueChartViewModel
            {
                Labels = labels,
                DataPoints = data
            };
        }

        private OccupancyChartViewModel GetMockOccupancyChart(int propertyId)
        {
            if (propertyId == 2)
            {
                return new OccupancyChartViewModel
                {
                    OccupiedCount = 14,
                    AvailableCount = 3,
                    MaintenanceCount = 1,
                    OccupancyPercentage = 78
                };
            }

            if (propertyId == 3)
            {
                return new OccupancyChartViewModel
                {
                    OccupiedCount = 22,
                    AvailableCount = 6,
                    MaintenanceCount = 2,
                    OccupancyPercentage = 73
                };
            }

            return new OccupancyChartViewModel
            {
                OccupiedCount = 19,
                AvailableCount = 4,
                MaintenanceCount = 1,
                OccupancyPercentage = 79
            };
        }

        private List<RecentInvoiceViewModel> GetMockInvoices(int propertyId)
        {
            if (propertyId == 2)
            {
                return new List<RecentInvoiceViewModel>
                {
                    new() { RoomCode = "B101", TenantName = "Phùng Huy Hoàng", BillingMonth = "07/2026", TotalAmountFormatted = "3.200.000 ₫", DueDate = DateTime.Now.AddDays(5), Status = "Paid" },
                    new() { RoomCode = "B103", TenantName = "Vũ Việt Đức", BillingMonth = "07/2026", TotalAmountFormatted = "2.900.000 ₫", DueDate = DateTime.Now.AddDays(-2), Status = "Overdue" },
                    new() { RoomCode = "B202", TenantName = "Lê Thị Thu", BillingMonth = "07/2026", TotalAmountFormatted = "3.550.000 ₫", DueDate = DateTime.Now.AddDays(4), Status = "Pending" },
                    new() { RoomCode = "B204", TenantName = "Nguyễn Hữu An", BillingMonth = "07/2026", TotalAmountFormatted = "3.100.000 ₫", DueDate = DateTime.Now.AddDays(3), Status = "Paid" }
                };
            }

            if (propertyId == 3)
            {
                return new List<RecentInvoiceViewModel>
                {
                    new() { RoomCode = "C102", TenantName = "Đỗ Thu Trang", BillingMonth = "07/2026", TotalAmountFormatted = "4.200.000 ₫", DueDate = DateTime.Now.AddDays(6), Status = "Paid" },
                    new() { RoomCode = "C105", TenantName = "Hoàng Trung Kiên", BillingMonth = "07/2026", TotalAmountFormatted = "3.800.000 ₫", DueDate = DateTime.Now.AddDays(-4), Status = "Overdue" },
                    new() { RoomCode = "C201", TenantName = "Nguyễn Minh Hằng", BillingMonth = "07/2026", TotalAmountFormatted = "4.650.000 ₫", DueDate = DateTime.Now.AddDays(3), Status = "Partial" },
                    new() { RoomCode = "C206", TenantName = "Trần Thanh Sơn", BillingMonth = "07/2026", TotalAmountFormatted = "3.900.000 ₫", DueDate = DateTime.Now.AddDays(5), Status = "Pending" }
                };
            }

            return new List<RecentInvoiceViewModel>
            {
                new() { RoomCode = "A101", TenantName = "Nguyễn Văn Anh", BillingMonth = "07/2026", TotalAmountFormatted = "3.450.000 ₫", DueDate = DateTime.Now.AddDays(3), Status = "Paid" },
                new() { RoomCode = "A102", TenantName = "Trần Thị Bình", BillingMonth = "07/2026", TotalAmountFormatted = "2.800.000 ₫", DueDate = DateTime.Now.AddDays(-1), Status = "Overdue" },
                new() { RoomCode = "A203", TenantName = "Phạm Minh Cường", BillingMonth = "07/2026", TotalAmountFormatted = "3.200.000 ₫", DueDate = DateTime.Now.AddDays(2), Status = "Pending" },
                new() { RoomCode = "B201", TenantName = "Lê Hoàng Dương", BillingMonth = "07/2026", TotalAmountFormatted = "3.650.000 ₫", DueDate = DateTime.Now.AddDays(4), Status = "Partial" }
            };
        }

        private List<OverduePaymentViewModel> GetMockOverduePayments(int propertyId)
        {
            if (propertyId == 2)
            {
                return new List<OverduePaymentViewModel>
                {
                    new() { RoomCode = "B103", TenantName = "Vũ Việt Đức", AmountFormatted = "2.900.000 ₫", OverdueDays = 2 },
                    new() { RoomCode = "B201", TenantName = "Nguyễn Duy Mạnh", AmountFormatted = "1.500.000 ₫", OverdueDays = 12 }
                };
            }

            if (propertyId == 3)
            {
                return new List<OverduePaymentViewModel>
                {
                    new() { RoomCode = "C105", TenantName = "Hoàng Trung Kiên", AmountFormatted = "3.800.000 ₫", OverdueDays = 4 },
                    new() { RoomCode = "C201", TenantName = "Nguyễn Minh Hằng", AmountFormatted = "2.350.000 ₫", OverdueDays = 7 },
                    new() { RoomCode = "C302", TenantName = "Phan Anh Tuấn", AmountFormatted = "4.100.000 ₫", OverdueDays = 18 }
                };
            }

            return new List<OverduePaymentViewModel>
            {
                new() { RoomCode = "A102", TenantName = "Trần Thị Bình", AmountFormatted = "2.800.000 ₫", OverdueDays = 1 },
                new() { RoomCode = "B201", TenantName = "Lê Hoàng Dương", AmountFormatted = "1.800.000 ₫", OverdueDays = 9 },
                new() { RoomCode = "A204", TenantName = "Phan Văn Đông", AmountFormatted = "3.200.000 ₫", OverdueDays = 15 }
            };
        }

        private List<ExpiringContractViewModel> GetMockExpiringContracts(int propertyId)
        {
            if (propertyId == 2)
            {
                return new List<ExpiringContractViewModel>
                {
                    new() { TenantName = "Nguyễn Ngọc Ánh", RoomCode = "B102", ExpiryDate = DateTime.Now.AddDays(7), RemainingDays = 7, AvatarUrl = null },
                    new() { TenantName = "Phạm Gia Bảo", RoomCode = "B203", ExpiryDate = DateTime.Now.AddDays(15), RemainingDays = 15, AvatarUrl = null }
                };
            }

            if (propertyId == 3)
            {
                return new List<ExpiringContractViewModel>
                {
                    new() { TenantName = "Bùi Hồng Nhung", RoomCode = "C104", ExpiryDate = DateTime.Now.AddDays(5), RemainingDays = 5, AvatarUrl = null },
                    new() { TenantName = "Vũ Đình Khải", RoomCode = "C205", ExpiryDate = DateTime.Now.AddDays(12), RemainingDays = 12, AvatarUrl = null },
                    new() { TenantName = "Trương Mỹ Linh", RoomCode = "C303", ExpiryDate = DateTime.Now.AddDays(28), RemainingDays = 28, AvatarUrl = null }
                };
            }

            return new List<ExpiringContractViewModel>
            {
                new() { TenantName = "Nguyễn Văn Hùng", RoomCode = "A103", ExpiryDate = DateTime.Now.AddDays(4), RemainingDays = 4, AvatarUrl = null },
                new() { TenantName = "Đặng Thị Mai", RoomCode = "A201", ExpiryDate = DateTime.Now.AddDays(14), RemainingDays = 14, AvatarUrl = null },
                new() { TenantName = "Phan Minh Tuấn", RoomCode = "B104", ExpiryDate = DateTime.Now.AddDays(25), RemainingDays = 25, AvatarUrl = null }
            };
        }

        private List<MaintenanceRequestViewModel> GetMockMaintenanceRequests(int propertyId)
        {
            if (propertyId == 2)
            {
                return new List<MaintenanceRequestViewModel>
                {
                    new() { Category = "Điện", RoomCode = "B103", Description = "Hỏng ổ cắm tường cạnh giường ngủ", Priority = "High", Status = "New", SubmittedTimeText = "1 giờ trước" },
                    new() { Category = "Nước", RoomCode = "B202", Description = "Nước vòi chậu rửa mặt chảy yếu", Priority = "Medium", Status = "In-Progress", SubmittedTimeText = "Hôm qua" }
                };
            }

            if (propertyId == 3)
            {
                return new List<MaintenanceRequestViewModel>
                {
                    new() { Category = "Thiết bị", RoomCode = "C102", Description = "Điều hòa không mát, có tiếng kêu to", Priority = "High", Status = "New", SubmittedTimeText = "3 giờ trước" },
                    new() { Category = "Internet", RoomCode = "C203", Description = "Không kết nối được wifi phòng", Priority = "Low", Status = "New", SubmittedTimeText = "5 giờ trước" },
                    new() { Category = "Khác", RoomCode = "C304", Description = "Cửa ban công bị kẹt khó đóng", Priority = "Medium", Status = "In-Progress", SubmittedTimeText = "2 ngày trước" }
                };
            }

            return new List<MaintenanceRequestViewModel>
            {
                new() { Category = "Điện", RoomCode = "A101", Description = "Hỏng atomat bình nóng lạnh", Priority = "High", Status = "New", SubmittedTimeText = "30 phút trước" },
                new() { Category = "Nước", RoomCode = "A203", Description = "Vòi sen tắm bị rỉ nước liên tục", Priority = "Medium", Status = "New", SubmittedTimeText = "2 giờ trước" },
                new() { Category = "Thiết bị", RoomCode = "B201", Description = "Quạt trần rung lắc mạnh khi bật số lớn", Priority = "Low", Status = "In-Progress", SubmittedTimeText = "Hôm qua" },
                new() { Category = "An ninh", RoomCode = "Tầng 1", Description = "Khóa vân tay cửa ra vào khó nhận dạng", Priority = "High", Status = "Resolved", SubmittedTimeText = "3 ngày trước" }
            };
        }

        private List<ActivityItemViewModel> GetMockActivities(int propertyId)
        {
            return new List<ActivityItemViewModel>
            {
                new() { Icon = "check-circle", Title = "Xác nhận đóng tiền phòng A101 (3.450.000 ₫)", TimestampText = "10 phút trước", Category = "payment" },
                new() { Icon = "user-plus", Title = "Đã thêm người thuê Nguyễn Văn Đạt vào phòng A104", TimestampText = "2 giờ trước", Category = "tenant" },
                new() { Icon = "file-text", Title = "Đã tạo hợp đồng mới cho phòng B102 (Đại diện: Trần Mai)", TimestampText = "5 giờ trước", Category = "contract" },
                new() { Icon = "gauge", Title = "Ghi xong số điện nước tháng 7 cho toàn bộ khu trọ A", TimestampText = "Hôm qua", Category = "meter" },
                new() { Icon = "tool", Title = "Cập nhật trạng thái sửa chữa phòng B201: Đang tiến hành", TimestampText = "Hôm qua", Category = "maintenance" },
                new() { Icon = "invoice", Title = "Hệ thống tự động lập hóa đơn nháp tháng 7", TimestampText = "2 ngày trước", Category = "billing" }
            };
        }

        private List<QuickActionViewModel> GetMockQuickActions()
        {
            return new List<QuickActionViewModel>
            {
                new() { Id = "add_room", Label = "Thêm phòng mới", IconName = "plus-circle", ActionRoute = "#", Description = "Khai báo cấu hình phòng mới" },
                new() { Id = "add_tenant", Label = "Thêm người thuê", IconName = "user-plus", ActionRoute = "#", Description = "Lập thông tin cư dân mới" },
                new() { Id = "create_contract", Label = "Tạo hợp đồng", IconName = "file-plus", ActionRoute = "#", Description = "Lập tờ trình ký hợp đồng" },
                new() { Id = "record_utilities", Label = "Ghi số điện nước", IconName = "gauge", ActionRoute = "#", Description = "Nhập chỉ số đồng hồ dịch vụ" },
                new() { Id = "generate_invoice", Label = "Tạo hóa đơn", IconName = "receipt", ActionRoute = "#", Description = "Lập hóa đơn thanh toán tháng" },
                new() { Id = "confirm_payment", Label = "Xác nhận thanh toán", IconName = "check-square", ActionRoute = "#", Description = "Khớp dòng tiền thực nhận" }
            };
        }

        private List<NotificationSummaryViewModel> GetMockNotifications()
        {
            return new List<NotificationSummaryViewModel>
            {
                new() { Id = "notif_1", Title = "Yêu cầu sửa chữa khẩn cấp", Description = "Phòng A101 báo hỏng atomat bình nóng lạnh cần xử lý gấp.", TimeText = "30 phút trước", IsUnread = true, IconName = "tool", Type = "danger" },
                new() { Id = "notif_2", Title = "Hóa đơn quá hạn", Description = "Phòng A102 có hóa đơn Tháng 7 trị giá 2.800.000 ₫ đã quá hạn đóng 1 ngày.", TimeText = "1 giờ trước", IsUnread = true, IconName = "alert-triangle", Type = "warning" },
                new() { Id = "notif_3", Title = "Hợp đồng sắp hết hạn", Description = "Hợp đồng thuê phòng A103 của Nguyễn Văn Hùng sẽ hết hạn sau 4 ngày nữa.", TimeText = "2 giờ trước", IsUnread = false, IconName = "calendar", Type = "info" },
                new() { Id = "notif_4", Title = "Thanh toán phòng thành công", Description = "Hóa đơn phòng A101 (3.450.000 ₫) đã được thanh toán đầy đủ.", TimeText = "3 giờ trước", IsUnread = false, IconName = "check-circle", Type = "success" }
            };
        }
    }
}
