using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("VacationTransaction")]
public class VacationTransaction
{
    [PrimaryKey, Identity] public int VacationTransactionId { get; set; }

    [Column, NotNull] public int EmployeeId { get; set; }

    [Column, NotNull] public int VacationBalanceId { get; set; }

    [Column, NotNull] public string TransactionType { get; set; } = string.Empty;

    [Column, NotNull] public decimal Hours { get; set; }

    [Column, NotNull] public DateOnly TransactionDate { get; set; }

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(Employee.EmployeeId))]
    public Employee? Employee { get; set; }

    [Association(ThisKey = nameof(VacationBalanceId), OtherKey = nameof(VacationBalance.VacationBalanceId))]
    public VacationBalance? VacationBalance { get; set; }
}