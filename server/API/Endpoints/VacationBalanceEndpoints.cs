using Infra.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Service;

namespace API.Endpoints;

public static class VacationBalanceEndpoints
{
    public static void MapVacationBalanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vacation-balance").WithTags("VacationBalance");

        group.MapGet("/", async Task<Results<Ok<VacationBalanceDto>, NotFound>> (
            int employeeId,
            VacationBalanceService service) =>
        {
            var balance = await service.GetBalanceAsync(employeeId);
            return balance is null ? TypedResults.NotFound() : TypedResults.Ok(balance);
        }).WithName("GetVacationBalance");

        group.MapGet("/projection", async Task<Results<Ok<VacationProjectionDto>, NotFound, ValidationProblem>> (
            int employeeId,
            DateOnly date,
            VacationBalanceService service) =>
        {
            if (date < service.Today())
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["date"] = ["The date must be today or later."]
                });
            }

            var projection = await service.GetProjectionAsync(employeeId, date);
            return projection is null ? TypedResults.NotFound() : TypedResults.Ok(projection);
        }).WithName("GetVacationBalanceProjection");
    }
}