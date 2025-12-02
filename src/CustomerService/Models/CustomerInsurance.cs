namespace CustomerService.Models;

public record CustomerInsurance(
    Guid Id,
    string Pid,
    InsuranceType Type,
    string Status,
    decimal Premium,
    Vehicle? Vehicle = null);
