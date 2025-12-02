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
        try
        {
            var encodedRegnr = Uri.EscapeDataString(regnr);
            var response = await _httpClient.GetAsync($"/vehicles/{encodedRegnr}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Vehicle>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch vehicle {Regnr} from legacy database", regnr);
            throw;
        }
    }
}
