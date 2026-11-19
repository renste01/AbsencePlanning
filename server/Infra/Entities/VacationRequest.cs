using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("VacationRequest")]
public class VacationRequest
{
    [PrimaryKey, Identity] public int VacationRequestId { get; set; }

    [Column, NotNull] public int EmployeeId { get; set; }

    [Column, NotNull] public DateOnly StartDate { get; set; }

    [Column, NotNull] public DateOnly EndDate { get; set; }

    [Column, NotNull] public string HolidayType { get; set; } = HolidayTypes.Weeks1To5;

    [Column(Precision = 8, Scale = 2), NotNull]
    public decimal RequestedHours { get; set; }

    [Column, NotNull] public string RequestStatus { get; set; } = RequestStatuses.Pending;

    [Column, NotNull] public DateTime RequestedAt { get; set; }

    [Column] public int? ProcessedByUserAccountId { get; set; }

    [Column] public DateTime? ProcessedAt { get; set; }

    [Column] public string? RejectionReason { get; set; }

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(Employee.EmployeeId))]
    public Employee? Employee { get; set; }

    [Association(ThisKey = nameof(ProcessedByUserAccountId), OtherKey = nameof(UserAccount.UserAccountId))]
    public UserAccount? ProcessedBy { get; set; }

    [Association(ThisKey = nameof(VacationRequestId), OtherKey = nameof(VacationRequestDay.VacationRequestId))]
    public List<VacationRequestDay> VacationRequestDays { get; set; } = new();
}