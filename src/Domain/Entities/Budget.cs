using Domain.Common;

namespace Domain.Entities;

public class Budget : BaseEntity, IAuditableEntity
{
    public required string UserId { get; set; }
    public Guid CategoryId { get; set; }

    /// <summary>Always the first day of the budgeted month.</summary>
    public DateOnly Month { get; set; }

    public required decimal Amount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
