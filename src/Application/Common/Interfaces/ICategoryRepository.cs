using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(Guid id, string userId, CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
}
