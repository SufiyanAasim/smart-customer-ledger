using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

public interface ICustomerInteractionService
{
    Task<PagedResult<CustomerInteraction>> GetPagedAsync(int? branchId, int? customerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CustomerInteraction?> GetByIdAsync(long customerInteractionId, CancellationToken cancellationToken = default);
    Task<CustomerInteraction> CreateAsync(CustomerInteraction interaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerInteraction interaction, CancellationToken cancellationToken = default);
}
