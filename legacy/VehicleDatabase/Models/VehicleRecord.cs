namespace VehicleDatabase.Models;

public record VehicleRecord(
    string Vin,
    string Regnr,
    string Make,
    string Model,
    int Year);
