namespace Service.Dtos;

public class CalendarEventDto
{
    public DateTime Date { get; set; }
    public string Title { get; set; } = "";
    public string Type { get; set; } = ""; //er det en ferie eller offentlig fridag?
    public int? EmployeeId { get; set; }
}