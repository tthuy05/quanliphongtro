using TroiSinhVien.Domain.Common;
using TroiSinhVien.Domain.Enums;

namespace TroiSinhVien.Domain.Entities;

public sealed class MeterReading : AuditableEntity
{
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public MeterType MeterType { get; set; }
    public int BillingYear { get; set; }
    public int BillingMonth { get; set; }
    public decimal PreviousReading { get; set; }
    public decimal CurrentReading { get; set; }
    public decimal Consumption { get; set; }
    public DateTimeOffset ReadAt { get; set; }
}

public sealed class Invoice : AuditableEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public int BillingYear { get; set; }
    public int BillingMonth { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal PreviousDebtAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTimeOffset? IssuedAt { get; set; }
    public string? CancellationReason { get; set; }
    public ICollection<InvoiceDetail> Details { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}

public sealed class InvoiceDetail : AuditableEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public InvoiceDetailSourceType SourceType { get; set; }
    public int? SourceReferenceId { get; set; }
}

public sealed class Payment : AuditableEntity
{
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ReferenceCode { get; set; }
    public int? EvidenceFileId { get; set; }
    public UploadedFile? EvidenceFile { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public Guid? ConfirmedBy { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public string? RejectionReason { get; set; }
}
