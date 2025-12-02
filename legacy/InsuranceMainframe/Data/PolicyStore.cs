using InsuranceMainframe.Models;

namespace InsuranceMainframe.Data;

public static class PolicyStore
{
    private static readonly List<Policy> Policies = new()
    {
        // Customer 1: 199001011234 - has all three types
        new Policy(Guid.Parse("11111111-1111-1111-1111-111111111111"), "199001011234", PolicyType.Car, "Active", 30m, "ABC123"),
        new Policy(Guid.Parse("22222222-2222-2222-2222-222222222222"), "199001011234", PolicyType.Pet, "Active", 10m),
        new Policy(Guid.Parse("33333333-3333-3333-3333-333333333333"), "199001011234", PolicyType.Health, "Active", 20m),

        // Customer 2: 198505152345 - has only car
        new Policy(Guid.Parse("44444444-4444-4444-4444-444444444444"), "198505152345", PolicyType.Car, "Active", 30m, "XYZ789"),

        // Customer 3: 197212123456 - has pet and health
        new Policy(Guid.Parse("55555555-5555-5555-5555-555555555555"), "197212123456", PolicyType.Pet, "Pending", 10m),
        new Policy(Guid.Parse("66666666-6666-6666-6666-666666666666"), "197212123456", PolicyType.Health, "Active", 20m)
    };

    public static IReadOnlyList<Policy> GetByPid(string pid)
    {
        return Policies.Where(p => p.Pid == pid).ToList();
    }
}
