using Infra.Entities;
using Xunit;

namespace Service.Tests;

public class VacationRequestEvaluatorTests
{
    private static readonly DateOnly Monday = new(2027, 2, 15);
    private static readonly DateOnly Friday = new(2027, 2, 19);

    [Fact]
    public void FullWeek_IsValid_AndCosts37Hours()
    {
        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday));

        Assert.True(preview.IsValid);
        Assert.Empty(preview.Issues);
        Assert.Equal(5, preview.TotalDays);
        Assert.Equal(37m, preview.TotalHours);
    }

    [Fact]
    public void Balances_AreReportedBeforeAndAfterTheRequest()
    {
        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday));

        Assert.Equal(67.28m, preview.BalanceBeforeHours);
        Assert.Equal(30.28m, preview.BalanceAfterHours);
        Assert.Equal(37m, preview.MaxDebtHours);
    }

    [Fact]
    public void Weekend_IsSkipped_AndFridayCosts7Hours()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2027, 2, 18), new DateOnly(2027, 2, 22)));

        Assert.Equal(3, preview.TotalDays);
        Assert.Equal(22m, preview.TotalHours);
        Assert.Equal(2, preview.Days.Count(d => d.SkipReason == VacationRequestSkipReasons.Weekend));
    }

    [Fact]
    public void PublicHoliday_IsSkipped_AndKeepsItsName()
    {
        var holidays = new List<PublicHoliday> { TestData.Holiday(new DateOnly(2027, 3, 29), "2. påskedag") };

        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2027, 3, 29), new DateOnly(2027, 4, 2)),
            holidays: holidays);

        Assert.Equal(4, preview.TotalDays);
        Assert.Equal(29.5m, preview.TotalHours);
        Assert.Equal("2. påskedag", preview.Days.First().SkipReason);
    }

    [Fact]
    public void ScheduleChangeInsideThePeriod_UsesTheScheduleThatAppliesToEachDay()
    {
        var schedules = new List<Infra.Dtos.EmployeeSchedule>
        {
            TestData.Schedule(TestData.FullTimeSchedule(), new DateOnly(2020, 1, 1), new DateOnly(2027, 2, 17)),
            TestData.Schedule(TestData.SixHourSchedule(), new DateOnly(2027, 2, 18))
        };

        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday), schedules: schedules);

        Assert.Equal(34.5m, preview.TotalHours);
    }

    [Fact]
    public void PartTimeEmployee_DaysWithoutWorkHours_AreSkipped()
    {
        var schedules = new List<Infra.Dtos.EmployeeSchedule>
        {
            TestData.Schedule(TestData.PartTimeSchedule(), new DateOnly(2020, 1, 1))
        };

        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday), schedules: schedules);

        Assert.Equal(3, preview.TotalDays);
        Assert.Equal(22.5m, preview.TotalHours);
        Assert.Equal(2, preview.Days.Count(d => d.SkipReason == VacationRequestSkipReasons.NotWorkingDay));
    }

    [Fact]
    public void EndBeforeStart_IsRejected()
    {
        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Weeks1To5, Friday, Monday));

        Assert.False(preview.IsValid);
        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.EndBeforeStart));
    }

    [Fact]
    public void StartInThePast_IsRejected()
    {
        var yesterday = TestData.Today.AddDays(-1);

        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Week6, yesterday, yesterday));

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.StartInPast));
    }

    [Fact]
    public void RangeLongerThanAYear_IsRejectedWithoutBuildingDays()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2027, 1, 4), new DateOnly(2028, 2, 1)));

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.RangeTooLong));
        Assert.Empty(preview.Days);
    }

    [Fact]
    public void WeekendOnly_HasNoWorkingDays()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2027, 2, 20), new DateOnly(2027, 2, 21)));

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.NoWorkingDays));
        Assert.Equal(0, preview.TotalDays);
    }

    [Fact]
    public void UnknownHolidayType_IsRejected()
    {
        var preview = TestData.Evaluate(TestData.Request("Weeks7", Monday, Friday));

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.InvalidHolidayType));
    }

    [Fact]
    public void MissingSchedule_IsRejected()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday),
            schedules: new List<Infra.Dtos.EmployeeSchedule>());

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.MissingSchedule));
    }

    [Fact]
    public void PeriodAfterTheEmploymentEnds_IsRejected()
    {
        var employee = TestData.NewEmployee(end: new DateOnly(2027, 2, 16));

        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday), employee: employee);

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.OutsideEmployment));
    }

    [Fact]
    public void OverlapWithAnExistingRequest_IsRejected()
    {
        var reserved = new List<Infra.Dtos.PlannedVacationDay>
        {
            TestData.Planned(HolidayTypes.Weeks1To5, new DateOnly(2027, 2, 17), 7.5m)
        };

        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday), reserved: reserved);

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.OverlapsExistingRequest));
    }

    [Fact]
    public void ReservedHours_ReduceTheBalanceBeforeTheRequest()
    {
        var balances = new List<VacationBalance>
        {
            TestData.Balance(1, HolidayTypes.Weeks1To5, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31), 40m)
        };
        var reserved = new List<Infra.Dtos.PlannedVacationDay>
        {
            TestData.Planned(HolidayTypes.Weeks1To5, new DateOnly(2027, 5, 3), 20m)
        };

        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday), balances: balances, reserved: reserved);

        Assert.Equal(20m, preview.BalanceBeforeHours);
        Assert.Equal(-17m, preview.BalanceAfterHours);
        Assert.True(preview.IsValid);
    }

    [Fact]
    public void ShortNotice_OnOtherHoliday_IsRejected()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2026, 12, 7), new DateOnly(2026, 12, 11)));

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.NoticeTooShort));
        Assert.Equal(1, preview.NoticeMonths);
    }

    [Fact]
    public void OneMonthNotice_OnOtherHoliday_IsEnough()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2026, 12, 21), new DateOnly(2026, 12, 23)));

        Assert.False(TestData.HasIssue(preview, VacationRequestIssueCodes.NoticeTooShort));
    }

    [Fact]
    public void MainHoliday_NeedsThreeMonthsNotice()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2027, 7, 5), new DateOnly(2027, 7, 23)),
            today: new DateOnly(2027, 5, 1));

        Assert.True(preview.IsMainHoliday);
        Assert.Equal(3, preview.NoticeMonths);
        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.NoticeTooShort));
    }

    [Fact]
    public void MainHoliday_WithEnoughNoticeAndAccrual_IsValid()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2027, 7, 5), new DateOnly(2027, 7, 23)),
            today: new DateOnly(2027, 3, 1),
            rules: TestData.DefaultRules());

        Assert.True(preview.IsMainHoliday);
        Assert.Equal(15, preview.TotalDays);
        Assert.Equal(111m, preview.TotalHours);
        Assert.True(preview.IsValid);
    }

    [Fact]
    public void ThreeWeeksOutsideSummer_IsNotAMainHoliday()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2027, 2, 1), new DateOnly(2027, 2, 19)));

        Assert.False(preview.IsMainHoliday);
        Assert.Equal(1, preview.NoticeMonths);
    }

    [Fact]
    public void Week6_HasNoNoticeRequirement()
    {
        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Week6, new DateOnly(2027, 6, 7), new DateOnly(2027, 6, 9)),
            today: new DateOnly(2027, 6, 1));

        Assert.True(preview.IsValid);
        Assert.Equal(0, preview.NoticeMonths);
        Assert.Equal(22.5m, preview.TotalHours);
    }

    [Fact]
    public void Week6_BeforeTheHoursBecomeUsable_IsRejected()
    {
        var balances = new List<VacationBalance>
        {
            TestData.Balance(1, HolidayTypes.Week6, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 33.91m)
        };

        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Week6, new DateOnly(2026, 12, 7), new DateOnly(2026, 12, 11)),
            balances: balances);

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.NoBalancePeriod));
    }

    [Fact]
    public void RequestLargerThanBalanceAndDebtLimit_IsRejected()
    {
        var balances = new List<VacationBalance>
        {
            TestData.Balance(1, HolidayTypes.Weeks1To5, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31), 10m)
        };

        var preview = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, new DateOnly(2027, 2, 15), new DateOnly(2027, 3, 5)),
            balances: balances);

        Assert.True(TestData.HasIssue(preview, VacationRequestIssueCodes.ExceedsMaxDebt));
        Assert.Equal(-101m, preview.BalanceAfterHours);
    }

    [Fact]
    public void DebtUpToOneWeek_IsAllowed()
    {
        var balances = new List<VacationBalance>
        {
            TestData.Balance(1, HolidayTypes.Weeks1To5, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31), 10m)
        };

        var preview = TestData.Evaluate(TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday), balances: balances);

        Assert.False(TestData.HasIssue(preview, VacationRequestIssueCodes.ExceedsMaxDebt));
        Assert.Equal(-27m, preview.BalanceAfterHours);
    }

    [Fact]
    public void DebtLimit_ForPartTimeEmployee_IsTheirOwnWeek()
    {
        var schedules = new List<Infra.Dtos.EmployeeSchedule>
        {
            TestData.Schedule(TestData.PartTimeSchedule(), new DateOnly(2020, 1, 1))
        };
        var balances = new List<VacationBalance>
        {
            TestData.Balance(1, HolidayTypes.Weeks1To5, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31), 0m)
        };

        var oneWeek = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, Monday, Friday), schedules: schedules, balances: balances);
        var twoWeeks = TestData.Evaluate(
            TestData.Request(HolidayTypes.Weeks1To5, Monday, new DateOnly(2027, 2, 26)),
            schedules: schedules, balances: balances);

        Assert.Equal(22.5m, oneWeek.MaxDebtHours);
        Assert.False(TestData.HasIssue(oneWeek, VacationRequestIssueCodes.ExceedsMaxDebt));
        Assert.True(TestData.HasIssue(twoWeeks, VacationRequestIssueCodes.ExceedsMaxDebt));
    }
}
