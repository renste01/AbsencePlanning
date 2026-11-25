using Infra.Dtos;
using Infra.Entities;

namespace Service;

public static class VacationRequestEvaluator
{
    public const int MaxRangeDays = 366;
    public const int MainHolidayDays = 15;
    public const int MainHolidayNoticeMonths = 3;
    public const int OtherHolidayNoticeMonths = 1;

    private const int MainWindowFirstMonth = 5;
    private const int MainWindowLastMonth = 9;

    public static VacationRequestPreviewDto Evaluate(
        CreateVacationRequestDto request,
        DateOnly today,
        Employee employee,
        IReadOnlyList<EmployeeSchedule> schedules,
        IReadOnlyList<PublicHoliday> publicHolidays,
        IReadOnlyList<VacationBalance> balances,
        IReadOnlyList<PlannedVacationDay> reservedDays,
        IReadOnlyList<HolidayRule> rules)
    {
        var result = new VacationRequestPreviewDto
        {
            HolidayType = request.HolidayType,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var validType = request.HolidayType == HolidayTypes.Weeks1To5 || request.HolidayType == HolidayTypes.Week6;

        if (!validType)
            AddIssue(result, VacationRequestIssueCodes.InvalidHolidayType, "holidayType",
                "The holiday type must be Weeks1to5 or Week6.");

        if (request.EndDate < request.StartDate)
        {
            AddIssue(result, VacationRequestIssueCodes.EndBeforeStart, "endDate",
                "The end date cannot be before the start date.");
            return Finish(result);
        }

        if (request.EndDate.DayNumber - request.StartDate.DayNumber + 1 > MaxRangeDays)
        {
            AddIssue(result, VacationRequestIssueCodes.RangeTooLong, "endDate",
                $"A request can cover at most {MaxRangeDays} days.");
            return Finish(result);
        }

        if (request.StartDate < today)
            AddIssue(result, VacationRequestIssueCodes.StartInPast, "startDate",
                "The start date cannot be in the past.");

        if (request.StartDate < employee.EmployeeStartDate
            || (employee.EmployeeEndDate != null && request.EndDate > employee.EmployeeEndDate))
            AddIssue(result, VacationRequestIssueCodes.OutsideEmployment, "startDate",
                "The period must be within the employee's employment.");

        var missingSchedule = BuildDays(request, schedules, publicHolidays, result);

        var counted = result.Days.Where(d => d.SkipReason == null).ToList();
        result.TotalDays = counted.Count;
        result.TotalHours = Round(counted.Sum(d => d.Hours));

        if (missingSchedule)
            AddIssue(result, VacationRequestIssueCodes.MissingSchedule, "startDate",
                "The employee has no work schedule for part of the period.");
        else if (counted.Count == 0)
            AddIssue(result, VacationRequestIssueCodes.NoWorkingDays, "startDate",
                "The period contains no working days.");

        var reservedDates = reservedDays.Select(d => d.Date).ToHashSet();
        var overlap = counted.FirstOrDefault(d => reservedDates.Contains(d.Date));
        if (overlap is not null)
            AddIssue(result, VacationRequestIssueCodes.OverlapsExistingRequest, "startDate",
                $"{overlap.Date:yyyy-MM-dd} is already part of another request.");

        if (request.HolidayType == HolidayTypes.Weeks1To5)
        {
            result.IsMainHoliday = counted.Count >= MainHolidayDays
                                   && InMainWindow(request.StartDate)
                                   && InMainWindow(request.EndDate);
            result.NoticeMonths = result.IsMainHoliday ? MainHolidayNoticeMonths : OtherHolidayNoticeMonths;

            if (request.StartDate < today.AddMonths(result.NoticeMonths))
                AddIssue(result, VacationRequestIssueCodes.NoticeTooShort, "startDate",
                    $"The request must be made at least {result.NoticeMonths} month(s) ahead.");
        }

        if (validType)
        {
            var uncovered = counted.FirstOrDefault(day => !balances.Any(b =>
                b.HolidayType == request.HolidayType
                && day.Date >= VacationBalanceCalculator.UsableFrom(b)
                && day.Date <= VacationBalanceCalculator.UsableTo(b)));

            if (uncovered is not null)
                AddIssue(result, VacationRequestIssueCodes.NoBalancePeriod, "holidayType",
                    $"No {request.HolidayType} balance can be used on {uncovered.Date:yyyy-MM-dd}.");

            var before = VacationBalanceCalculator.ProjectedAvailableHours(
                request.HolidayType, today, request.StartDate, balances, reservedDays, rules, schedules);
            var after = before - result.TotalHours;
            var maxDebt = WorkScheduleCalculator.WeeklyHours(schedules, request.StartDate);

            result.BalanceBeforeHours = Round(before);
            result.BalanceAfterHours = Round(after);
            result.MaxDebtHours = Round(maxDebt);

            if (after < -maxDebt)
                AddIssue(result, VacationRequestIssueCodes.ExceedsMaxDebt, "holidayType",
                    $"The request exceeds the maximum holiday debt of {Round(maxDebt):0.00} hours.");
        }

        return Finish(result);
    }

    private static bool BuildDays(
        CreateVacationRequestDto request,
        IReadOnlyList<EmployeeSchedule> schedules,
        IReadOnlyList<PublicHoliday> publicHolidays,
        VacationRequestPreviewDto result)
    {
        var holidayNames = publicHolidays
            .GroupBy(h => h.Date)
            .ToDictionary(g => g.Key, g => g.First().Name);

        var missingSchedule = false;

        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            var day = new VacationRequestDayDto { Date = date };

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                day.SkipReason = VacationRequestSkipReasons.Weekend;
            }
            else if (holidayNames.TryGetValue(date, out var holidayName))
            {
                day.SkipReason = holidayName;
            }
            else
            {
                var schedule = WorkScheduleCalculator.ActiveSchedule(schedules, date);

                if (schedule is null)
                {
                    missingSchedule = true;
                    day.SkipReason = VacationRequestSkipReasons.NoSchedule;
                }
                else
                {
                    var hours = WorkScheduleCalculator.HoursOn(schedule, date.DayOfWeek);

                    if (hours <= 0) day.SkipReason = VacationRequestSkipReasons.NotWorkingDay;
                    else day.Hours = Round(hours);
                }
            }

            result.Days.Add(day);
        }

        return missingSchedule;
    }

    private static bool InMainWindow(DateOnly date)
    {
        return date.Month >= MainWindowFirstMonth && date.Month <= MainWindowLastMonth;
    }

    private static void AddIssue(VacationRequestPreviewDto result, string code, string field, string message)
    {
        result.Issues.Add(new VacationRequestIssueDto { Code = code, Field = field, Message = message });
    }

    private static VacationRequestPreviewDto Finish(VacationRequestPreviewDto result)
    {
        result.IsValid = result.Issues.Count == 0;
        return result;
    }

    private static decimal Round(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}