namespace CustomerLedger.Domain.Enums;

/// <summary>
/// Lifecycle status of a customer record. Persisted as a string (see ApplicationDbContext
/// configuration) so the database remains self-describing without relying on enum ordinals.
/// </summary>
public enum CustomerStatus
{
    Active,
    Inactive,
    Blacklisted
}
