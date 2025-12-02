using VehicleDatabase.Models;

namespace VehicleDatabase.Data;

public static class VehicleStore
{
    private static readonly Dictionary<string, VehicleRecord> Vehicles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ABC123"] = new VehicleRecord("WBA3A5C55DF123456", "ABC123", "BMW", "320i", 2019),
        ["XYZ789"] = new VehicleRecord("YV1CZ91H841234567", "XYZ789", "Volvo", "XC60", 2021),
        ["DEF456"] = new VehicleRecord("WVWZZZ3CZWE123456", "DEF456", "Volkswagen", "Golf", 2020)
    };

    public static VehicleRecord? GetByRegnr(string regnr)
    {
        return Vehicles.TryGetValue(regnr, out var vehicle) ? vehicle : null;
    }
}
