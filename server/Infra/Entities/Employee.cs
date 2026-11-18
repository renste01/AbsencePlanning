using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("Employee")]
public class Employee
{
    [PrimaryKey, Identity]
    public int EmployeeId { get; set; }

    [Column, NotNull]
    public string FirstName { get; set; } = string.Empty;

    [Column, NotNull]
    public string LastName { get; set; } = string.Empty;

    [Column]
    public string? Address { get; set; }

    [Column]
    public string? PostalCode { get; set; }

    [Column]
    public string? City { get; set; }

    [Column]
    public string? Country { get; set; }

    [Column]
    public string? PhoneNumber { get; set; }

    [Column, NotNull]
    public DateOnly EmployeeStartDate { get; set; }

    [Column]
    public DateOnly? EmployeeEndDate { get; set; }

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(UserAccount.EmployeeId))]
    public List<UserAccount> UserAccounts { get; set; } = new();

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(VacationRequest.EmployeeId))]
    public List<VacationRequest> VacationRequests { get; set; } = new();

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(VacationBalance.EmployeeId))]
    public List<VacationBalance> VacationBalances { get; set; } = new();

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(WorkSchedulePeriod.EmployeeId))]
    public List<WorkSchedulePeriod> WorkSchedulePeriods { get; set; } = new();
}