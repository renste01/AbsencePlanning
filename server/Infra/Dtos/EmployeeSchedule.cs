using Infra.Entities;

namespace Infra.Dtos;

public class EmployeeSchedule
{
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public WorkSchedule Schedule { get; set; } = new();
}