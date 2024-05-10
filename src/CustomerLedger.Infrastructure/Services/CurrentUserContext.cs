using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;

namespace CustomerLedger.Infrastructure.Services;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    public int? BranchId
    {
        get
        {
            var value = User?.FindFirst(ApplicationClaimsPrincipalFactory.BranchIdClaimType)?.Value;
            return int.TryParse(value, out var branchId) ? branchId : null;
        }
    }

    public bool IsAdministrator => User?.IsInRole(Roles.Administrator) ?? false;
    public bool IsBranchManager => User?.IsInRole(Roles.BranchManager) ?? false;
    public bool IsStaff => User?.IsInRole(Roles.Staff) ?? false;

    public bool CanAccessBranch(int branchId)
    {
        if (IsAdministrator)
        {
            return true;
        }

        return BranchId.HasValue && BranchId.Value == branchId;
    }
}
