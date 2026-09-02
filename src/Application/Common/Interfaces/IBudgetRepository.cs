using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsForCategoryAndMonthAsync(Guid categoryId, DateOnly month, string userId, CancellationToken cancellationToken);
    Task AddAsync(Budget budget, CancellationToken cancellationToken);
    void Update(Budget budget);
    void Remove(Budget budget);
}
