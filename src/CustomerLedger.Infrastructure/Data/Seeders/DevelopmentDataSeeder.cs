using CustomerLedger.Domain.Entities;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Data.Seeders;

/// <summary>
/// Adds a single demonstration branch for local development so the admin account seeded by
/// AdminUserSeeder has somewhere to assign staff/customers during a fresh clone. Intended to
/// be invoked only in the Development environment — see Program.cs.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Branches.AnyAsync())
        {
            return;
        }

        dbContext.Branches.Add(new Branch
        {
            BranchCode = "MAIN",
            Name = "Main Branch",
            Email = "main.branch@customerledger.local",
            PhoneNumber = "021-0000000",
            Address = "Head Office",
            City = "Karachi",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
