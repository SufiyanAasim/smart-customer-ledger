using CustomerLedger.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CustomerLedger.Infrastructure.Data.Seeders;

/// <summary>
/// Ensures the three canonical roles exist. Safe to run on every startup — each role is
/// created only if missing, so re-running never duplicates or resets role data.
/// </summary>
public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        foreach (var roleName in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                logger.LogError(
                    "Failed to seed role {RoleName}: {Errors}",
                    roleName,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
