using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Models.ViewModels;
using TroiSinhVien.Services.Common;

namespace TroiSinhVien.Services.Interfaces;

public interface IBoardingHouseService
{
    Task<IReadOnlyList<BoardingHouseListItem>> ListAsync(CancellationToken ct = default);
    Task<BoardingHouseDetailsViewModel?> DetailsAsync(int id, CancellationToken ct = default);
    Task<BoardingHouseInputModel?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateAsync(BoardingHouseInputModel input, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(int id, BoardingHouseInputModel input, CancellationToken ct = default);
    Task<ServiceResult> DeactivateAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> ActivateAsync(int id, CancellationToken ct = default);
}
public interface IRoomService
{
    Task<PagedResult<RoomListItem>> ListAsync(int? propertyId, RoomStatus? status, int? floor, decimal? minPrice, decimal? maxPrice, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<RoomDetailsViewModel?> DetailsAsync(int id, CancellationToken ct = default);
    Task<RoomInputModel?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateAsync(RoomInputModel input, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(int id, RoomInputModel input, CancellationToken ct = default);
    Task<ServiceResult> DeactivateAsync(int id, CancellationToken ct = default);
}
public interface ITenantService
{
    Task<IReadOnlyList<TenantListItem>> ListAsync(string? search, CancellationToken ct = default);
    Task<TenantDetailsViewModel?> DetailsAsync(int id, CancellationToken ct = default);
    Task<TenantInputModel?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateAsync(TenantInputModel input, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(int id, TenantInputModel input, CancellationToken ct = default);
}
public interface IContractService
{
    Task<IReadOnlyList<ContractListItem>> ListAsync(CancellationToken ct = default);
    Task<ContractDetailsViewModel?> DetailsAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateDraftAsync(ContractInputModel input, CancellationToken ct = default);
    Task<ServiceResult> AddMemberAsync(int contractId, int tenantId, CancellationToken ct = default);
    Task<ServiceResult> ActivateAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> ExtendAsync(int id, DateOnly newEndDate, CancellationToken ct = default);
    Task<ServiceResult> EndAsync(int id, bool completeMoveOut, CancellationToken ct = default);
    Task<ServiceResult> CancelAsync(int id, string reason, CancellationToken ct = default);
}
public interface IServiceCatalogService
{
    Task<IReadOnlyList<PropertyServiceListItem>> ListAsync(int? propertyId, CancellationToken ct = default);
    Task<PropertyServiceInputModel?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateAsync(PropertyServiceInputModel input, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(int id, PropertyServiceInputModel input, CancellationToken ct = default);
    Task<ServiceResult> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
    Task<ServiceResult> AddToContractAsync(AddContractServiceInputModel input, CancellationToken ct = default);
}
public interface IMeterReadingService
{
    Task<IReadOnlyList<MeterReadingListItem>> ListAsync(int propertyId, CancellationToken ct = default);
    Task<MeterReadingInputModel?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateAsync(MeterReadingInputModel input, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(int id, MeterReadingInputModel input, CancellationToken ct = default);
}
public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceListItem>> ListOwnerAsync(CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceListItem>> ListOwnerFilteredAsync(int? propertyId, InvoiceStatus? status, int? year, int? month, CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceListItem>> ListTenantAsync(CancellationToken ct = default);
    Task<InvoiceDetailsViewModel?> DetailsOwnerAsync(int id, CancellationToken ct = default);
    Task<InvoiceDetailsViewModel?> DetailsTenantAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> GenerateAsync(GenerateInvoiceInputModel input, CancellationToken ct = default);
    Task<ServiceResult<int>> BulkGenerateAsync(BulkGenerateInvoiceInputModel input, CancellationToken ct = default);
    Task<ServiceResult> IssueAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> CancelAsync(int id, string reason, CancellationToken ct = default);
}
public interface IPaymentService
{
    Task<IReadOnlyList<PaymentListItem>> ListOwnerAsync(CancellationToken ct = default);
    Task<ServiceResult<int>> RecordOwnerAsync(RecordPaymentInputModel input, CancellationToken ct = default);
    Task<ServiceResult<int>> SubmitTenantAsync(RecordPaymentInputModel input, CancellationToken ct = default);
    Task<ServiceResult> ConfirmAsync(int paymentId, CancellationToken ct = default);
    Task<ServiceResult> RejectAsync(int paymentId, string reason, CancellationToken ct = default);
    Task<ServiceResult<EvidenceDownload>> GetEvidenceAsync(int paymentId, CancellationToken ct = default);
}
public interface IMaintenanceService
{
    Task<IReadOnlyList<MaintenanceListItem>> ListOwnerAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MaintenanceListItem>> ListTenantAsync(CancellationToken ct = default);
    Task<MaintenanceDetailsViewModel?> DetailsOwnerAsync(int id, CancellationToken ct = default);
    Task<MaintenanceDetailsViewModel?> DetailsTenantAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateTenantAsync(CreateMaintenanceRequestInputModel input, CancellationToken ct = default);
    Task<ServiceResult> UpdateOwnerAsync(UpdateMaintenanceStatusInputModel input, CancellationToken ct = default);
    Task<ServiceResult> AddCommentAsync(AddMaintenanceCommentInputModel input, CancellationToken ct = default);
}
public interface INotificationService
{
    Task GenerateOperationalAsync(CancellationToken ct = default);
    Task<IReadOnlyList<NotificationListItem>> ListAsync(CancellationToken ct = default);
    Task<int> UnreadCountAsync(CancellationToken ct = default);
    Task<ServiceResult> MarkReadAsync(int id, CancellationToken ct = default);
    Task MarkAllReadAsync(CancellationToken ct = default);
}
public interface IReportService
{
    Task<RevenueReportViewModel> RevenueAsync(ReportFilterInputModel filter, CancellationToken ct = default);
    Task<OccupancyReportViewModel> OccupancyAsync(ReportFilterInputModel filter, CancellationToken ct = default);
    Task<DebtReportViewModel> DebtAsync(ReportFilterInputModel filter, CancellationToken ct = default);
}
