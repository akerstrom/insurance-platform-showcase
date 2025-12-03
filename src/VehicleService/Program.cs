using VehicleService.Clients;

var builder = WebApplication.CreateBuilder(args);

var httpTimeoutSeconds = builder.Configuration.GetValue<int?>("HttpClient:TimeoutSeconds") ?? 30;

builder.Services.AddHttpClient<IVehicleDatabaseClient, VehicleDatabaseClient>(client =>
{
    var baseUrl = builder.Configuration["LegacyServices:VehicleDatabase"]
        ?? "http://localhost:5101";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(httpTimeoutSeconds);
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"
}));

app.MapGet("/vehicles/{regnr}", async (string regnr, IVehicleDatabaseClient client, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(regnr))
    {
        return Results.BadRequest(new { error = "Bad request", message = "Registration number is required" });
    }

    try
    {
        var vehicle = await client.GetVehicleAsync(regnr, ct);

        return vehicle is not null
            ? Results.Ok(vehicle)
            : Results.NotFound(new { error = "Not found", message = $"No vehicle found with registration {regnr}" });
    }
    catch (TaskCanceledException)
    {
        return Results.StatusCode(504);
    }
    catch (HttpRequestException)
    {
        return Results.StatusCode(503);
    }
});

app.Run();

public partial class Program { }
