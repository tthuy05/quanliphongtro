using TroiSinhVien.Domain.Common;
using TroiSinhVien.Domain.Enums;

namespace TroiSinhVien.Domain.Entities;

public sealed class BoardingHouse : AuditableEntity
{
    public Guid OwnerId { get; set; }
    public ApplicationUser Owner { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public decimal DefaultElectricityPrice { get; set; }
    public decimal DefaultWaterPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Room> Rooms { get; set; } = [];
    public ICollection<PropertyService> Services { get; set; } = [];
}

public sealed class Room : AuditableEntity
{
    public int BoardingHouseId { get; set; }
    public BoardingHouse BoardingHouse { get; set; } = null!;
    public string RoomCode { get; set; } = string.Empty;
    public string? RoomName { get; set; }
    public int? Floor { get; set; }
    public decimal? Area { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal DepositAmount { get; set; }
    public int MaximumOccupants { get; set; } = 1;
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public string? Description { get; set; }
    public ICollection<Contract> Contracts { get; set; } = [];
    public ICollection<RoomTenant> RoomTenants { get; set; } = [];
    public ICollection<MeterReading> MeterReadings { get; set; } = [];
}

public sealed class PropertyService : AuditableEntity
{
    public int BoardingHouseId { get; set; }
    public BoardingHouse BoardingHouse { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public ServiceCalculationType CalculationType { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ContractService> ContractServices { get; set; } = [];
}
