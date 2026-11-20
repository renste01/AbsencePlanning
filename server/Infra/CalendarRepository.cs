using Infra.Entities;
using LinqToDB;
using Infra.Dtos;
using LinqToDB.Async;

namespace Infra;

public class CalendarRepository
{
    private readonly DatabaseConnection _db;

    public CalendarRepository(DatabaseConnection db)
    {
        _db = db;
    }
//henter alle events i en periode.
    public async Task<List<CalendarEventDto>> GetEventsAsync(DateOnly fromDate, DateOnly toDate) 
    {
        //kobler 3 tables sammen med join så man kan finde status og medarbejderens navn på en enkelt dag.
        var vacations = await (
            from d in _db.GetTable<VacationRequestDay>()
            join r in _db.GetTable<VacationRequest>() on d.VacationRequestId equals r.VacationRequestId
            join e in _db.GetTable<Employee>() on r.EmployeeId equals e.EmployeeId
            where d.Date >= fromDate && d.Date <= toDate //her er det en filtreing så man kun finder de dage man leder efter
                                     && (r.RequestStatus == RequestStatuses.Approved
                                         || r.RequestStatus == RequestStatuses.CancellationRequested)
            select new CalendarEventDto
            {
                Date = d.Date,
                Title = e.FirstName + " " + e.LastName + " - Ferie",
                Type = "Vacation",
                EmployeeId = e.EmployeeId
            }).ToListAsync();

        var holidays = await (
            from h in _db.GetTable<PublicHoliday>()
            where h.Date >= fromDate && h.Date <= toDate
            select new CalendarEventDto
            {
                Date = h.Date,
                Title = h.Name,
                Type = "PublicHoliday",
                EmployeeId = null
            }).ToListAsync();

        //Sætter resultaterne sammen.
        return vacations.Concat(holidays).OrderBy(x => x.Date).ToList();
    }
}