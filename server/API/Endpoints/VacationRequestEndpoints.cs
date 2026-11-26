using Infra.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Service;

namespace API.Endpoints;

public static class VacationRequestEndpoints
{
    public static void MapVacationRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vacation-requests").WithTags("VacationRequests");

        group.MapPost("/preview", async Task<Results<Ok<VacationRequestPreviewDto>, NotFound>> (
            CreateVacationRequestDto request,
            VacationRequestService service) =>
        {
            var preview = await service.PreviewAsync(request);
            return preview is null ? TypedResults.NotFound() : TypedResults.Ok(preview);
        }).WithName("PreviewVacationRequest");

        group.MapPost("/", async Task<Results<Created<VacationRequestCreatedDto>, NotFound, ValidationProblem>> (
            CreateVacationRequestDto request,
            VacationRequestService service) =>
        {
            var result = await service.CreateAsync(request);

            if (result is null) return TypedResults.NotFound();

            if (!result.Succeeded || result.Created is null)
            {
                var errors = result.Preview.Issues
                    .GroupBy(i => i.Field)
                    .ToDictionary(g => g.Key, g => g.Select(i => i.Message).ToArray());

                var extensions = new Dictionary<string, object?>
                {
                    ["codes"] = result.Preview.Issues.Select(i => i.Code).ToArray()
                };

                return TypedResults.ValidationProblem(errors, extensions: extensions);
            }

            return TypedResults.Created($"/api/vacation-requests/{result.Created.VacationRequestId}", result.Created);
        }).WithName("CreateVacationRequest");
    }
}