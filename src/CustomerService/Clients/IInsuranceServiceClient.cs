using CustomerService.Models;

namespace CustomerService.Clients;

public interface IInsuranceServiceClient
{
    Task<IReadOnlyList<InsuranceInfo>> GetInsurancesAsync(string pid, CancellationToken cancellationToken = default);
}

public record InsuranceInfo(
    Guid Id,
    string Pid,
    InsuranceType Type,
    string Status,
    decimal Premium,
    string? Regnr = null);
