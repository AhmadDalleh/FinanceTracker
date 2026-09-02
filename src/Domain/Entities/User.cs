using Domain.Common;

namespace Domain.Entities;

public class User : BaseEntity, IAuditableEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
