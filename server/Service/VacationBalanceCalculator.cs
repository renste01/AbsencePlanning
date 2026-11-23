using Infra.Dtos;
using Infra.Entities;

namespace Service;

public static class VacationBalanceCalculator
{
    public const decimal FullTimeWeeklyHours = 37m;
    public const int ExpiryWarningMonths = 6;

    private const int WorkingDaysPerWeek = 5;
    private const string DaysUnit = "Days";
    private const string HoursUnit = "Hours";

    private static readonly string[] AccountTypes = { HolidayTypes.Weeks1To5, HolidayTypes.Week6 };

    private sealed class PeriodState(VacationBalance balance, DateOnly usableFrom, DateOnly usableTo)
    {
        public VacationBalance Balance { get; } = balance;
        public DateOnly UsableFrom { get; } = usableFrom;
        public DateOnly UsableTo { get; } = usableTo;
        public decimal PlannedHours { get; set; }
        public decimal RemainingHours => Balance.BalanceHours - PlannedHours;
    }

    public static VacationBalanceDto BuildBalance(
        int employeeId,
        DateOnly today,
        IReadOnlyList<VacationBalance> balances,
        IReadOnlyList<PlannedVacationDay> plannedDays)
    {
        var periods = BuildPeriods(balances, plannedDays);
        var warningLimit = today.AddMonths(ExpiryWarningMonths);
        var result = new VacationBalanceDto { EmployeeId = employeeId, AsOfDate = today };

        foreach (var type in AccountTypes)
        {
            var account = new VacationAccountDto { HolidayType = type };
            var available = 0m;

            foreach (var period in periods
                         .Where(p => p.Balance.HolidayType == type)
                         .OrderBy(p => p.Balance.PeriodStartDate))
            {
                var usableNow = today >= period.UsableFrom && today <= period.UsableTo;
                var remaining = period.RemainingHours;

                account.Periods.Add(new VacationPeriodDto
                {
                    VacationBalanceId = period.Balance.VacationBalanceId,
                    EarnedFrom = period.Balance.PeriodStartDate,
                    EarnedTo = period.Balance.PeriodEndDate,
                    UsableFrom = period.UsableFrom,
                    UsableTo = period.UsableTo,
                    EarnedHours = Round(period.Balance.EarnedHours),
                    TransferredHours = Round(period.Balance.TransferredHours),
                    UsedHours = Round(period.Balance.UsedHours),
                    PlannedHours = Round(period.PlannedHours),
                    RemainingHours = Round(remaining),
                    IsUsableNow = usableNow,
                    ExpiresSoon = usableNow && remaining > 0 && period.UsableTo <= warningLimit
                });

                if (usableNow) available += remaining;
            }

            account.AvailableHours = Round(available);
            result.Accounts.Add(account);
        }

        return result;
    }

    public static VacationProjectionDto BuildProjection(
        int employeeId,
        DateOnly today,
        DateOnly target,
        IReadOnlyList<VacationBalance> balances,
        IReadOnlyList<PlannedVacationDay> plannedDays,
        IReadOnlyList<HolidayRule> rules,
        IReadOnlyList<EmployeeSchedule> schedules)
    {
        var periods = BuildPeriods(balances, plannedDays);
        var result = new VacationProjectionDto { EmployeeId = employeeId, Date = target };

        var weeklyHours = WeeklyHours(schedules, target);
        if (weeklyHours <= 0) weeklyHours = WeeklyHours(schedules, today);
        var averageDayHours = weeklyHours / WorkingDaysPerWeek;

        foreach (var type in AccountTypes)
        {
            var projected = 0m;
            var expiring = 0m;

            foreach (var period in periods.Where(p => p.Balance.HolidayType == type))
            {
                var remaining = period.RemainingHours;

                if (period.UsableTo < target)
                {
                    expiring += Math.Max(0m, remaining);
                    continue;
                }

                if (target < period.UsableFrom) continue;

                projected += remaining + AccruedBetween(period.Balance, type, today, target, rules, schedules);
            }

            var days = projected > 0 && averageDayHours > 0
                ? (int)Math.Floor(projected / averageDayHours)
                : 0;

            result.Accounts.Add(new VacationProjectionAccountDto
            {
                HolidayType = type,
                ProjectedHours = Round(projected),
                ApproximateWorkingDays = days,
                ExpiringHours = Round(expiring)
            });
        }

        return result;
    }

