using InsuranceService.Models;

namespace InsuranceService.Clients;

public interface IInsuranceMainframeClient
{
    Task<IReadOnlyList<Insurance>> GetInsurancesAsync(string pid, CancellationToken cancellationToken = default);
}
