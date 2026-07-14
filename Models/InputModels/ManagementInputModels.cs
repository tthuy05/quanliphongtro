using System.ComponentModel.DataAnnotations;
using TroiSinhVien.Domain.Enums;

namespace TroiSinhVien.Models.InputModels;

public sealed class BoardingHouseInputModel
{
    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(400)] public string Address { get; set; } = string.Empty;
    [Phone, StringLength(20)] public string? ContactPhone { get; set; }
    [Range(0, 1_000_000)] public decimal DefaultElectricityPrice { get; set; }
    [Range(0, 1_000_000)] public decimal DefaultWaterPrice { get; set; }
}

public sealed class RoomInputModel
{
    [Range(1, int.MaxValue)] public int BoardingHouseId { get; set; }
    [Required, StringLength(30)] public string RoomCode { get; set; } = string.Empty;
    [StringLength(100)] public string? RoomName { get; set; }
    [Range(-10, 200)] public int? Floor { get; set; }
    [Range(0, 10_000)] public decimal? Area { get; set; }
    [Range(0, 1_000_000_000)] public decimal MonthlyRent { get; set; }
    [Range(0, 1_000_000_000)] public decimal DepositAmount { get; set; }
    [Range(1, 100)] public int MaximumOccupants { get; set; } = 1;
    [EnumDataType(typeof(RoomStatus))] public RoomStatus Status { get; set; } = RoomStatus.Available;
    [StringLength(1000)] public string? Description { get; set; }
}

public sealed class TenantInputModel
{
    [Required, StringLength(150)] public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    [Required, Phone, StringLength(20)] public string PhoneNumber { get; set; } = string.Empty;
    [EmailAddress, StringLength(256)] public string? Email { get; set; }
    [StringLength(30), RegularExpression(@"^[A-Za-z0-9-]*$", ErrorMessage = "Số giấy tờ chỉ được chứa chữ, số và dấu gạch ngang.")] public string? IdentityNumber { get; set; }
    public DateOnly? IdentityIssueDate { get; set; }
    [StringLength(400)] public string? PermanentAddress { get; set; }
    [StringLength(20)] public string? VehiclePlate { get; set; }
    [StringLength(150)] public string? EmergencyContactName { get; set; }
    [Phone, StringLength(20)] public string? EmergencyContactPhone { get; set; }
}

public sealed class ContractInputModel : IValidatableObject
{
    [Required, StringLength(40)] public string ContractCode { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int RoomId { get; set; }
    [Range(1, int.MaxValue)] public int RepresentativeTenantId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    [Range(0, 1_000_000_000)] public decimal MonthlyRent { get; set; }
    [Range(0, 1_000_000_000)] public decimal DepositAmount { get; set; }
    [Range(0, 1_000_000)] public decimal ElectricityPrice { get; set; }
    [Range(0, 1_000_000)] public decimal WaterPrice { get; set; }
    [Range(1, 28)] public int PaymentDueDay { get; set; } = 10;
    [StringLength(4000)] public string? Terms { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate) yield return new("Ngày kết thúc phải sau ngày bắt đầu.", [nameof(EndDate)]);
    }
}

public sealed class PropertyServiceInputModel
{
    [Range(1, int.MaxValue)] public int BoardingHouseId { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Unit { get; set; } = string.Empty;
    [EnumDataType(typeof(ServiceCalculationType))] public ServiceCalculationType CalculationType { get; set; }
    [Range(0, 1_000_000_000)] public decimal UnitPrice { get; set; }
}

public sealed class AddContractMemberInputModel
{
    [Range(1, int.MaxValue)] public int ContractId { get; set; }
    [Range(1, int.MaxValue)] public int TenantId { get; set; }
}

public sealed class AddContractServiceInputModel
{
    [Range(1, int.MaxValue)] public int ContractId { get; set; }
    [Range(1, int.MaxValue)] public int PropertyServiceId { get; set; }
    [Range(typeof(decimal), "0.01", "1000000")] public decimal Quantity { get; set; } = 1;
    [Range(typeof(decimal), "0", "1000000000")] public decimal? UnitPriceOverride { get; set; }
}

public sealed class MeterReadingInputModel
{
    [Range(1, int.MaxValue)] public int RoomId { get; set; }
    [EnumDataType(typeof(MeterType))] public MeterType MeterType { get; set; }
    [Range(2000, 2200)] public int BillingYear { get; set; }
    [Range(1, 12)] public int BillingMonth { get; set; }
    [Range(0, 999_999_999)] public decimal CurrentReading { get; set; }
}

public sealed class GenerateInvoiceInputModel
{
    [Range(1, int.MaxValue)] public int ContractId { get; set; }
    [Range(2000, 2200)] public int BillingYear { get; set; }
    [Range(1, 12)] public int BillingMonth { get; set; }
    [Range(0, 1_000_000_000)] public decimal DiscountAmount { get; set; }
}

public sealed class BulkGenerateInvoiceInputModel
{
    [Range(1, int.MaxValue)] public int BoardingHouseId { get; set; }
    [Range(2000, 2200)] public int BillingYear { get; set; }
    [Range(1, 12)] public int BillingMonth { get; set; }
}

public sealed class RecordPaymentInputModel
{
    [Range(1, int.MaxValue)] public int InvoiceId { get; set; }
    [Range(typeof(decimal), "0.01", "1000000000")] public decimal Amount { get; set; }
    [EnumDataType(typeof(PaymentMethod))] public PaymentMethod Method { get; set; }
    [StringLength(100)] public string? ReferenceCode { get; set; }
    public IFormFile? Evidence { get; set; }
}

public sealed class ConfirmPaymentInputModel { [Range(1, int.MaxValue)] public int PaymentId { get; set; } }
public sealed class RejectPaymentInputModel { [Range(1, int.MaxValue)] public int PaymentId { get; set; } [Required, StringLength(500)] public string Reason { get; set; } = string.Empty; }

public sealed class CreateMaintenanceRequestInputModel
{
    [Required, StringLength(50)] public string Category { get; set; } = string.Empty;
    [Required, StringLength(150)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(2000)] public string Description { get; set; } = string.Empty;
    [EnumDataType(typeof(MaintenancePriority))] public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;
}

public sealed class UpdateMaintenanceStatusInputModel
{
    [Range(1, int.MaxValue)] public int RequestId { get; set; }
    [EnumDataType(typeof(MaintenanceStatus))] public MaintenanceStatus Status { get; set; }
    [EnumDataType(typeof(MaintenancePriority))] public MaintenancePriority Priority { get; set; }
    [StringLength(2000)] public string? Comment { get; set; }
}

public sealed class AddMaintenanceCommentInputModel
{
    [Range(1, int.MaxValue)] public int RequestId { get; set; }
    [Required, StringLength(2000)] public string Content { get; set; } = string.Empty;
}

public sealed class CreateSystemUserInputModel
{
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(150)] public string DisplayName { get; set; } = string.Empty;
    [Phone, StringLength(30)] public string? PhoneNumber { get; set; }
    [Required, StringLength(30)] public string Role { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
    public int? TenantProfileId { get; set; }
}

public sealed class ReportFilterInputModel : IValidatableObject
{
    public int? PropertyId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate.HasValue && ToDate.HasValue && ToDate < FromDate)
            yield return new("Ngày kết thúc phải bằng hoặc sau ngày bắt đầu.", [nameof(ToDate)]);
    }
}
