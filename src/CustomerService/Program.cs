using System.Text.Json.Serialization;
using CustomerService.Clients;
using CustomerService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHttpClient<IInsuranceServiceClient, InsuranceServiceClient>(client =>
{
    var baseUrl = builder.Configuration["Services:InsuranceService"]
        ?? "http://localhost:5002";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<IVehicleServiceClient, VehicleServiceClient>(client =>
{
    var baseUrl = builder.Configuration["Services:VehicleService"]
        ?? "http://localhost:5001";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/customers/{pid}/insurances", async (
    string pid,
    IInsuranceServiceClient insuranceClient,
    IVehicleServiceClient vehicleClient,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(pid))
    {
        return Results.BadRequest(new { error = "Bad request", message = "Personal identification number is required" });
    }

    try
    {
        var insurances = await insuranceClient.GetInsurancesAsync(pid, ct);

        if (insurances.Count == 0)
        {
            return Results.NotFound(new { error = "Not found", message = $"No insurances found for PID {pid}" });
        }

        var customerInsurances = await EnrichWithVehicleDetails(insurances, vehicleClient, logger, ct);

        return Results.Ok(customerInsurances);
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

static async Task<List<CustomerInsurance>> EnrichWithVehicleDetails(
    IReadOnlyList<InsuranceInfo> insurances,
    IVehicleServiceClient vehicleClient,
    ILogger logger,
    CancellationToken ct)
{
    var result = new List<CustomerInsurance>();

    var carInsurances = insurances.Where(i => i.Type == InsuranceType.Car && !string.IsNullOrEmpty(i.Regnr)).ToList();
    var vehicleTasks = carInsurances.ToDictionary(
        i => i.Regnr!,
        i => FetchVehicleSafe(vehicleClient, i.Regnr!, logger, ct));

    await Task.WhenAll(vehicleTasks.Values);

    foreach (var insurance in insurances)
    {
        Vehicle? vehicle = null;

        if (insurance.Type == InsuranceType.Car && !string.IsNullOrEmpty(insurance.Regnr))
        {
            vehicle = vehicleTasks[insurance.Regnr].Result;
        }

        result.Add(new CustomerInsurance(
            insurance.Id,
            insurance.Pid,
            insurance.Type,
            insurance.Status,
            insurance.Premium,
            vehicle));
    }

    return result;
}

static async Task<Vehicle?> FetchVehicleSafe(
    IVehicleServiceClient vehicleClient,
    string regnr,
    ILogger logger,
    CancellationToken ct)
{
    try
    {
        return await vehicleClient.GetVehicleAsync(regnr, ct);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to fetch vehicle {Regnr}, returning insurance without vehicle details", regnr);
        return null;
    }
}

public partial class Program { }
