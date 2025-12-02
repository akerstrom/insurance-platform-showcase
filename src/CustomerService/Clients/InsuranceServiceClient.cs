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
        var encodedPid = Uri.EscapeDataString(pid);
        var requestUrl = $"{_httpClient.BaseAddress}/insurances/{encodedPid}";
        _logger.LogInformation("Calling InsuranceService: GET {Url}", requestUrl);

        try
        {
            var response = await _httpClient.GetAsync($"/insurances/{encodedPid}", cancellationToken);
            _logger.LogInformation("InsuranceService responded with {StatusCode}", response.StatusCode);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No insurances found for PID {Pid}", pid);
                return Array.Empty<InsuranceInfo>();
            }

            response.EnsureSuccessStatusCode();

            var insurances = await response.Content.ReadFromJsonAsync<List<InsuranceDto>>(_jsonOptions, cancellationToken);
            _logger.LogInformation("Received {Count} insurances for PID {Pid}", insurances?.Count ?? 0, pid);

            return insurances?.Select(ToInsuranceInfo).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch insurances for PID {Pid} from InsuranceService at {Url}", pid, requestUrl);
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
