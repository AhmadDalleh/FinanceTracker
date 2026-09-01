using Domain.Common;

namespace Domain.Entities;

public class Category : BaseEntity, IAuditableEntity
{
    public required string UserId { get; set; }
    public required string Name { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
