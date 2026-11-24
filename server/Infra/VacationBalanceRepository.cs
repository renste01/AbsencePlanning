using Infra.Dtos;
using Infra.Entities;
using LinqToDB;
using LinqToDB.Async;

namespace Infra;

public class VacationBalanceRepository(DatabaseConnection db)
{
    public async Task<bool> EmployeeExistsAsync(int employeeId)
    {
        return await db.GetTable<Employee>().AnyAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<List<VacationBalance>> GetBalancesAsync(int employeeId)
    {
        return await db.GetTable<VacationBalance>()
            .Where(b => b.EmployeeId == employeeId)
            .OrderBy(b => b.PeriodStartDate)
            .ToListAsync();
    }

    public async Task<List<PlannedVacationDay>> GetPlannedDaysAsync(int employeeId, DateOnly fromDate)
    {
        return await (
            from d in db.GetTable<VacationRequestDay>()
            join r in db.GetTable<VacationRequest>() on d.VacationRequestId equals r.VacationRequestId
            where r.EmployeeId == employeeId
                  && d.Date >= fromDate
                  && (r.RequestStatus == RequestStatuses.Approved
                      || r.RequestStatus == RequestStatuses.CancellationRequested)
            select new PlannedVacationDay
            {
                HolidayType = r.HolidayType,
                Date = d.Date,
                Hours = d.Hours
            }).ToListAsync();
    }

    public async Task<List<HolidayRule>> GetHolidayRulesAsync()
    {
        return await db.GetTable<HolidayRule>().ToListAsync();
    }

    public async Task<List<EmployeeSchedule>> GetSchedulesAsync(int employeeId)
    {
        return await (
            from p in db.GetTable<WorkSchedulePeriod>()
            join s in db.GetTable<WorkSchedule>() on p.WorkScheduleId equals s.WorkScheduleId
            where p.EmployeeId == employeeId
            select new EmployeeSchedule
            {
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Schedule = s
            }).ToListAsync();
    }
}