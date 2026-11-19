using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("VacationBalance")]
public class VacationBalance
{
    [PrimaryKey, Identity] public int VacationBalanceId { get; set; }

    [Column, NotNull] public int EmployeeId { get; set; }

    [Column, NotNull] public string HolidayType { get; set; } = HolidayTypes.Weeks1To5;

    [Column, NotNull] public DateOnly PeriodStartDate { get; set; }

    [Column, NotNull] public DateOnly PeriodEndDate { get; set; }

    [Column(Precision = 8, Scale = 2), NotNull]
    public decimal EarnedHours { get; set; }

    [Column(Precision = 8, Scale = 2), NotNull]
    public decimal UsedHours { get; set; }

    [Column(Precision = 8, Scale = 2), NotNull]
    public decimal TransferredHours { get; set; }

    [NotColumn] public decimal BalanceHours => EarnedHours + TransferredHours - UsedHours;

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(Employee.EmployeeId))]
    public Employee? Employee { get; set; }

    [Association(ThisKey = nameof(VacationBalanceId), OtherKey = nameof(VacationTransaction.VacationBalanceId))]
    public List<VacationTransaction> Transactions { get; set; } = new();

    [Association(ThisKey = nameof(VacationBalanceId), OtherKey = nameof(VacationTransferRequest.FromVacationBalanceId))]
    public List<VacationTransferRequest> TransferRequestsFrom { get; set; } = new();

    [Association(ThisKey = nameof(VacationBalanceId), OtherKey = nameof(VacationTransferRequest.ToVacationBalanceId))]
    public List<VacationTransferRequest> TransferRequestsTo { get; set; } = new();
}