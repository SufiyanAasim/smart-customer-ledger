using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;

namespace CustomerLedger.IntegrationTests.Fakes;

/// <summary>Test double for ICurrentUserContext — lets service-level tests exercise branch-isolation logic without a real signed-in HTTP request.</summary>
public class FakeCurrentUserContext : ICurrentUserContext
{
    public string? UserId { get; set; } = "test-user-id";
    public int? BranchId { get; set; }
    public bool IsAdministrator { get; set; }
    public bool IsBranchManager { get; set; }
    public bool IsStaff { get; set; } = true;

    public bool CanAccessBranch(int branchId) => IsAdministrator || BranchId == branchId;

    public static FakeCurrentUserContext ForBranch(int branchId) => new() { BranchId = branchId };
    public static FakeCurrentUserContext ForAdministrator() => new() { IsAdministrator = true, IsStaff = false };
}
