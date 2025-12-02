using CustomerService.Models;

namespace CustomerService.Clients;

public interface IVehicleServiceClient
{
    Task<Vehicle?> GetVehicleAsync(string regnr, CancellationToken cancellationToken = default);
}
