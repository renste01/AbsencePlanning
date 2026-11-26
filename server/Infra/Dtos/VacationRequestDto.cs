namespace Infra.Dtos;

public class CreateVacationRequestDto
{
    public int EmployeeId { get; set; }
    public string HolidayType { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class VacationRequestDayDto
{
    public DateOnly Date { get; set; }
    public decimal Hours { get; set; }
    public string? SkipReason { get; set; }
}

public class VacationRequestIssueDto
{
    public string Code { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class VacationRequestPreviewDto
{
    public string HolidayType { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<VacationRequestDayDto> Days { get; set; } = new();
    public int TotalDays { get; set; }
    public decimal TotalHours { get; set; }
    public decimal BalanceBeforeHours { get; set; }
    public decimal BalanceAfterHours { get; set; }
    public decimal MaxDebtHours { get; set; }
    public bool IsMainHoliday { get; set; }
    public int NoticeMonths { get; set; }
    public bool IsValid { get; set; }
    public List<VacationRequestIssueDto> Issues { get; set; } = new();
}

public class VacationRequestCreatedDto
{
    public int VacationRequestId { get; set; }
    public int EmployeeId { get; set; }
    public string HolidayType { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalDays { get; set; }
    public decimal RequestedHours { get; set; }
    public string RequestStatus { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}