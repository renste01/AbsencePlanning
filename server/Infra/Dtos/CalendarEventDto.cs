namespace Infra.Dtos;

public class CalendarEventDto
{
    public DateOnly Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // "Vacation" eller "PublicHoliday"
    public int? EmployeeId { get; set; }
}