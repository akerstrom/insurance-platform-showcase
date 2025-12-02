namespace InsuranceService.Models;

public record Insurance(
    Guid Id,
    string Pid,
    InsuranceType Type,
    string Status,
    decimal Premium,
    string? Regnr = null);
