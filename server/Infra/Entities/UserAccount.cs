using LinqToDB.Mapping;

namespace Infra.Entities;


[Table("UserAccount")]
public class UserAccount
{
    [PrimaryKey, Identity]
    public int UserAccountId { get; set; }

    [Column, NotNull]
    public int EmployeeId { get; set; }
    
    [Column, NotNull]
    public string RoleName { get; set; } = Roles.Employee;

    [Column, NotNull]
    public string Email { get; set; } = string.Empty;

    [Column, NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    [Association(ThisKey = nameof(EmployeeId), OtherKey = nameof(Employee.EmployeeId))]
    public Employee? Employee { get; set; }
    
}