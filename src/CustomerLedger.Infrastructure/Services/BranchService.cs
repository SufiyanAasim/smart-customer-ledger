using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly ApplicationDbContext _db;

    public BranchService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<Branch>> GetPagedAsync(string? search, bool includeInactive, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Branches.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b =>
                b.Name.Contains(search) ||
                b.BranchCode.Contains(search) ||
                b.City.Contains(search));
        }

        query = query.OrderBy(b => b.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Branch>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public Task<Branch?> GetByIdAsync(int branchId, CancellationToken cancellationToken = default) =>
        _db.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId, cancellationToken);

    public async Task<Branch> CreateAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        var codeExists = await _db.Branches.AnyAsync(b => b.BranchCode == branch.BranchCode, cancellationToken);
        if (codeExists)
        {
            throw new BusinessRuleException($"Branch code '{branch.BranchCode}' is already in use.");
        }

        branch.CreatedAtUtc = DateTime.UtcNow;
        branch.IsActive = true;

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(cancellationToken);
        return branch;
    }

    public async Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Branches.FirstOrDefaultAsync(b => b.BranchId == branch.BranchId, cancellationToken)
            ?? throw new BusinessRuleException("Branch not found.");

        var codeInUse = await _db.Branches.AnyAsync(
            b => b.BranchCode == branch.BranchCode && b.BranchId != branch.BranchId, cancellationToken);
        if (codeInUse)
        {
            throw new BusinessRuleException($"Branch code '{branch.BranchCode}' is already in use.");
        }

        existing.BranchCode = branch.BranchCode;
        existing.Name = branch.Name;
        existing.Email = branch.Email;
        existing.PhoneNumber = branch.PhoneNumber;
        existing.Address = branch.Address;
        existing.City = branch.City;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId, cancellationToken)
            ?? throw new BusinessRuleException("Branch not found.");

        branch.IsActive = false;
        branch.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReactivateAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId, cancellationToken)
            ?? throw new BusinessRuleException("Branch not found.");

        branch.IsActive = true;
        branch.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Branch>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Branches
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }
}
