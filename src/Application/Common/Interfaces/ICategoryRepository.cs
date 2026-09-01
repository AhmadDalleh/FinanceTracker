namespace Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(Guid id, string userId, CancellationToken cancellationToken);
}
