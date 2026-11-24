namespace Infra.Dtos;

public class VacationProjectionDto
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public List<VacationProjectionAccountDto> Accounts { get; set; } = new();
}

public class VacationProjectionAccountDto
{
    public string HolidayType { get; set; } = string.Empty;
    public decimal ProjectedHours { get; set; }
    public int ApproximateWorkingDays { get; set; }
    public decimal ExpiringHours { get; set; }
}