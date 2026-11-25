namespace Service;

public static class VacationRequestIssueCodes
{
    public const string InvalidHolidayType = "INVALID_HOLIDAY_TYPE";
    public const string EndBeforeStart = "END_BEFORE_START";
    public const string RangeTooLong = "RANGE_TOO_LONG";
    public const string StartInPast = "START_IN_PAST";
    public const string OutsideEmployment = "OUTSIDE_EMPLOYMENT";
    public const string MissingSchedule = "MISSING_SCHEDULE";
    public const string NoWorkingDays = "NO_WORKING_DAYS";
    public const string OverlapsExistingRequest = "OVERLAPS_EXISTING_REQUEST";
    public const string NoticeTooShort = "NOTICE_TOO_SHORT";
    public const string NoBalancePeriod = "NO_BALANCE_PERIOD";
    public const string ExceedsMaxDebt = "EXCEEDS_MAX_DEBT";
}

public static class VacationRequestSkipReasons
{
    public const string Weekend = "Weekend";
    public const string NotWorkingDay = "NotWorkingDay";
    public const string NoSchedule = "NoSchedule";
}