using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Data;
using TroiSinhVien.Domain.Entities;
using TroiSinhVien.Domain.Enums;
using TroiSinhVien.Models.InputModels;
using TroiSinhVien.Models.ViewModels;
using TroiSinhVien.Services.Common;
using TroiSinhVien.Services.Interfaces;

namespace TroiSinhVien.Services.Implementations;

public sealed class InvoiceService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock) : IInvoiceService
{
    private Guid UserId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<IReadOnlyList<InvoiceListItem>> ListOwnerAsync(CancellationToken ct = default)
    {
        return await ListOwnerFilteredAsync(null, null, null, null, ct);
    }
    public async Task<IReadOnlyList<InvoiceListItem>> ListOwnerFilteredAsync(int? propertyId, InvoiceStatus? status, int? year, int? month, CancellationToken ct = default)
    {
        var owner = UserId; var query = Query().Where(x => x.Room.BoardingHouse.OwnerId == owner);
        if (propertyId is > 0) query = query.Where(x => x.Room.BoardingHouseId == propertyId); if (status.HasValue) query = query.Where(x => x.Status == status); if (year.HasValue) query = query.Where(x => x.BillingYear == year); if (month.HasValue) query = query.Where(x => x.BillingMonth == month);
        return await query.OrderByDescending(x => x.BillingYear).ThenByDescending(x => x.BillingMonth).ThenByDescending(x => x.Id).Select(Map(clock.UtcToday)).ToListAsync(ct);
    }
    public async Task<IReadOnlyList<InvoiceListItem>> ListTenantAsync(CancellationToken ct = default)
    {
        var user = UserId; return await Query().Where(x => x.Contract.Members.Any(m => m.TenantProfile.UserId == user)).OrderByDescending(x => x.BillingYear).ThenByDescending(x => x.BillingMonth).Select(Map(clock.UtcToday)).ToListAsync(ct);
    }

    public Task<InvoiceDetailsViewModel?> DetailsOwnerAsync(int id, CancellationToken ct = default) => DetailsAsync(id, false, ct);
    public Task<InvoiceDetailsViewModel?> DetailsTenantAsync(int id, CancellationToken ct = default) => DetailsAsync(id, true, ct);

    private async Task<InvoiceDetailsViewModel?> DetailsAsync(int id, bool tenantView, CancellationToken ct)
    {
        var user = UserId; var query = db.Invoices.AsNoTracking().Where(x => x.Id == id);
        query = tenantView ? query.Where(x => x.Contract.Members.Any(m => m.TenantProfile.UserId == user)) : query.Where(x => x.Room.BoardingHouse.OwnerId == user);
        var invoice = await query.Include(x => x.Room).Include(x => x.Contract).ThenInclude(x => x.RepresentativeTenant).Include(x => x.Details).Include(x => x.Payments).FirstOrDefaultAsync(ct);
        if (invoice is null) return null;
        var previousDebt = await db.Invoices.AsNoTracking().Where(x => x.ContractId == invoice.ContractId && x.Id != invoice.Id && x.Status != InvoiceStatus.Cancelled && x.RemainingAmount > 0 && (x.BillingYear < invoice.BillingYear || x.BillingYear == invoice.BillingYear && x.BillingMonth < invoice.BillingMonth)).SumAsync(x => (decimal?)x.RemainingAmount, ct) ?? 0;
        var accountDebt = await db.Invoices.AsNoTracking().Where(x => x.ContractId == invoice.ContractId && x.Status != InvoiceStatus.Cancelled).SumAsync(x => (decimal?)x.RemainingAmount, ct) ?? 0;
        return new()
        {
            Invoice = ToListItem(invoice, clock.UtcToday), IsTenantView = tenantView, PriorOutstandingDebt = previousDebt, AccountOutstandingDebt = accountDebt,
            Details = invoice.Details.OrderBy(x => x.Id).Select(x => new InvoiceDetailListItem(x.Description, x.Quantity, x.Unit, x.UnitPrice, x.Amount, x.SourceType)).ToList(),
            Payments = invoice.Payments.OrderByDescending(x => x.PaidAt).Select(x => new PaymentListItem(x.Id, invoice.InvoiceNumber, x.Amount, x.Method, x.Status, x.PaidAt, x.ReferenceCode, x.EvidenceFileId, x.RejectionReason)).ToList()
        };
    }

