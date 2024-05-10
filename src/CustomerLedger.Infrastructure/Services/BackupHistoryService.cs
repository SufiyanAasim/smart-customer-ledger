using CustomerLedger.Application.Interfaces;
using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Services;

public class BackupHistoryService : IBackupHistoryService
{
    private readonly ApplicationDbContext _db;

    public BackupHistoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<BackupHistory>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.BackupHistories.AsNoTracking().OrderByDescending(b => b.StartedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BackupHistory>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public Task<BackupHistory?> GetByIdAsync(long backupHistoryId, CancellationToken cancellationToken = default) =>
        _db.BackupHistories.FirstOrDefaultAsync(b => b.BackupHistoryId == backupHistoryId, cancellationToken);
}
