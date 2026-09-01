using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);
    void Update(Transaction transaction);
    void Remove(Transaction transaction);
}
