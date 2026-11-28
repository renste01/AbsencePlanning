using Infra.Dtos;

namespace Service;

public enum ApprovalStatus
{
    Ok,
    Forbidden,
    NotFound,
    Conflict,
    Invalid
}

public class ApprovalResult<T>
{
    public ApprovalStatus Status { get; private set; }
    public T? Value { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public List<ApprovalWarningDto> Issues { get; private set; } = new();

    public static ApprovalResult<T> Ok(T value) => new() { Status = ApprovalStatus.Ok, Value = value };

    public static ApprovalResult<T> Forbidden() => new()
    {
        Status = ApprovalStatus.Forbidden,
        Message = "Only an accountant can process requests."
    };

    public static ApprovalResult<T> NotFound() => new() { Status = ApprovalStatus.NotFound };

    public static ApprovalResult<T> Conflict(string message) => new()
    {
        Status = ApprovalStatus.Conflict,
        Message = message
    };

    public static ApprovalResult<T> Invalid(List<ApprovalWarningDto> issues) => new()
    {
        Status = ApprovalStatus.Invalid,
        Issues = issues
    };
}

public static class ApprovalKinds
{
    public const string Vacation = "vacation";
    public const string Cancellation = "cancellation";
    public const string Transfer = "transfer";

    public static bool IsValid(string? kind)
    {
        return kind == Vacation || kind == Cancellation || kind == Transfer;
    }
}

public static class ApprovalViews
{
    public const string Pending = "pending";
    public const string Processed = "processed";
}

public static class ApprovalIssueCodes
{
    public const string ReasonRequired = "REASON_REQUIRED";
    public const string ReasonTooLong = "REASON_TOO_LONG";
    public const string BalanceNotFound = "BALANCE_NOT_FOUND";
    public const string TransferTypeMismatch = "TRANSFER_TYPE_MISMATCH";
    public const string TransferNotForward = "TRANSFER_NOT_FORWARD";
    public const string TransferExceedsOneWeek = "TRANSFER_EXCEEDS_ONE_WEEK";
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
}