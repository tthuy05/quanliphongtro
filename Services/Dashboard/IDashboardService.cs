using System.Threading.Tasks;
using TroiSinhVien.Models.ViewModels.Dashboard;

namespace TroiSinhVien.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardPageViewModel> GetDashboardDataAsync(int propertyId, string month);
    }
}
