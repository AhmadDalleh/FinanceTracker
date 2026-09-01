using Domain.Common;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Account : BaseEntity, IAuditableEntity
{
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public AccountType Type { get; set; }
    public required Money Balance { get; set; }
    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
