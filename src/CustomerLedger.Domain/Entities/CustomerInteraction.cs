using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

public class CustomerInteraction
{
    public long CustomerInteractionId { get; set; }
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public InteractionType InteractionType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime InteractionDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public InteractionStatus Status { get; set; } = InteractionStatus.Open;
    public string RecordedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Customer Customer { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ApplicationUser RecordedByUser { get; set; } = null!;
}