    public async Task<ServiceResult<int>> GenerateAsync(GenerateInvoiceInputModel input, CancellationToken ct = default)
    {
        var owner = UserId; await using var tx = await db.Database.BeginTransactionAsync(ct);
        var contract = await db.Contracts.Include(x => x.Room).ThenInclude(x => x.BoardingHouse).Include(x => x.Services).Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == input.ContractId && x.Room.BoardingHouse.OwnerId == owner, ct);
        if (contract is null) return ServiceResult<int>.Failure("not_found", "Không tìm thấy hợp đồng."); if (contract.Status != ContractStatus.Active) return ServiceResult<int>.Failure("conflict", "Chỉ tạo hóa đơn cho hợp đồng hiệu lực."); if (await db.Invoices.AnyAsync(x => x.ContractId == input.ContractId && x.BillingYear == input.BillingYear && x.BillingMonth == input.BillingMonth && x.Status != InvoiceStatus.Cancelled, ct)) return ServiceResult<int>.Failure("conflict", "Hóa đơn kỳ này đã tồn tại.");
        var first = new DateOnly(input.BillingYear, input.BillingMonth, 1); var last = first.AddMonths(1).AddDays(-1); if (contract.StartDate > last || contract.EndDate < first) return ServiceResult<int>.Failure("validation", "Kỳ hóa đơn nằm ngoài thời hạn hợp đồng.");
        var details = new List<InvoiceDetail> { Detail("Tiền phòng", 1, "tháng", contract.MonthlyRent, InvoiceDetailSourceType.Rent) };
        var readings = await db.MeterReadings.AsNoTracking().Where(x => x.ContractId == contract.Id && x.BillingYear == input.BillingYear && x.BillingMonth == input.BillingMonth).ToListAsync(ct);
        var electricity = readings.FirstOrDefault(x => x.MeterType == MeterType.Electricity); if (electricity is not null) details.Add(Detail("Tiền điện", electricity.Consumption, "kWh", contract.ElectricityPrice, InvoiceDetailSourceType.Electricity, electricity.Id));
        var water = readings.FirstOrDefault(x => x.MeterType == MeterType.Water); if (water is not null) details.Add(Detail("Tiền nước", water.Consumption, "m³", contract.WaterPrice, InvoiceDetailSourceType.Water, water.Id));
        var memberCount = contract.Members.Count(x => x.LeftDate == null || x.LeftDate >= first);
        foreach (var service in contract.Services.Where(x => x.IsActive))
        {
            var quantity = service.CalculationType switch { ServiceCalculationType.PerPerson => memberCount, ServiceCalculationType.PerUnit or ServiceCalculationType.Custom => service.Quantity, _ => 1 };
            details.Add(Detail(service.ServiceNameSnapshot, quantity, service.UnitSnapshot, service.UnitPrice, InvoiceDetailSourceType.Service, service.PropertyServiceId));
        }
        // Previous invoices remain independent ledger items. Rolling their balance into this
        // invoice would count the same debt twice and make payment allocation ambiguous.
        var subtotal = details.Sum(x => x.Amount); const decimal previousDebt = 0;
        var total = subtotal + previousDebt - input.DiscountAmount; if (total < 0) return ServiceResult<int>.Failure("validation", "Giảm giá không được lớn hơn tổng tiền.");
        var invoice = new Invoice { InvoiceNumber = $"INV-{input.BillingYear}{input.BillingMonth:00}-{contract.Id}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}", ContractId = contract.Id, RoomId = contract.RoomId, BillingYear = input.BillingYear, BillingMonth = input.BillingMonth, DueDate = new DateOnly(input.BillingYear, input.BillingMonth, contract.PaymentDueDay), SubtotalAmount = subtotal, PreviousDebtAmount = previousDebt, DiscountAmount = input.DiscountAmount, TotalAmount = total, RemainingAmount = total, Status = InvoiceStatus.Draft, CreatedBy = owner, Details = details };
        db.Invoices.Add(invoice); Audit("Generate", invoice, owner); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return ServiceResult<int>.Success(invoice.Id);
    }
    public async Task<ServiceResult<int>> BulkGenerateAsync(BulkGenerateInvoiceInputModel input, CancellationToken ct = default)
    {
        var owner = UserId; var first = new DateOnly(input.BillingYear, input.BillingMonth, 1); var last = first.AddMonths(1).AddDays(-1);
        if (!await db.BoardingHouses.AnyAsync(x => x.Id == input.BoardingHouseId && x.OwnerId == owner && x.IsActive, ct)) return ServiceResult<int>.Failure("not_found", "Không tìm thấy khu trọ đang hoạt động.");
        var contractIds = await db.Contracts.AsNoTracking().Where(x => x.Room.BoardingHouseId == input.BoardingHouseId && x.Room.BoardingHouse.OwnerId == owner && x.Status == ContractStatus.Active && x.StartDate <= last && x.EndDate >= first).Select(x => x.Id).ToListAsync(ct);
        if (contractIds.Count == 0) return ServiceResult<int>.Failure("conflict", "Không có hợp đồng hiệu lực trong kỳ này.");
        var created = 0; foreach (var contractId in contractIds) { var result = await GenerateAsync(new GenerateInvoiceInputModel { ContractId = contractId, BillingYear = input.BillingYear, BillingMonth = input.BillingMonth }, ct); if (result.Succeeded) created++; }
        return ServiceResult<int>.Success(created);
    }

    public async Task<ServiceResult> IssueAsync(int id, CancellationToken ct = default)
    {
        var owner = UserId; var invoice = await db.Invoices.Include(x => x.Contract).ThenInclude(x => x.Members).ThenInclude(x => x.TenantProfile).FirstOrDefaultAsync(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner, ct); if (invoice is null) return ServiceResult.Failure("not_found", "Không tìm thấy hóa đơn."); if (invoice.Status != InvoiceStatus.Draft) return ServiceResult.Failure("conflict", "Chỉ hóa đơn nháp mới được phát hành."); invoice.Status = InvoiceStatus.Issued; invoice.IssuedAt = clock.UtcNow; invoice.UpdatedBy = owner;
        foreach (var userId in invoice.Contract.Members.Select(x => x.TenantProfile.UserId).Where(x => x.HasValue).Select(x => x!.Value).Distinct()) db.Notifications.Add(new Notification { UserId = userId, Type = NotificationType.InvoiceIssued, Title = "Hóa đơn mới", Message = $"Hóa đơn {invoice.InvoiceNumber} đã được phát hành.", LinkUrl = $"/Tenant/Invoices/{invoice.Id}", CreatedBy = owner });
        Audit("Issue", invoice, owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }

    public async Task<ServiceResult> CancelAsync(int id, string reason, CancellationToken ct = default)
    {
        var owner = UserId; if (string.IsNullOrWhiteSpace(reason)) return ServiceResult.Failure("validation", "Phải nhập lý do hủy."); var invoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id && x.Room.BoardingHouse.OwnerId == owner, ct); if (invoice is null) return ServiceResult.Failure("not_found", "Không tìm thấy hóa đơn."); if (invoice.PaidAmount > 0 || invoice.Status == InvoiceStatus.Paid) return ServiceResult.Failure("conflict", "Không thể hủy hóa đơn đã thanh toán."); invoice.Status = InvoiceStatus.Cancelled; invoice.CancellationReason = reason.Trim(); invoice.UpdatedBy = owner; Audit("Cancel", invoice, owner); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }

    private IQueryable<Invoice> Query() => db.Invoices.AsNoTracking();
    private static System.Linq.Expressions.Expression<Func<Invoice, InvoiceListItem>> Map(DateOnly today) => x => new InvoiceListItem(x.Id, x.InvoiceNumber, x.Room.RoomCode, x.Contract.RepresentativeTenant.FullName, x.BillingYear, x.BillingMonth, x.TotalAmount, x.PaidAmount, x.RemainingAmount, x.RemainingAmount > 0 && x.DueDate < today && (x.Status == InvoiceStatus.Issued || x.Status == InvoiceStatus.PartiallyPaid || x.Status == InvoiceStatus.Overdue) ? InvoiceStatus.Overdue : x.Status, x.DueDate);
    private static InvoiceListItem ToListItem(Invoice x, DateOnly today) => new(x.Id, x.InvoiceNumber, x.Room.RoomCode, x.Contract.RepresentativeTenant.FullName, x.BillingYear, x.BillingMonth, x.TotalAmount, x.PaidAmount, x.RemainingAmount, x.RemainingAmount > 0 && x.DueDate < today && x.Status is InvoiceStatus.Issued or InvoiceStatus.PartiallyPaid or InvoiceStatus.Overdue ? InvoiceStatus.Overdue : x.Status, x.DueDate);
    private static InvoiceDetail Detail(string description, decimal quantity, string unit, decimal price, InvoiceDetailSourceType type, int? source = null) => new() { Description = description, Quantity = quantity, Unit = unit, UnitPrice = price, Amount = quantity * price, SourceType = type, SourceReferenceId = source };
    private void Audit(string action, Invoice i, Guid owner) => db.AuditLogs.Add(new() { UserId = owner, Action = action, EntityType = nameof(Invoice), EntityId = i.Id.ToString(), Timestamp = clock.UtcNow, Summary = $"{action} hóa đơn {i.InvoiceNumber}." });
}

