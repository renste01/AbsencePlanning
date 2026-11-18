using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("WorkSchedulePeriod")]
public class WorkSchedulePeriod
{
    [PrimaryKey, Identity]
    public int WorkSchedulePeriodId { get; set; }

    [Column, NotNull]
    public int EmployeeId { get; set; }

    [Column, NotNull]
    public int WorkScheduleId { get; set; }

    [Column, NotNull]
    public DateOnly StartDate { get; set; }

    [Column]
    public DateOnly? EndDate { get; set; }

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(Employee.EmployeeId))]
    public Employee? Employee { get; set; }

    [Association(ThisKey = nameof(WorkScheduleId), OtherKey = nameof(WorkSchedule.WorkScheduleId))]
    public WorkSchedule? WorkSchedule { get; set; }
}