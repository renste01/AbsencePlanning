using Infra.Dtos;
using Infra.Entities;
using Xunit;

namespace Service.Tests;

public class VacationBalanceCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 19);

    private static List<PlannedVacationDay> PlannedFebruary() => new()
    {
        TestData.Planned(HolidayTypes.Weeks1To5, new DateOnly(2027, 2, 15), 7.5m),
        TestData.Planned(HolidayTypes.Weeks1To5, new DateOnly(2027, 2, 16), 7.5m)
    };

    [Fact]
    public void UsableWindow_Weeks1To5_RunsFromPeriodStartToEndOfDecemberNextYear()
    {
        var balance = TestData.DefaultBalances()[0];

        Assert.Equal(new DateOnly(2025, 9, 1), VacationBalanceCalculator.UsableFrom(balance));
        Assert.Equal(new DateOnly(2027, 12, 31), VacationBalanceCalculator.UsableTo(balance));
    }

    [Fact]
    public void UsableWindow_Week6_RunsFromMayYearAfterEarningToAprilYearAfterThat()
    {
        var balance = TestData.DefaultBalances()[2];

        Assert.Equal(new DateOnly(2026, 5, 1), VacationBalanceCalculator.UsableFrom(balance));
        Assert.Equal(new DateOnly(2027, 4, 30), VacationBalanceCalculator.UsableTo(balance));
    }

    [Fact]
    public void BuildBalance_AvailableHours_OnlyCountsPeriodsUsableToday()
    {
        var result = VacationBalanceCalculator.BuildBalance(1, Today, TestData.DefaultBalances(), PlannedFebruary());

        var weeks = result.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5);
        var week6 = result.Accounts.Single(a => a.HolidayType == HolidayTypes.Week6);

        Assert.Equal(52.28m, weeks.AvailableHours);
        Assert.Equal(14.5m, week6.AvailableHours);
    }

    [Fact]
    public void BuildBalance_PlannedHours_AreAllocatedToTheEarliestExpiringPeriod()
    {
        var result = VacationBalanceCalculator.BuildBalance(1, Today, TestData.DefaultBalances(), PlannedFebruary());

        var periods = result.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5).Periods;

        Assert.Equal(15m, periods[0].PlannedHours);
        Assert.Equal(21.5m, periods[0].RemainingHours);
        Assert.Equal(0m, periods[1].PlannedHours);
    }

    [Fact]
    public void BuildBalance_Week6HoursEarnedThisYear_AreNotUsableYet()
    {
        var result = VacationBalanceCalculator.BuildBalance(1, Today, TestData.DefaultBalances(), PlannedFebruary());

        var periods = result.Accounts.Single(a => a.HolidayType == HolidayTypes.Week6).Periods;

        Assert.True(periods[0].IsUsableNow);
        Assert.False(periods[1].IsUsableNow);
    }

    [Fact]
    public void BuildBalance_ExpiresSoon_IsSetWhenHoursRemainAndWindowEndsWithinSixMonths()
    {
        var result = VacationBalanceCalculator.BuildBalance(1, Today, TestData.DefaultBalances(), PlannedFebruary());

        var week6 = result.Accounts.Single(a => a.HolidayType == HolidayTypes.Week6).Periods;
        var weeks = result.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5).Periods;

        Assert.True(week6[0].ExpiresSoon);
        Assert.False(weeks[0].ExpiresSoon);
    }

    [Fact]
    public void BuildBalance_TransferredHours_CountTowardsRemaining()
    {
        var balances = new List<VacationBalance>
        {
            TestData.Balance(1, HolidayTypes.Weeks1To5, new DateOnly(2025, 9, 1), new DateOnly(2026, 8, 31), 100m, 40m, 10m)
        };

        var result = VacationBalanceCalculator.BuildBalance(1, Today, balances, new List<PlannedVacationDay>());

        Assert.Equal(70m, result.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5).AvailableHours);
    }

    [Fact]
    public void BuildProjection_Weeks1To5_AddsOneMonthOfAccrualPerMonthStart()
    {
        var projection = VacationBalanceCalculator.BuildProjection(
            1, Today, new DateOnly(2026, 12, 31), TestData.DefaultBalances(), PlannedFebruary(),
            TestData.DefaultRules(), TestData.FullTimeSchedules());

        var weeks = projection.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5);

        Assert.Equal(67.67m, weeks.ProjectedHours);
        Assert.Equal(9, weeks.ApproximateWorkingDays);
    }

    [Fact]
    public void BuildProjection_Week6_ExpiredPeriodIsReportedAsExpiringAndNewPeriodBecomesUsable()
    {
        var projection = VacationBalanceCalculator.BuildProjection(
            1, Today, new DateOnly(2027, 5, 1), TestData.DefaultBalances(), PlannedFebruary(),
            TestData.DefaultRules(), TestData.FullTimeSchedules());

        var week6 = projection.Accounts.Single(a => a.HolidayType == HolidayTypes.Week6);

        Assert.Equal(36.99m, week6.ProjectedHours);
        Assert.Equal(14.5m, week6.ExpiringHours);
    }

    [Fact]
    public void BuildProjection_Weeks1To5_PeriodPastItsWindowIsExpiring()
    {
        var projection = VacationBalanceCalculator.BuildProjection(
            1, Today, new DateOnly(2028, 1, 1), TestData.DefaultBalances(), PlannedFebruary(),
            TestData.DefaultRules(), TestData.FullTimeSchedules());

        var weeks = projection.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5);

        Assert.Equal(169.31m, weeks.ProjectedHours);
        Assert.Equal(21.5m, weeks.ExpiringHours);
    }

    [Fact]
    public void BuildProjection_RuleChange_OnlyAffectsMonthsAfterTheNewRuleStarts()
    {
        var rules = new List<HolidayRule>
        {
            new()
            {
                HolidayType = HolidayTypes.Weeks1To5, AccrualAmount = 2.08m, AccrualUnit = "Days",
                ValidFrom = new DateOnly(2020, 9, 1), ValidTo = new DateOnly(2027, 1, 31)
            },
            new()
            {
                HolidayType = HolidayTypes.Weeks1To5, AccrualAmount = 2.16m, AccrualUnit = "Days",
                ValidFrom = new DateOnly(2027, 2, 1)
            }
        };

        var projection = VacationBalanceCalculator.BuildProjection(
            1, Today, new DateOnly(2027, 3, 1), TestData.DefaultBalances(), PlannedFebruary(),
            rules, TestData.FullTimeSchedules());

        var weeks = projection.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5);

        Assert.Equal(115.03m, weeks.ProjectedHours);
    }

    [Fact]
    public void BuildProjection_PartTimeSchedule_ScalesAccrual()
    {
        var schedules = new List<EmployeeSchedule>
        {
            TestData.Schedule(TestData.PartTimeSchedule(), new DateOnly(2020, 1, 1))
        };

        var today = new DateOnly(2026, 11, 19);
        var balances = new List<VacationBalance>
        {
            TestData.Balance(1, HolidayTypes.Weeks1To5, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31), 0m)
        };

        var projection = VacationBalanceCalculator.BuildProjection(
            1, today, new DateOnly(2026, 12, 31), balances, new List<PlannedVacationDay>(),
            TestData.DefaultRules(), schedules);

        var weeks = projection.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5);

        Assert.Equal(9.36m, weeks.ProjectedHours);
    }

    [Fact]
    public void ProjectedAvailableHours_MatchesTheProjectionForTheSameAccount()
    {
        var target = new DateOnly(2027, 3, 1);

        var single = VacationBalanceCalculator.ProjectedAvailableHours(
            HolidayTypes.Weeks1To5, Today, target, TestData.DefaultBalances(), PlannedFebruary(),
            TestData.DefaultRules(), TestData.FullTimeSchedules());

        var projection = VacationBalanceCalculator.BuildProjection(
            1, Today, target, TestData.DefaultBalances(), PlannedFebruary(),
            TestData.DefaultRules(), TestData.FullTimeSchedules());

        Assert.Equal(
            projection.Accounts.Single(a => a.HolidayType == HolidayTypes.Weeks1To5).ProjectedHours,
            Math.Round(single, 2, MidpointRounding.AwayFromZero));
    }
}
