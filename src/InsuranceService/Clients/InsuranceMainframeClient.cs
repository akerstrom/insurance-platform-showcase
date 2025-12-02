using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsuranceService.Models;

namespace InsuranceService.Clients;

public class InsuranceMainframeClient : IInsuranceMainframeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InsuranceMainframeClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public InsuranceMainframeClient(HttpClient httpClient, ILogger<InsuranceMainframeClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<IReadOnlyList<Insurance>> GetInsurancesAsync(string pid, CancellationToken cancellationToken = default)
    {
        var encodedPid = Uri.EscapeDataString(pid);
        var requestUrl = $"{_httpClient.BaseAddress}/policies/{encodedPid}";
        _logger.LogInformation("Calling LegacyMainframe: GET {Url}", requestUrl);

        try
        {
            var response = await _httpClient.GetAsync($"/policies/{encodedPid}", cancellationToken);
            _logger.LogInformation("LegacyMainframe responded with {StatusCode}", response.StatusCode);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("No policies found for PID {Pid}", pid);
                return Array.Empty<Insurance>();
            }

            response.EnsureSuccessStatusCode();

            var legacyPolicies = await response.Content.ReadFromJsonAsync<List<LegacyPolicy>>(_jsonOptions, cancellationToken);
            _logger.LogInformation("Received {Count} policies from mainframe for PID {Pid}", legacyPolicies?.Count ?? 0, pid);

            return legacyPolicies?.Select(TranslateToInsurance).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch policies for PID {Pid} from legacy mainframe at {Url}", pid, requestUrl);
            throw;
        }
    }

    private static Insurance TranslateToInsurance(LegacyPolicy policy)
    {
        return new Insurance(
            policy.Id,
            policy.Pid,
            (InsuranceType)policy.Type,
            policy.Status,
            policy.Premium,
            policy.Regnr);
    }

    private record LegacyPolicy(
        Guid Id,
        string Pid,
        LegacyPolicyType Type,
        string Status,
        decimal Premium,
        string? Regnr = null);

    private enum LegacyPolicyType
    {
        Car,
        Pet,
        Health
    }
}
