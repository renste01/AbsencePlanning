using Infra.Dtos;
using Infra.Entities;
using LinqToDB;
using LinqToDB.Async;

namespace Infra;

public class VacationRequestRepository(DatabaseConnection db)
{
    public async Task<Employee?> GetEmployeeAsync(int employeeId)
    {
        return await db.GetTable<Employee>().FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<List<PublicHoliday>> GetPublicHolidaysAsync(DateOnly fromDate, DateOnly toDate)
    {
        return await db.GetTable<PublicHoliday>()
            .Where(h => h.Date >= fromDate && h.Date <= toDate)
            .ToListAsync();
    }

    public async Task<List<PlannedVacationDay>> GetReservedDaysAsync(int employeeId, DateOnly fromDate)
    {
        return await (
            from d in db.GetTable<VacationRequestDay>()
            join r in db.GetTable<VacationRequest>() on d.VacationRequestId equals r.VacationRequestId
            where r.EmployeeId == employeeId
                  && d.Date >= fromDate
                  && (r.RequestStatus == RequestStatuses.Pending
                      || r.RequestStatus == RequestStatuses.Approved
                      || r.RequestStatus == RequestStatuses.CancellationRequested)
            select new PlannedVacationDay
            {
                HolidayType = r.HolidayType,
                Date = d.Date,
                Hours = d.Hours
            }).ToListAsync();
    }

    public async Task<int> InsertRequestAsync(VacationRequest request, IReadOnlyList<VacationRequestDay> days)
    {
        using var transaction = await db.BeginTransactionAsync();

        var requestId = await db.InsertWithInt32IdentityAsync(request);

        foreach (var day in days)
        {
            day.VacationRequestId = requestId;
            await db.InsertAsync(day);
        }

        await transaction.CommitAsync();
        return requestId;
    }
}