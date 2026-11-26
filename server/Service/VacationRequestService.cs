using Infra;
using Infra.Dtos;
using Infra.Entities;

namespace Service;

public class VacationRequestCreateResult
{
    public bool Succeeded { get; set; }
    public VacationRequestCreatedDto? Created { get; set; }
    public VacationRequestPreviewDto Preview { get; set; } = new();
}

public class VacationRequestService(
    VacationRequestRepository requestRepository,
    VacationBalanceRepository balanceRepository,
    TimeProvider timeProvider)
{
    public DateOnly Today()
    {
        return DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
    }

    public async Task<VacationRequestPreviewDto?> PreviewAsync(CreateVacationRequestDto request)
    {
        var employee = await requestRepository.GetEmployeeAsync(request.EmployeeId);
        if (employee is null) return null;

        return await EvaluateAsync(request, employee);
    }

    public async Task<VacationRequestCreateResult?> CreateAsync(CreateVacationRequestDto request)
    {
        var employee = await requestRepository.GetEmployeeAsync(request.EmployeeId);
        if (employee is null) return null;

        var preview = await EvaluateAsync(request, employee);
        if (!preview.IsValid) return new VacationRequestCreateResult { Succeeded = false, Preview = preview };

        var requestedAt = timeProvider.GetUtcNow().UtcDateTime;

        var entity = new VacationRequest
        {
            EmployeeId = request.EmployeeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            HolidayType = request.HolidayType,
            RequestedHours = preview.TotalHours,
            RequestStatus = RequestStatuses.Pending,
            RequestedAt = requestedAt
        };

        var days = preview.Days
            .Where(d => d.SkipReason == null)
            .Select(d => new VacationRequestDay { Date = d.Date, Hours = d.Hours })
            .ToList();

        var requestId = await requestRepository.InsertRequestAsync(entity, days);

        return new VacationRequestCreateResult
        {
            Succeeded = true,
            Preview = preview,
            Created = new VacationRequestCreatedDto
            {
                VacationRequestId = requestId,
                EmployeeId = request.EmployeeId,
                HolidayType = request.HolidayType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalDays = preview.TotalDays,
                RequestedHours = preview.TotalHours,
                RequestStatus = entity.RequestStatus,
                RequestedAt = requestedAt
            }
        };
    }

    private async Task<VacationRequestPreviewDto> EvaluateAsync(CreateVacationRequestDto request, Employee employee)
    {
        var today = Today();

        var rangeIsUsable = request.EndDate >= request.StartDate
                            && request.EndDate.DayNumber - request.StartDate.DayNumber + 1
                            <= VacationRequestEvaluator.MaxRangeDays;

        var holidays = rangeIsUsable
            ? await requestRepository.GetPublicHolidaysAsync(request.StartDate, request.EndDate)
            : new List<PublicHoliday>();

        var schedules = await balanceRepository.GetSchedulesAsync(request.EmployeeId);
        var balances = await balanceRepository.GetBalancesAsync(request.EmployeeId);
        var reserved = await requestRepository.GetReservedDaysAsync(request.EmployeeId, today);
        var rules = await balanceRepository.GetHolidayRulesAsync();

        return VacationRequestEvaluator.Evaluate(request, today, employee, schedules, holidays, balances, reserved, rules);
    }
}