public sealed class PaymentService(ApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock, IFileStorageService storage) : IPaymentService
{
    private Guid UserId => current.UserId ?? throw new UnauthorizedAccessException();
    public async Task<IReadOnlyList<PaymentListItem>> ListOwnerAsync(CancellationToken ct = default)
    {
        var owner = UserId; return await db.Payments.AsNoTracking().Where(x => x.Invoice.Room.BoardingHouse.OwnerId == owner).OrderByDescending(x => x.Id).Select(x => new PaymentListItem(x.Id, x.Invoice.InvoiceNumber, x.Amount, x.Method, x.Status, x.PaidAt, x.ReferenceCode, x.EvidenceFileId, x.RejectionReason)).ToListAsync(ct);
    }
    public async Task<ServiceResult<int>> RecordOwnerAsync(RecordPaymentInputModel input, CancellationToken ct = default)
    {
        var owner = UserId; await using var tx = await db.Database.BeginTransactionAsync(ct); var invoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == input.InvoiceId && x.Room.BoardingHouse.OwnerId == owner, ct); if (invoice is null) return ServiceResult<int>.Failure("not_found", "Không tìm thấy hóa đơn."); var validation = Validate(invoice, input.Amount); if (validation is not null) return ServiceResult<int>.Failure("conflict", validation);
        var payment = new Payment { InvoiceId = invoice.Id, Amount = input.Amount, Method = input.Method, Status = PaymentStatus.Confirmed, ReferenceCode = input.ReferenceCode?.Trim(), PaidAt = clock.UtcNow, ConfirmedAt = clock.UtcNow, ConfirmedBy = owner, CreatedBy = owner }; db.Payments.Add(payment); ApplyConfirmed(invoice, input.Amount); Audit("Confirm", payment, owner); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return ServiceResult<int>.Success(payment.Id);
    }
    public async Task<ServiceResult<int>> SubmitTenantAsync(RecordPaymentInputModel input, CancellationToken ct = default)
    {
        var user = UserId; if (input.Method != PaymentMethod.BankTransfer || input.Evidence is null) return ServiceResult<int>.Failure("validation", "Thanh toán do người thuê gửi phải là chuyển khoản và có ảnh minh chứng.");
        var invoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == input.InvoiceId && x.Contract.Members.Any(m => m.TenantProfile.UserId == user), ct); if (invoice is null) return ServiceResult<int>.Failure("not_found", "Không tìm thấy hóa đơn."); var validation = Validate(invoice, input.Amount); if (validation is not null) return ServiceResult<int>.Failure("conflict", validation);
        var pendingAmount = await db.Payments.Where(x => x.InvoiceId == invoice.Id && x.Status == PaymentStatus.Pending).SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
        if (pendingAmount + input.Amount > invoice.RemainingAmount) return ServiceResult<int>.Failure("conflict", "Tổng thanh toán đang chờ xác nhận vượt quá dư nợ hóa đơn.");
        StoredFileResult stored; try { stored = await storage.SaveEvidenceAsync(input.Evidence, ct); } catch (InvalidOperationException ex) { return ServiceResult<int>.Failure("validation", ex.Message); }
        try
        {
            var file = new UploadedFile { StoredName = stored.StoredName, OriginalName = stored.OriginalName, ContentType = stored.ContentType, Size = stored.Size, RelativePath = stored.RelativePath, UploadedBy = user, CreatedBy = user };
            var payment = new Payment { InvoiceId = invoice.Id, Amount = input.Amount, Method = input.Method, Status = PaymentStatus.Pending, ReferenceCode = input.ReferenceCode?.Trim(), EvidenceFile = file, PaidAt = clock.UtcNow, CreatedBy = user };
            db.Payments.Add(payment); await db.SaveChangesAsync(ct); return ServiceResult<int>.Success(payment.Id);
        }
        catch
        {
            await storage.DeleteAsync(stored.RelativePath, ct);
            throw;
        }
    }
    public async Task<ServiceResult> ConfirmAsync(int paymentId, CancellationToken ct = default)
    {
        var owner = UserId; await using var tx = await db.Database.BeginTransactionAsync(ct); var payment = await db.Payments.Include(x => x.Invoice).ThenInclude(x => x.Contract).ThenInclude(x => x.Members).ThenInclude(x => x.TenantProfile).FirstOrDefaultAsync(x => x.Id == paymentId && x.Invoice.Room.BoardingHouse.OwnerId == owner, ct); if (payment is null) return ServiceResult.Failure("not_found", "Không tìm thấy thanh toán."); if (payment.Status != PaymentStatus.Pending) return ServiceResult.Failure("conflict", "Thanh toán không ở trạng thái chờ xác nhận."); var validation = Validate(payment.Invoice, payment.Amount); if (validation is not null) return ServiceResult.Failure("conflict", validation);
        payment.Status = PaymentStatus.Confirmed; payment.ConfirmedAt = clock.UtcNow; payment.ConfirmedBy = owner; payment.UpdatedBy = owner; ApplyConfirmed(payment.Invoice, payment.Amount); Audit("Confirm", payment, owner); foreach (var uid in payment.Invoice.Contract.Members.Select(x => x.TenantProfile.UserId).Where(x => x.HasValue).Select(x => x!.Value).Distinct()) db.Notifications.Add(new Notification { UserId = uid, Type = NotificationType.PaymentConfirmed, Title = "Thanh toán đã xác nhận", Message = $"Thanh toán cho {payment.Invoice.InvoiceNumber} đã được xác nhận.", CreatedBy = owner }); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return ServiceResult.Success();
    }
    public async Task<ServiceResult> RejectAsync(int paymentId, string reason, CancellationToken ct = default)
    {
        var owner = UserId; if (string.IsNullOrWhiteSpace(reason)) return ServiceResult.Failure("validation", "Phải nhập lý do từ chối."); var payment = await db.Payments.Include(x => x.Invoice).ThenInclude(x => x.Contract).ThenInclude(x => x.Members).ThenInclude(x => x.TenantProfile).FirstOrDefaultAsync(x => x.Id == paymentId && x.Invoice.Room.BoardingHouse.OwnerId == owner, ct); if (payment is null) return ServiceResult.Failure("not_found", "Không tìm thấy thanh toán."); if (payment.Status != PaymentStatus.Pending) return ServiceResult.Failure("conflict", "Thanh toán không ở trạng thái chờ."); payment.Status = PaymentStatus.Rejected; payment.RejectionReason = reason.Trim(); payment.UpdatedBy = owner; Audit("Reject", payment, owner); foreach (var uid in payment.Invoice.Contract.Members.Select(x => x.TenantProfile.UserId).Where(x => x.HasValue).Select(x => x!.Value).Distinct()) db.Notifications.Add(new Notification { UserId = uid, Type = NotificationType.PaymentRejected, Title = "Thanh toán bị từ chối", Message = reason.Trim(), CreatedBy = owner }); await db.SaveChangesAsync(ct); return ServiceResult.Success();
    }
    public async Task<ServiceResult<EvidenceDownload>> GetEvidenceAsync(int paymentId, CancellationToken ct = default)
    {
        var user = UserId;
        var file = await db.Payments.AsNoTracking()
            .Where(x => x.Id == paymentId && x.EvidenceFileId != null &&
                (x.Invoice.Room.BoardingHouse.OwnerId == user || x.Invoice.Contract.Members.Any(m => m.TenantProfile.UserId == user)))
            .Select(x => new { x.EvidenceFile!.RelativePath, x.EvidenceFile.ContentType, x.EvidenceFile.OriginalName })
            .FirstOrDefaultAsync(ct);
        if (file is null) return ServiceResult<EvidenceDownload>.Failure("not_found", "Không tìm thấy chứng từ thanh toán.");
        var stream = await storage.OpenReadAsync(file.RelativePath, ct);
        return stream is null
            ? ServiceResult<EvidenceDownload>.Failure("not_found", "Tệp chứng từ không còn tồn tại.")
            : ServiceResult<EvidenceDownload>.Success(new EvidenceDownload(stream, file.ContentType, file.OriginalName));
    }
    private static string? Validate(Invoice i, decimal amount) { if (i.Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled or InvoiceStatus.Paid) return "Hóa đơn không thể nhận thanh toán."; if (amount <= 0) return "Số tiền phải lớn hơn 0."; if (amount > i.RemainingAmount) return "Số tiền vượt quá dư nợ hóa đơn."; return null; }
    private void ApplyConfirmed(Invoice i, decimal amount) { i.PaidAmount += amount; i.RemainingAmount -= amount; i.Status = i.RemainingAmount == 0 ? InvoiceStatus.Paid : i.DueDate < clock.UtcToday ? InvoiceStatus.Overdue : InvoiceStatus.PartiallyPaid; }
    private void Audit(string action, Payment p, Guid owner) => db.AuditLogs.Add(new() { UserId = owner, Action = action, EntityType = nameof(Payment), EntityId = p.Id.ToString(), Timestamp = clock.UtcNow, Summary = $"{action} thanh toán cho hóa đơn {p.InvoiceId}." });
}
