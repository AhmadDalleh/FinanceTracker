using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken) =>
        await _context.Accounts.AddAsync(account, cancellationToken);

    public void Update(Account account) =>
        _context.Accounts.Update(account);
}
