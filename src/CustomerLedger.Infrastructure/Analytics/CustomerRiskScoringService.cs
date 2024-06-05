using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Services;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Analytics;

/// <summary>
/// Extracts features per customer, trains a fresh LogisticRegressionModel on a heuristic
/// financial-distress label, and scores every customer in scope. See
/// docs/releases/v7.0.0-Capital.md for the full methodology and its limitations — most
/// importantly, the label is a same-day heuristic (overdue installment or reversed
/// payment present right now), not a real historical "did this customer eventually
/// default" outcome, because this project has no such labeled history to train on.
/// </summary>
public class CustomerRiskScoringService : ICustomerRiskScoringService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public CustomerRiskScoringService(ApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CustomerRiskScore>> ScoreCustomersAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveBranchId = _currentUser.IsAdministrator ? branchId : _currentUser.BranchId;

        var customersQuery = _db.Customers.AsNoTracking().Where(c => !c.IsDeleted && c.Status == CustomerStatus.Active);
        if (effectiveBranchId.HasValue)
        {
            customersQuery = customersQuery.Where(c => c.BranchId == effectiveBranchId.Value);
        }

        var customers = await customersQuery
            .Select(c => new
            {
                c.CustomerId,
                c.CustomerCode,
                c.FullName,
                c.RegistrationDate,
                CreditLimit = c.CustomerAccount != null ? c.CustomerAccount.CreditLimit : 0m,
                CurrentBalance = c.CustomerAccount != null ? c.CustomerAccount.CurrentBalance : 0m
            })
            .ToListAsync(cancellationToken);

        if (customers.Count == 0)
        {
            return Array.Empty<CustomerRiskScore>();
        }

        var customerIds = customers.Select(c => c.CustomerId).ToList();

        var invoiceStats = await _db.Invoices.AsNoTracking()
            .Where(i => customerIds.Contains(i.CustomerId) && !i.IsDeleted)
            .GroupBy(i => i.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalCount = g.Count(),
                UnpaidCount = g.Count(i => i.PaymentStatus != PaymentStatus.Paid),
                AverageAmount = g.Average(i => i.TotalAmount),
                TotalOutstanding = g.Sum(i => i.OutstandingAmount)
            })
            .ToListAsync(cancellationToken);
        var invoiceStatsByCustomer = invoiceStats.ToDictionary(x => x.CustomerId);

        var overdueCustomerIds = (await _db.InstallmentSchedules.AsNoTracking()
            .Where(s => s.Status == InstallmentStatus.Overdue && customerIds.Contains(s.InstallmentPlan.Invoice.CustomerId))
            .Select(s => s.InstallmentPlan.Invoice.CustomerId)
            .Distinct()
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var reversedPaymentCustomerIds = (await _db.Payments.AsNoTracking()
            .Where(p => p.PaymentStatus == PaymentStatus.Reversed && customerIds.Contains(p.CustomerId))
            .Select(p => p.CustomerId)
            .Distinct()
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var now = DateTime.UtcNow;
        var featuresByCustomer = customers.Select(c =>
        {
            invoiceStatsByCustomer.TryGetValue(c.CustomerId, out var stats);

            var creditUtilization = c.CreditLimit > 0 ? (double)(c.CurrentBalance / c.CreditLimit) : 0.0;
            var unpaidRatio = stats is not null && stats.TotalCount > 0 ? (double)stats.UnpaidCount / stats.TotalCount : 0.0;
            var avgInvoiceAmount = stats?.AverageAmount is decimal avg ? (double)avg : 0.0;
            var totalOutstanding = stats?.TotalOutstanding is decimal outstanding ? (double)outstanding : 0.0;
            var ageDays = (now - c.RegistrationDate).TotalDays;

            var isDistressed = overdueCustomerIds.Contains(c.CustomerId) || reversedPaymentCustomerIds.Contains(c.CustomerId);

            return new CustomerRiskFeatures
            {
                CustomerId = c.CustomerId,
                CreditUtilization = creditUtilization,
                UnpaidInvoiceRatio = unpaidRatio,
                AverageInvoiceAmount = avgInvoiceAmount,
                TotalOutstanding = totalOutstanding,
                CustomerAgeDays = ageDays,
                Label = isDistressed ? 1.0 : 0.0
            };
        }).ToList();

        var model = new LogisticRegressionModel();

        // With fewer than 2 examples of each class, gradient descent has nothing to
        // discriminate between — fall back to a flat 0 probability for everyone rather
        // than training on a degenerate single-class dataset (which would just produce an
        // arbitrarily confident but meaningless model).
        var canTrain = featuresByCustomer.Select(f => f.Label!.Value).Distinct().Count() > 1;

        if (canTrain)
        {
            model.Train(
                featuresByCustomer.Select(f => f.ToVector()).ToList(),
                featuresByCustomer.Select(f => f.Label!.Value).ToList());
        }

        var customersById = customers.ToDictionary(c => c.CustomerId);

        return featuresByCustomer
            .Select(f => new CustomerRiskScore
            {
                CustomerId = f.CustomerId,
                CustomerName = customersById[f.CustomerId].FullName,
                CustomerCode = customersById[f.CustomerId].CustomerCode,
                RiskProbability = canTrain ? model.PredictProbability(f.ToVector()) : 0.0,
                Features = f
            })
            .OrderByDescending(s => s.RiskProbability)
            .ToList();
    }
}
