using VehicleDatabase.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "legacy-vehicle-db",
    timestamp = DateTime.UtcNow,
    version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"
}));

app.MapGet("/vehicles/{regnr}", (string regnr) =>
{
    var vehicle = VehicleStore.GetByRegnr(regnr);
    return vehicle is not null
        ? Results.Ok(vehicle)
        : Results.NotFound(new { error = "Not found", message = $"No vehicle found with registration {regnr}" });
});

app.Run();
