using Infra;
using Infra.Dtos;

namespace Service;

public class VacationBalanceService(VacationBalanceRepository repository, TimeProvider timeProvider)
{
    public DateOnly Today()
    {
        return DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
    }

    public async Task<VacationBalanceDto?> GetBalanceAsync(int employeeId)
    {
        if (!await repository.EmployeeExistsAsync(employeeId)) return null;

        var today = Today();
        var balances = await repository.GetBalancesAsync(employeeId);
        var plannedDays = await repository.GetPlannedDaysAsync(employeeId, today);

        return VacationBalanceCalculator.BuildBalance(employeeId, today, balances, plannedDays);
    }

    public async Task<VacationProjectionDto?> GetProjectionAsync(int employeeId, DateOnly date)
    {
        if (!await repository.EmployeeExistsAsync(employeeId)) return null;

        var today = Today();
        var balances = await repository.GetBalancesAsync(employeeId);
        var plannedDays = await repository.GetPlannedDaysAsync(employeeId, today);
        var rules = await repository.GetHolidayRulesAsync();
        var schedules = await repository.GetSchedulesAsync(employeeId);

        return VacationBalanceCalculator.BuildProjection(employeeId, today, date, balances, plannedDays, rules, schedules);
    }
}