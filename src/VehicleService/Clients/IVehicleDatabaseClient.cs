using VehicleService.Models;

namespace VehicleService.Clients;

public interface IVehicleDatabaseClient
{
    Task<Vehicle?> GetVehicleAsync(string regnr, CancellationToken cancellationToken = default);
}
