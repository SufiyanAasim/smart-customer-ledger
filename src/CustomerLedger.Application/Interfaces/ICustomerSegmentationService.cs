using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Interfaces;

public interface ICustomerSegmentationService
{
    Task<IReadOnlyList<CustomerRfmSegment>> SegmentCustomersAsync(int? branchId, CancellationToken cancellationToken = default);
}
