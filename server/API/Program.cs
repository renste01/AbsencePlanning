using Infra;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Connection string 'DefaultConnection' mangler");
builder.Services.AddScoped(_ => new DatabaseConnection(cs));

var app = builder.Build();

app.MapGet("/db-test", (DatabaseConnection db) => $"Forbindelse OK. Employees: {db.Employees.Count()}");
// app.MapGet("/", () => "Hello World!");

app.MapGet("/api/calendar", async (DateOnly fromDate, DateOnly toDate, CalendarRepository repo) =>
    await repo.GetEventsAsync(fromDate, toDate));

app.Run();
