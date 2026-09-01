using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Account> Accounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
