using Infra.Dtos;
using Infra.Entities;

namespace Service.Tests;

public static class TestData
{
    public static readonly DateOnly Today = new(2026, 11, 20);

    public static WorkSchedule FullTimeSchedule() => new()
    {
        MondayFrom = new TimeOnly(8, 0), MondayTo = new TimeOnly(15, 30),
        TuesdayFrom = new TimeOnly(8, 0), TuesdayTo = new TimeOnly(15, 30),
        WednesdayFrom = new TimeOnly(8, 0), WednesdayTo = new TimeOnly(15, 30),
        ThursdayFrom = new TimeOnly(8, 0), ThursdayTo = new TimeOnly(15, 30),
        FridayFrom = new TimeOnly(8, 0), FridayTo = new TimeOnly(15, 0)
    };

    public static WorkSchedule PartTimeSchedule() => new()
    {
        MondayFrom = new TimeOnly(8, 0), MondayTo = new TimeOnly(15, 30),
        TuesdayFrom = new TimeOnly(8, 0), TuesdayTo = new TimeOnly(15, 30),
        WednesdayFrom = new TimeOnly(8, 0), WednesdayTo = new TimeOnly(15, 30)
    };

    public static WorkSchedule SixHourSchedule() => new()
    {
        MondayFrom = new TimeOnly(8, 0), MondayTo = new TimeOnly(14, 0),
        TuesdayFrom = new TimeOnly(8, 0), TuesdayTo = new TimeOnly(14, 0),
        WednesdayFrom = new TimeOnly(8, 0), WednesdayTo = new TimeOnly(14, 0),
        ThursdayFrom = new TimeOnly(8, 0), ThursdayTo = new TimeOnly(14, 0),
        FridayFrom = new TimeOnly(8, 0), FridayTo = new TimeOnly(14, 0)
    };

    public static List<EmployeeSchedule> FullTimeSchedules() =>
        new() { Schedule(FullTimeSchedule(), new DateOnly(2020, 1, 1)) };

    public static EmployeeSchedule Schedule(WorkSchedule schedule, DateOnly start, DateOnly? end = null) => new()
    {
        StartDate = start,
        EndDate = end,
        Schedule = schedule
    };

    public static Employee NewEmployee(DateOnly? start = null, DateOnly? end = null) => new()
    {
        EmployeeId = 1,
        FirstName = "Test",
        LastName = "Employee",
        EmployeeStartDate = start ?? new DateOnly(2020, 1, 1),
        EmployeeEndDate = end
    };

    public static VacationBalance Balance(
        int id, string type, DateOnly start, DateOnly end, decimal earned, decimal used = 0, decimal transferred = 0) => new()
    {
        VacationBalanceId = id,
        EmployeeId = 1,
        HolidayType = type,
        PeriodStartDate = start,
        PeriodEndDate = end,
        EarnedHours = earned,
        UsedHours = used,
        TransferredHours = transferred
    };

    public static List<VacationBalance> DefaultBalances() => new()
    {
        Balance(1, HolidayTypes.Weeks1To5, new DateOnly(2025, 9, 1), new DateOnly(2026, 8, 31), 185m, 148.5m),
        Balance(2, HolidayTypes.Weeks1To5, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31), 30.78m),
        Balance(3, HolidayTypes.Week6, new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31), 37m, 22.5m),
        Balance(4, HolidayTypes.Week6, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 33.91m)
    };

    public static List<HolidayRule> DefaultRules() => new()
    {
        new HolidayRule
        {
            HolidayType = HolidayTypes.Weeks1To5, AccrualAmount = 2.08m, AccrualUnit = "Days",
            ValidFrom = new DateOnly(2020, 9, 1)
        },
        new HolidayRule
        {
            HolidayType = HolidayTypes.Week6, AccrualAmount = 3.083m, AccrualUnit = "Hours",
            ValidFrom = new DateOnly(2020, 1, 1)
        }
    };

    public static PlannedVacationDay Planned(string type, DateOnly date, decimal hours) => new()
    {
        HolidayType = type,
        Date = date,
        Hours = hours
    };

    public static PublicHoliday Holiday(DateOnly date, string name) => new()
    {
        Date = date,
        Name = name,
        DayType = "PublicHoliday"
    };

    public static CreateVacationRequestDto Request(string type, DateOnly start, DateOnly end) => new()
    {
        EmployeeId = 1,
        HolidayType = type,
        StartDate = start,
        EndDate = end
    };

    public static VacationRequestPreviewDto Evaluate(
        CreateVacationRequestDto request,
        DateOnly? today = null,
        Employee? employee = null,
        List<EmployeeSchedule>? schedules = null,
        List<PublicHoliday>? holidays = null,
        List<VacationBalance>? balances = null,
        List<PlannedVacationDay>? reserved = null,
        List<HolidayRule>? rules = null)
    {
        return VacationRequestEvaluator.Evaluate(
            request,
            today ?? Today,
            employee ?? NewEmployee(),
            schedules ?? FullTimeSchedules(),
            holidays ?? new List<PublicHoliday>(),
            balances ?? DefaultBalances(),
            reserved ?? new List<PlannedVacationDay>(),
            rules ?? new List<HolidayRule>());
    }

    public static bool HasIssue(VacationRequestPreviewDto preview, string code) =>
        preview.Issues.Any(i => i.Code == code);
}
