using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Services;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Analytics;

public class CustomerSegmentationService : ICustomerSegmentationService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public CustomerSegmentationService(ApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CustomerRfmSegment>> SegmentCustomersAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveBranchId = _currentUser.IsAdministrator ? branchId : _currentUser.BranchId;

        var customersQuery = _db.Customers.AsNoTracking().Where(c => !c.IsDeleted && c.Status == CustomerStatus.Active);
        if (effectiveBranchId.HasValue)
        {
            customersQuery = customersQuery.Where(c => c.BranchId == effectiveBranchId.Value);
        }

        var customers = await customersQuery
            .Select(c => new { c.CustomerId, c.CustomerCode, c.FullName, c.RegistrationDate })
            .ToListAsync(cancellationToken);

        if (customers.Count == 0)
        {
            return Array.Empty<CustomerRfmSegment>();
        }

        var customerIds = customers.Select(c => c.CustomerId).ToList();

        var invoiceCounts = await _db.Invoices.AsNoTracking()
            .Where(i => customerIds.Contains(i.CustomerId) && !i.IsDeleted)
            .GroupBy(i => i.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var invoiceCountByCustomer = invoiceCounts.ToDictionary(x => x.CustomerId, x => x.Count);

        var paymentStats = await _db.Payments.AsNoTracking()
            .Where(p => customerIds.Contains(p.CustomerId) && p.PaymentStatus == PaymentStatus.Completed)
            .GroupBy(p => p.CustomerId)
            .Select(g => new { CustomerId = g.Key, LastPaymentDate = g.Max(p => p.PaymentDate), TotalPaid = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken);
        var paymentStatsByCustomer = paymentStats.ToDictionary(x => x.CustomerId);

        var now = DateTime.UtcNow;

        var rfmInputs = customers.Select(c =>
        {
            paymentStatsByCustomer.TryGetValue(c.CustomerId, out var payments);
            var lastActivity = payments?.LastPaymentDate ?? c.RegistrationDate;

            return new CustomerRfmInput
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.FullName,
                RecencyDays = (now - lastActivity).TotalDays,
                Frequency = invoiceCountByCustomer.GetValueOrDefault(c.CustomerId, 0),
                Monetary = payments?.TotalPaid is decimal total ? (double)total : 0.0
            };
        }).ToList();

        return RfmSegmenter.Segment(rfmInputs);
    }
}
