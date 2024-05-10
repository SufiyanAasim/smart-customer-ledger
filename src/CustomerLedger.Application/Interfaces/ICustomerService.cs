using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<Customer>> GetPagedAsync(int? branchId, string? search, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Customer> CreateAsync(Customer customer, decimal initialCreditLimit, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int customerId, CancellationToken cancellationToken = default);
}
