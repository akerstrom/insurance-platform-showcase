using System.Text.Json.Serialization;
using InsuranceService.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var httpTimeoutSeconds = builder.Configuration.GetValue<int?>("HttpClient:TimeoutSeconds") ?? 30;

builder.Services.AddHttpClient<IInsuranceMainframeClient, InsuranceMainframeClient>(client =>
{
    var baseUrl = builder.Configuration["LegacyServices:InsuranceMainframe"]
        ?? "http://localhost:5102";
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

app.MapGet("/insurances/{pid}", async (string pid, IInsuranceMainframeClient client, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(pid))
    {
        return Results.BadRequest(new { error = "Bad request", message = "Personal identification number is required" });
    }

    try
    {
        var insurances = await client.GetInsurancesAsync(pid, ct);

        return insurances.Count > 0
            ? Results.Ok(insurances)
            : Results.NotFound(new { error = "Not found", message = $"No insurances found for PID {pid}" });
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
