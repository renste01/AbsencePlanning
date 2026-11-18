using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("VacationTransferRequest")]
public class VacationTransferRequest
{
    [PrimaryKey, Identity]
    public int VacationTransferRequestId { get; set; }

    [Column, NotNull]
    public int EmployeeId { get; set; }

    [Column, NotNull]
    public int FromVacationBalanceId { get; set; }

    [Column, NotNull]
    public int ToVacationBalanceId { get; set; }

    [Column, NotNull]
    public decimal Hours { get; set; }

    [Column, NotNull]
    public string Status { get; set; } = "Pending";

    [Column, NotNull]
    public DateTime RequestedAt { get; set; }
    
    [Column]
    public int? ProcessedByUserAccountId { get; set; }

    [Column]
    public DateTime? ProcessedAt { get; set; }

    [Column]
    public string? RejectionReason { get; set; }

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(Employee.EmployeeId))]
    public Employee? Employee { get; set; }

    [Association(ThisKey = nameof(FromVacationBalanceId), OtherKey = nameof(VacationBalance.VacationBalanceId))]
    public VacationBalance? FromHolidayBalance { get; set; }

    [Association(ThisKey = nameof(ToVacationBalanceId), OtherKey = nameof(VacationBalance.VacationBalanceId))]
    public VacationBalance? ToHolidayBalance { get; set; }
}