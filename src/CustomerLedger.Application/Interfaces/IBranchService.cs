using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

public interface IBranchService
{
    Task<PagedResult<Branch>> GetPagedAsync(string? search, bool includeInactive, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Branch?> GetByIdAsync(int branchId, CancellationToken cancellationToken = default);
    Task<Branch> CreateAsync(Branch branch, CancellationToken cancellationToken = default);
    Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int branchId, CancellationToken cancellationToken = default);
    Task ReactivateAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Branch>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
