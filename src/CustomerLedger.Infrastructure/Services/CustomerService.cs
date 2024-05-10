using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICustomerAccountService _accountService;
    private readonly IAuditLogService _auditLog;

    public CustomerService(
        ApplicationDbContext db,
        ICurrentUserContext currentUser,
        ICustomerAccountService accountService,
        IAuditLogService auditLog)
    {
        _db = db;
        _currentUser = currentUser;
        _accountService = accountService;
        _auditLog = auditLog;
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(int? branchId, string? search, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.Customers.AsNoTracking().Where(c => !c.IsDeleted);

        // Branch isolation: non-administrators only ever see their own branch, regardless
        // of what a caller passes in — the controller's requested branchId is only honored
        // for administrators browsing a specific branch.
        if (!_currentUser.IsAdministrator)
        {
            query = query.Where(c => c.BranchId == _currentUser.BranchId);
        }
        else if (branchId.HasValue)
        {
            query = query.Where(c => c.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.FullName.Contains(search) ||
                c.CustomerCode.Contains(search) ||
                c.PhoneNumber.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CustomerStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(c => c.Status == parsedStatus);
        }

        query = query.OrderByDescending(c => c.RegistrationDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(c => c.Branch)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Customer>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers
            .Include(c => c.Branch)
            .Include(c => c.CustomerAccount)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsDeleted, cancellationToken);

        if (customer is not null && !_currentUser.CanAccessBranch(customer.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this customer's branch.");
        }

        return customer;
    }

    public async Task<Customer> CreateAsync(Customer customer, decimal initialCreditLimit, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.CanAccessBranch(customer.BranchId))
        {
            throw new BranchAccessDeniedException("You cannot register a customer for a different branch.");
        }

        var codeExists = await _db.Customers.AnyAsync(c => c.CustomerCode == customer.CustomerCode, cancellationToken);
        if (codeExists)
        {
            throw new BusinessRuleException($"Customer code '{customer.CustomerCode}' is already in use.");
        }

        customer.RegistrationDate = DateTime.UtcNow;
        customer.CreatedAtUtc = DateTime.UtcNow;
        customer.Status = CustomerStatus.Active;
        customer.IsDeleted = false;

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);

        // Every customer gets exactly one financial account at registration time.
        await _accountService.CreateForCustomerAsync(customer.CustomerId, initialCreditLimit, cancellationToken);

        await _auditLog.WriteAsync(new AuditLog
        {
            UserId = _currentUser.UserId,
            BranchId = customer.BranchId,
            TableName = "Customers",
            RecordId = customer.CustomerId.ToString(),
            ActionType = "Create",
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return customer;
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Customers.FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId && !c.IsDeleted, cancellationToken)
            ?? throw new BusinessRuleException("Customer not found.");

        if (!_currentUser.CanAccessBranch(existing.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this customer's branch.");
        }

        existing.FullName = customer.FullName;
        existing.Email = customer.Email;
        existing.PhoneNumber = customer.PhoneNumber;
        existing.CNIC = customer.CNIC;
        existing.Address = customer.Address;
        existing.City = customer.City;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(new AuditLog
        {
            UserId = _currentUser.UserId,
            BranchId = existing.BranchId,
            TableName = "Customers",
            RecordId = existing.CustomerId.ToString(),
            ActionType = "Update",
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task DeactivateAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsDeleted, cancellationToken)
            ?? throw new BusinessRuleException("Customer not found.");

        if (!_currentUser.CanAccessBranch(customer.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this customer's branch.");
        }

        customer.Status = CustomerStatus.Inactive;
        customer.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(new AuditLog
        {
            UserId = _currentUser.UserId,
            BranchId = customer.BranchId,
            TableName = "Customers",
            RecordId = customer.CustomerId.ToString(),
            ActionType = "Deactivate",
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);
    }
}
