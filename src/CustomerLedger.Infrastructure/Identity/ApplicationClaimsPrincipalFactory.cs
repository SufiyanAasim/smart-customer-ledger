using System.Security.Claims;
using CustomerLedger.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CustomerLedger.Infrastructure.Identity;

/// <summary>
/// Adds a "BranchId" claim (and EmployeeCode) at sign-in time so branch-scoped authorization
/// checks (ICurrentUserContext) never need a database round-trip per request. A user whose
/// BranchId changes must sign in again to pick up the new value — acceptable for this release.
/// </summary>
public class ApplicationClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public const string BranchIdClaimType = "BranchId";
    public const string EmployeeCodeClaimType = "EmployeeCode";

    public ApplicationClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (user.BranchId.HasValue)
        {
            identity.AddClaim(new Claim(BranchIdClaimType, user.BranchId.Value.ToString()));
        }

        identity.AddClaim(new Claim(EmployeeCodeClaimType, user.EmployeeCode));

        return identity;
    }
}
