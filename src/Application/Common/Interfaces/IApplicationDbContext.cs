using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Account> Accounts { get; }
    IQueryable<Category> Categories { get; }
    IQueryable<Transaction> Transactions { get; }
    IQueryable<Budget> Budgets { get; }
    IQueryable<User> Users { get; }
    IQueryable<PasswordResetToken> PasswordResetTokens { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
