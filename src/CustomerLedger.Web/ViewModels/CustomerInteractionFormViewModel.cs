using System.ComponentModel.DataAnnotations;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Web.ViewModels;

public class CustomerInteractionFormViewModel
{
    public long CustomerInteractionId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [Display(Name = "Interaction Type")]
    public InteractionType InteractionType { get; set; }

    [Required, StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required, DataType(DataType.DateTime)]
    [Display(Name = "Interaction Date")]
    public DateTime InteractionDate { get; set; } = DateTime.UtcNow;

    [DataType(DataType.Date)]
    [Display(Name = "Follow-Up Date")]
    public DateTime? FollowUpDate { get; set; }

    [Required]
    public InteractionStatus Status { get; set; } = InteractionStatus.Open;

    public Customer? Customer { get; set; }
}
