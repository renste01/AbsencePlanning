namespace Infra.Dtos;

public class ApprovalWarningDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Blocking { get; set; }
}

public class ApprovalItemDto
{
    public string Kind { get; set; } = string.Empty;
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string HolidayType { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int Days { get; set; }
    public decimal Hours { get; set; }
    public decimal? BalanceBeforeHours { get; set; }
    public decimal? BalanceAfterHours { get; set; }
    public int? AbsentColleagues { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public List<ApprovalWarningDto> Warnings { get; set; } = new();
}

public class ApprovalDecisionDto
{
    public int AccountantUserAccountId { get; set; }
}

public class RejectApprovalDto
{
    public int AccountantUserAccountId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ApprovalResultDto
{
    public string Kind { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}