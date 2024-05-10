namespace CustomerLedger.Domain.Constants;

/// <summary>
/// Canonical role names seeded into AspNetRoles. Referenced by [Authorize(Roles = ...)]
/// attributes so role names never appear as magic strings scattered across the codebase.
/// </summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string BranchManager = "BranchManager";
    public const string Staff = "Staff";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        Administrator,
        BranchManager,
        Staff
    };
}
