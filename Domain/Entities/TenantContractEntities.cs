using TroiSinhVien.Domain.Common;
using TroiSinhVien.Domain.Enums;

namespace TroiSinhVien.Domain.Entities;

public sealed class TenantProfile : AuditableEntity
{
    public Guid OwnerId { get; set; }
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IdentityNumber { get; set; }
    public DateOnly? IdentityIssueDate { get; set; }
    public string? PermanentAddress { get; set; }
    public string? VehiclePlate { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Contract> RepresentedContracts { get; set; } = [];
    public ICollection<ContractMember> ContractMemberships { get; set; } = [];
    public ICollection<RoomTenant> RoomTenancies { get; set; } = [];
}

public sealed class RoomTenant : AuditableEntity
{
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public int TenantProfileId { get; set; }
    public TenantProfile TenantProfile { get; set; } = null!;
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public DateOnly MoveInDate { get; set; }
    public DateOnly? MoveOutDate { get; set; }
}

public sealed class Contract : AuditableEntity
{
    public string ContractCode { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public int RepresentativeTenantId { get; set; }
    public TenantProfile RepresentativeTenant { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal ElectricityPrice { get; set; }
    public decimal WaterPrice { get; set; }
    public int PaymentDueDay { get; set; }
    public string? Terms { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string? CancellationReason { get; set; }
    public ICollection<ContractMember> Members { get; set; } = [];
    public ICollection<ContractService> Services { get; set; } = [];
    public ICollection<RoomTenant> RoomTenants { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
}

public sealed class ContractMember : AuditableEntity
{
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public int TenantProfileId { get; set; }
    public TenantProfile TenantProfile { get; set; } = null!;
    public bool IsRepresentative { get; set; }
    public DateOnly JoinedDate { get; set; }
    public DateOnly? LeftDate { get; set; }
}

public sealed class ContractService : AuditableEntity
{
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public int PropertyServiceId { get; set; }
    public PropertyService PropertyService { get; set; } = null!;
    public string ServiceNameSnapshot { get; set; } = string.Empty;
    public string UnitSnapshot { get; set; } = string.Empty;
    public ServiceCalculationType CalculationType { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
}
