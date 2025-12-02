namespace InsuranceMainframe.Models;

public record Policy(
    Guid Id,
    string Pid,
    PolicyType Type,
    string Status,
    decimal Premium,
    string? Regnr = null);
