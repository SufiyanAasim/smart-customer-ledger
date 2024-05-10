using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class CustomerInteractionService : ICustomerInteractionService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public CustomerInteractionService(ApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CustomerInteraction>> GetPagedAsync(int? branchId, int? customerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.CustomerInteractions.AsNoTracking().AsQueryable();

        if (!_currentUser.IsAdministrator)
        {
            query = query.Where(ci => ci.BranchId == _currentUser.BranchId);
        }
        else if (branchId.HasValue)
        {
            query = query.Where(ci => ci.BranchId == branchId.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(ci => ci.CustomerId == customerId.Value);
        }

        query = query.OrderByDescending(ci => ci.InteractionDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(ci => ci.Customer)
            .Include(ci => ci.RecordedByUser)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerInteraction>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CustomerInteraction?> GetByIdAsync(long customerInteractionId, CancellationToken cancellationToken = default)
    {
        var interaction = await _db.CustomerInteractions
            .Include(ci => ci.Customer)
            .Include(ci => ci.RecordedByUser)
            .FirstOrDefaultAsync(ci => ci.CustomerInteractionId == customerInteractionId, cancellationToken);

        if (interaction is not null && !_currentUser.CanAccessBranch(interaction.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this interaction's branch.");
        }

        return interaction;
    }

    public async Task<CustomerInteraction> CreateAsync(CustomerInteraction interaction, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(
            c => c.CustomerId == interaction.CustomerId && !c.IsDeleted, cancellationToken)
            ?? throw new BusinessRuleException("Customer not found.");

        if (!_currentUser.CanAccessBranch(customer.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this customer's branch.");
        }

        if (interaction.FollowUpDate.HasValue && interaction.FollowUpDate < interaction.InteractionDate)
        {
            throw new BusinessRuleException("Follow-up date cannot be earlier than the interaction date.");
        }

        interaction.BranchId = customer.BranchId;
        interaction.RecordedByUserId = _currentUser.UserId ?? throw new BusinessRuleException("Authenticated user required.");
        interaction.CreatedAtUtc = DateTime.UtcNow;

        _db.CustomerInteractions.Add(interaction);
        await _db.SaveChangesAsync(cancellationToken);
        return interaction;
    }

    public async Task UpdateAsync(CustomerInteraction interaction, CancellationToken cancellationToken = default)
    {
        var existing = await _db.CustomerInteractions.FirstOrDefaultAsync(
            ci => ci.CustomerInteractionId == interaction.CustomerInteractionId, cancellationToken)
            ?? throw new BusinessRuleException("Interaction not found.");

        if (!_currentUser.CanAccessBranch(existing.BranchId))
        {
            throw new BranchAccessDeniedException("You do not have access to this interaction's branch.");
        }

        if (interaction.FollowUpDate.HasValue && interaction.FollowUpDate < interaction.InteractionDate)
        {
            throw new BusinessRuleException("Follow-up date cannot be earlier than the interaction date.");
        }

        existing.InteractionType = interaction.InteractionType;
        existing.Subject = interaction.Subject;
        existing.Description = interaction.Description;
        existing.InteractionDate = interaction.InteractionDate;
        existing.FollowUpDate = interaction.FollowUpDate;
        existing.Status = interaction.Status;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
