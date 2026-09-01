using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Transaction : BaseEntity, IAuditableEntity
{
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public required decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateOnly Date { get; set; }
    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
