namespace Infra.Entities;

public static class HolidayTypes
{
    public const string Weeks1To5 = "Weeks1to5";
    public const string Week6 = "Week6";
}

public static class RequestStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string CancellationRequested = "CancellationRequested";
    public const string Cancelled = "Cancelled";
}

public static class Roles
{
    public const string Employee = "Employee";
    public const string Accountant = "Accountant";
    public const string SystemOwner = "SystemOwner";
}
