using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

/// <summary>
/// Append-oriented audit trail. Normal users never edit AuditLog rows; Administrators may
/// only add a review note or archive a row (IsArchived), never alter OldValuesJson/NewValuesJson.
/// Sensitive fields (password hashes, security stamps, tokens) must be filtered by the writer
/// before serialization — see AuditLogWriter in Application/Services.
/// </summary>
public class AuditLog
{
    public long AuditLogId { get; set; }
    public string? UserId { get; set; }
    public int? BranchId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public AuditReviewStatus ReviewStatus { get; set; } = AuditReviewStatus.Unreviewed;
    public string? AdminNote { get; set; }
    public bool IsArchived { get; set; }
}
