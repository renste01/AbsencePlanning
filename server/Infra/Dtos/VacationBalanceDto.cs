namespace Infra.Dtos;

public class VacationBalanceDto
{
    public int EmployeeId { get; set; }
    public DateOnly AsOfDate { get; set; }
    public List<VacationAccountDto> Accounts { get; set; } = new();
}

public class VacationAccountDto
{
    public string HolidayType { get; set; } = string.Empty;
    public decimal AvailableHours { get; set; }
    public List<VacationPeriodDto> Periods { get; set; } = new();
}

public class VacationPeriodDto
{
    public int VacationBalanceId { get; set; }
    public DateOnly EarnedFrom { get; set; }
    public DateOnly EarnedTo { get; set; }
    public DateOnly UsableFrom { get; set; }
    public DateOnly UsableTo { get; set; }
    public decimal EarnedHours { get; set; }
    public decimal TransferredHours { get; set; }
    public decimal UsedHours { get; set; }
    public decimal PlannedHours { get; set; }
    public decimal RemainingHours { get; set; }
    public bool IsUsableNow { get; set; }
    public bool ExpiresSoon { get; set; }
}