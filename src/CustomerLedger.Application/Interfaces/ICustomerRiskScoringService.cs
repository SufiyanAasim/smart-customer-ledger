using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Trains a logistic regression model on the current branch's (or, for an Administrator,
/// the whole organization's) customer data and scores every active customer's payment risk.
/// Training happens fresh on every call rather than being persisted — see
/// docs/releases/v7.0.0-Capital.md for why that trade-off was made at this project's scale.
/// </summary>
public interface ICustomerRiskScoringService
{
    Task<IReadOnlyList<CustomerRiskScore>> ScoreCustomersAsync(int? branchId, CancellationToken cancellationToken = default);
}
