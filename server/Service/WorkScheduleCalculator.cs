using Infra.Dtos;
using Infra.Entities;

namespace Service;

public static class WorkScheduleCalculator
{
    public static WorkSchedule? ActiveSchedule(IReadOnlyList<EmployeeSchedule> schedules, DateOnly date)
    {
        return schedules
            .Where(s => s.StartDate <= date && (s.EndDate == null || s.EndDate >= date))
            .OrderByDescending(s => s.StartDate)
            .Select(s => s.Schedule)
            .FirstOrDefault();
    }

    public static decimal HoursOn(WorkSchedule schedule, DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => DayHours(schedule.MondayFrom, schedule.MondayTo),
            DayOfWeek.Tuesday => DayHours(schedule.TuesdayFrom, schedule.TuesdayTo),
            DayOfWeek.Wednesday => DayHours(schedule.WednesdayFrom, schedule.WednesdayTo),
            DayOfWeek.Thursday => DayHours(schedule.ThursdayFrom, schedule.ThursdayTo),
            DayOfWeek.Friday => DayHours(schedule.FridayFrom, schedule.FridayTo),
            _ => 0m
        };
    }

    public static decimal WeeklyHours(IReadOnlyList<EmployeeSchedule> schedules, DateOnly date)
    {
        var schedule = ActiveSchedule(schedules, date);
        if (schedule is null) return 0m;

        return HoursOn(schedule, DayOfWeek.Monday)
               + HoursOn(schedule, DayOfWeek.Tuesday)
               + HoursOn(schedule, DayOfWeek.Wednesday)
               + HoursOn(schedule, DayOfWeek.Thursday)
               + HoursOn(schedule, DayOfWeek.Friday);
    }

    private static decimal DayHours(TimeOnly? from, TimeOnly? to)
    {
        if (from is null || to is null || to.Value <= from.Value) return 0m;
        return (decimal)(to.Value - from.Value).TotalHours;
    }
}