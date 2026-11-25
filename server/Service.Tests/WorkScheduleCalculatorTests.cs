using Infra.Entities;
using Xunit;

namespace Service.Tests;

public class WorkScheduleCalculatorTests
{
    [Fact]
    public void WeeklyHours_FullTimeSchedule_Is37()
    {
        var weekly = WorkScheduleCalculator.WeeklyHours(TestData.FullTimeSchedules(), TestData.Today);

        Assert.Equal(37m, weekly);
    }

    [Fact]
    public void WeeklyHours_PartTimeSchedule_IsProportional()
    {
        var schedules = new List<Infra.Dtos.EmployeeSchedule>
        {
            TestData.Schedule(TestData.PartTimeSchedule(), new DateOnly(2020, 1, 1))
        };

        Assert.Equal(22.5m, WorkScheduleCalculator.WeeklyHours(schedules, TestData.Today));
    }

    [Fact]
    public void HoursOn_MondayToThursday_Is7Point5AndFridayIs7()
    {
        var schedule = TestData.FullTimeSchedule();

        Assert.Equal(7.5m, WorkScheduleCalculator.HoursOn(schedule, DayOfWeek.Monday));
        Assert.Equal(7.5m, WorkScheduleCalculator.HoursOn(schedule, DayOfWeek.Thursday));
        Assert.Equal(7m, WorkScheduleCalculator.HoursOn(schedule, DayOfWeek.Friday));
    }

    [Fact]
    public void HoursOn_Weekend_IsZero()
    {
        var schedule = TestData.FullTimeSchedule();

        Assert.Equal(0m, WorkScheduleCalculator.HoursOn(schedule, DayOfWeek.Saturday));
        Assert.Equal(0m, WorkScheduleCalculator.HoursOn(schedule, DayOfWeek.Sunday));
    }

    [Fact]
    public void ActiveSchedule_PicksTheScheduleCoveringTheDate()
    {
        var schedules = new List<Infra.Dtos.EmployeeSchedule>
        {
            TestData.Schedule(TestData.FullTimeSchedule(), new DateOnly(2020, 1, 1), new DateOnly(2027, 2, 17)),
            TestData.Schedule(TestData.SixHourSchedule(), new DateOnly(2027, 2, 18))
        };

        var before = WorkScheduleCalculator.ActiveSchedule(schedules, new DateOnly(2027, 2, 17));
        var after = WorkScheduleCalculator.ActiveSchedule(schedules, new DateOnly(2027, 2, 18));

        Assert.Equal(37m, WorkScheduleCalculator.WeeklyHours(schedules, new DateOnly(2027, 2, 17)));
        Assert.Equal(30m, WorkScheduleCalculator.WeeklyHours(schedules, new DateOnly(2027, 2, 18)));
        Assert.NotNull(before);
        Assert.NotNull(after);
    }

    [Fact]
    public void ActiveSchedule_NoScheduleForDate_ReturnsNull()
    {
        var schedules = new List<Infra.Dtos.EmployeeSchedule>
        {
            TestData.Schedule(TestData.FullTimeSchedule(), new DateOnly(2027, 1, 1))
        };

        Assert.Null(WorkScheduleCalculator.ActiveSchedule(schedules, new DateOnly(2026, 12, 31)));
        Assert.Equal(0m, WorkScheduleCalculator.WeeklyHours(schedules, new DateOnly(2026, 12, 31)));
    }
}
