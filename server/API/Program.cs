using API.Endpoints;
using Infra;
using Service;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Default")
         ?? throw new InvalidOperationException("Connection string 'Default' mangler");
builder.Services.AddScoped(_ => new DatabaseConnection(cs));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<CalendarRepository>();
builder.Services.AddScoped<VacationBalanceRepository>();
builder.Services.AddScoped<VacationBalanceService>();
builder.Services.AddScoped<VacationRequestRepository>();
builder.Services.AddScoped<VacationRequestService>();

var app = builder.Build();

app.MapGet("/db-test", (DatabaseConnection db) => $"Forbindelse OK. Employees: {db.Employees.Count()}");
// app.MapGet("/", () => "Hello World!");

app.MapGet("/api/calendar", async (DateOnly fromDate, DateOnly toDate, CalendarRepository repo) =>
    await repo.GetEventsAsync(fromDate, toDate));

app.MapVacationBalanceEndpoints();
app.MapVacationRequestEndpoints();

app.Run();