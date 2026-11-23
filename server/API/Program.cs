using Infra;
using Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument();

var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? builder.Configuration.GetConnectionString("Default")
         ?? throw new InvalidOperationException("Connection string mangler");

builder.Services.AddScoped(_ => new DatabaseConnection(cs));
builder.Services.AddScoped<Authservice>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<UserAccountService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});


var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.UseCors("Frontend");
app.UseSwaggerUi();
app.UseOpenApi();
app.Run();