using System.Net;
using CustomerService.Models;

namespace CustomerService.Clients;

public class VehicleServiceClient : IVehicleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleServiceClient> _logger;

    public VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Vehicle?> GetVehicleAsync(string regnr, CancellationToken cancellationToken = default)
    {
        var encodedRegnr = Uri.EscapeDataString(regnr);
        var requestUrl = $"{_httpClient.BaseAddress}/vehicles/{encodedRegnr}";
        _logger.LogInformation("Calling VehicleService: GET {Url}", requestUrl);

        try
        {
            var response = await _httpClient.GetAsync($"/vehicles/{encodedRegnr}", cancellationToken);
            _logger.LogInformation("VehicleService responded with {StatusCode}", response.StatusCode);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Vehicle {Regnr} not found", regnr);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var vehicle = await response.Content.ReadFromJsonAsync<Vehicle>(cancellationToken);
            _logger.LogInformation("Received vehicle {Regnr}: {Make} {Model}", regnr, vehicle?.Make, vehicle?.Model);

            return vehicle;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch vehicle {Regnr} from VehicleService at {Url}", regnr, requestUrl);
            throw;
        }
    }
}
