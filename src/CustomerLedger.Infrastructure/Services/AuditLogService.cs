using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _db;

    public AuditLogService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(int? branchId, string? tableName, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs.AsNoTracking().Where(a => !a.IsArchived);

        if (branchId.HasValue)
        {
            query = query.Where(a => a.BranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tableName))
        {
            query = query.Where(a => a.TableName == tableName);
        }

        query = query.OrderByDescending(a => a.CreatedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AuditLog> WriteAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        entry.CreatedAtUtc = DateTime.UtcNow;
        entry.ReviewStatus = AuditReviewStatus.Unreviewed;
        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync(cancellationToken);
        return entry;
    }

    public async Task SetReviewStatusAsync(long auditLogId, AuditReviewStatus status, string? adminNote, CancellationToken cancellationToken = default)
    {
        var entry = await _db.AuditLogs.FirstOrDefaultAsync(a => a.AuditLogId == auditLogId, cancellationToken);
        if (entry is null)
        {
            return;
        }

        entry.ReviewStatus = status;
        entry.AdminNote = adminNote;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ArchiveAsync(long auditLogId, CancellationToken cancellationToken = default)
    {
        var entry = await _db.AuditLogs.FirstOrDefaultAsync(a => a.AuditLogId == auditLogId, cancellationToken);
        if (entry is null)
        {
            return;
        }

        entry.IsArchived = true;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
