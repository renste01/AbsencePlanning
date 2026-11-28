namespace Infra.Dtos;

public class PlannedVacationDay
{
    public int VacationRequestId { get; set; }
    public string HolidayType { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal Hours { get; set; }
}