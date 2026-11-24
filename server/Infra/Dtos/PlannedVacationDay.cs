namespace Infra.Dtos;

public class PlannedVacationDay
{
    public string HolidayType { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Hours { get; set; }
}