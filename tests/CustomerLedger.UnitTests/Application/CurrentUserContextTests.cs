using System.Security.Claims;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Infrastructure.Identity;
using CustomerLedger.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CustomerLedger.UnitTests.Application;

public class CurrentUserContextTests
{
    private static CurrentUserContext CreateContext(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        var accessor = new HttpContextAccessorStub(httpContext);
        return new CurrentUserContext(accessor);
    }

    [Fact]
    public void CanAccessBranch_Administrator_CanAccessAnyBranch()
    {
        var context = CreateContext(new Claim(ClaimTypes.Role, Roles.Administrator));

        Assert.True(context.CanAccessBranch(1));
        Assert.True(context.CanAccessBranch(999));
    }

    [Fact]
    public void CanAccessBranch_BranchManager_CanOnlyAccessOwnBranch()
    {
        var context = CreateContext(
            new Claim(ClaimTypes.Role, Roles.BranchManager),
            new Claim(ApplicationClaimsPrincipalFactory.BranchIdClaimType, "5"));

        Assert.True(context.CanAccessBranch(5));
        Assert.False(context.CanAccessBranch(6));
    }

    [Fact]
    public void CanAccessBranch_NoBranchClaim_CannotAccessAnyBranch()
    {
        var context = CreateContext(new Claim(ClaimTypes.Role, Roles.Staff));

        Assert.False(context.CanAccessBranch(1));
    }

    private class HttpContextAccessorStub : IHttpContextAccessor
    {
        public HttpContextAccessorStub(HttpContext context) => HttpContext = context;
        public HttpContext? HttpContext { get; set; }
    }
}
