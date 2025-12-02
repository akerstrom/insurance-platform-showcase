using System.Net;
using VehicleService.Models;

namespace VehicleService.Clients;

public class VehicleDatabaseClient : IVehicleDatabaseClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleDatabaseClient> _logger;

    public VehicleDatabaseClient(HttpClient httpClient, ILogger<VehicleDatabaseClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Vehicle?> GetVehicleAsync(string regnr, CancellationToken cancellationToken = default)
    {
        var encodedRegnr = Uri.EscapeDataString(regnr);
        var requestUrl = $"{_httpClient.BaseAddress}/vehicles/{encodedRegnr}";
        _logger.LogInformation("Calling LegacyVehicleDb: GET {Url}", requestUrl);

        try
        {
            var response = await _httpClient.GetAsync($"/vehicles/{encodedRegnr}", cancellationToken);
            _logger.LogInformation("LegacyVehicleDb responded with {StatusCode}", response.StatusCode);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Vehicle {Regnr} not found in legacy database", regnr);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var vehicle = await response.Content.ReadFromJsonAsync<Vehicle>(cancellationToken);
            _logger.LogInformation("Received vehicle {Regnr}: {Make} {Model}", regnr, vehicle?.Make, vehicle?.Model);

            return vehicle;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch vehicle {Regnr} from legacy database at {Url}", regnr, requestUrl);
            throw;
        }
    }
}
