namespace TroiSinhVien.Domain.Enums;

public enum RoomStatus { Available, Occupied, Maintenance, Inactive }
public enum ContractStatus { Draft, Active, Expired, Ended, Cancelled }
public enum InvoiceStatus { Draft, Issued, PartiallyPaid, Paid, Overdue, Cancelled }
public enum PaymentStatus { Pending, Confirmed, Rejected }
public enum PaymentMethod { Cash, BankTransfer }
public enum MeterType { Electricity, Water }
public enum ServiceCalculationType { FixedPerRoom, PerPerson, PerUnit, Custom }
public enum MaintenancePriority { Low, Medium, High, Urgent }
public enum MaintenanceStatus { New, InProgress, WaitingForTenant, Completed, Cancelled }
public enum NotificationType { Info, InvoiceIssued, InvoiceDue, InvoiceOverdue, PaymentConfirmed, PaymentRejected, ContractExpiring, MaintenanceUpdated }
public enum InvoiceDetailSourceType { Rent, Electricity, Water, Service, PreviousDebt, Discount, Custom }
