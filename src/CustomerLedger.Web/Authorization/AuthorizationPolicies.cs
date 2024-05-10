namespace CustomerLedger.Web.Authorization;

/// <summary>
/// Named authorization policies registered in Program.cs. Controllers reference these
/// instead of role strings directly so the mapping from "policy" to "which roles" lives
/// in exactly one place.
/// </summary>
public static class AuthorizationPolicies
{
    public const string AdministratorOnly = "AdministratorOnly";
    public const string ManagerOrAbove = "ManagerOrAbove";
    public const string AnyStaffRole = "AnyStaffRole";
}
