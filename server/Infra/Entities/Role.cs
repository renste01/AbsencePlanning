using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("Role")]
public class Role
{
    [PrimaryKey, Identity]
    public int RoleId { get; set; }

    [Column, NotNull]
    public string RoleName { get; set; } = string.Empty;
}