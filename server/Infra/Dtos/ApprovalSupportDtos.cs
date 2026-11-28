using Infra.Entities;

namespace Infra.Dtos;

public class ColleagueAbsence
{
    public DateOnly Date { get; set; }
    public int EmployeeId { get; set; }
}

public class EmployeeInfo
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CancellationWithRequest
{
    public VacationCancellationRequest Cancellation { get; set; } = new();
    public VacationRequest Request { get; set; } = new();
}