    public static DateOnly UsableFrom(VacationBalance balance)
    {
        return balance.HolidayType == HolidayTypes.Week6
            ? new DateOnly(balance.PeriodEndDate.Year + 1, 5, 1)
            : balance.PeriodStartDate;
    }

    public static DateOnly UsableTo(VacationBalance balance)
    {
        return balance.HolidayType == HolidayTypes.Week6
            ? new DateOnly(balance.PeriodEndDate.Year + 2, 4, 30)
            : new DateOnly(balance.PeriodEndDate.Year + 1, 12, 31);
    }

    public static decimal WeeklyHours(IReadOnlyList<EmployeeSchedule> schedules, DateOnly date)
    {
        var active = schedules
            .Where(s => s.StartDate <= date && (s.EndDate == null || s.EndDate >= date))
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault();

        if (active is null) return 0m;

        var schedule = active.Schedule;
        return DayHours(schedule.MondayFrom, schedule.MondayTo)
               + DayHours(schedule.TuesdayFrom, schedule.TuesdayTo)
               + DayHours(schedule.WednesdayFrom, schedule.WednesdayTo)
               + DayHours(schedule.ThursdayFrom, schedule.ThursdayTo)
               + DayHours(schedule.FridayFrom, schedule.FridayTo);
    }

    private static List<PeriodState> BuildPeriods(
        IReadOnlyList<VacationBalance> balances,
        IReadOnlyList<PlannedVacationDay> plannedDays)
    {
        var periods = balances
            .Select(b => new PeriodState(b, UsableFrom(b), UsableTo(b)))
            .ToList();

        foreach (var day in plannedDays.OrderBy(d => d.Date))
        {
            var eligible = periods
                .Where(p => p.Balance.HolidayType == day.HolidayType
                            && day.Date >= p.UsableFrom
                            && day.Date <= p.UsableTo)
                .OrderBy(p => p.UsableTo)
                .ToList();

            var left = day.Hours;

            foreach (var period in eligible)
            {
                if (left <= 0) break;

                var room = Math.Max(0m, period.RemainingHours);
                var take = Math.Min(room, left);
                period.PlannedHours += take;
                left -= take;
            }

            if (left > 0 && eligible.Count > 0) eligible[^1].PlannedHours += left;
        }

        return periods;
    }

    private static decimal AccruedBetween(
        VacationBalance balance,
        string type,
        DateOnly today,
        DateOnly target,
        IReadOnlyList<HolidayRule> rules,
        IReadOnlyList<EmployeeSchedule> schedules)
    {
        var total = 0m;
        var last = target < balance.PeriodEndDate ? target : balance.PeriodEndDate;
        var month = new DateOnly(today.Year, today.Month, 1).AddMonths(1);

        while (month <= last)
        {
            if (month >= balance.PeriodStartDate)
                total += MonthlyAccrualHours(type, month, rules, schedules);

            month = month.AddMonths(1);
        }

        return total;
    }

    private static decimal MonthlyAccrualHours(
        string type,
        DateOnly month,
        IReadOnlyList<HolidayRule> rules,
        IReadOnlyList<EmployeeSchedule> schedules)
    {
        var rule = rules
            .Where(r => r.HolidayType == type && r.ValidFrom <= month && (r.ValidTo == null || r.ValidTo >= month))
            .OrderByDescending(r => r.ValidFrom)
            .FirstOrDefault();

        if (rule is null) return 0m;

        var weeklyHours = WeeklyHours(schedules, month);

        if (string.Equals(rule.AccrualUnit, DaysUnit, StringComparison.OrdinalIgnoreCase))
            return rule.AccrualAmount * weeklyHours / WorkingDaysPerWeek;

        if (string.Equals(rule.AccrualUnit, HoursUnit, StringComparison.OrdinalIgnoreCase))
            return rule.AccrualAmount * weeklyHours / FullTimeWeeklyHours;

        return 0m;
    }

    private static decimal DayHours(TimeOnly? from, TimeOnly? to)
    {
        if (from is null || to is null || to.Value <= from.Value) return 0m;
        return (decimal)(to.Value - from.Value).TotalHours;
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}