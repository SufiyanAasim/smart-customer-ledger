namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Resolves the authenticated user's identity and branch assignment from claims. Every
/// branch-scoped service depends on this rather than trusting a branch id supplied by the
/// caller/URL — see section 5 (Authorization) of the project specification: "Every service
/// must verify branch ownership."
/// </summary>
public interface ICurrentUserContext
{
    string? UserId { get; }
    int? BranchId { get; }
    bool IsAdministrator { get; }
    bool IsBranchManager { get; }
    bool IsStaff { get; }

    /// <summary>
    /// True if the current user may operate on data belonging to <paramref name="branchId"/>:
    /// Administrators may access any branch, everyone else only their own.
    /// </summary>
    bool CanAccessBranch(int branchId);
}
