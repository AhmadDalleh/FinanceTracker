using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Transactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken) =>
        await _context.Transactions.AddAsync(transaction, cancellationToken);

    public void Update(Transaction transaction) =>
        _context.Transactions.Update(transaction);

    public void Remove(Transaction transaction) =>
        _context.Transactions.Remove(transaction);
}
