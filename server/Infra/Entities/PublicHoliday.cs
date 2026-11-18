using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("PublicHoliday")]
public class PublicHoliday
{
    [PrimaryKey, Identity]
    public int PublicHolidayId  { get; set; }

    [Column, NotNull]
    public DateOnly Date { get; set; }

    [Column, NotNull]
    public string Name { get; set; } = string.Empty;

    [Column, NotNull]
    public string DayType { get; set; } = ""; 
}