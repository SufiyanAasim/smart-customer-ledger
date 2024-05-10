using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AuditLog>> GetPagedAsync(int? branchId, string? tableName, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<AuditLog> WriteAsync(AuditLog entry, CancellationToken cancellationToken = default);
    Task SetReviewStatusAsync(long auditLogId, Domain.Enums.AuditReviewStatus status, string? adminNote, CancellationToken cancellationToken = default);
    Task ArchiveAsync(long auditLogId, CancellationToken cancellationToken = default);
}
