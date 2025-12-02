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
        try
        {
            var encodedPid = Uri.EscapeDataString(pid);
            var response = await _httpClient.GetAsync($"/policies/{encodedPid}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Array.Empty<Insurance>();
            }

            response.EnsureSuccessStatusCode();

            var legacyPolicies = await response.Content.ReadFromJsonAsync<List<LegacyPolicy>>(_jsonOptions, cancellationToken);

            return legacyPolicies?.Select(TranslateToInsurance).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch policies for PID {Pid} from legacy mainframe", pid);
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
