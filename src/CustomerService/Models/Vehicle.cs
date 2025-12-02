namespace CustomerService.Models;

public record Vehicle(
    string Vin,
    string Regnr,
    string Make,
    string Model,
    int Year);
