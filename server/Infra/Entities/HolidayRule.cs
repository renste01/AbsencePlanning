using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("HolidayRule")]
public class HolidayRule
{
    [PrimaryKey, Identity]
    public int HolidayRuleId { get; set; }

    [Column, NotNull]
    public string HolidayType { get; set; } = string.Empty;

    [Column, NotNull]
    public decimal AccrualAmount { get; set; }

    [Column, NotNull]
    public string AccrualUnit { get; set; } = string.Empty;

    [Column, NotNull]
    public DateOnly ValidFrom { get; set; }

    [Column]
    public DateOnly? ValidTo { get; set; }
}