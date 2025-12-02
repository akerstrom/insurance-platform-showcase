using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CustomerService.Models;

namespace CustomerService.Clients;

public class InsuranceServiceClient : IInsuranceServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InsuranceServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public InsuranceServiceClient(HttpClient httpClient, ILogger<InsuranceServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<IReadOnlyList<InsuranceInfo>> GetInsurancesAsync(string pid, CancellationToken cancellationToken = default)
    {
        try
        {
            var encodedPid = Uri.EscapeDataString(pid);
            var response = await _httpClient.GetAsync($"/insurances/{encodedPid}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Array.Empty<InsuranceInfo>();
            }

            response.EnsureSuccessStatusCode();

            var insurances = await response.Content.ReadFromJsonAsync<List<InsuranceDto>>(_jsonOptions, cancellationToken);

            return insurances?.Select(ToInsuranceInfo).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch insurances for PID {Pid} from InsuranceService", pid);
            throw;
        }
    }

    private static InsuranceInfo ToInsuranceInfo(InsuranceDto dto)
    {
        return new InsuranceInfo(
            dto.Id,
            dto.Pid,
            dto.Type,
            dto.Status,
            dto.Premium,
            dto.Regnr);
    }

    private record InsuranceDto(
        Guid Id,
        string Pid,
        InsuranceType Type,
        string Status,
        decimal Premium,
        string? Regnr = null);
}
